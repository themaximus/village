using UnityEngine;
using TMPro;
using Yarn.Unity;

/// <summary>
/// Управляет отображением истории диалога.
/// </summary>
public class DialogueHistoryManager : MonoBehaviour
{
    [Header("Настройки UI")]
    [Tooltip("Контейнер, в котором будут отображаться сообщения. Должен иметь компонент Vertical Layout Group.")]
    public RectTransform historyContainer;

    [Tooltip("Префаб для реплик NPC.")]
    public GameObject npcLinePrefab;

    [Tooltip("Префаб для реплик игрока.")]
    public GameObject playerLinePrefab;

    [Tooltip("Объект Scroll View, который будет скрываться и показываться.")]
    public GameObject scrollViewObject;

    private DialogueRunner dialogueRunner;

    void Start()
    {
        dialogueRunner = FindObjectOfType<DialogueRunner>();
        if (dialogueRunner == null)
        {
            Debug.LogError("DialogueRunner не найден в сцене.");
            return;
        }

        // Регистрируем команду для логирования всех реплик.
        // Теперь она ожидает два параметра: имя и текст.
        dialogueRunner.AddCommandHandler("AddHistory", (string[] parameters) => {
            if (parameters.Length >= 2)
            {
                string speaker = parameters[0];
                string message = parameters[1];

                if (speaker == "Player")
                {
                    AddLineToHistory(speaker, message, playerLinePrefab);
                }
                else
                {
                    AddLineToHistory(speaker, message, npcLinePrefab);
                }
            }
        });

        // Добавляем подписки на события начала и завершения диалога.
        dialogueRunner.onDialogueStart.AddListener(ShowHistoryContainer);
        dialogueRunner.onDialogueComplete.AddListener(ClearAndHideHistory);

        // Скрываем Scroll View в самом начале, если он не скрыт.
        if (scrollViewObject != null)
        {
            scrollViewObject.SetActive(false);
        }
    }

    /// <summary>
    /// Создает и добавляет новый элемент истории в контейнер.
    /// </summary>
    /// <param name="speakerName">Имя говорящего.</param>
    /// <param name="lineText">Текст сообщения.</param>
    /// <param name="prefab">Префаб для создания элемента.</param>
    private void AddLineToHistory(string speakerName, string lineText, GameObject prefab)
    {
        // Создаем новый элемент истории из префаба
        GameObject newLine = Instantiate(prefab, historyContainer);

        // Помещаем новый элемент в самый низ списка
        newLine.transform.SetAsLastSibling();

        // Находим компонент TextMeshPro и устанавливаем текст
        TextMeshProUGUI[] textComponents = newLine.GetComponentsInChildren<TextMeshProUGUI>();

        // Предполагаем, что первый компонент - это имя, а второй - текст
        if (textComponents.Length >= 2)
        {
            textComponents[0].text = speakerName;
            textComponents[1].text = lineText;
        }
        else if (textComponents.Length == 1)
        {
            // Если только один компонент, то это текст сообщения
            textComponents[0].text = lineText;
            Debug.LogWarning("В префабе найден только один компонент TextMeshProUGUI. Имя говорящего не будет отображено.");
        }
        else
        {
            Debug.LogError("В префабе не найден компонент TextMeshProUGUI.");
        }
    }

    /// <summary>
    /// Очищает историю и скрывает контейнер.
    /// </summary>
    public void ClearAndHideHistory()
    {
        // Очищаем историю.
        foreach (Transform child in historyContainer)
        {
            Destroy(child.gameObject);
        }

        // Скрываем Scroll View.
        if (scrollViewObject != null)
        {
            scrollViewObject.SetActive(false);
        }
    }

    /// <summary>
    /// Показывает контейнер истории.
    /// </summary>
    public void ShowHistoryContainer()
    {
        // Показываем Scroll View.
        if (scrollViewObject != null)
        {
            scrollViewObject.SetActive(true);
        }
    }
}
