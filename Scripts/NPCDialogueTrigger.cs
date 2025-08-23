using UnityEngine;
using Yarn.Unity; // пространство имён Yarn Spinner

public class NPCDialogueTrigger2D : MonoBehaviour
{
    [Header("Настройки NPC")]
    [Tooltip("Имя узла (node) в Yarn, с которого начнётся диалог")]
    public string talkToNode = "Start";

    [Tooltip("Радиус обнаружения игрока")]
    public float interactionRadius = 3f;

    [Tooltip("UI-подсказка для игрока (например, текст Press E to talk)")]
    public GameObject interactionUI;

    private DialogueRunner dialogueRunner;
    private Transform player;
    private bool playerInRange = false;
    private bool keyIsDown = false; // Добавляем новую переменную

    private void Start()
    {
        // Находим компонент DialogueRunner в сцене
        dialogueRunner = FindObjectOfType<DialogueRunner>();
        if (dialogueRunner == null)
        {
            Debug.LogError("DialogueRunner не найден в сцене. Пожалуйста, убедитесь, что он добавлен.");
        }
        else
        {
            Debug.Log("DialogueRunner успешно найден.");
        }

        // Выключаем UI-подсказку в начале
        if (interactionUI != null)
        {
            interactionUI.SetActive(false);
            Debug.Log("UI-подсказка была отключена.");
        }
    }

    private void Update()
    {
        // Проверяем, находится ли игрок в радиусе и не идёт ли диалог
        if (playerInRange && !dialogueRunner.IsDialogueRunning)
        {
            // Показываем UI-подсказку
            if (interactionUI != null)
            {
                if (!interactionUI.activeSelf)
                {
                    interactionUI.SetActive(true);
                    Debug.Log("Игрок в радиусе. UI-подсказка включена.");
                }
            }

            // Проверяем нажатие клавиши E для начала диалога
            // Используем Input.GetKeyDown, чтобы предотвратить многократное срабатывание
            if (Input.GetKeyDown(KeyCode.E) && !keyIsDown)
            {
                keyIsDown = true; // Устанавливаем флаг
                Debug.Log("Нажата клавиша 'E'. Запускаем диалог с узла: " + talkToNode);
                dialogueRunner.StartDialogue(talkToNode);
                // Скрываем UI-подсказку после начала диалога
                if (interactionUI != null)
                {
                    interactionUI.SetActive(false);
                    Debug.Log("Диалог начался. UI-подсказка отключена.");
                }
            }
        }
        else
        {
            // Если игрок вышел из радиуса или диалог начался, скрываем UI
            if (interactionUI != null && interactionUI.activeSelf)
            {
                interactionUI.SetActive(false);
                Debug.Log("Игрок вне радиуса или диалог идёт. UI-подсказка отключена.");
            }
        }

        // Сбрасываем флаг, когда клавиша отпущена
        if (Input.GetKeyUp(KeyCode.E))
        {
            keyIsDown = false;
        }
    }

    // Используем триггер-коллайдер для обнаружения игрока
    private void OnTriggerEnter2D(Collider2D other)
    {
        // Проверяем, что вошедший объект имеет тег "Player"
        if (other.CompareTag("Player"))
        {
            playerInRange = true;
            player = other.transform;
            Debug.Log("Игрок вошел в триггер-зону.");
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        // Проверяем, что вышедший объект имеет тег "Player"
        if (other.CompareTag("Player"))
        {
            playerInRange = false;
            player = null;
            Debug.Log("Игрок покинул триггер-зону.");
            // Скрываем UI, если игрок ушёл
            if (interactionUI != null)
            {
                interactionUI.SetActive(false);
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        // Рисуем радиус взаимодействия в редакторе
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, interactionRadius);
    }
}
