using UnityEngine;

/// <summary>
/// Simple melee enemy: chases the player inside a region and deals damage when close enough.
/// Damage scales with proximity so closer hits hurt more.
/// </summary>
public class EnemyMeleeAI : EnemyAIBase
{
    [Header("Melee Attack")]
    [Tooltip("Distance at which the melee enemy can attack.")]
    public float attackDistance = 1.75f;

    [Tooltip("Seconds between melee attacks.")]
    public float attackCooldown = 1.2f;

    [Tooltip("Damage dealt when the player is at the edge of the melee range.")]
    public float minDamage = 6f;

    [Tooltip("Damage dealt when the player is very close to the enemy.")]
    public float maxDamage = 20f;

    [Tooltip("If true, a damaged melee enemy immediately attacks when the player is in melee range.")]
    public bool retaliateWhenDamaged = true;

    [Header("Combat Movement")]
    [Tooltip("How often the melee enemy picks a small sidestep while in melee range.")]
    public float combatRepositionInterval = 1.4f;

    [Tooltip("Sideways distance used for small combat repositioning moves.")]
    public float strafeDistance = 1.1f;

    [Tooltip("Movement speed multiplier while strafing near the player.")]
    public float combatMoveSpeedMultiplier = 0.6f;

    private float nextAttackTime;
    private Vector3 combatMoveTarget;
    private float nextCombatRepositionTime;
    private int strafeDirection = 1;

    protected override Vector3 GetDesiredDestination(float deltaTime)
    {
        if (playerTarget == null)
        {
            return base.GetDesiredDestination(deltaTime);
        }

        float distance = GetTargetDistance();
        if (distance <= attackDistance)
        {
            SetAgentSpeed(moveSpeed * combatMoveSpeedMultiplier);
            return GetCombatRepositionTarget();
        }

        SetAgentSpeed(moveSpeed);
        return base.GetDesiredDestination(deltaTime);
    }

    protected override void FixedUpdate()
    {
        base.FixedUpdate();

        if (isDead || playerTarget == null)
        {
            return;
        }

        float distance = GetTargetDistance();
        if (distance > attackDistance || Time.time < nextAttackTime)
        {
            return;
        }

        PerformAttack(distance);
    }

    protected override void HandleDamaged(float damageAmount)
    {
        base.HandleDamaged(damageAmount);

        if (!retaliateWhenDamaged || isDead || playerTarget == null)
        {
            return;
        }

        float distance = GetTargetDistance();
        if (distance <= attackDistance)
        {
            nextAttackTime = Time.time;
            PerformAttack(distance);
        }
    }

    private void PerformAttack(float distance)
    {
        nextAttackTime = Time.time + attackCooldown;

        FacePlayer();
        float normalized = 1f - Mathf.Clamp01(distance / Mathf.Max(attackDistance, 0.01f));
        float damage = Mathf.Lerp(minDamage, maxDamage, normalized);

        TriggerAttackAnimation();
        ApplyDamageToTarget(damage, playerTarget.position, -transform.forward);
    }

    private void FacePlayer()
    {
        if (playerTarget == null)
        {
            return;
        }

        Vector3 directionToPlayer = playerTarget.position - transform.position;
        directionToPlayer.y = 0f;

        if (directionToPlayer.sqrMagnitude < 0.0001f)
        {
            return;
        }

        Quaternion targetRotation = Quaternion.LookRotation(directionToPlayer.normalized, Vector3.up);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, 18f * Time.deltaTime);
    }

    private Vector3 GetCombatRepositionTarget()
    {
        Vector3 currentPosition = GetCurrentPosition();

        if (Time.time < nextCombatRepositionTime && Vector3.Distance(currentPosition, combatMoveTarget) > 0.3f)
        {
            return combatMoveTarget;
        }

        nextCombatRepositionTime = Time.time + combatRepositionInterval;
        strafeDirection *= -1;

        Vector3 toPlayer = playerTarget.position - currentPosition;
        toPlayer.y = 0f;
        if (toPlayer.sqrMagnitude < 0.0001f)
        {
            toPlayer = transform.forward;
        }

        Vector3 strafe = Vector3.Cross(Vector3.up, toPlayer.normalized) * strafeDirection;
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
