using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class QuestTrigger : MonoBehaviour
{
    [Header("Quest to Start")]
    [Tooltip("�����, ������� ����� ������� ��� ����� � �������.")]
    public Quest questToStart;

    [Header("Trigger Settings")]
    [Tooltip("���� true, ������� ��������� ������ ���� ���.")]
    public bool triggerOnce = true;

    private bool hasBeenTriggered = false;

    private void Awake()
    {
        // ����������, ��� ��������� �������� ��� �������
        GetComponent<Collider2D>().isTrigger = true;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (triggerOnce && hasBeenTriggered)
        {
            return;
        }

        if (other.CompareTag("Player"))
        {
            Debug.Log("Player entered quest trigger for: " + questToStart.title);

            if (QuestManager.instance != null)
            {
                // �����������: �������� ����� ����� � �������� ��� ID ������
                QuestManager.instance.StartQuestByID(questToStart.questID);
            }

            hasBeenTriggered = true;
        }
    }
}
