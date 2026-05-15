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

    [Tooltip("Movement speed multiplier while backing away from a close player.")]
    public float closeRetreatSpeedMultiplier = 0.45f;

    [Tooltip("How often the enemy picks a small sidestep while already in a good firing position.")]
    public float combatRepositionInterval = 2.5f;

    [Tooltip("Sideways distance used for small combat repositioning moves.")]
    public float strafeDistance = 1.75f;

    [Tooltip("Distance tolerance around the preferred range before the enemy adjusts position.")]
    public float preferredDistanceBuffer = 0.75f;

    private float nextAttackTime;
    private Vector3 combatMoveTarget;
    private float nextCombatRepositionTime;
    private int strafeDirection = 1;

    protected override void Awake()
    {
        base.Awake();

        if (navAgent != null)
        {
            navAgent.updateRotation = false;
        }
    }

    protected override Vector3 GetDesiredDestination(float deltaTime)
    {
        if (playerTarget == null)
        {
            SetAgentSpeed(moveSpeed);
            return GetCurrentPosition();
        }

        float distance = GetTargetDistance();
        bool isEngaged = IsAggroActive() || distance <= Mathf.Max(chaseRange, attackRange);

        if (!isEngaged)
        {
            SetAgentSpeed(moveSpeed);
            return GetCurrentPosition();
        }

        Vector3 currentPosition = GetCurrentPosition();
        Vector3 targetPosition = FlatPosition(playerTarget.position);
        Vector3 awayFromPlayer = currentPosition - targetPosition;
        awayFromPlayer.y = 0f;

        if (awayFromPlayer.sqrMagnitude < 0.0001f)
        {
            awayFromPlayer = -transform.forward;
        }

        awayFromPlayer.Normalize();

        if (distance < minimumDistance)
        {
            SetAgentSpeed(moveSpeed * closeRetreatSpeedMultiplier);
            Vector3 retreatTarget = currentPosition + awayFromPlayer * Mathf.Max(1f, minimumDistance - distance + 1f);
            return KeepDestinationInRegion(retreatTarget, currentPosition);
        }

        if (distance > attackRange)
        {
            SetAgentSpeed(moveSpeed);
            Vector3 approachTarget = targetPosition + awayFromPlayer * preferredDistance;
            return KeepDestinationInRegion(approachTarget, currentPosition);
        }

        if (distance > preferredDistance + preferredDistanceBuffer)
        {
            SetAgentSpeed(moveSpeed * 0.75f);
            Vector3 closeToPreferredRange = targetPosition + awayFromPlayer * preferredDistance;
            return KeepDestinationInRegion(closeToPreferredRange, currentPosition);
        }

        if (distance < preferredDistance - preferredDistanceBuffer)
        {
            SetAgentSpeed(moveSpeed * closeRetreatSpeedMultiplier);
            Vector3 backpedalTarget = currentPosition + awayFromPlayer * Mathf.Max(0.75f, preferredDistance - distance);
            return KeepDestinationInRegion(backpedalTarget, currentPosition);
        }

        SetAgentSpeed(moveSpeed * 0.55f);
        return GetCombatRepositionTarget(currentPosition, awayFromPlayer);
    }

    protected override void FixedUpdate()
    {
        base.FixedUpdate();

        if (isDead || playerTarget == null)
        {
            return;
        }

        float distance = GetTargetDistance();
        if (distance > attackRange)
        {
            return;
        }

        FacePlayer();

        if (Time.time >= nextAttackTime && IsAlignedWithPlayer())
        {
            FireAtTarget();
        }
    }

    private bool IsAlignedWithPlayer()
    {
        if (playerTarget == null || navAgent == null)
        {
            return true;
        }

        Vector3 toPlayer = playerTarget.position - transform.position;
        toPlayer.y = 0f;
        if (toPlayer.sqrMagnitude < 0.0001f)
        {
            return true;
        }

        float dot = Vector3.Dot(transform.forward, toPlayer.normalized);
        return dot > 0.95f;
    }

    private void FacePlayer()
    {
        if (playerTarget == null)
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
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, aimRotationSpeed * Time.deltaTime);
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
                return;
            }

            if (hit.collider.transform == playerTarget || hit.collider.transform.IsChildOf(playerTarget))
            {
                ApplyDamageToTarget(damage, hit.point, hit.normal);
            }
        }
    }

    private Vector3 GetCombatRepositionTarget(Vector3 currentPosition, Vector3 awayFromPlayer)
    {
        if (Time.time < nextCombatRepositionTime && Vector3.Distance(currentPosition, combatMoveTarget) > 0.35f)
        {
            return combatMoveTarget;
        }

        nextCombatRepositionTime = Time.time + combatRepositionInterval;
        strafeDirection *= -1;

        Vector3 strafe = Vector3.Cross(Vector3.up, awayFromPlayer).normalized * strafeDirection;
        Vector3 repositionTarget = currentPosition + strafe * strafeDistance;
        combatMoveTarget = KeepDestinationInRegion(repositionTarget, currentPosition);
        return combatMoveTarget;
    }

    private Vector3 KeepDestinationInRegion(Vector3 destination, Vector3 fallback)
    {
        destination = FlatPosition(destination);
        return IsInsideRegion(destination) ? destination : fallback;
    }

    private void SetAgentSpeed(float speed)
    {
        if (navAgent != null)
        {
            navAgent.speed = Mathf.Max(0.1f, speed);
        }
    }
}
