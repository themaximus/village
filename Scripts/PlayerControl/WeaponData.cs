using UnityEngine;

[CreateAssetMenu(fileName = "NewWeapon", menuName = "Inventory/Weapon")]
public class WeaponData : ItemData
{
    [Header("Idle (Combat stance)")]
    public Sprite[] idleNorth;
    public Sprite[] idleEast;
    public Sprite[] idleSouth;
    public Sprite[] idleWest;

    [Header("Attack Animation")]
    public Sprite[] attackNorth;
    public Sprite[] attackEast;
    public Sprite[] attackSouth;
    public Sprite[] attackWest;

    [Header("Attack Settings")]
    public float attackDamage = 10f;        // ”рон оружи€
    public float attackRadius = 1.5f;       // ƒјЋ№Ќќ—“№ атаки (теперь это не радиус круга, а длина дуги)
    public float attackAngle = 90f;         // ”гол дуги атаки в градусах (например, 90 дл€ широкого взмаха)
    public float attackFrameRate = 0.1f;    // —корость анимации удара
    public float attackDuration = 0.5f;     // ƒлительность атаки
}
