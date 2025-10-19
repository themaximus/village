using UnityEngine;
using UnityEngine.UI; // Необходимо для работы со Slider
using TMPro; // Необходимо для работы с TextMeshPro

public class StatsUI : MonoBehaviour
{
    [Header("Health Display")]
    [SerializeField] private TextMeshProUGUI healthText;
    [SerializeField] private Slider healthSlider;

    [Header("Thirst Display")]
    [SerializeField] private TextMeshProUGUI thirstText;
    [SerializeField] private Slider thirstSlider;

    // Сюда можно будет добавить и другие элементы (голод, рассудок и т.д.)

    // Ссылка на контроллер статов игрока
    private StatController playerStatController;

    void Start()
    {
        // Находим объект игрока по тегу "Player"
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            playerStatController = player.GetComponent<StatController>();
            if (playerStatController != null)
            {
                // Подписываемся на события изменения здоровья и жажды
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
    /// Этот метод вызывается автоматически, когда меняется здоровье игрока,
    /// и обновляет все связанные UI элементы.
    /// </summary>
    private void UpdateHealthDisplay(float currentValue, float maxValue)
    {
        // Обновляем текст, если он назначен
        if (healthText != null)
        {
            healthText.text = $"Здоровье: {(int)currentValue} / {(int)maxValue}";
        }

        // Обновляем слайдер, если он назначен
        if (healthSlider != null)
        {
            // Устанавливаем значение слайдера в диапазоне от 0 до 1
            healthSlider.value = (maxValue > 0) ? (currentValue / maxValue) : 0;
        }
    }

    /// <summary>
    /// Этот метод вызывается автоматически, когда меняется жажда игрока.
    /// </summary>
    private void UpdateThirstDisplay(float currentValue, float maxValue)
    {
        // Обновляем текст, если он назначен
        if (thirstText != null)
        {
            thirstText.text = $"Жажда: {(int)currentValue} / {(int)maxValue}";
        }

        // Обновляем слайдер, если он назначен
        if (thirstSlider != null)
        {
            thirstSlider.value = (maxValue > 0) ? (currentValue / maxValue) : 0;
        }
    }

    // Важно отписаться от событий, когда объект UI уничтожается, чтобы избежать ошибок
    void OnDestroy()
    {
        if (playerStatController != null)
        {
            playerStatController.Health.OnValueChanged -= UpdateHealthDisplay;
            playerStatController.Thirst.OnValueChanged -= UpdateThirstDisplay;
        }
    }
}
