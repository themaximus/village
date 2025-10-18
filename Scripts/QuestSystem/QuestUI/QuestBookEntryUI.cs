using UnityEngine;
using TMPro; // ���������� ��� ������ � TextMeshPro
using System.Text; // ���������� ��� ������������� StringBuilder

public class QuestBookEntryUI : MonoBehaviour
{
    [Header("UI Elements")]
    [SerializeField] private TextMeshProUGUI titleText;
    [SerializeField] private TextMeshProUGUI descriptionText;
    [SerializeField] private TextMeshProUGUI goalsText; // ��������� ���� ��� ����� � ���������

    /// <summary>
    /// ��������� ��� UI-������ ������� �� ����������� ������.
    /// </summary>
    /// <param name="quest">�����, ������� ����� ����������.</param>
    public void Setup(Quest quest)
    {
        // ������������� ���������
        if (titleText != null)
        {
            titleText.text = quest.title;
        }

        // ������������� ��������
        if (descriptionText != null)
        {
            descriptionText.text = quest.description;
        }

        // �������� ��� �������� ����� � �� �������� � ���� ������
        if (goalsText != null)
        {
            StringBuilder goalsBuilder = new StringBuilder();
            goalsBuilder.AppendLine("<b>����:</b>"); // ��������� ������������

            foreach (var goal in quest.goals)
            {
                // ��������� �������� ���� � �� �������� (����������� ���� ����� ��������)
                string progress = goal.isCompleted ? "<color=green>(���������)</color>" : $"({goal.GetProgressText()})";
                goalsBuilder.AppendLine($"- {goal.description} {progress}");
            }
            goalsText.text = goalsBuilder.ToString();
        }
    }
}
