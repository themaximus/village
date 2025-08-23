using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "NewItem", menuName = "Inventory/Item")]
public class ItemData : ScriptableObject
{
    [Header("Item Info")]
    [Tooltip("”никальный ID предмета без пробелов (например, healing_potion)")]
    public string itemID;
    public string itemName;
    public Sprite icon;
    public bool isConsumable = true;

    [Header("Prefab on Ground")]
    public GameObject prefab;

    [Header("Item Actions")]
    public List<ItemAction> actions;
}
