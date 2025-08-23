using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Управляет процессом перехода между сценами.
/// Этот объект не уничтожается при загрузке новых сцен.
/// </summary>
public class SceneTransitionManager : MonoBehaviour
{
    public static SceneTransitionManager instance;

    // ID точки, в которой должен появиться игрок в новой сцене
    private string destinationPointID;

    void Awake()
    {
        // Настраиваем синглтон
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            instance = this;
            DontDestroyOnLoad(gameObject); // Делаем этот объект "вечным"
        }
    }

    // Подписываемся на событие загрузки сцены, когда объект включается
    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    // Отписываемся, когда выключается
    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    /// <summary>
    /// Начинает процесс перехода на другую сцену.
    /// </summary>
    /// <param name="sceneName">Имя сцены для загрузки.</param>
    /// <param name="destinationID">ID точки появления в новой сцене.</param>
    public void StartTransition(string sceneName, string destinationID)
    {
        // Сохраняем, где должен появиться игрок
        this.destinationPointID = destinationID;

        // --- КЛЮЧЕВОЕ ИЗМЕНЕНИЕ ---
        // Перед уходом со сцены, просим SaveManager сохранить ее состояние в "оперативную память"
        if (SaveManager.instance != null)
        {
            SaveManager.instance.SaveSceneStateToMemory();
        }
        // -------------------------

        // TODO: Здесь будет логика затемнения экрана (fade-out)

        // Загружаем новую сцену
        SceneManager.LoadScene(sceneName);
    }

    /// <summary>
    /// Этот метод вызывается автоматически после того, как новая сцена полностью загрузилась.
    /// </summary>
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Находим объект игрока
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player == null)
        {
            // Не выводим ошибку, если это сцена-загрузчик, в которой еще нет игрока
            if (scene.name != "Initializer")
            {
                Debug.LogError("Player object with tag 'Player' not found in the new scene!");
            }
            return;
        }

        // Находим все точки перехода в новой сцене
        var allTransitionPoints = FindObjectsOfType<SceneTransitionPoint>();
        SceneTransitionPoint targetPoint = null;

        // Ищем среди них ту, у которой ID совпадает с нашим ID назначения
        foreach (var point in allTransitionPoints)
        {
            if (point.GetTransitionPointID() == destinationPointID)
            {
                targetPoint = point;
                break;
            }
        }

        if (targetPoint != null)
        {
            // Перемещаем игрока в позицию найденной точки
            player.transform.position = targetPoint.transform.position;
            Debug.Log($"Player moved to transition point: {destinationPointID}");
        }
        else
        {
            Debug.LogWarning($"No transition point found with ID: '{destinationPointID}' in scene '{scene.name}'");
        }

        // TODO: Здесь будет логика появления экрана (fade-in)
    }
}
