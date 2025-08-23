using UnityEngine;
using System.Collections.Generic;

public class QuestBookUI : MonoBehaviour
{
    [Header("UI Components")]
    [SerializeField] private GameObject questBookPanel;
    [SerializeField] private GameObject questEntryPrefab;
    [SerializeField] private Transform contentParent;
    [SerializeField] private KeyCode toggleKey = KeyCode.Tab;

    private List<GameObject> spawnedEntries = new List<GameObject>();

    void Start()
    {
        if (questBookPanel != null)
        {
            questBookPanel.SetActive(false);
        }

        if (QuestManager.instance != null)
        {
            Debug.Log("QuestBookUI Start: QuestManager ������. ������������� �� �������.");
            // 1. ������������� �� ������� ���������
            QuestManager.instance.OnActiveQuestsChanged += UpdateQuestList;

            // 2. �����������: ����� �� ��������� ������ �������� ��������
            Debug.Log("QuestBookUI Start: ����������� ��������� ������ �������...");
            UpdateQuestList(QuestManager.instance.GetActiveQuests());
        }
        else
        {
            Debug.LogError("QuestBookUI Start: QuestManager �� ������! ���������, ��� �� ���� � ����� � ��� ������ ����������� ������.");
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(toggleKey))
        {
            if (questBookPanel != null)
            {
                questBookPanel.SetActive(!questBookPanel.activeSelf);
            }
        }
    }

    private void UpdateQuestList(List<Quest> activeQuests)
    {
        Debug.Log("UpdateQuestList ������. �������� �������: " + activeQuests.Count);

        foreach (var entry in spawnedEntries)
        {
            Destroy(entry);
        }
        spawnedEntries.Clear();

        if (questEntryPrefab == null || contentParent == null)
        {
            Debug.LogWarning("QuestEntryPrefab ��� ContentParent �� ��������� � QuestBookUI.");
            return;
        }

        foreach (var quest in activeQuests)
        {
            Debug.Log("������� ������ ��� ������: " + quest.title);
            GameObject entryInstance = Instantiate(questEntryPrefab, contentParent);
            QuestBookEntryUI entryUI = entryInstance.GetComponent<QuestBookEntryUI>();
            if (entryUI != null)
            {
                entryUI.Setup(quest);
            }
            spawnedEntries.Add(entryInstance);
        }
    }

    void OnDestroy()
    {
        if (QuestManager.instance != null)
        {
            QuestManager.instance.OnActiveQuestsChanged -= UpdateQuestList;
        }
    }
}
