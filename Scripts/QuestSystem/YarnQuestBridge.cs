using UnityEngine;
using Yarn.Unity;
using System;

public class YarnQuestBridge : MonoBehaviour
{
    private DialogueRunner dialogueRunner;

    void Awake()
    {
        dialogueRunner = GetComponent<DialogueRunner>();
        if (dialogueRunner == null)
        {
            Debug.LogError("YarnQuestBridge requires a DialogueRunner component on the same GameObject.");
            return;
        }

        // --- РЕГИСТРАЦИЯ КОМАНД ---
        dialogueRunner.AddCommandHandler<string>("start_quest", StartQuest);
        dialogueRunner.AddCommandHandler<string>("complete_quest", CompleteQuest);

        // --- РЕГИСТРАЦИЯ ФУНКЦИЙ ---
        dialogueRunner.AddFunction("is_quest_available", (Func<string, bool>)IsQuestAvailable);
        dialogueRunner.AddFunction("is_quest_in_progress", (Func<string, bool>)IsQuestInProgress);
        dialogueRunner.AddFunction("is_quest_completed", (Func<string, bool>)IsQuestCompleted);
        dialogueRunner.AddFunction("are_quest_goals_complete", (Func<string, bool>)AreQuestGoalsComplete);
    }

    // --- РЕАЛИЗАЦИЯ КОМАНД ---

    private void StartQuest(string questID)
    {
        if (QuestManager.instance != null)
        {
            // Вызываем реальный метод из QuestManager
            QuestManager.instance.StartQuestByID(questID);
        }
    }

    private void CompleteQuest(string questID)
    {
        if (QuestManager.instance != null)
        {
            // Вызываем реальный метод из QuestManager
            QuestManager.instance.CompleteQuestByID(questID);
        }
    }

    // --- РЕАЛИЗАЦИЯ ФУНКЦИЙ ---

    private bool IsQuestAvailable(string questID)
    {
        if (QuestManager.instance != null)
        {
            // Вызываем реальный метод из QuestManager
            return QuestManager.instance.IsQuestAvailable(questID);
        }
        return false;
    }

    private bool IsQuestInProgress(string questID)
    {
        if (QuestManager.instance != null)
        {
            // Вызываем реальный метод из QuestManager
            return QuestManager.instance.IsQuestInProgress(questID);
        }
        return false;
    }

    private bool IsQuestCompleted(string questID)
    {
        if (QuestManager.instance != null)
        {
            // Вызываем реальный метод из QuestManager
            return QuestManager.instance.IsQuestCompleted(questID);
        }
        return false;
    }

    private bool AreQuestGoalsComplete(string questID)
    {
        if (QuestManager.instance != null)
        {
            // Вызываем реальный метод из QuestManager
            return QuestManager.instance.AreQuestGoalsComplete(questID);
        }
        return false;
    }
}
