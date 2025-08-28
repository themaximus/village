using UnityEngine;
using System.Linq;
using Newtonsoft.Json.Linq; // Необходимо для JObject

// "Подписываем контракт" ISaveable
public class MovePlayer : MonoBehaviour, ISaveable
{
    [Header("Movement Settings")]
    [SerializeField] private float walkSpeed = 3.0f;
    [SerializeField] private float runSpeed = 7.0f;
    private float currentMoveSpeed;
    private Rigidbody2D rb;
    private Vector2 moveInput;

    [Header("Animation Settings")]
    [SerializeField] private SpriteRenderer spriteRenderer;
    public Sprite[] idleSpritesNorth, idleSpritesEast, idleSpritesSouth, idleSpritesWest;
    public Sprite[] moveSpritesNorth, moveSpritesEast, moveSpritesSouth, moveSpritesWest;
    public Sprite[] runSpritesNorth, runSpritesEast, runSpritesSouth, runSpritesWest;
    private Sprite[] currentAnimationFrames;
    private int currentSpriteIndex;
    private float animationTimer;
    [SerializeField] private float animationFrameRate = 0.2f;
    private Vector2 lastMoveDirection = Vector2.down;
    private bool isMoving;
    private bool isRunning;

    [Header("Death Animation")]
    public Sprite[] deathAnimation;
    public float deathAnimationDuration = 1.5f;

    [Header("Combat & Item Settings")]
    public Inventory playerInventory;
    public LayerMask enemyLayerMask;
    public KeyCode useItemKey = KeyCode.E;
    private WeaponData equippedWeapon;
    private bool isAttacking;
    private bool isUsingItem;
    private float actionTimer;
    private SpriteRenderer weaponAnimationRenderer;
    private StatController statController;
    private bool isDying = false;
    public static MovePlayer instance;
    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        statController = GetComponent<StatController>();
        rb.freezeRotation = true;
        statController.OnDeath += HandleDeath;

        if (lastMoveDirection == Vector2.zero) lastMoveDirection = Vector2.down;
        SetIdleAnimationBasedOnDirection();

        GameObject weaponObj = new GameObject("WeaponAnimation");
        weaponObj.transform.SetParent(transform);
        weaponObj.transform.localPosition = Vector3.zero;
        weaponAnimationRenderer = weaponObj.AddComponent<SpriteRenderer>();
        weaponAnimationRenderer.sortingOrder = spriteRenderer.sortingOrder + 1;

        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }
        else
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
    }

    void Update()
    {
        if (isDying)
        {
            HandleDeathAnimation();
            return;
        }
        if (isAttacking)
        {
            if (equippedWeapon == null)
            {
                isAttacking = false;
                SetIdleAnimationBasedOnDirection();
                return;
            }
            HandleActionAnimation(equippedWeapon.attackFrameRate, equippedWeapon.attackDuration);
            return;
        }
        if (isUsingItem)
        {
            ItemData currentItem = playerInventory.currentItemInHand;
            if (currentItem == null)
            {
                isUsingItem = false;
                SetIdleAnimationBasedOnDirection();
                return;
            }
            ItemAction firstAction = currentItem?.actions.FirstOrDefault();
            if (firstAction != null)
            {
                HandleActionAnimation(firstAction.animationFrameRate, firstAction.animationDuration);
            }
            return;
        }
        UpdateEquippedWeapon();
        HandleInput();
        UpdateAnimationSet();
        AnimateSprites();
        if (Input.GetMouseButtonDown(0) && equippedWeapon != null)
        {
            StartAttack();
        }
        else if (Input.GetKeyDown(useItemKey) && playerInventory.currentItemInHand != null)
        {
            StartItemUse();
        }
    }

    void FixedUpdate()
    {
        if (isDying || isAttacking || isUsingItem)
        {
            rb.velocity = Vector2.zero;
            return;
        }
        rb.velocity = moveInput * currentMoveSpeed;
    }

    // --- РЕАЛИЗАЦИЯ ИНТЕРФЕЙСА ISaveable ---

    [System.Serializable]
    private struct PositionData
    {
        public float x, y, z;
    }

    public object CaptureState()
    {
        return new PositionData
        {
            x = transform.position.x,
            y = transform.position.y,
            z = transform.position.z
        };
    }

    public void RestoreState(object state)
    {
        // --- ГЛАВНОЕ ИЗМЕНЕНИЕ ---
        // Мы восстанавливаем позицию игрока ТОЛЬКО в том случае,
        // если идет процесс загрузки сцены из файла сохранения.
        // Во всех остальных случаях (обычный переход) позицией управляет SceneTransitionManager.
        if (SaveManager.instance != null && SaveManager.instance.IsLoadingScene)
        {
            var positionData = ((JObject)state).ToObject<PositionData>();
            transform.position = new Vector3(positionData.x, positionData.y, positionData.z);
        }
        // --- КОНЕЦ ИЗМЕНЕНИЯ ---
    }

    // --- Остальной код без изменений ---

    void StartItemUse()
    {
        ItemData currentItem = playerInventory.currentItemInHand;
        if (currentItem == null || currentItem.actions == null || currentItem.actions.Count == 0) return;
        isUsingItem = true;
        actionTimer = 0f;
        currentSpriteIndex = 0;
        animationTimer = 0f;
        ItemAction firstAction = currentItem.actions.FirstOrDefault();
        if (firstAction == null) return;
        if (lastMoveDirection.y > 0.5f) currentAnimationFrames = firstAction.animationNorth;
        else if (lastMoveDirection.y < -0.5f) currentAnimationFrames = firstAction.animationSouth;
        else if (lastMoveDirection.x > 0.5f) currentAnimationFrames = firstAction.animationEast;
        else currentAnimationFrames = firstAction.animationWest;
        if (currentAnimationFrames != null && currentAnimationFrames.Length > 0)
            spriteRenderer.sprite = currentAnimationFrames[0];
    }

    void ExecuteItemActions()
    {
        ItemData currentItem = playerInventory.currentItemInHand;
        if (currentItem == null) return;
        foreach (var action in currentItem.actions)
        {
            action.Execute(this.gameObject);
        }
        if (currentItem.isConsumable)
        {
            playerInventory.RemoveItem(playerInventory.SelectedIndex);
        }
    }

    void HandleActionAnimation(float frameRate, float duration)
    {
        actionTimer += Time.deltaTime;
        animationTimer += Time.deltaTime;
        if (currentAnimationFrames != null && currentAnimationFrames.Length > 0)
        {
            if (animationTimer >= frameRate)
            {
                animationTimer = 0f;
                currentSpriteIndex = Mathf.Min(currentSpriteIndex + 1, currentAnimationFrames.Length - 1);
                spriteRenderer.sprite = currentAnimationFrames[currentSpriteIndex];
            }
        }
        if (actionTimer >= duration)
        {
            if (isUsingItem)
            {
                ExecuteItemActions();
            }
            isAttacking = false;
            isUsingItem = false;
            SetIdleAnimationBasedOnDirection();
        }
    }

    void HandleInput()
    {
        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");
        moveInput = new Vector2(h, v).normalized;
        isMoving = moveInput.sqrMagnitude > 0.01f;
        if (isMoving)
        {
            if (Mathf.Abs(h) > Mathf.Abs(v)) lastMoveDirection = new Vector2(Mathf.Sign(h), 0);
            else lastMoveDirection = new Vector2(0, Mathf.Sign(v));
        }
        isRunning = Input.GetKey(KeyCode.Space) && isMoving;
        currentMoveSpeed = isRunning ? runSpeed : walkSpeed;
    }

    public void UpdateEquippedWeapon()
    {
        equippedWeapon = playerInventory?.currentItemInHand as WeaponData;
    }

    void StartAttack()
    {
        if (equippedWeapon == null) return;
        isAttacking = true;
        actionTimer = 0f;
        currentSpriteIndex = 0;
        animationTimer = 0f;
        if (lastMoveDirection.y > 0.5f) currentAnimationFrames = equippedWeapon.attackNorth;
        else if (lastMoveDirection.y < -0.5f) currentAnimationFrames = equippedWeapon.attackSouth;
        else if (lastMoveDirection.x > 0.5f) currentAnimationFrames = equippedWeapon.attackEast;
        else currentAnimationFrames = equippedWeapon.attackWest;
        if (currentAnimationFrames != null && currentAnimationFrames.Length > 0)
            spriteRenderer.sprite = currentAnimationFrames[0];
        Collider2D[] potentialHits = Physics2D.OverlapCircleAll(transform.position, equippedWeapon.attackRadius, enemyLayerMask);
        foreach (Collider2D enemy in potentialHits)
        {
            Vector2 directionToEnemy = (enemy.transform.position - transform.position).normalized;
            if (Vector2.Angle(lastMoveDirection, directionToEnemy) < equippedWeapon.attackAngle / 2)
            {
                StatController enemyStats = enemy.GetComponent<StatController>();
                if (enemyStats != null) enemyStats.TakeDamage(equippedWeapon.attackDamage);
            }
        }
    }

    void UpdateAnimationSet()
    {
        Sprite[] newFrames = null;
        var directionSet = isRunning ? (runSpritesNorth, runSpritesSouth, runSpritesEast, runSpritesWest)
                                     : (moveSpritesNorth, moveSpritesSouth, moveSpritesEast, moveSpritesWest);
        if (isMoving)
        {
            if (lastMoveDirection.y > 0.5f) newFrames = directionSet.Item1;
            else if (lastMoveDirection.y < -0.5f) newFrames = directionSet.Item2;
            else if (lastMoveDirection.x > 0.5f) newFrames = directionSet.Item3;
            else newFrames = directionSet.Item4;
        }
        else
        {
            if (lastMoveDirection.y > 0.5f) newFrames = idleSpritesNorth;
            else if (lastMoveDirection.y < -0.5f) newFrames = idleSpritesSouth;
            else if (lastMoveDirection.x > 0.5f) newFrames = idleSpritesEast;
            else newFrames = idleSpritesWest;
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
        if (animationTimer >= animationFrameRate)
        {
            animationTimer = 0f;
            currentSpriteIndex = (currentSpriteIndex + 1) % currentAnimationFrames.Length;
            spriteRenderer.sprite = currentAnimationFrames[currentSpriteIndex];
        }
    }

    private void SetIdleAnimationBasedOnDirection()
    {
        Sprite[] idleSet = null;
        if (lastMoveDirection.y > 0.5f) idleSet = idleSpritesNorth;
        else if (lastMoveDirection.y < -0.5f) idleSet = idleSpritesSouth;
        else if (lastMoveDirection.x > 0.5f) idleSet = idleSpritesEast;
        else idleSet = idleSpritesWest;
        if (idleSet != null && idleSet.Length > 0)
        {
            currentAnimationFrames = idleSet;
            currentSpriteIndex = 0;
            spriteRenderer.sprite = currentAnimationFrames[0];
            animationTimer = 0f;
        }
    }

    private void HandleDeath()
    {
        isDying = true;
        moveInput = Vector2.zero;
        rb.velocity = Vector2.zero;
        GetComponent<Collider2D>().enabled = false;
        this.enabled = false;
        currentAnimationFrames = deathAnimation;
        if (currentAnimationFrames != null && currentAnimationFrames.Length > 0)
        {
            currentSpriteIndex = 0;
            animationTimer = 0f;
            spriteRenderer.sprite = currentAnimationFrames[0];
            Debug.Log("Player has died. Game Over!");
        }
    }

    private void HandleDeathAnimation()
    {
        if (currentAnimationFrames == null || currentAnimationFrames.Length <= 1) return;
        animationTimer += Time.deltaTime;
        float frameDuration = deathAnimationDuration / currentAnimationFrames.Length;
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

    void OnDestroy()
    {
        if (statController != null) statController.OnDeath -= HandleDeath;
    }

    void OnDrawGizmosSelected()
    {
        if (equippedWeapon != null)
        {
            Gizmos.color = Color.red;
            Vector3 forward = lastMoveDirection;
            Vector3 leftBoundary = Quaternion.Euler(0, 0, equippedWeapon.attackAngle / 2) * forward;
            Vector3 rightBoundary = Quaternion.Euler(0, 0, -equippedWeapon.attackAngle / 2) * forward;
            Gizmos.DrawLine(transform.position, (Vector2)transform.position + (Vector2)leftBoundary * equippedWeapon.attackRadius);
            Gizmos.DrawLine(transform.position, (Vector2)transform.position + (Vector2)rightBoundary * equippedWeapon.attackRadius);
        }
    }
}