using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneTransitionManager : MonoBehaviour
{
    public static SceneTransitionManager instance;
    private string destinationPointID;

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
        this.destinationPointID = destinationID;

        if (SaveManager.instance != null)
        {
            SaveManager.instance.CaptureCurrentSceneState();
        }

        SceneManager.LoadScene(sceneName);
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
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