using UnityEngine;
using UnityEngine.UI; // ���������� ��� ������ �� Slider
using TMPro; // ���������� ��� ������ � TextMeshPro

public class StatsUI : MonoBehaviour
{
    [Header("Health Display")]
    [SerializeField] private TextMeshProUGUI healthText;
    [SerializeField] private Slider healthSlider;

    [Header("Thirst Display")]
    [SerializeField] private TextMeshProUGUI thirstText;
    [SerializeField] private Slider thirstSlider;

    // ���� ����� ����� �������� � ������ �������� (�����, �������� � �.�.)

    // ������ �� ���������� ������ ������
    private StatController playerStatController;

    void Start()
    {
        // ������� ������ ������ �� ���� "Player"
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            playerStatController = player.GetComponent<StatController>();
            if (playerStatController != null)
            {
                // ������������� �� ������� ��������� �������� � �����
                playerStatController.Health.OnValueChanged += UpdateHealthDisplay;
                playerStatController.Thirst.OnValueChanged += UpdateThirstDisplay;
            }
            else
            {
                Debug.LogError("StatController not found on Player object!");
            }
        }
        else
        {
            Debug.LogError("Player object with tag 'Player' not found in the scene!");
        }
    }

    /// <summary>
    /// ���� ����� ���������� �������������, ����� �������� �������� ������,
    /// � ��������� ��� ��������� UI ��������.
    /// </summary>
    private void UpdateHealthDisplay(float currentValue, float maxValue)
    {
        // ��������� �����, ���� �� ��������
        if (healthText != null)
        {
            healthText.text = $"��������: {(int)currentValue} / {(int)maxValue}";
        }

        // ��������� �������, ���� �� ��������
        if (healthSlider != null)
        {
            // ������������� �������� �������� � ��������� �� 0 �� 1
            healthSlider.value = (maxValue > 0) ? (currentValue / maxValue) : 0;
        }
    }

    /// <summary>
    /// ���� ����� ���������� �������������, ����� �������� ����� ������.
    /// </summary>
    private void UpdateThirstDisplay(float currentValue, float maxValue)
    {
        // ��������� �����, ���� �� ��������
        if (thirstText != null)
        {
            thirstText.text = $"�����: {(int)currentValue} / {(int)maxValue}";
        }

        // ��������� �������, ���� �� ��������
        if (thirstSlider != null)
        {
            thirstSlider.value = (maxValue > 0) ? (currentValue / maxValue) : 0;
        }
    }

    // ����� ���������� �� �������, ����� ������ UI ������������, ����� �������� ������
    void OnDestroy()
    {
        if (playerStatController != null)
        {
            playerStatController.Health.OnValueChanged -= UpdateHealthDisplay;
            playerStatController.Thirst.OnValueChanged -= UpdateThirstDisplay;
        }
    }
}
