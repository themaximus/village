using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

/// <summary>
/// Универсальный скрипт для всех слотов (инвентарь, снаряжение).
/// Теперь он единолично управляет кликами, перетаскиванием и логикой взаимодействия.
/// </summary>
public class UniversalSlotUI : MonoBehaviour, IPointerClickHandler, IDropHandler, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    public enum SlotContext { Inventory, Equipment }

    [Header("Конфигурация слота")]
    [Tooltip("Определяет, является ли этот слот частью инвентаря или снаряжения.")]
    public SlotContext context;
    [Tooltip("Если это слот снаряжения, укажите его тип.")]
    public EquipmentSlotType equipmentSlotType;

    [Header("UI Элементы")]
    public Image iconImage;
    [SerializeField] private Sprite defaultSprite; // Спрайт по умолчанию для слотов снаряжения

    // Ссылки на системы
    private Inventory inventory;
    private EquipmentController equipmentController;

    // Данные слота
    private ItemData item;
    private int slotIndex; // Только для слотов инвентаря

    // Статическая переменная для "призрака" перетаскиваемого предмета
    private static GameObject dragGhost;

    // Публичные свойства для доступа к данным слота
    public ItemData Item => item;
    public int SlotIndex => slotIndex;
    public Inventory Inventory => inventory;

    void Awake()
    {
        // Находим контроллеры в сцене
        // Для больших сцен лучше использовать более производительный способ (например, через синглтон)
        equipmentController = FindObjectOfType<EquipmentController>();
        inventory = FindObjectOfType<Inventory>();
    }

    void Start()
    {
        // Если это слот снаряжения, подписываемся на событие смены экипировки
        if (context == SlotContext.Equipment && equipmentController != null)
        {
            equipmentController.OnEquipmentChanged += RefreshEquipmentSlot;
            RefreshEquipmentSlot(); // Первоначальное обновление
        }
    }

    /// <summary>
    /// Инициализирует слот инвентаря.
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
    /// Обновляет отображение слота (иконку).
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
                // Для слотов снаряжения показываем спрайт по умолчанию
                iconImage.sprite = defaultSprite;
                iconImage.enabled = (defaultSprite != null);
            }
            else
            {
                // Для слотов инвентаря просто скрываем иконку
                iconImage.enabled = false;
            }
        }
    }

    /// <summary>
    /// Обновляет слот снаряжения на основе данных из EquipmentController.
    /// </summary>
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
        if (IsEmpty()) return;

        // Клик правой кнопкой мыши
        if (eventData.button == PointerEventData.InputButton.Right)
        {
            // Если это слот инвентаря, пытаемся экипировать/использовать предмет
            if (context == SlotContext.Inventory && item is EquipmentData equipment)
            {
                // Снимаем старый предмет (если он был) и надеваем новый
                ItemData oldItem = equipmentController.EquipItem(equipment);
                // Помещаем старый предмет в этот слот
                AddItem(oldItem);
            }
            // Если это слот снаряжения, снимаем предмет в инвентарь
            else if (context == SlotContext.Equipment)
            {
                ItemData unequippedItem = equipmentController.UnequipItem(equipmentSlotType);
                if (unequippedItem != null)
                {
                    inventory.AddItem(unequippedItem);
                }
            }
        }
        // Клик левой кнопкой мыши по слоту инвентаря
        else if (context == SlotContext.Inventory && eventData.button == PointerEventData.InputButton.Left)
        {
            inventory.SelectSlot(slotIndex);
        }
    }

    // --- ЛОГИКА ПЕРЕТАСКИВАНИЯ (DRAG & DROP) ---

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (IsEmpty() || eventData.button != PointerEventData.InputButton.Left) return;

        // Создаем "призрака" предмета для перетаскивания
        dragGhost = new GameObject("DragGhost");
        dragGhost.transform.SetParent(GetComponentInParent<Canvas>().transform, false);
        dragGhost.transform.SetAsLastSibling();
        var image = dragGhost.AddComponent<Image>();
        image.sprite = item.icon;
        image.raycastTarget = false; // "Призрак" не будет блокировать события мыши
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
        // Этот метод вызывается на ЦЕЛЕВОМ слоте, куда бросили предмет
        if (eventData.button != PointerEventData.InputButton.Left) return;

        UniversalSlotUI sourceSlot = eventData.pointerDrag.GetComponent<UniversalSlotUI>();
        if (sourceSlot == null || sourceSlot == this || sourceSlot.IsEmpty()) return;

        ItemData sourceItem = sourceSlot.Item;
        ItemData targetItem = this.item; // Предмет, который уже лежит в этом слоте

        // --- ГЛАВНАЯ ЛОГИКА ОБМЕНА ---

        // Случай 1: Перетаскиваем на СЛОТ СНАРЯЖЕНИЯ
        if (this.context == SlotContext.Equipment)
        {
            // Проверяем, что предмет является снаряжением и подходит для этого слота
            if (sourceItem is EquipmentData equipment && equipment.slotType == this.equipmentSlotType)
            {
                // Экипируем новый предмет и получаем старый
                ItemData oldItem = equipmentController.EquipItem(equipment);
                // Кладем старый предмет в исходный слот инвентаря
                sourceSlot.AddItem(oldItem);
            }
            // Если предмет не подходит, ничего не делаем
            return;
        }

        // Случай 2: Перетаскиваем на СЛОТ ИНВЕНТАРЯ
        if (this.context == SlotContext.Inventory)
        {
            // Источник - тоже инвентарь (просто меняемся местами)
            if (sourceSlot.context == SlotContext.Inventory)
            {
                sourceSlot.AddItem(targetItem);
                this.AddItem(sourceItem);
            }
            // Источник - снаряжение (снимаем предмет в этот слот инвентаря)
            else if (sourceSlot.context == SlotContext.Equipment)
            {
                // Проверяем, можно ли положить предмет из инвентаря (targetItem) в слот снаряжения
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
        // Этот метод вызывается на ИСХОДНОМ слоте после завершения перетаскивания
        iconImage.color = new Color(1f, 1f, 1f, 1f);
        if (dragGhost != null)
        {
            Destroy(dragGhost);
        }

        // Проверяем, был ли предмет брошен мимо всех слотов (в мир)
        if (eventData.pointerEnter == null || eventData.pointerEnter.GetComponentInParent<UniversalSlotUI>() == null)
        {
            ItemData itemToDrop = null;
            // Выбрасываем из инвентаря
            if (context == SlotContext.Inventory)
            {
                itemToDrop = this.item;
                this.ClearSlot();
            }
            // Снимаем и выбрасываем снаряжение
            else if (context == SlotContext.Equipment)
            {
                itemToDrop = equipmentController.UnequipItem(this.equipmentSlotType);
            }

            // Создаем префаб предмета в мире
            if (itemToDrop != null && itemToDrop.prefab != null)
            {
                // Логика создания префаба (можно улучшить, чтобы предмет появлялся перед игроком)
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
        // Отписываемся от событий, чтобы избежать ошибок
        if (context == SlotContext.Equipment && equipmentController != null)
        {
            equipmentController.OnEquipmentChanged -= RefreshEquipmentSlot;
        }
    }
}