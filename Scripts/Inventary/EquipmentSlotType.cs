using UnityEngine;

// Перечисление для всех возможных слотов снаряжения.
// Мы выносим его наружу, чтобы другие скрипты тоже могли его использовать.
public enum EquipmentSlotType { Head, Chest, Legs, Feet, Accessory }

[CreateAssetMenu(fileName = "New Equipment", menuName = "Inventory/Equipment")]
public class EquipmentData : ItemData
{
    [Header("Equipment Settings")]
    public EquipmentSlotType slotType; // Тип слота, в который можно надеть этот предмет

    [Header("Stat Bonuses")]
    public int healthBonus;
    public int armorBonus;
    // Сюда можно будет добавить любые другие бонусы (к силе, ловкости и т.д.)

    [Header("Special Bonuses")]
    [Tooltip("Сколько дополнительных слотов инвентаря дает этот предмет (например, для рюкзака).")]
    public int bonusInventorySlots;
}
