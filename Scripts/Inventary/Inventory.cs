using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;

[RequireComponent(typeof(EquipmentController))]
public class Inventory : MonoBehaviour, ISaveable
{
    [Header("Настройки инвентаря")]
    public GameObject slotPrefab;
    public int baseSlotCount = 10;
    public Transform panel; // Панель для основных слотов инвентаря
    public Transform hotbarPanel; // Панель для хотбара (первые 5 слотов)
    public Sprite selectionOutlineSprite;

    private MovePlayer player;
    private EquipmentController equipmentController;

    private List<UniversalSlotUI> slots = new List<UniversalSlotUI>();

    [HideInInspector] public int selectedIndex = -1;
    [HideInInspector] public ItemData currentItemInHand;

    private Image selectionOutline;

    public int SelectedIndex => selectedIndex;

    [System.Serializable]
    private struct InventorySaveData
    {
        public int baseSlotCount;
        public string[] itemIDs;
    }

    void Awake()
    {
        player = GetComponent<MovePlayer>();
        equipmentController = GetComponent<EquipmentController>();
        equipmentController.OnEquipmentChanged += UpdateInventoryCapacity;
    }

    void Start()
    {
        CreateSelectionOutline();
        UpdateInventoryCapacity();
        SelectSlot(0);
    }

    void Update()
    {
        HandleSlotSelection();
        UpdateSelectionOutline();
    }

    public void SetBaseSlotCount(int newCount)
    {
        baseSlotCount = Mathf.Clamp(newCount, 0, 50);
        UpdateInventoryCapacity();
    }

    private void UpdateInventoryCapacity()
    {
        int targetCapacity = baseSlotCount + equipmentController.GetBonusInventorySlots();

        while (slots.Count < targetCapacity)
        {
            Transform parentPanel = slots.Count < 5 ? hotbarPanel : panel;
            GameObject slotObj = Instantiate(slotPrefab, parentPanel);
            UniversalSlotUI newSlot = slotObj.GetComponent<UniversalSlotUI>();
            newSlot.InitializeInventorySlot(this, slots.Count);
            slots.Add(newSlot);
        }

        while (slots.Count > targetCapacity)
        {
            int lastIndex = slots.Count - 1;
            UniversalSlotUI slotToRemove = slots[lastIndex];

            if (!slotToRemove.IsEmpty())
            {
                ItemData itemToDrop = slotToRemove.Item;
                if (itemToDrop != null && itemToDrop.prefab != null && player != null)
                {
                    Instantiate(itemToDrop.prefab, player.transform.position, Quaternion.identity);
                    Debug.Log($"Предмет '{itemToDrop.itemName}' был выброшен из-за уменьшения инвентаря.");
                }
            }

            if (selectedIndex == lastIndex)
            {
                SelectSlot(-1);
            }

            Destroy(slotToRemove.gameObject);
            slots.RemoveAt(lastIndex);
        }

        GameEvents.ReportInventoryUpdated();
    }

    private void UpdateSelectionOutline()
    {
        if (selectionOutline == null) return;
        bool shouldBeVisible = selectedIndex >= 0 && selectedIndex < 5;
        selectionOutline.enabled = shouldBeVisible;

        if (shouldBeVisible)
        {
            RectTransform slotRect = slots[selectedIndex].GetComponent<RectTransform>();
            selectionOutline.rectTransform.position = slotRect.position;
            selectionOutline.rectTransform.sizeDelta = slotRect.sizeDelta;
            selectionOutline.transform.SetAsLastSibling();
        }
    }

    private void CreateSelectionOutline()
    {
        if (hotbarPanel == null) return;
        GameObject outlineObj = new GameObject("SelectionOutline");
        outlineObj.transform.SetParent(hotbarPanel, false);
        selectionOutline = outlineObj.AddComponent<Image>();
        selectionOutline.sprite = selectionOutlineSprite;
        selectionOutline.type = Image.Type.Sliced;
        selectionOutline.raycastTarget = false;
        selectionOutline.enabled = false;
        LayoutElement le = outlineObj.AddComponent<LayoutElement>();
        le.ignoreLayout = true;
    }

    public int GetItemCount(ItemData itemData)
    {
        int count = 0;
        foreach (var slot in slots)
        {
            if (slot != null && !slot.IsEmpty() && slot.Item == itemData)
            {
                count++;
            }
        }
        return count;
    }

    public bool AddItem(ItemData newItem)
    {
        foreach (var slot in slots)
        {
            if (slot != null && slot.IsEmpty())
            {
                slot.AddItem(newItem);
                GameEvents.ReportInventoryUpdated();
                return true;
            }
        }
        Debug.Log("Инвентарь полон!");
        return false;
    }

    public void RemoveItem(int slotIndex)
    {
        if (slotIndex < 0 || slotIndex >= slots.Count || slots[slotIndex] == null) return;
        slots[slotIndex].ClearSlot();
        if (slotIndex == selectedIndex)
        {
            UpdateSelectedSlot();
        }
        GameEvents.ReportInventoryUpdated();
    }

    private void HandleSlotSelection()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1)) SelectSlot(0);
        if (Input.GetKeyDown(KeyCode.Alpha2)) SelectSlot(1);
        if (Input.GetKeyDown(KeyCode.Alpha3)) SelectSlot(2);
        if (Input.GetKeyDown(KeyCode.Alpha4)) SelectSlot(3);
        if (Input.GetKeyDown(KeyCode.Alpha5)) SelectSlot(4);
    }

    public void SelectSlot(int index)
    {
        if (index < -1 || index >= 5) return;
        selectedIndex = index;
        UpdateSelectedSlot();
    }

    private void UpdateSelectedSlot()
    {
        currentItemInHand = (selectedIndex >= 0 && selectedIndex < slots.Count) ? slots[selectedIndex].Item : null;
        player?.UpdateEquippedWeapon();
    }

    public object CaptureState()
    {
        var saveData = new InventorySaveData
        {
            baseSlotCount = this.baseSlotCount,
            itemIDs = new string[slots.Count]
        };

        for (int i = 0; i < slots.Count; i++)
        {
            saveData.itemIDs[i] = (slots[i] != null && !slots[i].IsEmpty()) ? slots[i].Item.itemID : null;
        }
        return saveData;
    }

    // --- ИЗМЕНЕННЫЙ МЕТОД ЗАГРУЗКИ ---
    public void RestoreState(object state)
    {
        var saveData = ((JObject)state).ToObject<InventorySaveData>();

        // 1. Восстанавливаем базовое количество слотов
        baseSlotCount = saveData.baseSlotCount;

        // 2. Определяем, сколько всего слотов было в сохранении
        int totalSavedSlots = saveData.itemIDs.Length;

        // 3. Полностью очищаем текущие слоты, чтобы избежать дубликатов
        foreach (var slot in slots)
        {
            Destroy(slot.gameObject);
        }
        slots.Clear();

        // 4. Создаем ТОЧНО ТАКОЕ ЖЕ количество слотов, как было в сохранении
        for (int i = 0; i < totalSavedSlots; i++)
        {
            Transform parentPanel = i < 5 ? hotbarPanel : panel;
            GameObject slotObj = Instantiate(slotPrefab, parentPanel);
            UniversalSlotUI newSlot = slotObj.GetComponent<UniversalSlotUI>();
            newSlot.InitializeInventorySlot(this, i);
            slots.Add(newSlot);
        }

        // 5. Теперь, когда все слоты на месте, заполняем их предметами
        for (int i = 0; i < totalSavedSlots; i++)
        {
            ItemData item = ItemDatabase.instance.GetItemByID(saveData.itemIDs[i]);
            slots[i].AddItem(item);
        }

        GameEvents.ReportInventoryUpdated();
    }
    // --- КОНЕЦ ИЗМЕНЕНИЙ ---

    void OnDestroy()
    {
        if (equipmentController != null)
        {
            equipmentController.OnEquipmentChanged -= UpdateInventoryCapacity;
        }
    }
}
