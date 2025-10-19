using UnityEngine;

[CreateAssetMenu(fileName = "New NPC", menuName = "NPC/New NPC Data")]
public class NpcData : ScriptableObject
{
    public enum NpcBehavior { Peaceful, Aggressive, Calm }
    public enum MovementPattern { RandomWander, PatrolArea, Waypoints }

    [Header("General Information")]
    public string npcName = "New NPC";
    public FactionData myFaction; // <-- ДОБАВЛЕНО: Фракция этого NPC
    public NpcBehavior behavior = NpcBehavior.Peaceful;
    public float moveSpeed = 2f;

    [Header("Movement Settings")]
    [Tooltip("Шаблон передвижения для NPC, когда он не занят чем-то другим (например, погоней).")]
    public MovementPattern movementPattern = MovementPattern.RandomWander;
    [Tooltip("Радиус для патрулирования области (для шаблона PatrolArea).")]
    public float patrolRadius = 5f;

    [Header("AI Settings (for Aggressive)")]
    public float detectionRadius = 8f;
    public float attackRadius = 1.5f;
    public float attackDamage = 10f;
    public float attackCooldown = 2f;

    [Header("Animation Settings")]
    public float animationFrameRate = 0.2f;
    public float attackAnimationFrameRate = 0.1f;

    [Header("Idle Sprites")]
    public Sprite[] idleNorth;
    public Sprite[] idleEast;
    public Sprite[] idleSouth;
    public Sprite[] idleWest;

    [Header("Movement Sprites")]
    public Sprite[] moveNorth;
    public Sprite[] moveEast;
    public Sprite[] moveSouth;
    public Sprite[] moveWest;

    [Header("Attack Sprites")]
    public Sprite[] attackNorth;
    public Sprite[] attackEast;
    public Sprite[] attackSouth;
    public Sprite[] attackWest;

    [Header("Death Animation")]
    [Tooltip("Кадры анимации смерти. Будут проиграны один раз.")]
    public Sprite[] deathAnimation;
    [Tooltip("Общая длительность анимации смерти в секундах.")]
    public float deathAnimationDuration = 1f;
}

