using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System;

public class QuestManager : MonoBehaviour
{
    public static QuestManager instance;

    private Dictionary<string, Quest> questDatabase = new Dictionary<string, Quest>();

    [Header("Quest Log")]
    [SerializeField] private List<Quest> activeQuests = new List<Quest>();
    [SerializeField] private List<Quest> completedQuests = new List<Quest>();

    public event Action<List<Quest>> OnActiveQuestsChanged;

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

        var allQuests = Resources.LoadAll<Quest>("Quests");
        foreach (var quest in allQuests)
        {
            quest.status = QuestStatus.NotStarted;
            if (!questDatabase.ContainsKey(quest.questID))
            {
                questDatabase.Add(quest.questID, quest);
            }
            else
            {
                Debug.LogWarning($"Duplicate quest ID found: {quest.questID}");
            }
        }
    }

    // Метод Update был полностью удален, так как он больше не нужен
    // в новой событийно-ориентированной архитектуре.

    // --- Методы для Yarn Spinner ---

    public void StartQuestByID(string questID)
    {
        Quest quest = FindQuestByID(questID);
        if (quest != null && !activeQuests.Contains(quest) && !completedQuests.Contains(quest))
        {
            quest.Initialize();
            activeQuests.Add(quest);
            OnActiveQuestsChanged?.Invoke(activeQuests);
        }
    }

    public void CompleteQuestByID(string questID)
    {
        Quest quest = FindQuestByID(questID);
        if (quest != null && activeQuests.Contains(quest))
        {
            if (quest.CheckCompletion())
            {
                quest.status = QuestStatus.Completed;
                activeQuests.Remove(quest);
                completedQuests.Add(quest);
                OnActiveQuestsChanged?.Invoke(activeQuests);
                Debug.Log($"Quest '{quest.title}' has been completed and turned in.");
            }
            else
            {
                Debug.LogWarning($"Attempted to complete quest '{quest.title}', but its goals are not met.");
            }
        }
    }

    public bool IsQuestAvailable(string questID)
    {
        Quest quest = FindQuestByID(questID);
        if (quest == null || activeQuests.Contains(quest) || completedQuests.Contains(quest))
        {
            return false;
        }
        return quest.prerequisites.All(p => p.status == QuestStatus.Completed);
    }

    public bool IsQuestInProgress(string questID)
    {
        Quest quest = FindQuestByID(questID);
        return quest != null && activeQuests.Contains(quest);
    }

    public bool IsQuestCompleted(string questID)
    {
        Quest quest = FindQuestByID(questID);
        return quest != null && completedQuests.Contains(quest);
    }

    public bool AreQuestGoalsComplete(string questID)
    {
        Quest quest = FindQuestByID(questID);
        return quest != null && activeQuests.Contains(quest) && quest.CheckCompletion();
    }

    // --- Вспомогательные методы ---

    public List<Quest> GetActiveQuests()
    {
        return activeQuests;
    }

    private Quest FindQuestByID(string questID)
    {
        questDatabase.TryGetValue(questID, out Quest quest);
        if (quest == null)
        {
            Debug.LogError($"Quest with ID '{questID}' not found in the database.");
        }
        return quest;
    }

    // Методы OnEnemyKilled и CheckGatherGoals теперь УДАЛЕНЫ, так как они больше не нужны.
}
