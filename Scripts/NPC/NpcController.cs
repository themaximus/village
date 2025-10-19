using UnityEngine;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;

// УБРАЛИ [RequireComponent(typeof(Animator))]
[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(StatController))]
[RequireComponent(typeof(FactionIdentity))]
[RequireComponent(typeof(Collider2D))]
[RequireComponent(typeof(SpriteRenderer))]
public class NpcController : MonoBehaviour, ISaveable
{
    [Header("NPC Data")]
    public NpcData npcData;

    [Header("Pathfinding")]
    public List<Transform> waypoints = new List<Transform>();

    [Header("Obstacle Avoidance")]
    public LayerMask obstacleLayerMask;
    public float obstacleCheckDistance = 1f;
    public float avoidanceAngle = 30f;

    // --- Компоненты ---
    private Rigidbody2D rb;
    private StatController statController;
    private FactionIdentity myFactionIdentity;
    // private Animator animator; // УБРАЛИ
    private Collider2D npcCollider;
    private SpriteRenderer spriteRenderer;

    // --- Переменные для AI ---
    private enum State { Patrolling, Chasing, Returning }
    private State currentState;
    private Transform currentTarget;
    private StatController currentTargetStats;
    private float targetScanTimer;
    private const float TARGET_SCAN_INTERVAL = 1.0f;

    private Vector2 startPosition;
    private int currentWaypointIndex = 0;
    private Vector2 currentPatrolTarget;
    private bool hasPatrolTarget = false;
    private Vector2 moveDirection;
    private float aiStateTimer;
    private float attackCooldownTimer;
    private bool isDying = false;

    // --- Переменные для анимации ---
    private Sprite[] currentAnimationFrames;
    private int currentSpriteIndex;
    private float animationTimer;
    private Vector2 lastMoveDirection = Vector2.down;

    // --- Переменные для Атаки ---
    private bool isAttacking = false;
    private float actionTimer;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        statController = GetComponent<StatController>();
        myFactionIdentity = GetComponent<FactionIdentity>();
        // animator = GetComponent<Animator>(); // УБРАЛИ
        npcCollider = GetComponent<Collider2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        rb.freezeRotation = true;

        if (statController != null)
        {
            statController.OnDeath += HandleDeath;
        }
    }

    void Start()
    {
        if (npcData == null || myFactionIdentity.Faction == null)
        {
            Debug.LogErrorFormat("NpcData или Faction не назначены для {0}!", this.gameObject.name);
            this.enabled = false;
            return;
        }

        startPosition = transform.position;
        currentState = State.Patrolling;
        targetScanTimer = Random.Range(0, TARGET_SCAN_INTERVAL);

        aiStateTimer = Random.Range(2f, 5f);
        if (npcData.behavior == NpcData.NpcBehavior.Aggressive)
        {
            attackCooldownTimer = npcData.attackCooldown;
        }

        SetIdleAnimationBasedOnDirection();
    }

    void Update()
    {
        // --- ИЗМЕНЕН ПОРЯДОК ---
        // 1. Смерть - наивысший приоритет.
        if (isDying)
        {
            HandleDeathAnimation(); // <-- НОВЫЙ МЕТОД
            return;
        }

        // 2. Атака - второй приоритет.
        if (isAttacking)
        {
            HandleAttackAnimation();
            return;
        }

        // 3. Ходьба/Простой - все остальное время.
        HandleManualAnimation();

        // Логика AI выполняется, только если NPC не умирает и не атакует
        aiStateTimer -= Time.deltaTime;
        switch (npcData.behavior)
        {
            case NpcData.NpcBehavior.Peaceful:
                HandlePeacefulAI();
                break;
            case NpcData.NpcBehavior.Aggressive:
                HandleAggressiveAI();
                break;
            case NpcData.NpcBehavior.Calm:
                HandleCalmAI();
                break;
        }
    }

    // --- (FixedUpdate, OnDestroy, AttackTarget, HandleAttackAnimation - БЕЗ ИЗМЕНЕНИЙ) ---
    #region Unchanged_Methods
    void FixedUpdate()
    {
        if (isDying || isAttacking)
        {
            rb.velocity = Vector2.zero;
            return;
        }

        Vector2 safeMoveDirection = GetSafeDirection(moveDirection);
        rb.velocity = safeMoveDirection * npcData.moveSpeed;
    }

    void OnDestroy()
    {
        if (statController != null)
        {
            statController.OnDeath -= HandleDeath;
        }
    }

    private void AttackTarget()
    {
        if (currentTarget == null || isAttacking) return;

        isAttacking = true;
        actionTimer = 0f;
        animationTimer = 0f;
        currentSpriteIndex = 0;

        if (lastMoveDirection.y > 0.5f) currentAnimationFrames = npcData.attackNorth;
        else if (lastMoveDirection.y < -0.5f) currentAnimationFrames = npcData.attackSouth;
        else if (lastMoveDirection.x > 0.5f) currentAnimationFrames = npcData.attackEast;
        else currentAnimationFrames = npcData.attackWest;

        if (currentAnimationFrames == null || currentAnimationFrames.Length == 0)
        {
            Debug.LogWarning("NPC Attack animation is missing for " + npcData.name);
            isAttacking = false;
            return;
        }

        spriteRenderer.sprite = currentAnimationFrames[0];

        if (currentTargetStats != null)
        {
            currentTargetStats.TakeDamage(npcData.attackDamage);
        }
    }

    private void HandleAttackAnimation()
    {
        actionTimer += Time.deltaTime;
        animationTimer += Time.deltaTime;

        if (currentAnimationFrames == null || currentAnimationFrames.Length == 0)
        {
            isAttacking = false;
            return;
        }

        float frameRate = npcData.attackAnimationFrameRate;
        float duration = frameRate * currentAnimationFrames.Length;

        if (animationTimer >= frameRate)
        {
            animationTimer = 0f;
            currentSpriteIndex = Mathf.Min(currentSpriteIndex + 1, currentAnimationFrames.Length - 1);
            spriteRenderer.sprite = currentAnimationFrames[currentSpriteIndex];
        }

        if (actionTimer >= duration)
        {
            isAttacking = false;
            SetIdleAnimationBasedOnDirection();
        }
    }

    private void HandlePeacefulAI()
    {
        currentState = State.Patrolling;
        UpdatePatrolMovement();
    }
    private void HandleAggressiveAI()
    {
        targetScanTimer -= Time.deltaTime;

        if (currentTarget != null)
        {
            float distanceToTarget = Vector2.Distance(transform.position, currentTarget.position);
            bool targetIsDead = (currentTargetStats != null && currentTargetStats.Health.CurrentValue <= 0);

            if (distanceToTarget > npcData.detectionRadius || !currentTarget.gameObject.activeInHierarchy || targetIsDead)
            {
                currentTarget = null;
                currentTargetStats = null;
                currentState = State.Returning;
            }
        }

        if (currentTarget == null && targetScanTimer <= 0)
        {
            targetScanTimer = TARGET_SCAN_INTERVAL;
            FindHostileTarget();
            if (currentTarget != null)
            {
                currentState = State.Chasing;
            }
        }

        switch (currentState)
        {
            case State.Patrolling:
                UpdatePatrolMovement();
                break;
            case State.Chasing:
                UpdateChasingMovement();
                break;
            case State.Returning:
                UpdateReturningMovement();
                break;
        }
    }
    private void HandleCalmAI()
    {
        moveDirection = Vector2.zero;
        rb.velocity = Vector2.zero;
    }
    private void UpdatePatrolMovement()
    {
        switch (npcData.movementPattern)
        {
            case NpcData.MovementPattern.PatrolArea:
                if (!hasPatrolTarget || Vector2.Distance(transform.position, currentPatrolTarget) < 1.0f)
                {
                    Vector2 randomPoint = Random.insideUnitCircle * npcData.patrolRadius;
                    currentPatrolTarget = startPosition + randomPoint;
                    hasPatrolTarget = true;
                }
                moveDirection = (currentPatrolTarget - (Vector2)transform.position).normalized;
                break;
            case NpcData.MovementPattern.Waypoints:
                if (waypoints.Count == 0) { moveDirection = Vector2.zero; return; }
                Transform targetWaypoint = waypoints[currentWaypointIndex];
                if (Vector2.Distance(transform.position, targetWaypoint.position) < 1.0f)
                {
                    currentWaypointIndex = (currentWaypointIndex + 1) % waypoints.Count;
                }
                moveDirection = (targetWaypoint.position - transform.position).normalized;
                break;
            case NpcData.MovementPattern.RandomWander:
                if (aiStateTimer <= 0)
                {
                    if (rb.velocity.sqrMagnitude > 0.01f)
                    {
                        moveDirection = Vector2.zero;
                        aiStateTimer = Random.Range(3f, 6f);
                    }
                    else
                    {
                        float randomAngle = Random.Range(0, 360);
                        moveDirection = new Vector2(Mathf.Cos(randomAngle * Mathf.Deg2Rad), Mathf.Sin(randomAngle * Mathf.Deg2Rad)).normalized;
                        aiStateTimer = Random.Range(2f, 5f);
                    }
                }
                break;
        }
    }
    private void UpdateChasingMovement()
    {
        if (currentTarget == null)
        {
            currentState = State.Returning;
            return;
        }
        float distanceToTarget = Vector2.Distance(transform.position, currentTarget.position);
        if (distanceToTarget <= npcData.attackRadius)
        {
            moveDirection = Vector2.zero;
            attackCooldownTimer -= Time.deltaTime;
            if (attackCooldownTimer <= 0)
            {
                AttackTarget();
                attackCooldownTimer = npcData.attackCooldown;
            }
        }
        else
        {
            moveDirection = (currentTarget.position - transform.position).normalized;
        }
    }
    private void UpdateReturningMovement()
    {
        float arrivalThreshold = 1.0f;
        if (Vector2.Distance(transform.position, startPosition) < arrivalThreshold)
        {
            currentState = State.Patrolling;
            hasPatrolTarget = false;
        }
        else
        {
            moveDirection = (startPosition - (Vector2)transform.position).normalized;
        }
    }
    private void FindHostileTarget()
    {
        if (myFactionIdentity.Faction == null || myFactionIdentity.Faction.HostileTo.Count == 0) return;

        float closestDistance = float.MaxValue;
        Transform bestTarget = null;
        StatController bestTargetStats = null;

        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, npcData.detectionRadius);
        foreach (var hit in hits)
        {
            if (hit.transform == this.transform) continue;

            FactionIdentity targetIdentity = hit.GetComponent<FactionIdentity>();
            if (targetIdentity != null && targetIdentity.Faction != null)
            {
                if (myFactionIdentity.Faction.HostileTo.Contains(targetIdentity.Faction))
                {
                    StatController targetStats = hit.GetComponent<StatController>();
                    if (targetStats != null && targetStats.Health.CurrentValue > 0)
                    {
                        float distance = Vector2.Distance(transform.position, hit.transform.position);
                        if (distance < closestDistance)
                        {
                            closestDistance = distance;
                            bestTarget = hit.transform;
                            bestTargetStats = targetStats;
                        }
                    }
                }
            }
        }

        currentTarget = bestTarget;
        currentTargetStats = bestTargetStats;
    }
    private void HandleManualAnimation()
    {
        bool isMoving = rb.velocity.sqrMagnitude > 0.1f;
        if (isMoving)
        {
            if (Mathf.Abs(rb.velocity.x) > Mathf.Abs(rb.velocity.y))
            {
                lastMoveDirection = new Vector2(Mathf.Sign(rb.velocity.x), 0);
            }
            else
            {
                lastMoveDirection = new Vector2(0, Mathf.Sign(rb.velocity.y));
            }
        }
        UpdateAnimationSet(isMoving);
        AnimateSprites();
    }
    private void UpdateAnimationSet(bool isMoving)
    {
        Sprite[] newFrames = null;
        if (isMoving)
        {
            if (lastMoveDirection.y > 0.5f) newFrames = npcData.moveNorth;
            else if (lastMoveDirection.y < -0.5f) newFrames = npcData.moveSouth;
            else if (lastMoveDirection.x > 0.5f) newFrames = npcData.moveEast;
            else newFrames = npcData.moveWest;
        }
        else
        {
            if (lastMoveDirection.y > 0.5f) newFrames = npcData.idleNorth;
            else if (lastMoveDirection.y < -0.5f) newFrames = npcData.idleSouth;
            else if (lastMoveDirection.x > 0.5f) newFrames = npcData.idleEast;
            else newFrames = npcData.idleWest;
        }

        if (newFrames != null && newFrames.Length > 0 && currentAnimationFrames != newFrames)
        {
            currentAnimationFrames = newFrames;
            currentSpriteIndex = 0;
            animationTimer = 0f;
            spriteRenderer.sprite = currentAnimationFrames[0];
        }
    }
    private void AnimateSprites()
    {
        if (currentAnimationFrames == null || currentAnimationFrames.Length <= 1) return;
        animationTimer += Time.deltaTime;
        if (animationTimer >= npcData.animationFrameRate)
        {
            animationTimer = 0f;
            currentSpriteIndex = (currentSpriteIndex + 1) % currentAnimationFrames.Length;
            spriteRenderer.sprite = currentAnimationFrames[currentSpriteIndex];
        }
    }
    private void SetIdleAnimationBasedOnDirection()
    {
        UpdateAnimationSet(false);
    }
    private Vector2 GetSafeDirection(Vector2 desiredDirection)
    {
        if (desiredDirection == Vector2.zero) return Vector2.zero;
        RaycastHit2D hit = Physics2D.Raycast(transform.position, desiredDirection, obstacleCheckDistance, obstacleLayerMask);
        if (hit.collider != null)
        {
            Vector2 rightAvoidDirection = Quaternion.Euler(0, 0, avoidanceAngle) * desiredDirection;
            Vector2 leftAvoidDirection = Quaternion.Euler(0, 0, -avoidanceAngle) * desiredDirection;
            if (Physics2D.Raycast(transform.position, rightAvoidDirection, obstacleCheckDistance, obstacleLayerMask).collider == null)
                return rightAvoidDirection;
            if (Physics2D.Raycast(transform.position, leftAvoidDirection, obstacleCheckDistance, obstacleLayerMask).collider == null)
                return leftAvoidDirection;
            return Vector2.zero;
        }
        return desiredDirection;
    }
    #endregion

    // --- ИЗМЕНЕНИЯ ЗДЕСЬ ---
    private void HandleDeath()
    {
        if (isDying) return;
        isDying = true;

        if (npcCollider != null) npcCollider.enabled = false;
        if (rb != null)
        {
            rb.velocity = Vector2.zero;
            rb.isKinematic = true;
        }
        GameEvents.ReportEnemyDied(npcData);

        // Начинаем ручную анимацию смерти
        currentAnimationFrames = npcData.deathAnimation; //
        if (currentAnimationFrames != null && currentAnimationFrames.Length > 0)
        {
            currentSpriteIndex = 0;
            animationTimer = 0f;
            spriteRenderer.sprite = currentAnimationFrames[0];
        }
        else
        {
            // Если анимации смерти нет, просто выключаем скрипт
            this.enabled = false;
        }

        // animator.SetTrigger("Death"); // <-- УБРАЛИ
        // this.enabled = false; // <-- УБРАЛИ (теперь выключается в HandleDeathAnimation)
    }

    // --- НОВЫЙ МЕТОД ---
    /// <summary>
    /// Проигрывает анимацию смерти кадр за кадром вручную.
    /// </summary>
    private void HandleDeathAnimation()
    {
        if (currentAnimationFrames == null || currentAnimationFrames.Length == 0)
        {
            this.enabled = false; // Анимации нет, выключаемся
            return;
        }

        // Проверяем, не на последнем ли мы уже кадре
        if (currentSpriteIndex >= currentAnimationFrames.Length - 1)
        {
            this.enabled = false; // Анимация закончена, выключаем Update
            return;
        }

        animationTimer += Time.deltaTime;

        // Используем NpcData для длительности
        float frameDuration = npcData.deathAnimationDuration / currentAnimationFrames.Length;

        if (animationTimer >= frameDuration)
        {
            animationTimer -= frameDuration; // Вычитаем для большей точности
            currentSpriteIndex++;
            spriteRenderer.sprite = currentAnimationFrames[currentSpriteIndex];
        }
    }

    // --- (Save/Load и Gizmos БЕЗ ИЗМЕНЕНИЙ) ---
    #region Save_Load_Gizmos
    [System.Serializable]
    private struct PositionData { public float x, y, z; }
    public object CaptureState()
    {
        return new PositionData { x = transform.position.x, y = transform.position.y, z = transform.position.z };
    }
    public void RestoreState(object state)
    {
        var positionData = ((JObject)state).ToObject<PositionData>();
        transform.position = new Vector3(positionData.x, positionData.y, positionData.z);
    }
    void OnDrawGizmosSelected()
    {
        if (npcData != null)
        {
            if (npcData.behavior == NpcData.NpcBehavior.Aggressive)
            {
                Gizmos.color = Color.yellow;
                Gizmos.DrawWireSphere(transform.position, npcData.detectionRadius);
                Gizmos.color = Color.red;
                Gizmos.DrawWireSphere(transform.position, npcData.attackRadius);
            }
            if (npcData.movementPattern == NpcData.MovementPattern.PatrolArea)
            {
                Gizmos.color = Color.blue;
                Vector3 pos = Application.isPlaying ? (Vector3)startPosition : transform.position;
                Gizmos.DrawWireSphere(pos, npcData.patrolRadius);
            }
        }
    }
    #endregion
}