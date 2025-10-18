using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

/// <summary>
/// Универсальный скрипт для всех слотов (инвентарь, снаряжение, крафт).
/// </summary>
public class UniversalSlotUI : MonoBehaviour, IPointerClickHandler, IDropHandler, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    public enum SlotContext { Inventory, Equipment, Crafting }

    [Header("Slot Configuration")]
    public SlotContext context;
    [Tooltip("Если это слот снаряжения, укажите его тип.")]
    public EquipmentSlotType equipmentSlotType;
    [Tooltip("Отметьте, если это слот для результата крафта (делает его неинтерактивным для игрока).")]
    [SerializeField] private bool isResultSlot = false; // <-- ДОБАВЛЕНО: Флаг для слота результата

    [Header("UI Elements")]
    public Image iconImage;
    [SerializeField] private Sprite defaultSprite;

    // Ссылки на системы
    private Inventory inventory;
    private EquipmentController equipmentController;
    private CraftingUI craftingUI;

    // Данные слота
    private ItemData item;
    private int slotIndex;

    private static GameObject dragGhost;

    public ItemData Item => item;
    public int SlotIndex => slotIndex;
    public Inventory Inventory => inventory;

    void Awake()
    {
        equipmentController = FindObjectOfType<EquipmentController>();
        inventory = FindObjectOfType<Inventory>();
        // GetComponentInParent найдет компонент, даже если он на несколько уровней выше
        if (context == SlotContext.Crafting)
        {
            craftingUI = GetComponentInParent<CraftingUI>();
        }
    }

    void Start()
    {
        if (context == SlotContext.Equipment && equipmentController != null)
        {
            equipmentController.OnEquipmentChanged += RefreshEquipmentSlot;
            RefreshEquipmentSlot();
        }
    }

    // Инициализация для разных типов слотов
    public void InitializeInventorySlot(Inventory inv, int index)
    {
        context = SlotContext.Inventory;
        inventory = inv;
        slotIndex = index;
    }

    public void InitializeCraftingSlot(CraftingUI ui)
    {
        context = SlotContext.Crafting;
        craftingUI = ui;
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

    private void RefreshEquipmentSlot()
    {
        if (context == SlotContext.Equipment && equipmentController != null)
        {
            item = equipmentController.GetItemInSlot(equipmentSlotType);
            UpdateUI();
        }
    }

    // --- ЛОГИКА ВЗАИМОДЕЙСТВИЯ ---

    public void OnPointerClick(PointerEventData eventData)
    {
        // ИЗМЕНЕНО: Слот результата не реагирует на клики
        if (isResultSlot || IsEmpty() || eventData.button != PointerEventData.InputButton.Right) return;

        switch (context)
        {
            case SlotContext.Inventory:
                if (item is EquipmentData equipment)
                {
                    ItemData oldItem = equipmentController.EquipItem(equipment);
                    AddItem(oldItem);
                }
                break;
            case SlotContext.Equipment:
                ItemData unequippedItem = equipmentController.UnequipItem(equipmentSlotType);
                if (unequippedItem != null) inventory.AddItem(unequippedItem);
                break;
            case SlotContext.Crafting:
                // Правый клик по слоту ингредиентов возвращает предмет в инвентарь
                ItemData itemToReturn = item;
                ClearSlot();
                inventory.AddItem(itemToReturn);
                craftingUI?.CheckRecipe();
                break;
        }
    }

    // --- ЛОГИКА ПЕРЕТАСКИВАНИЯ ---

    public void OnBeginDrag(PointerEventData eventData)
    {
        // ИЗМЕНЕНО: Нельзя перетаскивать предмет из слота результата
        if (isResultSlot || IsEmpty() || eventData.button != PointerEventData.InputButton.Left) return;

        dragGhost = new GameObject("DragGhost");
        dragGhost.transform.SetParent(GetComponentInParent<Canvas>().transform, false);
        dragGhost.transform.SetAsLastSibling();
        var image = dragGhost.AddComponent<Image>();
        image.sprite = item.icon;
        image.raycastTarget = false;
        dragGhost.GetComponent<RectTransform>().sizeDelta = GetComponent<RectTransform>().sizeDelta;

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
        // ИЗМЕНЕНО: Нельзя бросать предметы в слот результата
        if (isResultSlot || eventData.button != PointerEventData.InputButton.Left) return;

        UniversalSlotUI sourceSlot = eventData.pointerDrag.GetComponent<UniversalSlotUI>();
        if (sourceSlot == null || sourceSlot == this || sourceSlot.IsEmpty()) return;

        ItemData sourceItem = sourceSlot.Item;
        ItemData targetItem = this.item;

        // Случай 1: Перетаскиваем на СЛОТ КРАФТА (для ингредиентов)
        if (this.context == SlotContext.Crafting)
        {
            if (sourceSlot.context == SlotContext.Inventory)
            {
                this.AddItem(sourceItem);
                sourceSlot.AddItem(targetItem);
                craftingUI?.CheckRecipe();
            }
            return;
        }

        // Случай 2: Перетаскиваем на СЛОТ СНАРЯЖЕНИЯ
        if (this.context == SlotContext.Equipment)
        {
            if (sourceItem is EquipmentData equipment && equipment.slotType == this.equipmentSlotType)
            {
                ItemData oldItem = equipmentController.EquipItem(equipment);
                sourceSlot.AddItem(oldItem);
            }
            return;
        }

        // Случай 3: Перетаскиваем на СЛОТ ИНВЕНТАРЯ
        if (this.context == SlotContext.Inventory)
        {
            if (sourceSlot.context == SlotContext.Inventory)
            {
                sourceSlot.AddItem(targetItem);
                this.AddItem(sourceItem);
            }
            else if (sourceSlot.context == SlotContext.Equipment)
            {
                if (targetItem == null || (targetItem is EquipmentData eq && eq.slotType == sourceSlot.equipmentSlotType))
                {
                    ItemData unequippedItem = equipmentController.UnequipItem(sourceSlot.equipmentSlotType);
                    this.AddItem(unequippedItem);
                    equipmentController.EquipItem(targetItem as EquipmentData);
                }
            }
            else if (sourceSlot.context == SlotContext.Crafting)
            {
                this.AddItem(sourceItem);
                sourceSlot.AddItem(targetItem);
                sourceSlot.craftingUI?.CheckRecipe();
            }
        }
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        iconImage.color = new Color(1f, 1f, 1f, 1f);
        if (dragGhost != null) Destroy(dragGhost);

        if (eventData.pointerEnter == null || eventData.pointerEnter.GetComponentInParent<UniversalSlotUI>() == null)
        {
            // Эта логика отвечает за выбрасывание предмета из инвентаря/снаряжения,
            // но так как мы заблокировали OnBeginDrag для слота результата,
            // отсюда выбросить предмет уже не получится.
            ItemData itemToDrop = null;
            if (context == SlotContext.Inventory)
            {
                itemToDrop = this.item;
                this.ClearSlot();
            }
            else if (context == SlotContext.Equipment)
            {
                itemToDrop = equipmentController.UnequipItem(this.equipmentSlotType);
            }
            else if (context == SlotContext.Crafting && !isResultSlot) // Из слота ингредиентов
            {
                itemToDrop = this.item;
                this.ClearSlot();
                craftingUI?.CheckRecipe();
            }

            if (itemToDrop != null && itemToDrop.prefab != null)
            {
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
        if (context == SlotContext.Equipment && equipmentController != null)
        {
            equipmentController.OnEquipmentChanged -= RefreshEquipmentSlot;
        }
    }
}

