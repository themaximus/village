using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;

[RequireComponent(typeof(EquipmentController))]
public class Inventory : MonoBehaviour, ISaveable
{
    [Header("Inventory Settings")]
    public GameObject slotPrefab;
    public int baseSlotCount = 10;
    public Transform panel;
    public Transform hotbarPanel;
    public Sprite selectionOutlineSprite;

    private MovePlayer player;
    private EquipmentController equipmentController;

    private List<UniversalSlotUI> slots = new List<UniversalSlotUI>();

    [HideInInspector] public int selectedIndex = -1;
    [HideInInspector] public ItemData currentItemInHand;

    private Image selectionOutline;

    public int SelectedIndex => selectedIndex;

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
    }

    void Update()
    {
        HandleSlotSelection();
        UpdateSelectionOutline();
    }

    private void UpdateSelectionOutline()
    {
        if (selectionOutline == null)
        {
            CreateSelectionOutline();
            if (selectionOutline == null) return;
        }

        if (selectedIndex >= 0 && selectedIndex < slots.Count && slots[selectedIndex] != null)
        {
            if (selectedIndex < 5)
            {
                selectionOutline.enabled = true;
                RectTransform slotRect = slots[selectedIndex].GetComponent<RectTransform>();
                selectionOutline.rectTransform.position = slotRect.position;
                selectionOutline.rectTransform.sizeDelta = slotRect.sizeDelta;

                // --- ИСПРАВЛЕНИЕ ---
                // Принудительно перемещаем рамку на передний план, чтобы она
                // всегда рисовалась поверх слотов.
                selectionOutline.transform.SetAsLastSibling();
                // -------------------
            }
            else
            {
                selectionOutline.enabled = false;
            }
        }
        else
        {
            selectionOutline.enabled = false;
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

    public void HandleItemDrop(UniversalSlotUI sourceSlot, UniversalSlotUI targetSlot)
    {
        if (targetSlot != null && targetSlot != sourceSlot)
        {
            ItemData sourceItem = sourceSlot.Item;
            ItemData destinationItem = targetSlot.Item;
            sourceSlot.AddItem(destinationItem);
            targetSlot.AddItem(sourceItem);
        }
        else if (targetSlot == null)
        {
            ItemData itemToDrop = sourceSlot.Item;
            if (itemToDrop != null && itemToDrop.prefab != null)
            {
                Vector3 mousePos = Input.mousePosition;
                mousePos.z = Camera.main.nearClipPlane + 10f;
                Vector3 worldPos = Camera.main.ScreenToWorldPoint(mousePos);
                worldPos.z = 0;
                Instantiate(itemToDrop.prefab, worldPos, Quaternion.identity);
            }
            sourceSlot.ClearSlot();
        }
        GameEvents.ReportInventoryUpdated();
    }

    private void UpdateInventoryCapacity()
    {
        int targetCapacity = baseSlotCount + equipmentController.GetBonusInventorySlots();

        while (slots.Count < targetCapacity)
        {
            Transform parentPanel = slots.Count < 5 ? hotbarPanel : panel;
            GameObject slotObj = Instantiate(slotPrefab, parentPanel);
            UniversalSlotUI newSlot = slotObj.GetComponent<UniversalSlotUI>();
            newSlot.Initialize(this, slots.Count);
            slots.Add(newSlot);
        }

        while (slots.Count > targetCapacity)
        {
            int lastIndex = slots.Count - 1;
            UniversalSlotUI slotToRemove = slots[lastIndex];

            if (selectedIndex == lastIndex)
            {
                selectedIndex = -1;
            }

            Destroy(slotToRemove.gameObject);
            slots.RemoveAt(lastIndex);
        }

        if (selectedIndex >= slots.Count)
        {
            selectedIndex = -1;
            UpdateSelectedSlot();
        }

        GameEvents.ReportInventoryUpdated();
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
            currentItemInHand = null;
            if (player != null) player.UpdateEquippedWeapon();
        }
        GameEvents.ReportInventoryUpdated();
    }

    private void HandleSlotSelection()
    {
        // --- ДИАГНОСТИКА ---
        if (Input.GetKeyDown(KeyCode.Alpha1)) { Debug.Log("Нажата клавиша 1"); SelectSlot(0); }
        if (Input.GetKeyDown(KeyCode.Alpha2)) { Debug.Log("Нажата клавиша 2"); SelectSlot(1); }
        if (Input.GetKeyDown(KeyCode.Alpha3)) { Debug.Log("Нажата клавиша 3"); SelectSlot(2); }
        if (Input.GetKeyDown(KeyCode.Alpha4)) { Debug.Log("Нажата клавиша 4"); SelectSlot(3); }
        if (Input.GetKeyDown(KeyCode.Alpha5)) { Debug.Log("Нажата клавиша 5"); SelectSlot(4); }
        // -------------------
    }

    public void SelectSlot(int index)
    {
        if (index < 0 || index >= slots.Count) return;

        if (selectedIndex == index)
        {
            selectedIndex = -1;
            Debug.Log($"Слот {index} отменен."); // ДИАГНОСТИКА
        }
        else
        {
            selectedIndex = index;
            Debug.Log($"Выбран слот {index}."); // ДИАГНОСТИКА
        }
        UpdateSelectedSlot();
    }

    public void UpdateSelectedSlot()
    {
        if (selectedIndex < 0 || selectedIndex >= slots.Count || slots[selectedIndex] == null)
        {
            currentItemInHand = null;
        }
        else
        {
            currentItemInHand = slots[selectedIndex].Item;
        }

        if (player != null)
            player.UpdateEquippedWeapon();
    }

    public object CaptureState()
    {
        var slotItemIDs = new string[slots.Count];
        for (int i = 0; i < slots.Count; i++)
        {
            if (slots[i] == null || slots[i].IsEmpty())
            {
                slotItemIDs[i] = null;
            }
            else
            {
                slotItemIDs[i] = slots[i].Item.itemID;
            }
        }
        return slotItemIDs;
    }

    public void RestoreState(object state)
    {
        var slotItemIDs = ((JArray)state).ToObject<string[]>();

        if (slots.Count < slotItemIDs.Length)
        {
            Debug.LogWarning("Saved inventory has more slots than current capacity. Some items may not be restored.");
        }

        for (int i = 0; i < slots.Count; i++)
        {
            if (i < slotItemIDs.Length)
            {
                ItemData item = ItemDatabase.instance.GetItemByID(slotItemIDs[i]);
                slots[i].AddItem(item);
            }
            else
            {
                slots[i].ClearSlot();
            }
        }
        GameEvents.ReportInventoryUpdated();
    }

    void OnDestroy()
    {
        if (equipmentController != null)
        {
            equipmentController.OnEquipmentChanged -= UpdateInventoryCapacity;
        }
    }
}
