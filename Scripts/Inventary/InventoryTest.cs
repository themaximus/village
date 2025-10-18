using UnityEngine;

public class InventoryTest : MonoBehaviour
{
    public Inventory inventory;
    public ItemData testItem;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            inventory.AddItem(testItem);
        }
    }
}
