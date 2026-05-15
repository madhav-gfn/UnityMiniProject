using UnityEngine;
using UnityEngine.Events;
using UnityEngine.AI;

/// <summary>
/// Base behavior for simple VR-friendly enemy AI.
/// Handles target tracking, region wandering, movement, animation parameters,
/// and enemy damage integration through BNG.Damageable.
/// </summary>
[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(CapsuleCollider))]
[RequireComponent(typeof(BNG.Damageable))]
public class EnemyAIBase : MonoBehaviour
{
    [Header("Target")]
    [Tooltip("The player or headset transform. If empty, the script tries Camera.main.")]
    public Transform playerTarget;

    [Tooltip("If true, the script will try to find a target automatically.")]
    public bool autoFindTarget = true;

    [Header("Region Movement")]
    [Tooltip("Center of the movement region. If empty, the enemy's starting position is used.")]
    public Transform regionCenter;

    [Tooltip("Radius of the region the enemy can wander inside.")]
    public float regionRadius = 6f;

    [Tooltip("How fast the enemy moves while wandering or chasing.")]
    public float moveSpeed = 1.75f;
    [Tooltip("How quickly the enemy turns toward its movement direction.")]
    public float turnSpeed = 8f;

    [Header("Navigation")]
    [Tooltip("Obstacle avoidance quality for NavMeshAgent.")]
    public ObstacleAvoidanceType obstacleAvoidanceType = ObstacleAvoidanceType.HighQualityObstacleAvoidance;

    [Tooltip("Randomize avoidance priority to reduce crowding deadlocks.")]
    public bool randomizeAvoidancePriority = true;

    [Tooltip("Minimum randomized avoidance priority (0-99).")]
    public int avoidancePriorityMin = 35;

    [Tooltip("Maximum randomized avoidance priority (0-99).")]
    public int avoidancePriorityMax = 65;

    [Tooltip("Sync NavMeshAgent radius/height to the CapsuleCollider when available.")]
    public bool syncAgentWithCapsule = true;

    [Tooltip("If the target is within this range, the enemy will chase instead of wandering.")]
    public float chaseRange = 10f;

    [Tooltip("If true, taking damage makes the enemy focus the player even outside normal chase range.")]
    public bool aggroOnDamage = true;

    [Tooltip("How long damage aggro lasts. Use 0 or less to stay aggressive until death.")]
    public float damageAggroDuration = 12f;

    [Tooltip("How close the enemy must get to a wander point before choosing a new one.")]
    public float wanderPointTolerance = 0.5f;

    [Tooltip("Pause time between picking new wander points.")]
    public float wanderPause = 0.8f;

    [Header("Animation Clips")]
    public AnimationClip idleAnimation;
    public AnimationClip forwardAnimation;
    public AnimationClip takeDamageAnimation;
    public AnimationClip attackAnimation;
    public AnimationClip deathAnimation;

    [Header("Animator Parameters")]
    public string moveSpeedParameter = "MoveSpeed";
    public string attackTrigger = "Attack";
    public string takeDamageTrigger = "TakeDamage";
    public string deathTrigger = "Die";

    [Header("Death")]
    [Tooltip("Delay before destroying enemy if no death clip is provided.")]
    public float destroyDelayIfNoDeathClip = 1.25f;

    [Header("Events")]
    public UnityEvent onAttack;
    public UnityEvent onDeath;

    protected Animator animator;
    protected NavMeshAgent navAgent;
    protected Rigidbody body;
    protected CapsuleCollider capsule;
    protected BNG.Damageable damageable;

    protected Vector3 startPosition;
    protected Vector3 wanderTarget;
    protected float nextWanderTime;
    protected bool isDead;
    protected bool isAggroed;
    protected float aggroUntilTime;
    private bool isDeathRoutineRunning;

    protected virtual void Reset()
    {
        EnsureRequiredComponents();
        regionCenter = transform;
    }

    protected virtual void Awake()
    {
        CacheComponents();
        ConfigureComponents();
        startPosition = transform.position;

        if (regionCenter == null)
        {
            regionCenter = transform;
        }

        PickNewWanderTarget();
    }

    protected virtual void OnEnable()
    {
        CacheComponents();

        if (damageable != null)
        {
            damageable.onDamaged.AddListener(HandleDamaged);
            damageable.onDestroyed.AddListener(HandleDestroyed);
        }
    }

    protected virtual void OnDisable()
    {
        if (damageable != null)
        {
            damageable.onDamaged.RemoveListener(HandleDamaged);
            damageable.onDestroyed.RemoveListener(HandleDestroyed);
        }
    }

    public virtual void EnsureRequiredComponents()
    {
        if (TryGetComponent(out Animator existingAnimator) == false)
        {
            existingAnimator = gameObject.AddComponent<Animator>();
        }

        if (TryGetComponent(out NavMeshAgent existingAgent) == false)
        {
            existingAgent = gameObject.AddComponent<NavMeshAgent>();
        }

        if (TryGetComponent(out Rigidbody existingBody) == false)
        {
            existingBody = gameObject.AddComponent<Rigidbody>();
        }

        if (TryGetComponent(out CapsuleCollider existingCapsule) == false)
        {
            existingCapsule = gameObject.AddComponent<CapsuleCollider>();
        }

        if (TryGetComponent(out BNG.Damageable existingDamageable) == false)
        {
            existingDamageable = gameObject.AddComponent<BNG.Damageable>();
        }

        animator = existingAnimator;
        navAgent = existingAgent;
        body = existingBody;
        capsule = existingCapsule;
        damageable = existingDamageable;

        ConfigureComponents();
    }

    protected virtual void FixedUpdate()
    {
        if (isDead)
        {
            return;
        }

        if (autoFindTarget)
        {
            ResolveTarget();
        }

        Vector3 desiredDestination = GetDesiredDestination(Time.fixedDeltaTime);
        MoveTowards(desiredDestination, Time.fixedDeltaTime);
        UpdateAnimatorState(Time.fixedDeltaTime);
    }

    protected virtual Vector3 GetDesiredDestination(float deltaTime)
    {
        if (HasTargetInRange(chaseRange) || IsAggroActive())
        {
            return FlatPosition(playerTarget.position);
        }

        if (Time.time >= nextWanderTime && Vector3.Distance(GetCurrentPosition(), wanderTarget) <= wanderPointTolerance)
        {
            PickNewWanderTarget();
        }

        return wanderTarget;
    }

    protected virtual void MoveTowards(Vector3 destination, float deltaTime)
    {
        if (navAgent == null || !navAgent.isActiveAndEnabled || !navAgent.isOnNavMesh)
        {
            return;
        }

        if (navAgent.isStopped)
        {
            navAgent.isStopped = false;
        }

        if ((navAgent.destination - destination).sqrMagnitude > 0.01f)
        {
            navAgent.SetDestination(destination);
        }
    }

    protected virtual void UpdateAnimatorState(float deltaTime)
    {
        if (animator == null)
        {
            return;
        }

        float currentSpeed = navAgent != null ? navAgent.velocity.magnitude : 0f;
        float normalizedSpeed = moveSpeed <= 0f ? 0f : (currentSpeed / moveSpeed);
        animator.SetFloat(moveSpeedParameter, normalizedSpeed);
    }

    protected virtual void PickNewWanderTarget()
    {
        Vector3 center = regionCenter != null ? regionCenter.position : startPosition;
        float angle = Random.Range(0f, Mathf.PI * 2f);
        float radius = Mathf.Sqrt(Random.value) * regionRadius;
        Vector3 offset = new Vector3(Mathf.Cos(angle), 0f, Mathf.Sin(angle)) * radius;

        wanderTarget = FlatPosition(center + offset);
        nextWanderTime = Time.time + wanderPause;
    }

    protected bool HasTargetInRange(float range)
    {
        return playerTarget != null && Vector3.Distance(GetCurrentPosition(), playerTarget.position) <= range;
    }

    protected bool ResolveTarget()
    {
        if (playerTarget != null)
        {
            return true;
        }

        if (Camera.main != null)
        {
            playerTarget = Camera.main.transform;
            return true;
        }

        GameObject taggedCamera = GameObject.FindGameObjectWithTag("MainCamera");
        if (taggedCamera != null)
        {
            playerTarget = taggedCamera.transform;
            return true;
        }

        return false;
    }

    protected Vector3 FlatPosition(Vector3 position)
    {
        float y = regionCenter != null ? regionCenter.position.y : startPosition.y;
        return new Vector3(position.x, y, position.z);
    }

    protected float GetTargetDistance()
    {
        if (playerTarget == null)
        {
            return float.MaxValue;
        }

        return Vector3.Distance(GetCurrentPosition(), playerTarget.position);
    }

    protected bool IsInsideRegion(Vector3 position)
    {
        Vector3 center = regionCenter != null ? regionCenter.position : startPosition;
        position.y = 0f;
        center.y = 0f;
        return Vector3.Distance(position, center) <= regionRadius;
    }

    protected virtual void TriggerAttackAnimation()
    {
        if (animator != null && !string.IsNullOrEmpty(attackTrigger))
        {
            animator.SetTrigger(attackTrigger);
        }

        onAttack?.Invoke();
    }

    protected virtual void TriggerDamageAnimation()
    {
        if (animator != null && !string.IsNullOrEmpty(takeDamageTrigger))
        {
            animator.SetTrigger(takeDamageTrigger);
        }
    }

    protected virtual void HandleDamaged(float damageAmount)
    {
        if (isDead)
        {
            return;
        }

        if (aggroOnDamage)
        {
            BecomeAggroed();
        }

        TriggerDamageAnimation();
    }

    protected void BecomeAggroed()
    {
        isAggroed = true;
        aggroUntilTime = damageAggroDuration <= 0f ? float.PositiveInfinity : Time.time + damageAggroDuration;

        if (autoFindTarget)
        {
            ResolveTarget();
        }
    }

    protected bool IsAggroActive()
    {
        if (!isAggroed)
        {
            return false;
        }

        if (Time.time <= aggroUntilTime)
        {
            return playerTarget != null;
        }

        isAggroed = false;
        return false;
    }

    protected virtual void HandleDestroyed()
    {
        if (isDeathRoutineRunning)
        {
            return;
        }

        isDead = true;
        isDeathRoutineRunning = true;

        if (navAgent != null && navAgent.isActiveAndEnabled)
        {
            navAgent.isStopped = true;
        }

        if (capsule != null)
        {
            capsule.enabled = false;
        }

        TriggerDeathAnimation();
        onDeath?.Invoke();

        float deathDelay = deathAnimation != null ? deathAnimation.length : destroyDelayIfNoDeathClip;
        Destroy(gameObject, Mathf.Max(0.05f, deathDelay));
    }

    protected virtual void TriggerDeathAnimation()
    {
        if (animator == null)
        {
            return;
        }

        if (!string.IsNullOrEmpty(deathTrigger))
        {
            animator.SetTrigger(deathTrigger);
            return;
        }

        if (deathAnimation != null)
        {
            animator.CrossFade(deathAnimation.name, 0.05f);
        }
    }

    protected virtual void ApplyDamageToTarget(float damageAmount, Vector3 hitPosition, Vector3 hitNormal)
    {
        if (playerTarget == null)
        {
            return;
        }

        PlayerHealth playerHealth = playerTarget.GetComponentInParent<PlayerHealth>();
        if (playerHealth == null)
        {
            playerHealth = playerTarget.GetComponentInChildren<PlayerHealth>();
        }

        if (playerHealth != null)
        {
            playerHealth.TakeDamage(damageAmount);
            return;
        }

        BNG.Damageable targetDamageable = playerTarget.GetComponentInParent<BNG.Damageable>();
        if (targetDamageable == null)
        {
            targetDamageable = playerTarget.GetComponentInChildren<BNG.Damageable>();
        }

        if (targetDamageable != null)
        {
            targetDamageable.DealDamage(damageAmount, hitPosition, hitNormal, true, gameObject, playerTarget.gameObject);
        }
    }

    protected void ConfigureComponents()
    {
        if (navAgent != null)
        {
            navAgent.speed = Mathf.Max(0.1f, moveSpeed);
            navAgent.angularSpeed = Mathf.Max(30f, turnSpeed * 90f);
            navAgent.acceleration = Mathf.Max(1f, moveSpeed * 4f);
            navAgent.stoppingDistance = 0.15f;
            navAgent.autoBraking = true;
            navAgent.autoRepath = true;
            navAgent.obstacleAvoidanceType = obstacleAvoidanceType;

            if (randomizeAvoidancePriority)
            {
                int minPriority = Mathf.Clamp(avoidancePriorityMin, 0, 99);
                int maxPriority = Mathf.Clamp(avoidancePriorityMax, minPriority, 99);
                navAgent.avoidancePriority = Random.Range(minPriority, maxPriority + 1);
            }

            if (syncAgentWithCapsule && capsule != null)
            {
                navAgent.radius = Mathf.Max(0.1f, capsule.radius * 0.95f);
                navAgent.height = Mathf.Max(1f, capsule.height);
            }
        }

        if (body != null)
        {
            body.useGravity = true;
            body.isKinematic = true;
            body.constraints = RigidbodyConstraints.FreezeRotation;
            body.interpolation = RigidbodyInterpolation.Interpolate;
            body.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
        }

        if (capsule != null)
        {
            capsule.height = Mathf.Max(capsule.height, 1.8f);
            capsule.radius = Mathf.Max(capsule.radius, 0.35f);
            capsule.center = new Vector3(0f, capsule.height * 0.5f, 0f);
        }

        if (damageable != null)
        {
            damageable.DestroyOnDeath = false;
            damageable.Respawn = false;
        }
    }

    protected Vector3 GetCurrentPosition()
    {
        if (navAgent != null)
        {
            return navAgent.transform.position;
        }

        return transform.position;
    }

    protected void CacheComponents()
    {
        if (animator == null)
        {
            TryGetComponent(out animator);
        }

        if (body == null)
        {
            TryGetComponent(out body);
        }

        if (navAgent == null)
        {
            TryGetComponent(out navAgent);
        }

        if (capsule == null)
        {
            TryGetComponent(out capsule);
        }

        if (damageable == null)
        {
            TryGetComponent(out damageable);
        }
    }
}
