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

    private float nextAttackTime;

    protected override Vector3 GetDesiredDestination(float deltaTime)
    {
        if (HasTargetInRange(attackDistance))
        {
            return GetCurrentPosition();
        }

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
}
