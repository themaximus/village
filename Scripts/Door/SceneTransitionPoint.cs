using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// ���� ��������� �������� �� ������-������� (�����, ������),
/// ���������� �� ������� �� ������ ����� �� ������� �������.
/// </summary>
[RequireComponent(typeof(Collider2D))]
public class SceneTransitionPoint : MonoBehaviour
{
    [Header("This Point's Info")]
    [Tooltip("���������� ID ���� ����� �������� (��������, 'ForestCaveEntrance').")]
    [SerializeField] private string transitionPointID;

    [Header("Destination Info")]
    [Tooltip("��� �����, � ������� ����� ������� (��������, 'Village').")]
    [SerializeField] private string destinationSceneName;

    [Tooltip("���������� ID ����� ��������� � ����� ����� (��������, 'FromForestPath').")]
    [SerializeField] private string destinationPointID;

    [Header("Activation Settings")]
    [Tooltip("�������, ������� ����� ������ ��� ��������.")]
    [SerializeField] private KeyCode activationKey = KeyCode.E;

    [Tooltip("(�����������) ������ � ���������� (��������, '������� E'), ������� ����� ����������.")]
    [SerializeField] private GameObject interactionHint;

    // ����, ������� �����������, ��������� �� ����� ������ ��������
    private bool playerIsInside = false;

    private void Awake()
    {
        GetComponent<Collider2D>().isTrigger = true;
        // ������ ��������� ��� ������
        if (interactionHint != null)
        {
            interactionHint.SetActive(false);
        }
    }

    private void Update()
    {
        // ��������� ������� �������, ������ ���� ����� ��������� ������ ��������
        if (playerIsInside && Input.GetKeyDown(activationKey))
        {
            StartTransition();
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerIsInside = true;
            // ���������� ���������
            if (interactionHint != null)
            {
                interactionHint.SetActive(true);
            }
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerIsInside = false;
            // ������ ���������
            if (interactionHint != null)
            {
                interactionHint.SetActive(false);
            }
        }
    }

    private void StartTransition()
    {
        if (SceneTransitionManager.instance != null)
        {
            SceneTransitionManager.instance.StartTransition(destinationSceneName, destinationPointID);
        }
        else
        {
            Debug.LogError("SceneTransitionManager not found in the scene!");
        }
    }

    public string GetTransitionPointID()
    {
        return transitionPointID;
    }
}
