using UnityEngine;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;

// Требуем наличия всех необходимых компонентов для корректной работы.
// Unity автоматически добавит их при добавлении этого скрипта на объект.
[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(StatController))]
[RequireComponent(typeof(FactionIdentity))]
[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(Collider))] // Убедитесь, что есть 3D коллайдер
public class NpcController : MonoBehaviour, ISaveable
{
    [Header("NPC Data")]
    public NpcData npcData; // ScriptableObject с данными NPC (скорость, урон и т.д.)

    [Header("Pathfinding")]
    public List<Transform> waypoints = new List<Transform>();

    [Header("Obstacle Avoidance")]
    public LayerMask obstacleLayerMask;
    public float obstacleCheckDistance = 1f;
    public float avoidanceAngle = 30f;

    // --- Компоненты ---
    private Rigidbody rb;
    private StatController statController;
    private FactionIdentity myFactionIdentity;
    private Animator animator;
    private Collider npcCollider;

    // --- Переменные для AI ---
    private enum State { Patrolling, Chasing, Returning }
    private State currentState;
    private Transform currentTarget; // Текущая цель (враг)
    private float targetScanTimer;   // Таймер для сканирования врагов, чтобы не делать это каждый кадр
    private const float TARGET_SCAN_INTERVAL = 1.0f; // Сканируем цели раз в секунду

    private Vector3 startPosition; // Исходная позиция для возвращения
    private int currentWaypointIndex = 0;
    private Vector3 currentPatrolTarget;
    private bool hasPatrolTarget = false;
    private Vector3 moveDirection;
    private float aiStateTimer; // Таймер для смены состояний (например, в случайном блуждании)
    private float attackCooldownTimer;
    private bool isDying = false;

    // --- Unity Callbacks ---

    void Awake()
    {
        // Получаем ссылки на все необходимые компоненты
        rb = GetComponent<Rigidbody>();
        statController = GetComponent<StatController>();
        myFactionIdentity = GetComponent<FactionIdentity>();
        animator = GetComponent<Animator>();
        npcCollider = GetComponent<Collider>();

        // Убедимся, что NPC не будет вращаться из-за физических столкновений
        rb.freezeRotation = true;

        // Подписываемся на событие смерти. Когда StatController объявит о смерти, выполнится метод HandleDeath.
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
        targetScanTimer = Random.Range(0, TARGET_SCAN_INTERVAL); // Рандомизируем таймер для производительности

        aiStateTimer = Random.Range(2f, 5f);
        if (npcData.behavior == NpcData.NpcBehavior.Aggressive)
        {
            attackCooldownTimer = npcData.attackCooldown;
        }
    }

    void Update()
    {
        // Если NPC в процессе смерти, прекращаем выполнение любой логики
        if (isDying) return;
        if (npcData == null) return;

        aiStateTimer -= Time.deltaTime;

        // Выбираем поведение в зависимости от типа NPC
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

        // Обновляем параметры аниматора на основе текущего состояния
        UpdateAnimator();
    }

    void FixedUpdate()
    {
        // Всю физику (движение) выполняем в FixedUpdate
        if (isDying) return;

        Vector3 safeMoveDirection = GetSafeDirection(moveDirection);
        rb.velocity = safeMoveDirection * npcData.moveSpeed;
    }

    void OnDestroy()
    {
        // Важно отписаться от события, чтобы избежать утечек памяти
        if (statController != null)
        {
            statController.OnDeath -= HandleDeath;
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

        // Проверяем состояние текущей цели
        if (currentTarget != null)
        {
            float distanceToTarget = Vector3.Distance(transform.position, currentTarget.position);
            // Если цель слишком далеко или неактивна (умерла), теряем ее
            if (distanceToTarget > npcData.detectionRadius || !currentTarget.gameObject.activeInHierarchy)
            {
                currentTarget = null;
                currentState = State.Returning;
            }
        }

        // Если цели нет, ищем новую (периодически)
        if (currentTarget == null && targetScanTimer <= 0)
        {
            targetScanTimer = TARGET_SCAN_INTERVAL;
            FindHostileTarget();
            if (currentTarget != null)
            {
                currentState = State.Chasing;
            }
        }

        // Действуем в зависимости от состояния
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
        moveDirection = Vector3.zero;
        rb.velocity = Vector3.zero;
    }

    // --- Логика состояний AI ---

    private void UpdatePatrolMovement()
    {
        switch (npcData.movementPattern)
        {
            case NpcData.MovementPattern.PatrolArea:
                if (!hasPatrolTarget || Vector3.Distance(transform.position, currentPatrolTarget) < 1.0f)
                {
                    Vector2 randomPoint = Random.insideUnitCircle * npcData.patrolRadius;
                    currentPatrolTarget = startPosition + new Vector3(randomPoint.x, 0, randomPoint.y); // Для 3D используем X и Z
                    hasPatrolTarget = true;
                }
                moveDirection = (currentPatrolTarget - transform.position).normalized;
                break;

            case NpcData.MovementPattern.Waypoints:
                if (waypoints.Count == 0) { moveDirection = Vector3.zero; return; }
                Transform targetWaypoint = waypoints[currentWaypointIndex];
                if (Vector3.Distance(transform.position, targetWaypoint.position) < 1.0f)
                {
                    currentWaypointIndex = (currentWaypointIndex + 1) % waypoints.Count;
                }
                moveDirection = (targetWaypoint.position - transform.position).normalized;
                break;

            case NpcData.MovementPattern.RandomWander:
                if (aiStateTimer <= 0)
                {
                    if (rb.velocity.sqrMagnitude > 0.01f) // Если двигался
                    {
                        moveDirection = Vector3.zero; // Останавливаемся
                        aiStateTimer = Random.Range(3f, 6f);
                    }
                    else // Если стоял
                    {
                        float randomAngle = Random.Range(0, 360);
                        moveDirection = new Vector3(Mathf.Cos(randomAngle * Mathf.Deg2Rad), 0, Mathf.Sin(randomAngle * Mathf.Deg2Rad)).normalized;
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

        float distanceToTarget = Vector3.Distance(transform.position, currentTarget.position);

        if (distanceToTarget <= npcData.attackRadius)
        {
            // Находимся в радиусе атаки
            moveDirection = Vector3.zero;
            attackCooldownTimer -= Time.deltaTime;
            if (attackCooldownTimer <= 0)
            {
                AttackTarget();
                attackCooldownTimer = npcData.attackCooldown;
            }
        }
        else
        {
            // Движемся к цели
            moveDirection = (currentTarget.position - transform.position).normalized;
        }
    }

    private void UpdateReturningMovement()
    {
        float arrivalThreshold = 1.0f;
        if (Vector3.Distance(transform.position, startPosition) < arrivalThreshold)
        {
            currentState = State.Patrolling;
            hasPatrolTarget = false;
        }
        else
        {
            moveDirection = (startPosition - transform.position).normalized;
        }
    }

    // --- Вспомогательные методы ---

    private void FindHostileTarget()
    {
        if (myFactionIdentity.Faction == null || myFactionIdentity.Faction.HostileTo.Count == 0) return;

        float closestDistance = float.MaxValue;
        Transform bestTarget = null;

        Collider[] hits = Physics.OverlapSphere(transform.position, npcData.detectionRadius);
        foreach (var hit in hits)
        {
            if (hit.transform == this.transform) continue;

            FactionIdentity targetIdentity = hit.GetComponent<FactionIdentity>();
            if (targetIdentity != null && targetIdentity.Faction != null)
            {
                if (myFactionIdentity.Faction.HostileTo.Contains(targetIdentity.Faction))
                {
                    float distance = Vector3.Distance(transform.position, hit.transform.position);
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

    private void AttackTarget()
    {
        if (currentTarget == null) return;

        // Поворачиваемся лицом к цели перед атакой
        transform.LookAt(new Vector3(currentTarget.position.x, transform.position.y, currentTarget.position.z));

        // Запускаем триггер атаки в аниматоре
        animator.SetTrigger("Attack");

        // Урон можно наносить либо здесь напрямую, либо через Animation Event в анимации атаки
        StatController targetStats = currentTarget.GetComponent<StatController>();
        if (targetStats != null)
        {
            targetStats.TakeDamage(npcData.attackDamage);
        }
    }

    // --- Логика смерти ---

    private void HandleDeath()
    {
        if (isDying) return; // Предотвращаем двойной вызов

        isDying = true;

        // Отключаем компоненты, чтобы мертвый NPC не мешал
        if (npcCollider != null) npcCollider.enabled = false;
        if (rb != null)
        {
            rb.velocity = Vector3.zero;
            rb.isKinematic = true; // Делаем его невосприимчивым к физике
        }

        // Сообщаем игровым системам о смерти врага
        GameEvents.ReportEnemyDied(npcData);

        // Активируем триггер "Death" в Аниматоре.
        // Аниматор сам переключит анимацию и скрипт DestroyOnExit уничтожит объект.
        animator.SetTrigger("Death");

        // Выключаем сам скрипт, чтобы Update() перестал работать
        this.enabled = false;
    }

    // --- Анимация ---

    private void UpdateAnimator()
    {
        // Вычисляем, движется ли NPC
        bool isMoving = rb.velocity.sqrMagnitude > 0.1f;
        animator.SetBool("IsMoving", isMoving);

        // Если у вас есть анимации бега/ходьбы в разные стороны, можно передавать направление
        // Vector3 localMove = transform.InverseTransformDirection(rb.velocity);
        // animator.SetFloat("MoveX", localMove.x);
        // animator.SetFloat("MoveZ", localMove.z);
    }

    // --- Избегание препятствий и Сохранение ---

    private Vector3 GetSafeDirection(Vector3 desiredDirection)
    {
        if (desiredDirection == Vector3.zero) return Vector3.zero;
        RaycastHit hit;

        if (Physics.Raycast(transform.position, desiredDirection, out hit, obstacleCheckDistance, obstacleLayerMask))
        {
            // Простое избегание: пытаемся повернуть
            Vector3 rightAvoidDirection = Quaternion.Euler(0, avoidanceAngle, 0) * desiredDirection;
            Vector3 leftAvoidDirection = Quaternion.Euler(0, -avoidanceAngle, 0) * desiredDirection;

            if (!Physics.Raycast(transform.position, rightAvoidDirection, obstacleCheckDistance, obstacleLayerMask))
                return rightAvoidDirection;
            if (!Physics.Raycast(transform.position, leftAvoidDirection, obstacleCheckDistance, obstacleLayerMask))
                return leftAvoidDirection;

            return Vector3.zero; // Оба пути заблокированы
        }

        return desiredDirection; // Путь свободен
    }

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

    // --- Визуализация в редакторе ---

    void OnDrawGizmosSelected()
    {
        if (npcData != null)
        {
            // Радиус обнаружения
            if (npcData.behavior == NpcData.NpcBehavior.Aggressive)
            {
                Gizmos.color = Color.yellow;
                Gizmos.DrawWireSphere(transform.position, npcData.detectionRadius);
                Gizmos.color = Color.red;
                Gizmos.DrawWireSphere(transform.position, npcData.attackRadius);
            }

            // Радиус патрулирования
            if (npcData.movementPattern == NpcData.MovementPattern.PatrolArea)
            {
                Gizmos.color = Color.blue;
                Vector3 pos = Application.isPlaying ? startPosition : transform.position;
                Gizmos.DrawWireSphere(pos, npcData.patrolRadius);
            }
        }
    }
}