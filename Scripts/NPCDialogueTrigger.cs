using UnityEngine;
using Yarn.Unity; // ������������ ��� Yarn Spinner

public class NPCDialogueTrigger2D : MonoBehaviour
{
    [Header("��������� NPC")]
    [Tooltip("��� ���� (node) � Yarn, � �������� ������� ������")]
    public string talkToNode = "Start";

    [Tooltip("������ ����������� ������")]
    public float interactionRadius = 3f;

    [Tooltip("UI-��������� ��� ������ (��������, ����� Press E to talk)")]
    public GameObject interactionUI;

    private DialogueRunner dialogueRunner;
    private Transform player;
    private bool playerInRange = false;
    private bool keyIsDown = false; // ��������� ����� ����������

    private void Start()
    {
        // ������� ��������� DialogueRunner � �����
        dialogueRunner = FindObjectOfType<DialogueRunner>();
        if (dialogueRunner == null)
        {
            Debug.LogError("DialogueRunner �� ������ � �����. ����������, ���������, ��� �� ��������.");
        }
        else
        {
            Debug.Log("DialogueRunner ������� ������.");
        }

        // ��������� UI-��������� � ������
        if (interactionUI != null)
        {
            interactionUI.SetActive(false);
            Debug.Log("UI-��������� ���� ���������.");
        }
    }

    private void Update()
    {
        // ���������, ��������� �� ����� � ������� � �� ��� �� ������
        if (playerInRange && !dialogueRunner.IsDialogueRunning)
        {
            // ���������� UI-���������
            if (interactionUI != null)
            {
                if (!interactionUI.activeSelf)
                {
                    interactionUI.SetActive(true);
                    Debug.Log("����� � �������. UI-��������� ��������.");
                }
            }

            // ��������� ������� ������� E ��� ������ �������
            // ���������� Input.GetKeyDown, ����� ������������� ������������ ������������
            if (Input.GetKeyDown(KeyCode.E) && !keyIsDown)
            {
                keyIsDown = true; // ������������� ����
                Debug.Log("������ ������� 'E'. ��������� ������ � ����: " + talkToNode);
                dialogueRunner.StartDialogue(talkToNode);
                // �������� UI-��������� ����� ������ �������
                if (interactionUI != null)
                {
                    interactionUI.SetActive(false);
                    Debug.Log("������ �������. UI-��������� ���������.");
                }
            }
        }
        else
        {
            // ���� ����� ����� �� ������� ��� ������ �������, �������� UI
            if (interactionUI != null && interactionUI.activeSelf)
            {
                interactionUI.SetActive(false);
                Debug.Log("����� ��� ������� ��� ������ ���. UI-��������� ���������.");
            }
        }

        // ���������� ����, ����� ������� ��������
        if (Input.GetKeyUp(KeyCode.E))
        {
            keyIsDown = false;
        }
    }

    // ���������� �������-��������� ��� ����������� ������
    private void OnTriggerEnter2D(Collider2D other)
    {
        // ���������, ��� �������� ������ ����� ��� "Player"
        if (other.CompareTag("Player"))
        {
            playerInRange = true;
            player = other.transform;
            Debug.Log("����� ����� � �������-����.");
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        // ���������, ��� �������� ������ ����� ��� "Player"
        if (other.CompareTag("Player"))
        {
            playerInRange = false;
            player = null;
            Debug.Log("����� ������� �������-����.");
            // �������� UI, ���� ����� ����
            if (interactionUI != null)
            {
                interactionUI.SetActive(false);
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        // ������ ������ �������������� � ���������
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, interactionRadius);
    }
}
