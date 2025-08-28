using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Linq;

public class SaveManager : MonoBehaviour
{
    public static SaveManager instance;

    private const string SaveFileName = "savegame.json";

    // Единое хранилище состояния всего игрового мира.
    private Dictionary<string, object> worldState = new Dictionary<string, object>();

    // Флаг, который решает проблему с исчезновением предметов при старте новой игры.
    private bool isGameLoaded = false;

    public bool IsLoadingScene { get; private set; }

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

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.F5)) SaveToFile();
        if (Input.GetKeyDown(KeyCode.F9)) LoadFromFile();
    }

    /// <summary>
    /// Новый метод, который немедленно удаляет объект из состояния мира. Вызывается из ItemPickup.
    /// </summary>
    public void ForgetEntity(string id)
    {
        if (worldState.ContainsKey(id))
        {
            worldState.Remove(id);
        }
    }

    public void CaptureCurrentSceneState()
    {
        foreach (var saveable in FindObjectsOfType<SaveableEntity>(true))
        {
            worldState[saveable.GetUniqueIdentifier()] = saveable.CaptureStateWrapper();
        }
    }

    public void SaveToFile()
    {
        CaptureCurrentSceneState();

        var saveData = new SaveData
        {
            sceneName = SceneManager.GetActiveScene().name,
            sceneObjectsState = worldState
        };

        string jsonString = JsonConvert.SerializeObject(saveData, Formatting.Indented);
        File.WriteAllText(GetSavePath(), jsonString);
        Debug.Log("Игра сохранена в файл.");
    }

    public void LoadFromFile()
    {
        string path = GetSavePath();
        if (!File.Exists(path))
        {
            Debug.LogWarning("Файл сохранения не найден!");
            return;
        }

        string jsonString = File.ReadAllText(path);
        var saveData = JsonConvert.DeserializeObject<SaveData>(jsonString);

        worldState = saveData.sceneObjectsState;
        isGameLoaded = true;

        IsLoadingScene = true;
        SceneManager.LoadScene(saveData.sceneName);
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (isGameLoaded)
        {
            RestoreSceneState();
        }
        IsLoadingScene = false;
    }

    private void RestoreSceneState()
    {
        string currentSceneName = SceneManager.GetActiveScene().name;
        var currentEntitiesOnScene = FindObjectsOfType<SaveableEntity>(true).ToList();

        var requiredEntityIds = new HashSet<string>();
        foreach (var pair in worldState)
        {
            var objectData = JObject.FromObject(pair.Value);
            string savedSceneName = objectData["sceneName"]?.ToString();
            if (savedSceneName == currentSceneName || savedSceneName == "DontDestroyOnLoad")
            {
                requiredEntityIds.Add(pair.Key);
            }
        }

        foreach (var entity in currentEntitiesOnScene)
        {
            if (entity.gameObject.scene.name == "DontDestroyOnLoad") continue;

            if (!requiredEntityIds.Contains(entity.GetUniqueIdentifier()))
            {
                Destroy(entity.gameObject);
            }
        }

        foreach (string id in requiredEntityIds)
        {
            var objectData = JObject.FromObject(worldState[id]);
            SaveableEntity entity = currentEntitiesOnScene.FirstOrDefault(e => e.GetUniqueIdentifier() == id);

            if (entity != null)
            {
                entity.RestoreStateWrapper(objectData);
            }
            else if (objectData["sceneName"]?.ToString() == currentSceneName)
            {
                CreateEntityFromState(id, objectData);
            }
        }
    }

    private void CreateEntityFromState(string id, JObject objectData)
    {
        var components = objectData["components"]?.ToObject<Dictionary<string, JToken>>();
        if (components == null || !components.ContainsKey(typeof(ItemPickup).ToString())) return;

        var itemPickupState = components[typeof(ItemPickup).ToString()];
        string itemID = itemPickupState.Value<string>("itemID");
        ItemData itemData = ItemDatabase.instance.GetItemByID(itemID);

        if (itemData != null && itemData.prefab != null)
        {
            GameObject instance = Instantiate(itemData.prefab);
            var newEntity = instance.GetComponent<SaveableEntity>();
            newEntity.SetUniqueIdentifier(id);
            newEntity.RestoreStateWrapper(objectData);
        }
    }

    private string GetSavePath() => Path.Combine(Application.persistentDataPath, SaveFileName);
}