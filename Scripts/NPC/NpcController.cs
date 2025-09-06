using UnityEngine;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(SpriteRenderer))]
[RequireComponent(typeof(StatController))]
[RequireComponent(typeof(FactionIdentity))] // <-- Добавлено требование
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

    // Компоненты
    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;
    private StatController statController;
    private FactionIdentity myFactionIdentity; // <-- НОВОЕ

    // Анимация
    private Sprite[] currentAnimationFrames;
    private int currentSpriteIndex;
    private float animationTimer;
    private Vector2 lastMoveDirection = Vector2.down;
    private bool isMoving;
    private bool isAttacking = false;
    private float attackAnimationTimer = 0f;

    // Переменные для AI
    private enum State { Patrolling, Chasing, Returning }
    private State currentState;
    private Transform currentTarget; // <-- НОВОЕ: Универсальная цель
    private float targetScanTimer;   // <-- НОВОЕ: Таймер для сканирования
    private const float TARGET_SCAN_INTERVAL = 1.0f; // Сканируем цели раз в секунду

    private Vector3 startPosition;
    private int currentWaypointIndex = 0;
    private Vector2 currentPatrolTarget;
    private bool hasPatrolTarget = false;
    private Vector2 moveDirection;
    private float aiStateTimer;
    private float attackCooldownTimer;
    private bool isDying = false;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        statController = GetComponent<StatController>();
        myFactionIdentity = GetComponent<FactionIdentity>(); // <-- НОВОЕ
        rb.freezeRotation = true;
        statController.OnDeath += HandleDeath;
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
        targetScanTimer = Random.Range(0, TARGET_SCAN_INTERVAL); // Рандомизируем таймер

        SetIdleAnimationBasedOnDirection();
        aiStateTimer = Random.Range(2f, 5f);
        if (npcData.behavior == NpcData.NpcBehavior.Aggressive)
        {
            attackCooldownTimer = npcData.attackCooldown;
        }
    }

    void Update()
    {
        if (isDying)
        {
            HandleDeathAnimation();
            return;
        }
        if (npcData == null) return;

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

        UpdateAnimationSet();
        AnimateSprites();
    }

    void FixedUpdate()
    {
        if (isDying || isAttacking) return;

        Vector2 safeMoveDirection = GetSafeDirection(moveDirection);
        if (rb.bodyType == RigidbodyType2D.Dynamic)
        {
            rb.velocity = safeMoveDirection * npcData.moveSpeed;
        }
        isMoving = rb.velocity.sqrMagnitude > 0.01f;
        if (isMoving)
        {
            Vector2 currentVelocity = rb.velocity.normalized;
            if (Mathf.Abs(currentVelocity.x) > Mathf.Abs(currentVelocity.y))
            {
                lastMoveDirection = new Vector2(Mathf.Sign(currentVelocity.x), 0);
            }
            else
            {
                lastMoveDirection = new Vector2(0, Mathf.Sign(currentVelocity.y));
            }
        }
    }

    // --- Логика AI ---

    private void HandlePeacefulAI()
    {
        currentState = State.Patrolling;
        UpdatePatrolMovement();
    }

    private void HandleAggressiveAI()
    {
        targetScanTimer -= Time.deltaTime;

        // Если есть цель, проверяем ее состояние
        if (currentTarget != null)
        {
            float distanceToTarget = Vector2.Distance(transform.position, currentTarget.position);
            // Если цель убежала или умерла (стала неактивной), теряем ее
            if (distanceToTarget > npcData.detectionRadius || !currentTarget.gameObject.activeInHierarchy)
            {
                currentTarget = null;
                currentState = State.Returning;
            }
        }

        // Если цели нет, ищем новую (не каждый кадр)
        if (currentTarget == null && targetScanTimer <= 0)
        {
            targetScanTimer = TARGET_SCAN_INTERVAL;
            FindHostileTarget();
            if (currentTarget != null)
            {
                currentState = State.Chasing;
            }
        }

        // Действия в зависимости от состояния
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

    private void FindHostileTarget()
    {
        if (myFactionIdentity.Faction == null || myFactionIdentity.Faction.HostileTo.Count == 0) return;

        float closestDistance = float.MaxValue;
        Transform bestTarget = null;

        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, npcData.detectionRadius);
        foreach (var hit in hits)
        {
            if (hit.transform == this.transform) continue; // Игнорируем себя

            FactionIdentity targetIdentity = hit.GetComponent<FactionIdentity>();
            if (targetIdentity != null && targetIdentity.Faction != null)
            {
                // Проверяем, враждебна ли наша фракция к фракции цели
                if (myFactionIdentity.Faction.HostileTo.Contains(targetIdentity.Faction))
                {
                    float distance = Vector2.Distance(transform.position, hit.transform.position);
                    if (distance < closestDistance)
                    {
                        closestDistance = distance;
                        bestTarget = hit.transform;
                    }
                }
            }
        }
        currentTarget = bestTarget;
    }

    private void HandleCalmAI()
    {
        moveDirection = Vector2.zero;
        if (rb.bodyType != RigidbodyType2D.Kinematic)
        {
            rb.velocity = Vector2.zero;
            rb.bodyType = RigidbodyType2D.Kinematic;
        }
    }

    private void UpdatePatrolMovement()
    {
        if (rb.bodyType != RigidbodyType2D.Dynamic) rb.bodyType = RigidbodyType2D.Dynamic;
        isAttacking = false;

        switch (npcData.movementPattern)
        {
            case NpcData.MovementPattern.PatrolArea:
                if (!hasPatrolTarget || Vector2.Distance(transform.position, currentPatrolTarget) < 0.5f)
                {
                    Vector2 randomPoint = Random.insideUnitCircle * npcData.patrolRadius;
                    currentPatrolTarget = (Vector2)startPosition + randomPoint;
                    hasPatrolTarget = true;
                }
                moveDirection = (currentPatrolTarget - (Vector2)transform.position).normalized;
                break;

            case NpcData.MovementPattern.Waypoints:
                if (waypoints.Count == 0) { moveDirection = Vector2.zero; return; }
                Transform targetWaypoint = waypoints[currentWaypointIndex];
                if (Vector2.Distance(transform.position, targetWaypoint.position) < 0.5f)
                {
                    currentWaypointIndex = (currentWaypointIndex + 1) % waypoints.Count;
                }
                moveDirection = (targetWaypoint.position - transform.position).normalized;
                break;

            case NpcData.MovementPattern.RandomWander:
                if (aiStateTimer <= 0)
                {
                    if (isMoving)
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
            if (rb.bodyType != RigidbodyType2D.Kinematic)
            {
                rb.velocity = Vector2.zero;
                rb.bodyType = RigidbodyType2D.Kinematic;
            }

            if (!isAttacking)
            {
                attackCooldownTimer -= Time.deltaTime;
                if (attackCooldownTimer <= 0)
                {
                    isAttacking = true;
                    attackAnimationTimer = 0f;
                    AttackTarget();
                    attackCooldownTimer = npcData.attackCooldown;
                }
            }
        }
        else
        {
            isAttacking = false;
            if (rb.bodyType != RigidbodyType2D.Dynamic) rb.bodyType = RigidbodyType2D.Dynamic;
            moveDirection = (currentTarget.position - transform.position).normalized;
        }
    }

    private void UpdateReturningMovement()
    {
        if (rb.bodyType != RigidbodyType2D.Dynamic) rb.bodyType = RigidbodyType2D.Dynamic;
        isAttacking = false;

        Vector2 returnDestination;
        float arrivalThreshold = 0.5f;

        if (npcData.movementPattern == NpcData.MovementPattern.Waypoints && waypoints.Count > 0)
        {
            Transform closestWaypoint = waypoints[0];
            float minDistance = Vector2.Distance(transform.position, closestWaypoint.position);
            int closestIndex = 0;

            for (int i = 1; i < waypoints.Count; i++)
            {
                float dist = Vector2.Distance(transform.position, waypoints[i].position);
                if (dist < minDistance) { minDistance = dist; closestIndex = i; }
            }
            currentWaypointIndex = closestIndex;
            returnDestination = waypoints[currentWaypointIndex].position;
        }
        else
        {
            returnDestination = startPosition;
        }

        if (Vector2.Distance(transform.position, returnDestination) < arrivalThreshold)
        {
            currentState = State.Patrolling;
            hasPatrolTarget = false;
        }
        else
        {
            moveDirection = (returnDestination - (Vector2)transform.position).normalized;
        }
    }

    // --- Вспомогательные методы ---

    private void AttackTarget()
    {
        if (currentTarget == null) return;
        StatController targetStats = currentTarget.GetComponent<StatController>();
        if (targetStats != null)
        {
            targetStats.TakeDamage(npcData.attackDamage);
        }
    }

    private Vector2 GetSafeDirection(Vector2 desiredDirection)
    {
        if (desiredDirection == Vector2.zero) return Vector2.zero;
        RaycastHit2D hit = Physics2D.Raycast(transform.position, desiredDirection, obstacleCheckDistance, obstacleLayerMask);

        if (hit.collider == null) return desiredDirection;
        else
        {
            Vector2 rightAvoidDirection = Quaternion.Euler(0, 0, -avoidanceAngle) * desiredDirection;
            if (Physics2D.Raycast(transform.position, rightAvoidDirection, obstacleCheckDistance, obstacleLayerMask).collider == null)
            {
                return rightAvoidDirection;
            }

            Vector2 leftAvoidDirection = Quaternion.Euler(0, 0, avoidanceAngle) * desiredDirection;
            if (Physics2D.Raycast(transform.position, leftAvoidDirection, obstacleCheckDistance, obstacleLayerMask).collider == null)
            {
                return leftAvoidDirection;
            }

            return Vector2.zero;
        }
    }

    // --- Анимация и Смерть ---

    void UpdateAnimationSet()
    {
        Sprite[] newFrames = null;

        if (isAttacking && npcData.behavior == NpcData.NpcBehavior.Aggressive)
        {
            if (lastMoveDirection.y > 0.5f) newFrames = npcData.attackNorth;
            else if (lastMoveDirection.y < -0.5f) newFrames = npcData.attackSouth;
            else if (lastMoveDirection.x > 0.5f) newFrames = npcData.attackEast;
            else newFrames = npcData.attackWest;
        }
        else if (isMoving)
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
        }
    }

    void AnimateSprites()
    {
        if (currentAnimationFrames == null || currentAnimationFrames.Length <= 1) return;

        float currentFrameRate = isAttacking ? npcData.attackAnimationFrameRate : npcData.animationFrameRate;

        animationTimer += Time.deltaTime;
        if (isAttacking) attackAnimationTimer += Time.deltaTime;

        if (animationTimer >= currentFrameRate)
        {
            animationTimer -= currentFrameRate;
            currentSpriteIndex++;
            if (currentSpriteIndex >= currentAnimationFrames.Length)
            {
                currentSpriteIndex = 0;
                if (isAttacking)
                {
                    isAttacking = false;
                    attackAnimationTimer = 0f;
                    UpdateAnimationSet();
                }
            }
        }
        if (currentSpriteIndex < currentAnimationFrames.Length)
        {
            spriteRenderer.sprite = currentAnimationFrames[currentSpriteIndex];
        }
    }

    private void HandleDeath()
    {
        GameEvents.ReportEnemyDied(npcData);
        isDying = true;
        moveDirection = Vector2.zero;
        rb.velocity = Vector2.zero;
        rb.bodyType = RigidbodyType2D.Kinematic;
        GetComponent<Collider2D>().enabled = false;
        currentAnimationFrames = npcData.deathAnimation;
        if (currentAnimationFrames != null && currentAnimationFrames.Length > 0)
        {
            currentSpriteIndex = 0;
            animationTimer = 0f;
            spriteRenderer.sprite = currentAnimationFrames[0];
            Destroy(gameObject, npcData.deathAnimationDuration);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    [System.Serializable] private struct PositionData { public float x, y, z; }
    public object CaptureState() { return new PositionData { x = transform.position.x, y = transform.position.y, z = transform.position.z }; }
    public void RestoreState(object state)
    {
        var positionData = ((JObject)state).ToObject<PositionData>();
        transform.position = new Vector3(positionData.x, positionData.y, positionData.z);
    }

    private void HandleDeathAnimation()
    {
        if (currentAnimationFrames == null || currentAnimationFrames.Length <= 1) return;
        animationTimer += Time.deltaTime;
        float frameDuration = npcData.deathAnimationDuration / currentAnimationFrames.Length;
        if (animationTimer >= frameDuration && currentSpriteIndex < currentAnimationFrames.Length - 1)
        {
            animationTimer -= frameDuration;
            currentSpriteIndex++;
            spriteRenderer.sprite = currentAnimationFrames[currentSpriteIndex];
        }
    }

    private void SetIdleAnimationBasedOnDirection()
    {
        if (npcData == null) return;
        Sprite[] idleSet = npcData.idleSouth;
        if (lastMoveDirection.y > 0.5f) idleSet = npcData.idleNorth;
        else if (lastMoveDirection.y < -0.5f) idleSet = npcData.idleSouth;
        else if (lastMoveDirection.x > 0.5f) idleSet = npcData.idleEast;
        else idleSet = npcData.idleWest;
        if (idleSet != null && idleSet.Length > 0)
        {
            currentAnimationFrames = idleSet;
            currentSpriteIndex = 0;
            spriteRenderer.sprite = currentAnimationFrames[0];
            animationTimer = 0f;
        }
    }

    void OnDestroy()
    {
        if (statController != null)
        {
            statController.OnDeath -= HandleDeath;
        }
    }

    void OnDrawGizmosSelected()
    {
        if (npcData != null)
        {
            // Визуализация радиуса патрулирования
            if (npcData.movementPattern == NpcData.MovementPattern.PatrolArea)
            {
                Gizmos.color = Color.blue;
                Vector3 position = Application.isPlaying ? startPosition : transform.position;
                Gizmos.DrawWireSphere(position, npcData.patrolRadius);
            }

            // Визуализация маршрута
            if (npcData.movementPattern == NpcData.MovementPattern.Waypoints)
            {
                Gizmos.color = Color.green;
                for (int i = 0; i < waypoints.Count; i++)
                {
                    if (waypoints[i] != null)
                    {
                        Gizmos.DrawWireSphere(waypoints[i].position, 0.2f); // Маркер
                        if (i < waypoints.Count - 1)
                        {
                            if (waypoints[i + 1] != null)
                            {
                                Gizmos.DrawLine(waypoints[i].position, waypoints[i + 1].position);
                            }
                        }
                        else
                        {
                            if (waypoints[0] != null) // Замыкаем маршрут
                            {
                                Gizmos.DrawLine(waypoints[i].position, waypoints[0].position);
                            }
                        }
                    }
                }
            }

            // Визуализация радиуса обнаружения и атаки
            if (npcData.behavior == NpcData.NpcBehavior.Aggressive)
            {
                Gizmos.color = Color.yellow;
                Gizmos.DrawWireSphere(transform.position, npcData.detectionRadius);
                Gizmos.color = Color.red;
                Gizmos.DrawWireSphere(transform.position, npcData.attackRadius);
            }
        }

        // Визуализация избегания препятствий
        if (Application.isPlaying && moveDirection != Vector2.zero)
        {
            Gizmos.color = Color.cyan;
            // Прямой рейкаст
            Gizmos.DrawLine(transform.position, (Vector2)transform.position + moveDirection.normalized * obstacleCheckDistance);

            // "Усы" рейкастов
            Vector2 rightAvoidDirection = Quaternion.Euler(0, 0, -avoidanceAngle) * moveDirection.normalized;
            Gizmos.DrawLine(transform.position, (Vector2)transform.position + rightAvoidDirection * obstacleCheckDistance);

            Vector2 leftAvoidDirection = Quaternion.Euler(0, 0, avoidanceAngle) * moveDirection.normalized;
            Gizmos.DrawLine(transform.position, (Vector2)transform.position + leftAvoidDirection * obstacleCheckDistance);
        }
    }
}

