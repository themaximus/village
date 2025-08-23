using UnityEngine;

public class UIManager : MonoBehaviour
{
    // ����������� ������ �� ������ ���� (������� ��������), 
    // ����� ������ ������� ����� ����� �������� � ���� ������.
    public static UIManager instance;

    [Header("UI Panels")]
    public GameObject inventoryEquipmentWindow;
    public KeyCode inventoryKey = KeyCode.I;

    void Awake()
    {
        // ���������, �� ���������� �� ��� ������ ��������� UIManager
        if (instance != null && instance != this)
        {
            // ���� ��, ���������� ����, ����� ������������� ������� ������ ������
            Destroy(gameObject);
        }
        else
        {
            // ���� ���, ������ ���� ��������� ������������
            instance = this;
        }
    }

    void Update()
    {
        // ��������� ������� ������� ��� ��������/�������� ���������
        if (Input.GetKeyDown(inventoryKey))
        {
            ToggleInventoryWindow();
        }
    }

    /// <summary>
    /// ��������� ��� ��������� ���� ��������� � ����������.
    /// </summary>
    public void ToggleInventoryWindow()
    {
        if (inventoryEquipmentWindow != null)
        {
            bool isActive = !inventoryEquipmentWindow.activeSelf;
            inventoryEquipmentWindow.SetActive(isActive);

            // ������ ���� �� �����, ����� ���� �������, � ������� � �����, ����� �������.
            Time.timeScale = isActive ? 0f : 1f;
        }
    }
}
