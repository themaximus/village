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
    public float attackDamage = 10f;        // ���� ������
    public float attackRadius = 1.5f;       // ��������� ����� (������ ��� �� ������ �����, � ����� ����)
    public float attackAngle = 90f;         // ���� ���� ����� � �������� (��������, 90 ��� �������� ������)
    public float attackFrameRate = 0.1f;    // �������� �������� �����
    public float attackDuration = 0.5f;     // ������������ �����
}
