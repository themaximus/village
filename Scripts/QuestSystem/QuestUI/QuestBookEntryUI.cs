using UnityEngine;
using TMPro; // Необходимо для работы с TextMeshPro
using System.Text; // Необходимо для использования StringBuilder

public class QuestBookEntryUI : MonoBehaviour
{
    [Header("UI Elements")]
    [SerializeField] private TextMeshProUGUI titleText;
    [SerializeField] private TextMeshProUGUI descriptionText;
    [SerializeField] private TextMeshProUGUI goalsText; // Отдельное поле для целей и прогресса

    /// <summary>
    /// Заполняет эту UI-запись данными из конкретного квеста.
    /// </summary>
    /// <param name="quest">Квест, который нужно отобразить.</param>
    public void Setup(Quest quest)
    {
        // Устанавливаем заголовок
        if (titleText != null)
        {
            titleText.text = quest.title;
        }

        // Устанавливаем описание
        if (descriptionText != null)
        {
            descriptionText.text = quest.description;
        }

        // Собираем все описания целей и их прогресс в одну строку
        if (goalsText != null)
        {
            StringBuilder goalsBuilder = new StringBuilder();
            goalsBuilder.AppendLine("<b>Цели:</b>"); // Добавляем подзаголовок

            foreach (var goal in quest.goals)
            {
                // Добавляем описание цели и ее прогресс (выполненные цели будут зелеными)
                string progress = goal.isCompleted ? "<color=green>(Выполнено)</color>" : $"({goal.GetProgressText()})";
                goalsBuilder.AppendLine($"- {goal.description} {progress}");
            }
            goalsText.text = goalsBuilder.ToString();
        }
    }
}
