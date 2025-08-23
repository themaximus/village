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
        // --- ����� ����������� ---
        // ���������, ���� �� �� ������������ Canvas ��������� Graphic Raycaster,
        // ��� �������� ������� ������� UI �������� �� �����.
        if (GetComponentInParent<Canvas>().GetComponent<GraphicRaycaster>() == null)
        {
            Debug.LogError($"[EquipmentSlotUI] �� ������������ Canvas ��� ����� '{gameObject.name}' ����������� ��������� 'Graphic Raycaster'!");
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
        // --- ����������� ---
        Debug.Log($"[EquipmentSlotUI] OnDrop �������� �� ����� '{gameObject.name}'. ��� ��� �����: {slotType}.");

        if (equipmentController == null)
        {
            Debug.LogError("[EquipmentSlotUI] EquipmentController �� ������. ��������.");
            return;
        }

        DragItemDrop dragItem = eventData.pointerDrag.GetComponent<DragItemDrop>();
        if (dragItem != null)
        {
            Debug.Log("[EquipmentSlotUI] ������ ��������� DragItemDrop.");
            // ���������, �������� �� ������� �����������
            if (dragItem.slot.Item is EquipmentData equipment)
            {
                Debug.Log($"[EquipmentSlotUI] ��������������� ������� '{equipment.itemName}' �������� �����������. ��� ��� �����: '{equipment.slotType}'.");
                // ���������, ��������� �� ���� ������
                if (equipment.slotType == this.slotType)
                {
                    Debug.Log("<color=green>[EquipmentSlotUI] ���� ������ ���������! �������� �������...</color>");
                    equipmentController.EquipItem(equipment);
                    dragItem.slot.ClearSlot();
                }
                else
                {
                    Debug.LogError($"<color=red>[EquipmentSlotUI] ���� ������ �� ���������! ������� ������� '{equipment.slotType}', � ���� ���� ��� '{this.slotType}'.</color>");
                }
            }
            else
            {
                Debug.LogWarning($"[EquipmentSlotUI] ��������������� ������� '{dragItem.slot.Item.itemName}' �� �������� ����������� (EquipmentData).");
            }
        }
        else
        {
            Debug.LogError("[EquipmentSlotUI] �� ������� �������� ��������� DragItemDrop �� ���������������� �������.");
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
