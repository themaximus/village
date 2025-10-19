using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// ��������� ��� �������-�������� � ������� ����.
/// </summary>
[RequireComponent(typeof(Collider2D))]
public class Workbench : MonoBehaviour
{
    [Header("Crafting Settings")]
    [Tooltip("������ ��������, ��������� �� ���� ��������.")]
    [SerializeField] private List<RecipeData> availableRecipes;

    [Header("Interaction Settings")]
    [Tooltip("������� ��� ��������������.")]
    [SerializeField] private KeyCode interactionKey = KeyCode.E;
    [Tooltip("UI-���������, ������� ����������, ����� ����� � ���� ��������.")]
    [SerializeField] private GameObject interactionHint;

    private bool playerIsInside = false;

    private void Awake()
    {
        GetComponent<Collider2D>().isTrigger = true;
        if (interactionHint != null)
        {
            interactionHint.SetActive(false);
        }
    }

    private void Update()
    {
        if (playerIsInside && Input.GetKeyDown(interactionKey))
        {
            OnInteract();
        }
    }

    private void OnInteract()
    {
        // ���������: �������� UIManager ��� �������� ��� �������� ����
        if (UIManager.instance != null)
        {
            UIManager.instance.ToggleCraftingWindow(availableRecipes);
        }
        else
        {
            Debug.LogError("UIManager �� ������ � �����!");
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerIsInside = true;
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
            if (interactionHint != null)
            {
                interactionHint.SetActive(false);
            }
        }
    }
}
