using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Этот компонент вешается на объект-триггер (дверь, портал),
/// отвечающий за переход на другую сцену по нажатию клавиши.
/// </summary>
[RequireComponent(typeof(Collider2D))]
public class SceneTransitionPoint : MonoBehaviour
{
    [Header("This Point's Info")]
    [Tooltip("Уникальный ID этой точки перехода (например, 'ForestCaveEntrance').")]
    [SerializeField] private string transitionPointID;

    [Header("Destination Info")]
    [Tooltip("Имя сцены, в которую нужно перейти (например, 'Village').")]
    [SerializeField] private string destinationSceneName;

    [Tooltip("Уникальный ID точки появления в новой сцене (например, 'FromForestPath').")]
    [SerializeField] private string destinationPointID;

    [Header("Activation Settings")]
    [Tooltip("Клавиша, которую нужно нажать для перехода.")]
    [SerializeField] private KeyCode activationKey = KeyCode.E;

    [Tooltip("(Опционально) Объект с подсказкой (например, 'Нажмите E'), который будет появляться.")]
    [SerializeField] private GameObject interactionHint;

    // Флаг, который отслеживает, находится ли игрок внутри триггера
    private bool playerIsInside = false;

    private void Awake()
    {
        GetComponent<Collider2D>().isTrigger = true;
        // Прячем подсказку при старте
        if (interactionHint != null)
        {
            interactionHint.SetActive(false);
        }
    }

    private void Update()
    {
        // Проверяем нажатие клавиши, только если игрок находится внутри триггера
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
            // Показываем подсказку
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
            // Прячем подсказку
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
