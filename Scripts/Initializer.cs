using UnityEngine;
using UnityEngine.SceneManagement;
#if UNITY_EDITOR
using UnityEditor; // ���������� ��� ������� � EditorPrefs
#endif

public class Initializer : MonoBehaviour
{
    [Tooltip("��� �����, ������� ����� ��������� ����� ������������� (��� �����).")]
    [SerializeField] private string firstSceneToLoad = "MainMenu";

    void Start()
    {
#if UNITY_EDITOR
        // ���������, ������� �� ��� ��� ������������ ������ "�������"
        string sceneToLoadPath = EditorPrefs.GetString("LoadTargetScenePath");
        if (!string.IsNullOrEmpty(sceneToLoadPath))
        {
            // ���� ��, ������� "�������" � ��������� ������ �����
            Debug.Log($"[Editor Play] �������� ������� �����: {sceneToLoadPath}");
            EditorPrefs.DeleteKey("LoadTargetScenePath");
            SceneManager.LoadScene(sceneToLoadPath);
            return; // ��������� ����������, ����� �� ��������� ����� �� ���������
        }
#endif

        // ����������� ��������� ��� ����� ��� ���� �� ����������� �������� �� Initializer
        Debug.Log($"[Build Play] �������� ����� �� ���������: {firstSceneToLoad}");
        SceneManager.LoadScene(firstSceneToLoad);
    }
}
