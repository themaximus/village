using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class QuestTrigger : MonoBehaviour
{
    [Header("Quest to Start")]
    [Tooltip("Квест, который будет запущен при входе в триггер.")]
    public Quest questToStart;

    [Header("Trigger Settings")]
    [Tooltip("Если true, триггер сработает только один раз.")]
    public bool triggerOnce = true;

    private bool hasBeenTriggered = false;

    private void Awake()
    {
        // Убеждаемся, что коллайдер настроен как триггер
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
                // ИСПРАВЛЕНИЕ: Вызываем новый метод и передаем ему ID квеста
                QuestManager.instance.StartQuestByID(questToStart.questID);
            }

            hasBeenTriggered = true;
        }
    }
}
