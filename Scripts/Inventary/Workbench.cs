using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Компонент для объекта-верстака в игровом мире.
/// </summary>
[RequireComponent(typeof(Collider2D))]
public class Workbench : MonoBehaviour
{
    [Header("Crafting Settings")]
    [Tooltip("Список рецептов, доступных на этом верстаке.")]
    [SerializeField] private List<RecipeData> availableRecipes;

    [Header("Interaction Settings")]
    [Tooltip("Клавиша для взаимодействия.")]
    [SerializeField] private KeyCode interactionKey = KeyCode.E;
    [Tooltip("UI-подсказка, которая появляется, когда игрок в зоне действия.")]
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
        // ОБНОВЛЕНО: Вызываем UIManager для открытия или закрытия окна
        if (UIManager.instance != null)
        {
            UIManager.instance.ToggleCraftingWindow(availableRecipes);
        }
        else
        {
            Debug.LogError("UIManager не найден в сцене!");
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
