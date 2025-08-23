using UnityEngine;
using System;
using System.Collections.Generic;

[RequireComponent(typeof(Inventory))]
public class EquipmentController : MonoBehaviour
{
    private Dictionary<EquipmentSlotType, EquipmentData> equippedItems = new Dictionary<EquipmentSlotType, EquipmentData>();
    public event Action OnEquipmentChanged;
    private Inventory inventory;

    void Awake()
    {
        inventory = GetComponent<Inventory>();
        foreach (EquipmentSlotType slotType in Enum.GetValues(typeof(EquipmentSlotType)))
        {
            equippedItems[slotType] = null;
        }
    }

    /// <summary>
    /// Ёкипирует новый предмет и возвращает старый, если он был.
    /// </summary>
    public EquipmentData EquipItem(EquipmentData newItem)
    {
        if (newItem == null) return null;

        EquipmentData oldItem = equippedItems[newItem.slotType];
        equippedItems[newItem.slotType] = newItem;

        OnEquipmentChanged?.Invoke();
        return oldItem;
    }

    /// <summary>
    /// —нимает предмет из слота и возвращает его.
    /// </summary>
    public EquipmentData UnequipItem(EquipmentSlotType slotType)
    {
        EquipmentData itemToUnequip = equippedItems[slotType];
        if (itemToUnequip != null)
        {
            equippedItems[slotType] = null;
            OnEquipmentChanged?.Invoke();
        }
        return itemToUnequip;
    }

    public EquipmentData GetItemInSlot(EquipmentSlotType slotType)
    {
        return equippedItems[slotType];
    }

    public int GetBonusInventorySlots()
    {
        int totalBonus = 0;
        foreach (var item in equippedItems.Values)
        {
            if (item != null)
            {
                totalBonus += item.bonusInventorySlots;
            }
        }
        return totalBonus;
    }
}
