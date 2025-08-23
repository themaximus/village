using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

/// <summary>
/// Универсальный скрипт, который теперь сам управляет перетаскиванием.
/// </summary>
public class UniversalSlotUI : MonoBehaviour, IPointerClickHandler, IDropHandler, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    public enum SlotContext { Inventory, Equipment }

    [Header("Slot Configuration")]
    public SlotContext context;
    public EquipmentSlotType equipmentSlotType;

    [Header("UI Elements")]
    public Image iconImage;
    [SerializeField] private Sprite defaultSprite;

    // Ссылки на системы
    private Inventory inventory;
    private EquipmentController equipmentController;

    // Данные слота
    private ItemData item;
    private int slotIndex;

    // Статическая переменная для хранения "призрака" предмета
    private static GameObject dragGhost;

    public ItemData Item => item;
    public int SlotIndex => slotIndex;
    public Inventory Inventory => inventory;

    void Awake()
    {
        equipmentController = FindObjectOfType<EquipmentController>();
    }

    void Start()
    {
        if (context == SlotContext.Equipment && equipmentController != null)
        {
            equipmentController.OnEquipmentChanged += RefreshSlot;
            RefreshSlot();
        }
    }

    public void Initialize(Inventory inv, int index)
    {
        inventory = inv;
        slotIndex = index;
    }

    public void AddItem(ItemData newItem)
    {
        this.item = newItem;
        UpdateUI();
    }

    public void ClearSlot()
    {
        this.item = null;
        UpdateUI();
    }

    public bool IsEmpty()
    {
        return item == null;
    }

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
                iconImage.sprite = defaultSprite;
                iconImage.enabled = (defaultSprite != null);
            }
            else
            {
                iconImage.enabled = false;
            }
        }
    }

    private void RefreshSlot()
    {
        if (equipmentController != null)
        {
            this.item = equipmentController.GetItemInSlot(equipmentSlotType);
            UpdateUI();
        }
    }

    // --- НОВАЯ ЛОГИКА ПЕРЕТАСКИВАНИЯ ---

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (IsEmpty()) return;

        // Создаем "призрака"
        dragGhost = new GameObject("DragGhost");
        dragGhost.transform.SetParent(GetComponentInParent<Canvas>().transform, false);
        dragGhost.transform.SetAsLastSibling();
        var image = dragGhost.AddComponent<Image>();
        image.sprite = this.item.icon;
        image.raycastTarget = false;
        dragGhost.GetComponent<RectTransform>().sizeDelta = GetComponent<RectTransform>().sizeDelta;

        // Делаем иконку в исходном слоте полупрозрачной
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
        // Этот метод вызывается на ЦЕЛЕВОМ слоте
        UniversalSlotUI sourceSlot = eventData.pointerDrag.GetComponent<UniversalSlotUI>();
        if (sourceSlot == null || sourceSlot == this || sourceSlot.IsEmpty()) return;

        ItemData sourceItem = sourceSlot.Item;
        ItemData targetItem = this.Item;

        // Логика зависит от контекста ЦЕЛЕВОГО слота
        switch (this.context)
        {
            case SlotContext.Inventory:
                // Если источник - тоже инвентарь, меняемся местами
                if (sourceSlot.context == SlotContext.Inventory)
                {
                    this.AddItem(sourceItem);
                    sourceSlot.AddItem(targetItem);
                }
                // Если источник - снаряжение, снимаем его в этот слот
                else if (sourceSlot.context == SlotContext.Equipment)
                {
                    EquipmentData unequippedItem = equipmentController.UnequipItem(sourceSlot.equipmentSlotType);
                    this.AddItem(unequippedItem);
                    sourceSlot.AddItem(targetItem); // Возвращаем предмет из инвентаря на место снятого (если там что-то было)
                }
                break;

            case SlotContext.Equipment:
                // Если источник - инвентарь и предмет подходит, экипируем
                if (sourceSlot.context == SlotContext.Inventory && sourceItem is EquipmentData equipment && equipment.slotType == this.equipmentSlotType)
                {
                    EquipmentData oldEquipment = equipmentController.EquipItem(equipment);
                    sourceSlot.AddItem(oldEquipment); // Возвращаем старый предмет в инвентарь
                }
                break;
        }
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        // Этот метод вызывается на ИСХОДНОМ слоте
        iconImage.color = new Color(1f, 1f, 1f, 1f);
        if (dragGhost != null)
        {
            Destroy(dragGhost);
        }

        // Если мы бросили предмет не на другой слот (а в мир)
        if (eventData.pointerEnter == null || eventData.pointerEnter.GetComponentInParent<UniversalSlotUI>() == null)
        {
            ItemData itemToDrop = null;
            if (context == SlotContext.Inventory)
            {
                itemToDrop = this.Item;
                this.ClearSlot();
            }
            else if (context == SlotContext.Equipment)
            {
                itemToDrop = equipmentController.UnequipItem(this.equipmentSlotType);
            }

            if (itemToDrop != null && itemToDrop.prefab != null)
            {
                Instantiate(itemToDrop.prefab, transform.position, Quaternion.identity); // Позицию можно улучшить
            }
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        // Клик правой кнопкой мыши для снятия снаряжения
        if (context == SlotContext.Equipment && eventData.button == PointerEventData.InputButton.Right)
        {
            ItemData unequippedItem = equipmentController.UnequipItem(equipmentSlotType);
            if (unequippedItem != null)
            {
                inventory.AddItem(unequippedItem);
            }
        }
        else if (context == SlotContext.Inventory)
        {
            inventory.SelectSlot(slotIndex);
        }
    }

    void OnDestroy()
    {
        if (context == SlotContext.Equipment && equipmentController != null)
        {
            equipmentController.OnEquipmentChanged -= RefreshSlot;
        }
    }
}
