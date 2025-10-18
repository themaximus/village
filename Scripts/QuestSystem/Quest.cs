using UnityEngine;
using System.Collections.Generic;

public enum QuestStatus { NotStarted, InProgress, Completed }

[CreateAssetMenu(fileName = "New Quest", menuName = "Quests/New Quest")]
public class Quest : ScriptableObject
{
    [Header("Quest Information")]
    [Tooltip("���������� ID ������ ��� �������� (��������, KillTheWolf)")]
    public string questID;
    public string title;
    [TextArea] public string description;

    // ������ ������ ���������, ����� �� �������� ������ QuestManager
    [HideInInspector] public QuestStatus status;

    [Header("Quest Prerequisites")]
    [Tooltip("������ �������, ������� ������ ���� ���������, ����� ���� ����� ���� ��������.")]
    public List<Quest> prerequisites;

    [Header("Quest Goals")]
    public List<QuestGoal> goals;

    [Header("Quest Rewards")]
    public int experienceReward;
    public ItemData itemReward;

    /// <summary>
    /// �������������� ��� ���� ������.
    /// </summary>
    public void Initialize()
    {
        status = QuestStatus.InProgress;
        foreach (var goal in goals)
        {
            goal.Initialize();
        }
    }

    /// <summary>
    /// ���������, ��������� �� ��� ���� ������.
    /// </summary>
    public bool CheckCompletion()
    {
        foreach (var goal in goals)
        {
            if (!goal.isCompleted)
            {
                return false;
            }
        }

        // ������ �������� �� Completed ������ ��� ����� ������
        // status = QuestStatus.Completed; 
        return true;
    }
}
