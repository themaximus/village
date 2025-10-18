using UnityEngine;
using System.Collections.Generic; // <-- ДОБАВЛЕНО для работы со списками

public class UIManager : MonoBehaviour
{
    public static UIManager instance;

    [Header("UI Panels")]
    public GameObject inventoryEquipmentWindow;
    public CraftingUI craftingUI; // <-- ДОБАВЛЕНО: Ссылка на интерфейс крафта
    public KeyCode inventoryKey = KeyCode.I;

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
    }

    void Start()
    {
        // Убеждаемся, что окна закрыты при старте
        if (inventoryEquipmentWindow != null) inventoryEquipmentWindow.SetActive(false);
        if (craftingUI != null) craftingUI.gameObject.SetActive(false);
    }

    void Update()
    {
        if (Input.GetKeyDown(inventoryKey))
        {
            ToggleInventoryWindow();
        }
    }

    /// <summary>
    /// Открывает или закрывает окно инвентаря.
    /// Закрывает окно крафта, если оно открыто.
    /// </summary>
    public void ToggleInventoryWindow()
    {
        if (inventoryEquipmentWindow == null) return;

        bool isActive = !inventoryEquipmentWindow.activeSelf;

        // Если открываем инвентарь, убеждаемся, что крафт закрыт
        if (isActive && craftingUI != null && craftingUI.gameObject.activeSelf)
        {
            craftingUI.Close();
        }

        inventoryEquipmentWindow.SetActive(isActive);
        Time.timeScale = (isActive || (craftingUI != null && craftingUI.gameObject.activeSelf)) ? 0f : 1f;
    }

    /// <summary>
    /// Открывает или закрывает окно крафта.
    /// </summary>
    public void ToggleCraftingWindow(List<RecipeData> recipes)
    {
        if (craftingUI == null) return;

        bool isActive = !craftingUI.gameObject.activeSelf;

        if (isActive)
        {
            // Если открываем крафт, убеждаемся, что инвентарь тоже открыт (для перетаскивания)
            if (inventoryEquipmentWindow != null && !inventoryEquipmentWindow.activeSelf)
            {
                inventoryEquipmentWindow.SetActive(true);
            }
            craftingUI.Open(recipes);
        }
        else
        {
            craftingUI.Close();
        }

        Time.timeScale = (isActive || (inventoryEquipmentWindow != null && inventoryEquipmentWindow.activeSelf)) ? 0f : 1f;
    }
}
