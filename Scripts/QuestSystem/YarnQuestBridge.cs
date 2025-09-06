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

        // --- ����������� ������ ---
        dialogueRunner.AddCommandHandler<string>("start_quest", StartQuest);
        dialogueRunner.AddCommandHandler<string>("complete_quest", CompleteQuest);

        // --- ����������� ������� ---
        dialogueRunner.AddFunction("is_quest_available", (Func<string, bool>)IsQuestAvailable);
        dialogueRunner.AddFunction("is_quest_in_progress", (Func<string, bool>)IsQuestInProgress);
        dialogueRunner.AddFunction("is_quest_completed", (Func<string, bool>)IsQuestCompleted);
        dialogueRunner.AddFunction("are_quest_goals_complete", (Func<string, bool>)AreQuestGoalsComplete);
    }

    // --- ���������� ������ ---

    private void StartQuest(string questID)
    {
        if (QuestManager.instance != null)
        {
            // �������� �������� ����� �� QuestManager
            QuestManager.instance.StartQuestByID(questID);
        }
    }

    private void CompleteQuest(string questID)
    {
        if (QuestManager.instance != null)
        {
            // �������� �������� ����� �� QuestManager
            QuestManager.instance.CompleteQuestByID(questID);
        }
    }

    // --- ���������� ������� ---

    private bool IsQuestAvailable(string questID)
    {
        if (QuestManager.instance != null)
        {
            // �������� �������� ����� �� QuestManager
            return QuestManager.instance.IsQuestAvailable(questID);
        }
        return false;
    }

    private bool IsQuestInProgress(string questID)
    {
        if (QuestManager.instance != null)
        {
            // �������� �������� ����� �� QuestManager
            return QuestManager.instance.IsQuestInProgress(questID);
        }
        return false;
    }

    private bool IsQuestCompleted(string questID)
    {
        if (QuestManager.instance != null)
        {
            // �������� �������� ����� �� QuestManager
            return QuestManager.instance.IsQuestCompleted(questID);
        }
        return false;
    }

    private bool AreQuestGoalsComplete(string questID)
    {
        if (QuestManager.instance != null)
        {
            // �������� �������� ����� �� QuestManager
            return QuestManager.instance.AreQuestGoalsComplete(questID);
        }
        return false;
    }
}
