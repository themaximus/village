using UnityEngine;
using TMPro;
using Yarn.Unity;

/// <summary>
/// ��������� ������������ ������� �������.
/// </summary>
public class DialogueHistoryManager : MonoBehaviour
{
    [Header("��������� UI")]
    [Tooltip("���������, � ������� ����� ������������ ���������. ������ ����� ��������� Vertical Layout Group.")]
    public RectTransform historyContainer;

    [Tooltip("������ ��� ������ NPC.")]
    public GameObject npcLinePrefab;

    [Tooltip("������ ��� ������ ������.")]
    public GameObject playerLinePrefab;

    [Tooltip("������ Scroll View, ������� ����� ���������� � ������������.")]
    public GameObject scrollViewObject;

    private DialogueRunner dialogueRunner;

    void Start()
    {
        dialogueRunner = FindObjectOfType<DialogueRunner>();
        if (dialogueRunner == null)
        {
            Debug.LogError("DialogueRunner �� ������ � �����.");
            return;
        }

        // ������������ ������� ��� ����������� ���� ������.
        // ������ ��� ������� ��� ���������: ��� � �����.
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

        // ��������� �������� �� ������� ������ � ���������� �������.
        dialogueRunner.onDialogueStart.AddListener(ShowHistoryContainer);
        dialogueRunner.onDialogueComplete.AddListener(ClearAndHideHistory);

        // �������� Scroll View � ����� ������, ���� �� �� �����.
        if (scrollViewObject != null)
        {
            scrollViewObject.SetActive(false);
        }
    }

    /// <summary>
    /// ������� � ��������� ����� ������� ������� � ���������.
    /// </summary>
    /// <param name="speakerName">��� ����������.</param>
    /// <param name="lineText">����� ���������.</param>
    /// <param name="prefab">������ ��� �������� ��������.</param>
    private void AddLineToHistory(string speakerName, string lineText, GameObject prefab)
    {
        // ������� ����� ������� ������� �� �������
        GameObject newLine = Instantiate(prefab, historyContainer);

        // �������� ����� ������� � ����� ��� ������
        newLine.transform.SetAsLastSibling();

        // ������� ��������� TextMeshPro � ������������� �����
        TextMeshProUGUI[] textComponents = newLine.GetComponentsInChildren<TextMeshProUGUI>();

        // ������������, ��� ������ ��������� - ��� ���, � ������ - �����
        if (textComponents.Length >= 2)
        {
            textComponents[0].text = speakerName;
            textComponents[1].text = lineText;
        }
        else if (textComponents.Length == 1)
        {
            // ���� ������ ���� ���������, �� ��� ����� ���������
            textComponents[0].text = lineText;
            Debug.LogWarning("� ������� ������ ������ ���� ��������� TextMeshProUGUI. ��� ���������� �� ����� ����������.");
        }
        else
        {
            Debug.LogError("� ������� �� ������ ��������� TextMeshProUGUI.");
        }
    }

    /// <summary>
    /// ������� ������� � �������� ���������.
    /// </summary>
    public void ClearAndHideHistory()
    {
        // ������� �������.
        foreach (Transform child in historyContainer)
        {
            Destroy(child.gameObject);
        }

        // �������� Scroll View.
        if (scrollViewObject != null)
        {
            scrollViewObject.SetActive(false);
        }
    }

    /// <summary>
    /// ���������� ��������� �������.
    /// </summary>
    public void ShowHistoryContainer()
    {
        // ���������� Scroll View.
        if (scrollViewObject != null)
        {
            scrollViewObject.SetActive(true);
        }
    }
}
