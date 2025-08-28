using UnityEngine;
using System.Collections.Generic;

public class ItemDatabase : MonoBehaviour
{
    public static ItemDatabase instance;

    private Dictionary<string, ItemData> database = new Dictionary<string, ItemData>();

    void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            instance = this;
        }

        // Загружаем все предметы из папки Resources/Items
        var allItems = Resources.LoadAll<ItemData>("Items");
        foreach (var item in allItems)
        {
            if (!database.ContainsKey(item.itemID))
            {
                database.Add(item.itemID, item);
            }
            else
            {
                Debug.LogWarning($"Duplicate item ID found: {item.itemID}");
            }
        }
    }

    /// <summary>
    /// Находит ItemData по его уникальному ID.
    /// </summary>
    public ItemData GetItemByID(string id)
    {
        if (string.IsNullOrEmpty(id)) return null;

        database.TryGetValue(id, out ItemData item);
        return item;
    }
}
