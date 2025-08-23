using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// ��������� ��������� �������� ����� �������.
/// ���� ������ �� ������������ ��� �������� ����� ����.
/// </summary>
public class SceneTransitionManager : MonoBehaviour
{
    public static SceneTransitionManager instance;

    // ID �����, � ������� ������ ��������� ����� � ����� �����
    private string destinationPointID;

    void Awake()
    {
        // ����������� ��������
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            instance = this;
            DontDestroyOnLoad(gameObject); // ������ ���� ������ "������"
        }
    }

    // ������������� �� ������� �������� �����, ����� ������ ����������
    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    // ������������, ����� �����������
    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    /// <summary>
    /// �������� ������� �������� �� ������ �����.
    /// </summary>
    /// <param name="sceneName">��� ����� ��� ��������.</param>
    /// <param name="destinationID">ID ����� ��������� � ����� �����.</param>
    public void StartTransition(string sceneName, string destinationID)
    {
        // ���������, ��� ������ ��������� �����
        this.destinationPointID = destinationID;

        // --- �������� ��������� ---
        // ����� ������ �� �����, ������ SaveManager ��������� �� ��������� � "����������� ������"
        if (SaveManager.instance != null)
        {
            SaveManager.instance.SaveSceneStateToMemory();
        }
        // -------------------------

        // TODO: ����� ����� ������ ���������� ������ (fade-out)

        // ��������� ����� �����
        SceneManager.LoadScene(sceneName);
    }

    /// <summary>
    /// ���� ����� ���������� ������������� ����� ����, ��� ����� ����� ��������� �����������.
    /// </summary>
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // ������� ������ ������
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player == null)
        {
            // �� ������� ������, ���� ��� �����-���������, � ������� ��� ��� ������
            if (scene.name != "Initializer")
            {
                Debug.LogError("Player object with tag 'Player' not found in the new scene!");
            }
            return;
        }

        // ������� ��� ����� �������� � ����� �����
        var allTransitionPoints = FindObjectsOfType<SceneTransitionPoint>();
        SceneTransitionPoint targetPoint = null;

        // ���� ����� ��� ��, � ������� ID ��������� � ����� ID ����������
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
            // ���������� ������ � ������� ��������� �����
            player.transform.position = targetPoint.transform.position;
            Debug.Log($"Player moved to transition point: {destinationPointID}");
        }
        else
        {
            Debug.LogWarning($"No transition point found with ID: '{destinationPointID}' in scene '{scene.name}'");
        }

        // TODO: ����� ����� ������ ��������� ������ (fade-in)
    }
}
