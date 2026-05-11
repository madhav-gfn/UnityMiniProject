using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Simple player health component for VR/player damage tracking.
/// Attach this to the player root or a parent of the player camera.
/// </summary>
public class PlayerHealth : MonoBehaviour
{
    [Header("Health")]
    public float maxHealth = 100f;

    [Tooltip("Current player health. Read-only in the inspector.")]
    [SerializeField]
    private float currentHealth = 100f;

    [Header("Events")]
    public UnityEvent<float> onHealthChanged;
    public UnityEvent onDeath;

    public float CurrentHealth => currentHealth;
    public bool IsDead => currentHealth <= 0f;

    private void Awake()
    {
        currentHealth = Mathf.Clamp(currentHealth, 0f, maxHealth);

        if (currentHealth <= 0f)
        {
            currentHealth = maxHealth;
        }
    }

    private void Start()
    {
        onHealthChanged?.Invoke(currentHealth);
    }

    public void SetHealth(float value)
    {
        currentHealth = Mathf.Clamp(value, 0f, maxHealth);
        onHealthChanged?.Invoke(currentHealth);

        if (currentHealth <= 0f)
        {
            onDeath?.Invoke();
        }
    }

    public void Heal(float amount)
    {
        if (amount <= 0f || IsDead)
        {
            return;
        }

        SetHealth(currentHealth + amount);
    }

    public void TakeDamage(float amount)
    {
        if (amount <= 0f || IsDead)
        {
            return;
        }

        SetHealth(currentHealth - amount);
    }

    public void ResetHealth()
    {
        SetHealth(maxHealth);
    }
}
