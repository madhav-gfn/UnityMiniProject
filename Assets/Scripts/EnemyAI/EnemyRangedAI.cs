using UnityEngine;

/// <summary>
/// Simple ranged enemy: keeps distance, shoots at the player, and can optionally spawn a projectile prefab.
/// Falls back to raycast damage if no projectile prefab is assigned.
/// </summary>
public class EnemyRangedAI : EnemyAIBase
{
    [Header("Ranged Attack")]
    [Tooltip("Preferred distance from the player.")]
    public float preferredDistance = 7f;

    [Tooltip("If the player is closer than this, the enemy retreats.")]
    public float minimumDistance = 4f;

    [Tooltip("Maximum range for ranged attacks.")]
    public float attackRange = 12f;

    [Tooltip("Seconds between ranged attacks.")]
    public float attackCooldown = 1.6f;

    [Tooltip("Damage dealt by the ranged attack.")]
    public float damage = 12f;

    [Tooltip("Optional spawn point for projectiles. If empty, the enemy's transform is used.")]
    public Transform firePoint;

    [Tooltip("Optional projectile prefab. If not assigned, the attack uses a raycast.")]
    public GameObject projectilePrefab;

    [Tooltip("Projectile speed if a prefab is spawned.")]
    public float projectileSpeed = 18f;

    [Tooltip("Layer mask used for ranged raycasts.")]
    public LayerMask hitMask = ~0;

    [Tooltip("How quickly the ranged enemy rotates to face the target before firing.")]
    public float aimRotationSpeed = 12f;

    private float nextAttackTime;
    private bool isAiming;

    protected override Vector3 GetDesiredDestination(float deltaTime)
    {
        if (playerTarget == null)
        {
            return base.GetDesiredDestination(deltaTime);
        }

        float distance = GetTargetDistance();
        Vector3 targetPosition = FlatPosition(playerTarget.position);

        if (distance > preferredDistance)
        {
            return targetPosition;
        }

        if (distance < minimumDistance)
        {
            Vector3 away = GetCurrentPosition() - playerTarget.position;
            away.y = 0f;

            if (away.sqrMagnitude < 0.0001f)
            {
                away = -transform.forward;
            }

            return FlatPosition(GetCurrentPosition() + away.normalized * 2f);
        }

        return GetCurrentPosition();
    }

    protected override void FixedUpdate()
    {
        base.FixedUpdate();

        if (isDead || playerTarget == null)
        {
            isAiming = false;
            return;
        }

        float distance = GetTargetDistance();
        if (distance > attackRange || Time.time < nextAttackTime)
        {
            isAiming = false;
            return;
        }

        if (!isAiming)
        {
            FacePlayer();
            isAiming = true;
        }
        else if (IsAlignedWithPlayer())
        {
            FireAtTarget();
            isAiming = false;
        }
    }

    private bool IsAlignedWithPlayer()
    {
        if (playerTarget == null || navAgent == null)
        {
            return true;
        }

        Vector3 toPlayer = (playerTarget.position - transform.position).normalized;
        toPlayer.y = 0f;
        float dot = Vector3.Dot(transform.forward, toPlayer);
        return dot > 0.95f;
    }

    private void FacePlayer()
    {
        if (playerTarget == null || navAgent == null)
        {
            return;
        }

        Vector3 directionToPlayer = (playerTarget.position - transform.position);
        directionToPlayer.y = 0f;

        if (directionToPlayer.sqrMagnitude < 0.0001f)
        {
            return;
        }

        Quaternion targetRotation = Quaternion.LookRotation(directionToPlayer.normalized, Vector3.up);
        navAgent.transform.rotation = Quaternion.Slerp(navAgent.transform.rotation, targetRotation, aimRotationSpeed * Time.deltaTime);
    }

    private void FireAtTarget()
    {
        nextAttackTime = Time.time + attackCooldown;
        TriggerAttackAnimation();

        Transform spawnPoint = firePoint != null ? firePoint : transform;
        Vector3 direction = (playerTarget.position - spawnPoint.position).normalized;

        if (projectilePrefab != null)
        {
            GameObject projectile = Object.Instantiate(projectilePrefab, spawnPoint.position, Quaternion.LookRotation(direction, Vector3.up));
            
            if (projectile.TryGetComponent(out Rigidbody projectileBody))
            {
                projectileBody.isKinematic = false;
                projectileBody.useGravity = true;
                projectileBody.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
                projectileBody.linearVelocity = direction * projectileSpeed;
            }

            if (projectile.TryGetComponent(out Collider projectileCollider))
            {
                projectileCollider.enabled = true;
            }

            return;
        }

        Ray ray = new Ray(spawnPoint.position, direction);
        if (Physics.Raycast(ray, out RaycastHit hit, attackRange, hitMask, QueryTriggerInteraction.Ignore))
        {
            BNG.Damageable targetDamageable = hit.collider.GetComponentInParent<BNG.Damageable>();
            if (targetDamageable != null)
            {
                targetDamageable.DealDamage(damage, hit.point, hit.normal, true, gameObject, hit.collider.gameObject);
            }
        }
    }
}
