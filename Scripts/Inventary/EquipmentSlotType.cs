using UnityEngine;

// ������������ ��� ���� ��������� ������ ����������.
// �� ������� ��� ������, ����� ������ ������� ���� ����� ��� ������������.
public enum EquipmentSlotType { Head, Chest, Legs, Feet, Accessory }

[CreateAssetMenu(fileName = "New Equipment", menuName = "Inventory/Equipment")]
public class EquipmentData : ItemData
{
    [Header("Equipment Settings")]
    public EquipmentSlotType slotType; // ��� �����, � ������� ����� ������ ���� �������

    [Header("Stat Bonuses")]
    public int healthBonus;
    public int armorBonus;
    // ���� ����� ����� �������� ����� ������ ������ (� ����, �������� � �.�.)

    [Header("Special Bonuses")]
    [Tooltip("������� �������������� ������ ��������� ���� ���� ������� (��������, ��� �������).")]
    public int bonusInventorySlots;
}
