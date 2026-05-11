using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Tracks enemy health through BNG.Damageable and drops one or more prefabs on death.
/// Attach this to the same GameObject as the enemy's Damageable component.
/// </summary>
[RequireComponent(typeof(BNG.Damageable))]
public class EnemyHealthDrops : MonoBehaviour
{
    [Header("Health")]
    [Tooltip("Initial/max health value for this enemy.")]
    public float maxHealth = 100f;

    [Tooltip("If true, the script writes maxHealth into the Damageable component at startup.")]
    public bool syncDamageableHealth = true;

    [Header("Drops")]
    [Tooltip("Prefabs to spawn when the enemy dies.")]
    public GameObject[] dropPrefabs;

    [Tooltip("How many drops to spawn from the array.")]
    public int dropsToSpawn = 1;

    [Tooltip("Random spawn radius around the enemy for dropped prefabs.")]
    public float dropRadius = 0.35f;

    [Tooltip("Upward force applied to each spawned drop.")]
    public float dropUpwardForce = 1.5f;

    [Tooltip("Forward force applied to each spawned drop.")]
    public float dropForwardForce = 2f;

    [Tooltip("Destroy spawned drops if they have no Rigidbody or physics interaction.")]
    public bool destroyDropsAfterSpawn = false;

    [Tooltip("Lifetime for spawned drops when destroyDropsAfterSpawn is true.")]
    public float dropLifetime = 10f;

    [Header("Events")]
    public UnityEvent onDeath;
    public UnityEvent<float> onHealthChanged;

    public float CurrentHealth { get; private set; }

    private BNG.Damageable damageable;
    private bool hasDied;

    private void Awake()
    {
        damageable = GetComponent<BNG.Damageable>();
        CurrentHealth = Mathf.Max(1f, maxHealth);
    }

    private void Start()
    {
        if (damageable == null)
        {
            return;
        }

        if (syncDamageableHealth)
        {
            damageable.Health = maxHealth;
        }

        CurrentHealth = damageable.Health;
        onHealthChanged?.Invoke(CurrentHealth);

        damageable.onDamaged.AddListener(HandleDamaged);
        damageable.onDestroyed.AddListener(HandleDestroyed);
    }

    private void OnDisable()
    {
        if (damageable == null)
        {
            return;
        }

        damageable.onDamaged.RemoveListener(HandleDamaged);
        damageable.onDestroyed.RemoveListener(HandleDestroyed);
    }

    public void SetHealth(float value)
    {
        CurrentHealth = Mathf.Clamp(value, 0f, maxHealth);

        if (damageable != null)
        {
            damageable.Health = CurrentHealth;
        }

        onHealthChanged?.Invoke(CurrentHealth);
    }

    public void Heal(float amount)
    {
        if (amount <= 0f)
        {
            return;
        }

        SetHealth(CurrentHealth + amount);
    }

    public void DealDamage(float amount)
    {
        if (damageable == null || hasDied)
        {
            return;
        }

        damageable.DealDamage(amount);
    }

    private void HandleDamaged(float damageAmount)
    {
        if (damageable == null)
        {
            return;
        }

        CurrentHealth = Mathf.Max(0f, damageable.Health);
        onHealthChanged?.Invoke(CurrentHealth);
    }

    private void HandleDestroyed()
    {
        if (hasDied)
        {
            return;
        }

        hasDied = true;
        CurrentHealth = 0f;
        onHealthChanged?.Invoke(CurrentHealth);

        SpawnDrops();
        onDeath?.Invoke();
    }

    private void SpawnDrops()
    {
        if (dropPrefabs == null || dropPrefabs.Length == 0 || dropsToSpawn <= 0)
        {
            return;
        }

        int spawnCount = Mathf.Min(dropsToSpawn, dropPrefabs.Length);
        Vector3 basePosition = transform.position;

        for (int i = 0; i < spawnCount; i++)
        {
            GameObject prefab = dropPrefabs[Random.Range(0, dropPrefabs.Length)];
            if (prefab == null)
            {
                continue;
            }

            Vector3 offset = Random.insideUnitSphere * dropRadius;
            offset.y = Mathf.Abs(offset.y);

            GameObject drop = Instantiate(prefab, basePosition + offset, Random.rotation);

            if (drop.TryGetComponent(out Rigidbody rb))
            {
                Vector3 throwDirection = (transform.forward + Vector3.up * dropUpwardForce).normalized;
                rb.isKinematic = false;
                rb.AddForce(throwDirection * dropForwardForce, ForceMode.Impulse);
            }

            if (destroyDropsAfterSpawn)
            {
                Destroy(drop, dropLifetime);
            }
        }
    }
}
