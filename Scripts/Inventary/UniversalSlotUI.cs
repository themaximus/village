using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

/// <summary>
/// ������������� ������ ��� ���� ������ (���������, ����������).
/// ������ �� ���������� ��������� �������, ��������������� � ������� ��������������.
/// </summary>
public class UniversalSlotUI : MonoBehaviour, IPointerClickHandler, IDropHandler, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    public enum SlotContext { Inventory, Equipment }

    [Header("������������ �����")]
    [Tooltip("����������, �������� �� ���� ���� ������ ��������� ��� ����������.")]
    public SlotContext context;
    [Tooltip("���� ��� ���� ����������, ������� ��� ���.")]
    public EquipmentSlotType equipmentSlotType;

    [Header("UI ��������")]
    public Image iconImage;
    [SerializeField] private Sprite defaultSprite; // ������ �� ��������� ��� ������ ����������

    // ������ �� �������
    private Inventory inventory;
    private EquipmentController equipmentController;

    // ������ �����
    private ItemData item;
    private int slotIndex; // ������ ��� ������ ���������

    // ����������� ���������� ��� "��������" ���������������� ��������
    private static GameObject dragGhost;

    // ��������� �������� ��� ������� � ������ �����
    public ItemData Item => item;
    public int SlotIndex => slotIndex;
    public Inventory Inventory => inventory;

    void Awake()
    {
        // ������� ����������� � �����
        // ��� ������� ���� ����� ������������ ����� ���������������� ������ (��������, ����� ��������)
        equipmentController = FindObjectOfType<EquipmentController>();
        inventory = FindObjectOfType<Inventory>();
    }

    void Start()
    {
        // ���� ��� ���� ����������, ������������� �� ������� ����� ����������
        if (context == SlotContext.Equipment && equipmentController != null)
        {
            equipmentController.OnEquipmentChanged += RefreshEquipmentSlot;
            RefreshEquipmentSlot(); // �������������� ����������
        }
    }

    /// <summary>
    /// �������������� ���� ���������.
    /// </summary>
    public void InitializeInventorySlot(Inventory inv, int index)
    {
        context = SlotContext.Inventory;
        inventory = inv;
        slotIndex = index;
    }

    public void AddItem(ItemData newItem)
    {
        item = newItem;
        UpdateUI();
    }

    public void ClearSlot()
    {
        item = null;
        UpdateUI();
    }

    public bool IsEmpty()
    {
        return item == null;
    }

    /// <summary>
    /// ��������� ����������� ����� (������).
    /// </summary>
    public void UpdateUI()
    {
        if (item != null)
        {
            iconImage.sprite = item.icon;
            iconImage.enabled = true;
        }
        else
        {
            if (context == SlotContext.Equipment)
            {
                // ��� ������ ���������� ���������� ������ �� ���������
                iconImage.sprite = defaultSprite;
                iconImage.enabled = (defaultSprite != null);
            }
            else
            {
                // ��� ������ ��������� ������ �������� ������
                iconImage.enabled = false;
            }
        }
    }

    /// <summary>
    /// ��������� ���� ���������� �� ������ ������ �� EquipmentController.
    /// </summary>
    private void RefreshEquipmentSlot()
    {
        if (context == SlotContext.Equipment && equipmentController != null)
        {
            item = equipmentController.GetItemInSlot(equipmentSlotType);
            UpdateUI();
        }
    }

    // --- ������ �������������� ---

    public void OnPointerClick(PointerEventData eventData)
    {
        if (IsEmpty()) return;

        // ���� ������ ������� ����
        if (eventData.button == PointerEventData.InputButton.Right)
        {
            // ���� ��� ���� ���������, �������� �����������/������������ �������
            if (context == SlotContext.Inventory && item is EquipmentData equipment)
            {
                // ������� ������ ������� (���� �� ���) � �������� �����
                ItemData oldItem = equipmentController.EquipItem(equipment);
                // �������� ������ ������� � ���� ����
                AddItem(oldItem);
            }
            // ���� ��� ���� ����������, ������� ������� � ���������
            else if (context == SlotContext.Equipment)
            {
                ItemData unequippedItem = equipmentController.UnequipItem(equipmentSlotType);
                if (unequippedItem != null)
                {
                    inventory.AddItem(unequippedItem);
                }
            }
        }
        // ���� ����� ������� ���� �� ����� ���������
        else if (context == SlotContext.Inventory && eventData.button == PointerEventData.InputButton.Left)
        {
            inventory.SelectSlot(slotIndex);
        }
    }

    // --- ������ �������������� (DRAG & DROP) ---

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (IsEmpty() || eventData.button != PointerEventData.InputButton.Left) return;

        // ������� "��������" �������� ��� ��������������
        dragGhost = new GameObject("DragGhost");
        dragGhost.transform.SetParent(GetComponentInParent<Canvas>().transform, false);
        dragGhost.transform.SetAsLastSibling();
        var image = dragGhost.AddComponent<Image>();
        image.sprite = item.icon;
        image.raycastTarget = false; // "�������" �� ����� ����������� ������� ����
        dragGhost.GetComponent<RectTransform>().sizeDelta = GetComponent<RectTransform>().sizeDelta;

        // ������ ������ � �������� ����� ��������������
        iconImage.color = new Color(1f, 1f, 1f, 0.5f);
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (dragGhost != null)
        {
            dragGhost.transform.position = eventData.position;
        }
    }

    public void OnDrop(PointerEventData eventData)
    {
        // ���� ����� ���������� �� ������� �����, ���� ������� �������
        if (eventData.button != PointerEventData.InputButton.Left) return;

        UniversalSlotUI sourceSlot = eventData.pointerDrag.GetComponent<UniversalSlotUI>();
        if (sourceSlot == null || sourceSlot == this || sourceSlot.IsEmpty()) return;

        ItemData sourceItem = sourceSlot.Item;
        ItemData targetItem = this.item; // �������, ������� ��� ����� � ���� �����

        // --- ������� ������ ������ ---

        // ������ 1: ������������� �� ���� ����������
        if (this.context == SlotContext.Equipment)
        {
            // ���������, ��� ������� �������� ����������� � �������� ��� ����� �����
            if (sourceItem is EquipmentData equipment && equipment.slotType == this.equipmentSlotType)
            {
                // ��������� ����� ������� � �������� ������
                ItemData oldItem = equipmentController.EquipItem(equipment);
                // ������ ������ ������� � �������� ���� ���������
                sourceSlot.AddItem(oldItem);
            }
            // ���� ������� �� ��������, ������ �� ������
            return;
        }

        // ������ 2: ������������� �� ���� ���������
        if (this.context == SlotContext.Inventory)
        {
            // �������� - ���� ��������� (������ �������� �������)
            if (sourceSlot.context == SlotContext.Inventory)
            {
                sourceSlot.AddItem(targetItem);
                this.AddItem(sourceItem);
            }
            // �������� - ���������� (������� ������� � ���� ���� ���������)
            else if (sourceSlot.context == SlotContext.Equipment)
            {
                // ���������, ����� �� �������� ������� �� ��������� (targetItem) � ���� ����������
                if (targetItem == null || (targetItem is EquipmentData eq && eq.slotType == sourceSlot.equipmentSlotType))
                {
                    ItemData unequippedItem = equipmentController.UnequipItem(sourceSlot.equipmentSlotType);
                    this.AddItem(unequippedItem);
                    equipmentController.EquipItem(targetItem as EquipmentData);
                }
            }
        }
    }


    public void OnEndDrag(PointerEventData eventData)
    {
        // ���� ����� ���������� �� �������� ����� ����� ���������� ��������������
        iconImage.color = new Color(1f, 1f, 1f, 1f);
        if (dragGhost != null)
        {
            Destroy(dragGhost);
        }

        // ���������, ��� �� ������� ������ ���� ���� ������ (� ���)
        if (eventData.pointerEnter == null || eventData.pointerEnter.GetComponentInParent<UniversalSlotUI>() == null)
        {
            ItemData itemToDrop = null;
            // ����������� �� ���������
            if (context == SlotContext.Inventory)
            {
                itemToDrop = this.item;
                this.ClearSlot();
            }
            // ������� � ����������� ����������
            else if (context == SlotContext.Equipment)
            {
                itemToDrop = equipmentController.UnequipItem(this.equipmentSlotType);
            }

            // ������� ������ �������� � ����
            if (itemToDrop != null && itemToDrop.prefab != null)
            {
                // ������ �������� ������� (����� ��������, ����� ������� ��������� ����� �������)
                Vector3 mousePos = Input.mousePosition;
                mousePos.z = Camera.main.nearClipPlane + 10f;
                Vector3 worldPos = Camera.main.ScreenToWorldPoint(mousePos);
                worldPos.z = 0;
                Instantiate(itemToDrop.prefab, worldPos, Quaternion.identity);
            }
        }
    }

    void OnDestroy()
    {
        // ������������ �� �������, ����� �������� ������
        if (context == SlotContext.Equipment && equipmentController != null)
        {
            equipmentController.OnEquipmentChanged -= RefreshEquipmentSlot;
        }
    }
}