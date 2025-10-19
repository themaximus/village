using UnityEngine;
using Newtonsoft.Json.Linq;

[RequireComponent(typeof(SaveableEntity))]
[RequireComponent(typeof(Collider2D))]
public class ItemPickup : MonoBehaviour, ISaveable
{
    [Tooltip("������ �� ������ ��������, ������� ������������ ���� ������.")]
    [SerializeField] private ItemData itemData;

    private void Awake()
    {
        GetComponent<Collider2D>().isTrigger = true;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            Inventory playerInventory = other.GetComponent<Inventory>();
            if (playerInventory != null && playerInventory.AddItem(itemData))
            {
                // ������� ���������: ����� ������������, ����������� ��������� "������" ���� ������.
                // ��� ���������� ������� ��� �� ������������ ��������� ����, ������������ ���������.
                string uniqueId = GetComponent<SaveableEntity>().GetUniqueIdentifier();
                if (!string.IsNullOrEmpty(uniqueId) && SaveManager.instance != null)
                {
                    SaveManager.instance.ForgetEntity(uniqueId);
                }

                Destroy(gameObject);
            }
        }
    }

    // --- ������ ����������/�������� ������ ������� ---
    [System.Serializable]
    private struct SaveData
    {
        public string itemID;
        public float posX, posY, posZ;
    }

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

    public void RestoreState(object state)
    {
        var saveData = ((JObject)state).ToObject<SaveData>();
        transform.position = new Vector3(saveData.posX, saveData.posY, saveData.posZ);
    }

    public ItemData GetItemData() => itemData;
}