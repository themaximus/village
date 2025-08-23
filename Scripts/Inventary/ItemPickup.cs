using UnityEngine;
using Newtonsoft.Json.Linq;

/// <summary>
/// Универсальный компонент для подбираемых предметов.
/// Отвечает и за игровую логику (подбор игроком), и за сохранение/загрузку.
/// </summary>
[RequireComponent(typeof(SaveableEntity))]
[RequireComponent(typeof(Collider2D))]
public class ItemPickup : MonoBehaviour, ISaveable
{
    [Tooltip("Ссылка на данные предмета, который представляет этот объект.")]
    [SerializeField] private ItemData itemData;

    private void Awake()
    {
        // Убеждаемся, что коллайдер предмета является триггером
        GetComponent<Collider2D>().isTrigger = true;
    }

    // --- ИГРОВАЯ ЛОГИКА: ПОДБОР ПРЕДМЕТА ---

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Проверяем, что с предметом столкнулся игрок
        if (other.CompareTag("Player"))
        {
            // Находим инвентарь игрока
            Inventory playerInventory = other.GetComponent<Inventory>();
            if (playerInventory != null)
            {
                // Если предмет успешно добавлен в инвентарь...
                if (playerInventory.AddItem(itemData))
                {
                    // ...уничтожаем этот объект со сцены.
                    Destroy(gameObject);
                }
            }
        }
    }

    // --- ЛОГИКА СОХРАНЕНИЯ: РЕАЛИЗАЦИЯ ISaveable ---

    // Вспомогательная структура для хранения данных
    [System.Serializable]
    private struct SaveData
    {
        public string itemID;
        public float posX, posY, posZ;
    }

    /// <summary>
    /// "Фотографирует" ID предмета и его позицию.
    /// </summary>
    public object CaptureState()
    {
        if (itemData == null) return null;

        return new SaveData
        {
            itemID = itemData.itemID,
            posX = transform.position.x,
            posY = transform.position.y,
            posZ = transform.position.z
        };
    }

    /// <summary>
    /// Восстанавливает позицию предмета.
    /// </summary>
    public void RestoreState(object state)
    {
        var saveData = ((JObject)state).ToObject<SaveData>();
        transform.position = new Vector3(saveData.posX, saveData.posY, saveData.posZ);
    }

    // Публичный метод, чтобы другие скрипты могли узнать, какой это предмет
    public ItemData GetItemData()
    {
        return itemData;
    }
}
