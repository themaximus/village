using UnityEngine;
using UnityEngine.UI; // <-- ��������� ��� ������ � UI
using UnityEngine.SceneManagement;
using System.Collections; // <-- ��������� ��� ������ � ����������

public class SceneTransitionManager : MonoBehaviour
{
    public static SceneTransitionManager instance;

    [Header("Fade Screen Settings")]
    [Tooltip("UI Image ������� ����� ��� ���������� ������.")]
    public Image fadeScreen;
    [Tooltip("�������� ���������/������������ ������ ����������.")]
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

            // ����������, ��� ����� ���������� ��������� ��������� � ������
            if (fadeScreen != null)
            {
                fadeScreen.color = new Color(0, 0, 0, 0);
                fadeScreen.gameObject.SetActive(true); // ������ ����� ������ �������, �� ��������� ������ ��� �������������
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
        // ������������� ������� ������ ��������
        if (fadeCoroutine == null)
        {
            this.destinationPointID = destinationID;
            fadeCoroutine = StartCoroutine(TransitionCoroutine(sceneName));
        }
    }

    private IEnumerator TransitionCoroutine(string sceneName)
    {
        // 1. ������� ���������� ������
        yield return StartCoroutine(Fade(1f)); // Fade In (�� ������ ��������������)

        // 2. ���������� ��������� � �������� ����� �����
        if (SaveManager.instance != null)
        {
            SaveManager.instance.CaptureCurrentSceneState();
        }

        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName);
        // ����, ���� ����� ��������� �� ����������
        while (!asyncLoad.isDone)
        {
            yield return null;
        }

        // 3. ������� ��������� ����� �����
        // OnSceneLoaded ���������� ������������� � ���������� ������
        yield return StartCoroutine(Fade(0f)); // Fade Out (�� ������ ������������)

        // 4. ���������� ��������
        fadeCoroutine = null;
    }

    private IEnumerator Fade(float targetAlpha)
    {
        if (fadeScreen == null)
        {
            Debug.LogWarning("Fade Screen �� �������� � SceneTransitionManager!");
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

        // ����������, ��� � ����� �����-����� ����� ����� �������� ��������
        fadeScreen.color = new Color(currentColor.r, currentColor.g, currentColor.b, targetAlpha);
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // ����������� ������ ���������� �����, ��� � ������
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
