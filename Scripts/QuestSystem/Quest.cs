using UnityEngine;
using System.Collections.Generic;

public enum QuestStatus { NotStarted, InProgress, Completed }

[CreateAssetMenu(fileName = "New Quest", menuName = "Quests/New Quest")]
public class Quest : ScriptableObject
{
    [Header("Quest Information")]
    [Tooltip("”никальный ID квеста без пробелов (например, KillTheWolf)")]
    public string questID;
    public string title;
    [TextArea] public string description;

    // —татус теперь приватный, чтобы им управл€л только QuestManager
    [HideInInspector] public QuestStatus status;

    [Header("Quest Prerequisites")]
    [Tooltip("—писок квестов, которые должны быть выполнены, чтобы этот квест стал доступен.")]
    public List<Quest> prerequisites;

    [Header("Quest Goals")]
    public List<QuestGoal> goals;

    [Header("Quest Rewards")]
    public int experienceReward;
    public ItemData itemReward;

    /// <summary>
    /// »нициализирует все цели квеста.
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
    /// ѕровер€ет, выполнены ли все цели квеста.
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

        // —татус мен€етс€ на Completed только при сдаче квеста
        // status = QuestStatus.Completed; 
        return true;
    }
}
