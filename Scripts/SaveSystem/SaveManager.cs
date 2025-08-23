using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Linq;

public class SaveManager : MonoBehaviour
{
    // --- ДОБАВЛЕНО: Синглтон для глобального доступа ---
    public static SaveManager instance;

    private const string SaveFileName = "savegame.json";
    private Dictionary<string, JObject> loadedFileState;
    private Dictionary<string, Dictionary<string, object>> temporarySceneStates = new Dictionary<string, Dictionary<string, object>>();

    // --- ДОБАВЛЕНО: Метод Awake для инициализации синглтона ---
    void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            instance = this;
            DontDestroyOnLoad(gameObject); // Делаем SaveManager "вечным"
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

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.F5))
        {
            SaveToFile();
        }

        if (Input.GetKeyDown(KeyCode.F9))
        {
            LoadFromFile();
        }
    }

    public void SaveSceneStateToMemory()
    {
        string sceneName = SceneManager.GetActiveScene().name;
        temporarySceneStates[sceneName] = CaptureCurrentState();
        Debug.Log($"Состояние сцены '{sceneName}' сохранено во временное хранилище.");
    }

    public void SaveToFile()
    {
        var saveData = new SaveData();
        saveData.sceneName = SceneManager.GetActiveScene().name;
        saveData.sceneObjectsState = CaptureCurrentState();

        string jsonString = JsonConvert.SerializeObject(saveData, Formatting.Indented);
        File.WriteAllText(GetSavePath(), jsonString);

        Debug.Log("Game Saved to file: " + GetSavePath());
    }

    public void LoadFromFile()
    {
        string path = GetSavePath();
        if (!File.Exists(path))
        {
            Debug.LogWarning("No save file found!");
            return;
        }

        temporarySceneStates.Clear();
        Debug.Log("Временное хранилище сцен очищено.");

        string jsonString = File.ReadAllText(path);
        var saveData = JsonConvert.DeserializeObject<SaveData>(jsonString);

        loadedFileState = saveData.sceneObjectsState.ToDictionary(
            kvp => kvp.Key,
            kvp => (JObject)kvp.Value
        );

        SceneManager.LoadScene(saveData.sceneName);
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (loadedFileState != null)
        {
            RestoreStateFromData(loadedFileState);
            loadedFileState = null;
            Debug.Log("Состояние сцены восстановлено из основного файла сохранения.");
            return;
        }

        if (temporarySceneStates.TryGetValue(scene.name, out var sceneData))
        {
            var dataForRestore = sceneData.ToDictionary(
                kvp => kvp.Key,
                kvp => JObject.FromObject(kvp.Value)
            );
            RestoreStateFromData(dataForRestore);
            Debug.Log($"Состояние сцены '{scene.name}' восстановлено из временного хранилища.");
        }
    }

    private Dictionary<string, object> CaptureCurrentState()
    {
        var sceneState = new Dictionary<string, object>();
        foreach (var saveableEntity in FindObjectsOfType<SaveableEntity>())
        {
            string id = saveableEntity.GetUniqueIdentifier();
            if (string.IsNullOrEmpty(id)) continue;

            var componentStates = new Dictionary<string, object>();
            foreach (var saveableComponent in saveableEntity.GetComponents<ISaveable>())
            {
                object state = saveableComponent.CaptureState();
                if (state != null)
                {
                    componentStates[saveableComponent.GetType().ToString()] = state;
                }
            }
            sceneState[id] = componentStates;
        }
        return sceneState;
    }

    private void RestoreStateFromData(Dictionary<string, JObject> stateData)
    {
        var sceneEntities = FindObjectsOfType<SaveableEntity>().ToDictionary(e => e.GetUniqueIdentifier());

        foreach (var savedState in stateData)
        {
            string id = savedState.Key;
            var componentStates = savedState.Value.ToObject<Dictionary<string, JToken>>();

            if (sceneEntities.TryGetValue(id, out SaveableEntity entity))
            {
                foreach (var saveableComponent in entity.GetComponents<ISaveable>())
                {
                    string componentType = saveableComponent.GetType().ToString();
                    if (componentStates.TryGetValue(componentType, out JToken state))
                    {
                        saveableComponent.RestoreState(state);
                    }
                }
                sceneEntities.Remove(id);
            }
            else
            {
                if (componentStates.TryGetValue(typeof(ItemPickup).ToString(), out JToken itemPickupState))
                {
                    string itemID = itemPickupState["itemID"].ToString();
                    ItemData itemData = ItemDatabase.instance.GetItemByID(itemID);
                    if (itemData != null && itemData.prefab != null)
                    {
                        GameObject instance = Instantiate(itemData.prefab);
                        instance.GetComponent<SaveableEntity>().SetUniqueIdentifier(id);
                        instance.GetComponent<ISaveable>().RestoreState(itemPickupState);
                    }
                }
            }
        }

        foreach (var entity in sceneEntities.Values)
        {
            if (entity.GetComponent<ItemPickup>() != null)
            {
                Destroy(entity.gameObject);
            }
        }
    }

    private string GetSavePath()
    {
        return Path.Combine(Application.persistentDataPath, SaveFileName);
    }
}
