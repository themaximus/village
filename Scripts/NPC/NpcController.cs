using UnityEngine;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;

// ������� ������� ���� ����������� ����������� ��� ���������� ������.
// Unity ������������� ������� �� ��� ���������� ����� ������� �� ������.
[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(StatController))]
[RequireComponent(typeof(FactionIdentity))]
[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(Collider))] // ���������, ��� ���� 3D ���������
public class NpcController : MonoBehaviour, ISaveable
{
    [Header("NPC Data")]
    public NpcData npcData; // ScriptableObject � ������� NPC (��������, ���� � �.�.)

    [Header("Pathfinding")]
    public List<Transform> waypoints = new List<Transform>();

    [Header("Obstacle Avoidance")]
    public LayerMask obstacleLayerMask;
    public float obstacleCheckDistance = 1f;
    public float avoidanceAngle = 30f;

    // --- ���������� ---
    private Rigidbody rb;
    private StatController statController;
    private FactionIdentity myFactionIdentity;
    private Animator animator;
    private Collider npcCollider;

    // --- ���������� ��� AI ---
    private enum State { Patrolling, Chasing, Returning }
    private State currentState;
    private Transform currentTarget; // ������� ���� (����)
    private float targetScanTimer;   // ������ ��� ������������ ������, ����� �� ������ ��� ������ ����
    private const float TARGET_SCAN_INTERVAL = 1.0f; // ��������� ���� ��� � �������

    private Vector3 startPosition; // �������� ������� ��� �����������
    private int currentWaypointIndex = 0;
    private Vector3 currentPatrolTarget;
    private bool hasPatrolTarget = false;
    private Vector3 moveDirection;
    private float aiStateTimer; // ������ ��� ����� ��������� (��������, � ��������� ���������)
    private float attackCooldownTimer;
    private bool isDying = false;

    // --- Unity Callbacks ---

    void Awake()
    {
        // �������� ������ �� ��� ����������� ����������
        rb = GetComponent<Rigidbody>();
        statController = GetComponent<StatController>();
        myFactionIdentity = GetComponent<FactionIdentity>();
        animator = GetComponent<Animator>();
        npcCollider = GetComponent<Collider>();

        // ��������, ��� NPC �� ����� ��������� ��-�� ���������� ������������
        rb.freezeRotation = true;

        // ������������� �� ������� ������. ����� StatController ������� � ������, ���������� ����� HandleDeath.
        if (statController != null)
        {
            statController.OnDeath += HandleDeath;
        }
    }

    void Start()
    {
        if (npcData == null || myFactionIdentity.Faction == null)
        {
            Debug.LogErrorFormat("NpcData ��� Faction �� ��������� ��� {0}!", this.gameObject.name);
            this.enabled = false;
            return;
        }

        startPosition = transform.position;
        currentState = State.Patrolling;
        targetScanTimer = Random.Range(0, TARGET_SCAN_INTERVAL); // ������������� ������ ��� ������������������

        aiStateTimer = Random.Range(2f, 5f);
        if (npcData.behavior == NpcData.NpcBehavior.Aggressive)
        {
            attackCooldownTimer = npcData.attackCooldown;
        }
    }

    void Update()
    {
        // ���� NPC � �������� ������, ���������� ���������� ����� ������
        if (isDying) return;
        if (npcData == null) return;

        aiStateTimer -= Time.deltaTime;

        // �������� ��������� � ����������� �� ���� NPC
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

        // ��������� ��������� ��������� �� ������ �������� ���������
        UpdateAnimator();
    }

    void FixedUpdate()
    {
        // ��� ������ (��������) ��������� � FixedUpdate
        if (isDying) return;

        Vector3 safeMoveDirection = GetSafeDirection(moveDirection);
        rb.velocity = safeMoveDirection * npcData.moveSpeed;
    }

    void OnDestroy()
    {
        // ����� ���������� �� �������, ����� �������� ������ ������
        if (statController != null)
        {
            statController.OnDeath -= HandleDeath;
        }
    }

    // --- ������ AI ---

    private void HandlePeacefulAI()
    {
        currentState = State.Patrolling;
        UpdatePatrolMovement();
    }

    private void HandleAggressiveAI()
    {
        targetScanTimer -= Time.deltaTime;

        // ��������� ��������� ������� ����
        if (currentTarget != null)
        {
            float distanceToTarget = Vector3.Distance(transform.position, currentTarget.position);
            // ���� ���� ������� ������ ��� ��������� (������), ������ ��
            if (distanceToTarget > npcData.detectionRadius || !currentTarget.gameObject.activeInHierarchy)
            {
                currentTarget = null;
                currentState = State.Returning;
            }
        }

        // ���� ���� ���, ���� ����� (������������)
        if (currentTarget == null && targetScanTimer <= 0)
        {
            targetScanTimer = TARGET_SCAN_INTERVAL;
            FindHostileTarget();
            if (currentTarget != null)
            {
                currentState = State.Chasing;
            }
        }

        // ��������� � ����������� �� ���������
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

    // --- ������ ��������� AI ---

    private void UpdatePatrolMovement()
    {
        switch (npcData.movementPattern)
        {
            case NpcData.MovementPattern.PatrolArea:
                if (!hasPatrolTarget || Vector3.Distance(transform.position, currentPatrolTarget) < 1.0f)
                {
                    Vector2 randomPoint = Random.insideUnitCircle * npcData.patrolRadius;
                    currentPatrolTarget = startPosition + new Vector3(randomPoint.x, 0, randomPoint.y); // ��� 3D ���������� X � Z
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
                    if (rb.velocity.sqrMagnitude > 0.01f) // ���� ��������
                    {
                        moveDirection = Vector3.zero; // ���������������
                        aiStateTimer = Random.Range(3f, 6f);
                    }
                    else // ���� �����
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
            // ��������� � ������� �����
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
            // �������� � ����
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

    // --- ��������������� ������ ---

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

        // �������������� ����� � ���� ����� ������
        transform.LookAt(new Vector3(currentTarget.position.x, transform.position.y, currentTarget.position.z));

        // ��������� ������� ����� � ���������
        animator.SetTrigger("Attack");

        // ���� ����� �������� ���� ����� ��������, ���� ����� Animation Event � �������� �����
        StatController targetStats = currentTarget.GetComponent<StatController>();
        if (targetStats != null)
        {
            targetStats.TakeDamage(npcData.attackDamage);
        }
    }

    // --- ������ ������ ---

    private void HandleDeath()
    {
        if (isDying) return; // ������������� ������� �����

        isDying = true;

        // ��������� ����������, ����� ������� NPC �� �����
        if (npcCollider != null) npcCollider.enabled = false;
        if (rb != null)
        {
            rb.velocity = Vector3.zero;
            rb.isKinematic = true; // ������ ��� ��������������� � ������
        }

        // �������� ������� �������� � ������ �����
        GameEvents.ReportEnemyDied(npcData);

        // ���������� ������� "Death" � ���������.
        // �������� ��� ���������� �������� � ������ DestroyOnExit ��������� ������.
        animator.SetTrigger("Death");

        // ��������� ��� ������, ����� Update() �������� ��������
        this.enabled = false;
    }

    // --- �������� ---

    private void UpdateAnimator()
    {
        // ���������, �������� �� NPC
        bool isMoving = rb.velocity.sqrMagnitude > 0.1f;
        animator.SetBool("IsMoving", isMoving);

        // ���� � ��� ���� �������� ����/������ � ������ �������, ����� ���������� �����������
        // Vector3 localMove = transform.InverseTransformDirection(rb.velocity);
        // animator.SetFloat("MoveX", localMove.x);
        // animator.SetFloat("MoveZ", localMove.z);
    }

    // --- ��������� ����������� � ���������� ---

    private Vector3 GetSafeDirection(Vector3 desiredDirection)
    {
        if (desiredDirection == Vector3.zero) return Vector3.zero;
        RaycastHit hit;

        if (Physics.Raycast(transform.position, desiredDirection, out hit, obstacleCheckDistance, obstacleLayerMask))
        {
            // ������� ���������: �������� ���������
            Vector3 rightAvoidDirection = Quaternion.Euler(0, avoidanceAngle, 0) * desiredDirection;
            Vector3 leftAvoidDirection = Quaternion.Euler(0, -avoidanceAngle, 0) * desiredDirection;

            if (!Physics.Raycast(transform.position, rightAvoidDirection, obstacleCheckDistance, obstacleLayerMask))
                return rightAvoidDirection;
            if (!Physics.Raycast(transform.position, leftAvoidDirection, obstacleCheckDistance, obstacleLayerMask))
                return leftAvoidDirection;

            return Vector3.zero; // ��� ���� �������������
        }

        return desiredDirection; // ���� ��������
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

    // --- ������������ � ��������� ---

    void OnDrawGizmosSelected()
    {
        if (npcData != null)
        {
            // ������ �����������
            if (npcData.behavior == NpcData.NpcBehavior.Aggressive)
            {
                Gizmos.color = Color.yellow;
                Gizmos.DrawWireSphere(transform.position, npcData.detectionRadius);
                Gizmos.color = Color.red;
                Gizmos.DrawWireSphere(transform.position, npcData.attackRadius);
            }

            // ������ ��������������
            if (npcData.movementPattern == NpcData.MovementPattern.PatrolArea)
            {
                Gizmos.color = Color.blue;
                Vector3 pos = Application.isPlaying ? startPosition : transform.position;
                Gizmos.DrawWireSphere(pos, npcData.patrolRadius);
            }
        }
    }
}