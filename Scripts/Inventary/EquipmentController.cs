using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq; // Необходимо для ToDictionary
using Newtonsoft.Json.Linq; // Необходимо для JObject

// Добавляем "контракт" ISaveable
public class EquipmentController : MonoBehaviour, ISaveable
{
    private Dictionary<EquipmentSlotType, EquipmentData> equippedItems = new Dictionary<EquipmentSlotType, EquipmentData>();
    public event Action OnEquipmentChanged;
    private Inventory inventory;

    void Awake()
    {
        inventory = GetComponent<Inventory>();
        // Инициализируем словарь пустыми значениями
        foreach (EquipmentSlotType slotType in Enum.GetValues(typeof(EquipmentSlotType)))
        {
            equippedItems[slotType] = null;
        }
    }

    public EquipmentData EquipItem(EquipmentData newItem)
    {
        EquipmentData oldItem = null;
        if (newItem != null)
        {
            oldItem = equippedItems[newItem.slotType];
            equippedItems[newItem.slotType] = newItem;
        }

        OnEquipmentChanged?.Invoke();
        return oldItem;
    }

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

    // --- РЕАЛИЗАЦИЯ ИНТЕРФЕЙСА ISaveable ---

    /// <summary>
    /// "Фотографирует" ID всех надетых предметов.
    /// </summary>
    public object CaptureState()
    {
        // Создаем словарь, где ключ - это название слота (например, "Head"),
        // а значение - это ID предмета в этом слоте.
        var equippedItemIDs = new Dictionary<string, string>();
        foreach (var pair in equippedItems)
        {
            if (pair.Value != null)
            {
                equippedItemIDs[pair.Key.ToString()] = pair.Value.itemID;
            }
        }
        return equippedItemIDs;
    }

    /// <summary>
    /// Восстанавливает надетые предметы из сохранения.
    /// </summary>
    public void RestoreState(object state)
    {
        var equippedItemIDs = ((JObject)state).ToObject<Dictionary<string, string>>();

        // Сначала полностью очищаем все слоты
        foreach (EquipmentSlotType slotType in Enum.GetValues(typeof(EquipmentSlotType)))
        {
            equippedItems[slotType] = null;
        }

        // Теперь надеваем предметы из сохранения
        foreach (var pair in equippedItemIDs)
        {
            EquipmentSlotType slotType = (EquipmentSlotType)Enum.Parse(typeof(EquipmentSlotType), pair.Key);
            ItemData itemData = ItemDatabase.instance.GetItemByID(pair.Value);

            if (itemData is EquipmentData equipmentData)
            {
                equippedItems[slotType] = equipmentData;
            }
        }

        // Вызываем событие, чтобы UI обновился
        OnEquipmentChanged?.Invoke();
    }
}
