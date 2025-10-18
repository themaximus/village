using UnityEngine;
using UnityEngine.UI; // <-- Добавлено для работы с UI
using UnityEngine.SceneManagement;
using System.Collections; // <-- Добавлено для работы с корутинами

public class SceneTransitionManager : MonoBehaviour
{
    public static SceneTransitionManager instance;

    [Header("Fade Screen Settings")]
    [Tooltip("UI Image черного цвета для затемнения экрана.")]
    public Image fadeScreen;
    [Tooltip("Скорость появления/исчезновения экрана затемнения.")]
    public float fadeSpeed = 1.5f;

    private string destinationPointID;
    private Coroutine fadeCoroutine;

    void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            instance = this;
            DontDestroyOnLoad(gameObject);

            // Убеждаемся, что экран затемнения полностью прозрачен в начале
            if (fadeScreen != null)
            {
                fadeScreen.color = new Color(0, 0, 0, 0);
                fadeScreen.gameObject.SetActive(true); // Теперь экран всегда активен, мы управляем только его прозрачностью
            }
        }
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    public void StartTransition(string sceneName, string destinationID)
    {
        // Предотвращаем двойной запуск перехода
        if (fadeCoroutine == null)
        {
            this.destinationPointID = destinationID;
            fadeCoroutine = StartCoroutine(TransitionCoroutine(sceneName));
        }
    }

    private IEnumerator TransitionCoroutine(string sceneName)
    {
        // 1. Плавное затемнение экрана
        yield return StartCoroutine(Fade(1f)); // Fade In (до полной непрозрачности)

        // 2. Сохранение состояния и загрузка новой сцены
        if (SaveManager.instance != null)
        {
            SaveManager.instance.CaptureCurrentSceneState();
        }

        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName);
        // Ждем, пока сцена полностью не загрузится
        while (!asyncLoad.isDone)
        {
            yield return null;
        }

        // 3. Плавное появление новой сцены
        // OnSceneLoaded выполнится автоматически и переместит игрока
        yield return StartCoroutine(Fade(0f)); // Fade Out (до полной прозрачности)

        // 4. Завершение перехода
        fadeCoroutine = null;
    }

    private IEnumerator Fade(float targetAlpha)
    {
        if (fadeScreen == null)
        {
            Debug.LogWarning("Fade Screen не назначен в SceneTransitionManager!");
            yield break;
        }

        Color currentColor = fadeScreen.color;
        float startAlpha = currentColor.a;
        float elapsedTime = 0f;

        while (elapsedTime < 1f)
        {
            elapsedTime += Time.deltaTime * fadeSpeed;
            float newAlpha = Mathf.Lerp(startAlpha, targetAlpha, elapsedTime);
            fadeScreen.color = new Color(currentColor.r, currentColor.g, currentColor.b, newAlpha);
            yield return null;
        }

        // Убеждаемся, что в конце альфа-канал точно равен целевому значению
        fadeScreen.color = new Color(currentColor.r, currentColor.g, currentColor.b, targetAlpha);
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Перемещение игрока происходит здесь, как и раньше
        if (SaveManager.instance != null && SaveManager.instance.IsLoadingScene)
        {
            return;
        }

        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player == null) return;

        var allTransitionPoints = FindObjectsOfType<SceneTransitionPoint>();
        foreach (var point in allTransitionPoints)
        {
            if (point.GetTransitionPointID() == destinationPointID)
            {
                player.transform.position = point.transform.position;
                break;
            }
        }
    }
}
