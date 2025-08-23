using UnityEngine;
using Newtonsoft.Json.Linq; // Необходимо для JObject

// "Подписываем контракт" ISaveable
[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(SpriteRenderer))]
[RequireComponent(typeof(StatController))]
public class NpcController : MonoBehaviour, ISaveable
{
    [Header("NPC Data")]
    public NpcData npcData;

    [Header("Obstacle Avoidance")]
    public LayerMask obstacleLayerMask;
    public float obstacleCheckDistance = 1f;
    public float avoidanceAngle = 30f;

    // Компоненты
    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;
    private StatController statController;
    private Transform playerTransform;

    // Переменные для анимации и AI
    private Sprite[] currentAnimationFrames;
    private int currentSpriteIndex;
    private float animationTimer;
    private Vector2 lastMoveDirection = Vector2.down;
    private bool isMoving;

    // Переменные для AI
    private Vector2 moveDirection;
    private float aiStateTimer;
    private float attackCooldownTimer;
    private bool isDying = false;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        statController = GetComponent<StatController>();
        rb.freezeRotation = true;

        statController.OnDeath += HandleDeath;
    }

    void Start()
    {
        if (npcData == null)
        {
            Debug.LogError("NpcData is not assigned!", this);
            this.enabled = false;
            return;
        }

        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            playerTransform = playerObj.transform;
        }

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

        if (npcData.behavior == NpcData.NpcBehavior.Peaceful)
        {
            if (rb.bodyType != RigidbodyType2D.Dynamic) rb.bodyType = RigidbodyType2D.Dynamic;
            HandlePeacefulAI();
        }
        else if (npcData.behavior == NpcData.NpcBehavior.Aggressive)
        {
            HandleAggressiveAI();
        }

        UpdateAnimationSet();
        AnimateSprites();
    }

    void FixedUpdate()
    {
        if (isDying) return;

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

    // --- РЕАЛИЗАЦИЯ ИНТЕРФЕЙСА ISaveable ---

    // Вспомогательная структура для хранения позиции
    [System.Serializable]
    private struct PositionData
    {
        public float x, y, z;
    }

    /// <summary>
    /// "Фотографирует" текущую позицию NPC.
    /// </summary>
    public object CaptureState()
    {
        return new PositionData
        {
            x = transform.position.x,
            y = transform.position.y,
            z = transform.position.z
        };
    }

    /// <summary>
    /// Восстанавливает позицию NPC из "фотографии".
    /// </summary>
    public void RestoreState(object state)
    {
        var positionData = ((JObject)state).ToObject<PositionData>();
        transform.position = new Vector3(positionData.x, positionData.y, positionData.z);
    }

    // --- Остальной код без изменений ---

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

    private void HandleDeathAnimation()
    {
        if (currentAnimationFrames == null || currentAnimationFrames.Length <= 1) return;
        animationTimer += Time.deltaTime;
        float frameDuration = npcData.deathAnimationDuration / currentAnimationFrames.Length;
        if (animationTimer >= frameDuration)
        {
            animationTimer -= frameDuration;
            if (currentSpriteIndex < currentAnimationFrames.Length - 1)
            {
                currentSpriteIndex++;
                spriteRenderer.sprite = currentAnimationFrames[currentSpriteIndex];
            }
        }
    }

    private Vector2 GetSafeDirection(Vector2 direction)
    {
        if (direction == Vector2.zero) return Vector2.zero;
        RaycastHit2D hit = Physics2D.Raycast(transform.position, direction, obstacleCheckDistance, obstacleLayerMask);
        if (hit.collider != null)
        {
            Vector2 rightWhisker = Quaternion.Euler(0, 0, -avoidanceAngle) * direction;
            if (Physics2D.Raycast(transform.position, rightWhisker, obstacleCheckDistance, obstacleLayerMask).collider == null) return rightWhisker;
            Vector2 leftWhisker = Quaternion.Euler(0, 0, avoidanceAngle) * direction;
            if (Physics2D.Raycast(transform.position, leftWhisker, obstacleCheckDistance, obstacleLayerMask).collider == null) return leftWhisker;
            return Vector2.zero;
        }
        return direction;
    }

    private void HandlePeacefulAI()
    {
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
    }

    private void HandleAggressiveAI()
    {
        if (playerTransform == null)
        {
            moveDirection = Vector2.zero;
            if (rb.bodyType != RigidbodyType2D.Dynamic) rb.bodyType = RigidbodyType2D.Dynamic;
            return;
        }
        float distanceToPlayer = Vector2.Distance(transform.position, playerTransform.position);
        if (distanceToPlayer <= npcData.detectionRadius)
        {
            if (distanceToPlayer <= npcData.attackRadius)
            {
                moveDirection = Vector2.zero;
                if (rb.bodyType != RigidbodyType2D.Kinematic)
                {
                    rb.velocity = Vector2.zero;
                    rb.bodyType = RigidbodyType2D.Kinematic;
                }
                attackCooldownTimer -= Time.deltaTime;
                if (attackCooldownTimer <= 0)
                {
                    AttackPlayer();
                    attackCooldownTimer = npcData.attackCooldown;
                }
            }
            else
            {
                if (rb.bodyType != RigidbodyType2D.Dynamic) rb.bodyType = RigidbodyType2D.Dynamic;
                moveDirection = (playerTransform.position - transform.position).normalized;
                if (attackCooldownTimer < npcData.attackCooldown) attackCooldownTimer = npcData.attackCooldown;
            }
        }
        else
        {
            if (rb.bodyType != RigidbodyType2D.Dynamic) rb.bodyType = RigidbodyType2D.Dynamic;
            moveDirection = Vector2.zero;
        }
    }

    private void AttackPlayer()
    {
        if (playerTransform == null) return;
        StatController playerStats = playerTransform.GetComponent<StatController>();
        if (playerStats != null)
        {
            playerStats.TakeDamage(npcData.attackDamage);
        }
    }

    void UpdateAnimationSet()
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

    void AnimateSprites()
    {
        if (currentAnimationFrames == null || currentAnimationFrames.Length <= 1) return;
        animationTimer += Time.deltaTime;
        if (animationTimer >= npcData.animationFrameRate)
        {
            animationTimer -= npcData.animationFrameRate;
            currentSpriteIndex = (currentSpriteIndex + 1) % currentAnimationFrames.Length;
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
            spriteRenderer.sprite = currentAnimationFrames[currentSpriteIndex];
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
        if (moveDirection != Vector2.zero)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawLine(transform.position, (Vector2)transform.position + moveDirection.normalized * obstacleCheckDistance);
            Gizmos.color = Color.blue;
            Vector2 rightWhisker = Quaternion.Euler(0, 0, -avoidanceAngle) * moveDirection.normalized;
            Gizmos.DrawLine(transform.position, (Vector2)transform.position + rightWhisker * obstacleCheckDistance);
            Vector2 leftWhisker = Quaternion.Euler(0, 0, avoidanceAngle) * moveDirection.normalized;
            Gizmos.DrawLine(transform.position, (Vector2)transform.position + leftWhisker * obstacleCheckDistance);
        }
    }
}
