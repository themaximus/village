using UnityEngine;

public class UIManager : MonoBehaviour
{
    // Статическая ссылка на самого себя (паттерн Синглтон), 
    // чтобы другие скрипты могли легко получить к нему доступ.
    public static UIManager instance;

    [Header("UI Panels")]
    public GameObject inventoryEquipmentWindow;
    public KeyCode inventoryKey = KeyCode.I;

    void Awake()
    {
        // Проверяем, не существует ли уже другой экземпляр UIManager
        if (instance != null && instance != this)
        {
            // Если да, уничтожаем этот, чтобы гарантировать наличие только одного
            Destroy(gameObject);
        }
        else
        {
            // Если нет, делаем этот экземпляр единственным
            instance = this;
        }
    }

    void Update()
    {
        // Проверяем нажатие клавиши для открытия/закрытия инвентаря
        if (Input.GetKeyDown(inventoryKey))
        {
            ToggleInventoryWindow();
        }
    }

    /// <summary>
    /// Открывает или закрывает окно инвентаря и снаряжения.
    /// </summary>
    public void ToggleInventoryWindow()
    {
        if (inventoryEquipmentWindow != null)
        {
            bool isActive = !inventoryEquipmentWindow.activeSelf;
            inventoryEquipmentWindow.SetActive(isActive);

            // Ставим игру на паузу, когда окно открыто, и снимаем с паузы, когда закрыто.
            Time.timeScale = isActive ? 0f : 1f;
        }
    }
}
