using UnityEngine;
using UnityEngine.SceneManagement;
#if UNITY_EDITOR
using UnityEditor; // Необходимо для доступа к EditorPrefs
#endif

public class Initializer : MonoBehaviour
{
    [Tooltip("Имя сцены, которую нужно загрузить после инициализации (для билда).")]
    [SerializeField] private string firstSceneToLoad = "MainMenu";

    void Start()
    {
#if UNITY_EDITOR
        // Проверяем, оставил ли для нас редакторский скрипт "записку"
        string sceneToLoadPath = EditorPrefs.GetString("LoadTargetScenePath");
        if (!string.IsNullOrEmpty(sceneToLoadPath))
        {
            // Если да, очищаем "записку" и загружаем нужную сцену
            Debug.Log($"[Editor Play] Загрузка целевой сцены: {sceneToLoadPath}");
            EditorPrefs.DeleteKey("LoadTargetScenePath");
            SceneManager.LoadScene(sceneToLoadPath);
            return; // Прерываем выполнение, чтобы не загрузить сцену по умолчанию
        }
#endif

        // Стандартное поведение для билда или если мы запустились напрямую из Initializer
        Debug.Log($"[Build Play] Загрузка сцены по умолчанию: {firstSceneToLoad}");
        SceneManager.LoadScene(firstSceneToLoad);
    }
}
