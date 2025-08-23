using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class EquipmentSlotUI : MonoBehaviour, IDropHandler, IPointerClickHandler
{
    [Header("Slot Settings")]
    public EquipmentSlotType slotType;

    [Header("UI Elements")]
    [SerializeField] private Image iconImage;
    [SerializeField] private Sprite defaultSprite;

    private EquipmentController equipmentController;

    void Start()
    {
        // --- НОВАЯ ДИАГНОСТИКА ---
        // Проверяем, есть ли на родительском Canvas компонент Graphic Raycaster,
        // без которого система событий UI работать не будет.
        if (GetComponentInParent<Canvas>().GetComponent<GraphicRaycaster>() == null)
        {
            Debug.LogError($"[EquipmentSlotUI] На родительском Canvas для слота '{gameObject.name}' отсутствует компонент 'Graphic Raycaster'!");
        }
        // -------------------------

        equipmentController = FindObjectOfType<EquipmentController>();
        if (equipmentController == null)
        {
            Debug.LogError("EquipmentController not found in the scene!");
            return;
        }
        equipmentController.OnEquipmentChanged += UpdateSlotUI;
        UpdateSlotUI();
    }

    public void OnDrop(PointerEventData eventData)
    {
        // --- ДИАГНОСТИКА ---
        Debug.Log($"[EquipmentSlotUI] OnDrop сработал на слоте '{gameObject.name}'. Мой тип слота: {slotType}.");

        if (equipmentController == null)
        {
            Debug.LogError("[EquipmentSlotUI] EquipmentController не найден. Прерываю.");
            return;
        }

        DragItemDrop dragItem = eventData.pointerDrag.GetComponent<DragItemDrop>();
        if (dragItem != null)
        {
            Debug.Log("[EquipmentSlotUI] Найден компонент DragItemDrop.");
            // Проверяем, является ли предмет снаряжением
            if (dragItem.slot.Item is EquipmentData equipment)
            {
                Debug.Log($"[EquipmentSlotUI] Перетаскиваемый предмет '{equipment.itemName}' ЯВЛЯЕТСЯ снаряжением. Его тип слота: '{equipment.slotType}'.");
                // Проверяем, совпадают ли типы слотов
                if (equipment.slotType == this.slotType)
                {
                    Debug.Log("<color=green>[EquipmentSlotUI] Типы слотов СОВПАДАЮТ! Экипирую предмет...</color>");
                    equipmentController.EquipItem(equipment);
                    dragItem.slot.ClearSlot();
                }
                else
                {
                    Debug.LogError($"<color=red>[EquipmentSlotUI] Типы слотов НЕ СОВПАДАЮТ! Предмет требует '{equipment.slotType}', а этот слот для '{this.slotType}'.</color>");
                }
            }
            else
            {
                Debug.LogWarning($"[EquipmentSlotUI] Перетаскиваемый предмет '{dragItem.slot.Item.itemName}' НЕ является снаряжением (EquipmentData).");
            }
        }
        else
        {
            Debug.LogError("[EquipmentSlotUI] Не удалось получить компонент DragItemDrop из перетаскиваемого объекта.");
        }
    }
    // -------------------

    public void OnPointerClick(PointerEventData eventData)
    {
        if (equipmentController == null) return;
        equipmentController.UnequipItem(slotType);
    }

    private void UpdateSlotUI()
    {
        if (equipmentController == null) return;
        EquipmentData equippedItem = equipmentController.GetItemInSlot(slotType);
        if (equippedItem != null)
        {
            iconImage.sprite = equippedItem.icon;
            iconImage.enabled = true;
        }
        else
        {
            iconImage.sprite = defaultSprite;
            iconImage.enabled = (defaultSprite != null);
        }
    }

    void OnDestroy()
    {
        if (equipmentController != null)
        {
            equipmentController.OnEquipmentChanged -= UpdateSlotUI;
        }
    }
}
