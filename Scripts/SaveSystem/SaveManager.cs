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

    // Глобальное хранилище состояний из файла
    private Dictionary<string, JObject> masterStateFromFile;

    // Временное хранилище состояний для текущей сессии
    private Dictionary<string, Dictionary<string, object>> temporarySceneStates = new Dictionary<string, Dictionary<string, object>>();

    private bool isInitialLoadAfterFile = false;

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

        var finalState = masterStateFromFile != null
            ? masterStateFromFile.ToDictionary(kvp => kvp.Key, kvp => (object)kvp.Value)
            : new Dictionary<string, object>();

        foreach (var sceneState in temporarySceneStates.Values)
        {
            foreach (var objectState in sceneState)
            {
                finalState[objectState.Key] = objectState.Value;
            }
        }

        var currentState = CaptureCurrentState();
        foreach (var objectState in currentState)
        {
            finalState[objectState.Key] = objectState.Value;
        }

        saveData.sceneObjectsState = finalState;

        string jsonString = JsonConvert.SerializeObject(saveData, Formatting.Indented);
        File.WriteAllText(GetSavePath(), jsonString);

        masterStateFromFile = finalState.ToDictionary(kvp => kvp.Key, kvp => JObject.FromObject(kvp.Value));
        temporarySceneStates.Clear();

        Debug.Log("Игра сохранена в файл (включая все посещенные локации).");
    }

    public void LoadFromFile()
    {
        string path = GetSavePath();
        if (!File.Exists(path))
        {
            Debug.LogWarning("Файл сохранения не найден!");
            return;
        }

        temporarySceneStates.Clear();
        isInitialLoadAfterFile = true;
        Debug.Log("Временное хранилище сцен очищено. Начата загрузка из файла.");

        string jsonString = File.ReadAllText(path);
        var saveData = JsonConvert.DeserializeObject<SaveData>(jsonString);

        masterStateFromFile = saveData.sceneObjectsState.ToDictionary(
            kvp => kvp.Key,
            kvp => (JObject)kvp.Value
        );

        SceneManager.LoadScene(saveData.sceneName);
    }

    // --- ПОЛНОСТЬЮ ПЕРЕРАБОТАННАЯ ЛОГИКА ЗАГРУЗКИ СЦЕНЫ ---
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        string currentSceneName = scene.name;

        // 1. Собираем "мировое" состояние, объединяя данные из файла и временной памяти.
        var worldState = masterStateFromFile != null
            ? new Dictionary<string, JObject>(masterStateFromFile)
            : new Dictionary<string, JObject>();

        foreach (var sceneData in temporarySceneStates.Values)
        {
            var tempJObjectData = sceneData.ToDictionary(kvp => kvp.Key, kvp => JObject.FromObject(kvp.Value));
            foreach (var pair in tempJObjectData)
            {
                worldState[pair.Key] = pair.Value; // Перезаписываем, так как временные данные свежее
            }
        }

        if (worldState.Count == 0) return;

        // 2. Фильтруем мировое состояние, чтобы получить данные ТОЛЬКО для текущей сцены и "вечных" объектов.
        var sceneSpecificState = new Dictionary<string, JObject>();
        foreach (var pair in worldState)
        {
            var wrapper = pair.Value.ToObject<Dictionary<string, JToken>>();
            if (wrapper.TryGetValue("sceneName", out JToken sceneNameToken))
            {
                string savedSceneName = sceneNameToken.ToString();
                // Объект принадлежит нам, если его сцена совпадает с текущей ИЛИ если он "вечный" (DontDestroyOnLoad)
                if (savedSceneName == currentSceneName || savedSceneName == "DontDestroyOnLoad")
                {
                    sceneSpecificState[pair.Key] = pair.Value;
                }
            }
        }

        // 3. Восстанавливаем сцену, используя только отфильтрованные данные.
        bool allowCreation = isInitialLoadAfterFile || temporarySceneStates.ContainsKey(currentSceneName);
        RestoreStateFromData(sceneSpecificState, isInitialLoadAfterFile, allowCreation);
        Debug.Log($"Состояние для сцены '{currentSceneName}' восстановлено.");

        isInitialLoadAfterFile = false;
    }

    private Dictionary<string, object> CaptureCurrentState()
    {
        var state = new Dictionary<string, object>();
        foreach (var saveableEntity in FindObjectsOfType<SaveableEntity>(false))
        {
            string id = saveableEntity.GetUniqueIdentifier();
            if (string.IsNullOrEmpty(id)) continue;

            var componentStates = new Dictionary<string, object>();
            foreach (var saveableComponent in saveableEntity.GetComponents<ISaveable>())
            {
                object componentState = saveableComponent.CaptureState();
                if (componentState != null)
                {
                    componentStates[saveableComponent.GetType().ToString()] = componentState;
                }
            }

            if (componentStates.Count > 0)
            {
                var objectWrapper = new Dictionary<string, object>
                {
                    { "sceneName", saveableEntity.gameObject.scene.name },
                    { "components", componentStates }
                };
                state[id] = objectWrapper;
            }
        }
        return state;
    }

    // --- УПРОЩЕННАЯ ЛОГИКА ВОССТАНОВЛЕНИЯ ---
    private void RestoreStateFromData(Dictionary<string, JObject> stateDataForScene, bool isFullFileLoad, bool allowCreation)
    {
        var sceneEntities = FindObjectsOfType<SaveableEntity>(false).ToDictionary(e => e.GetUniqueIdentifier());

        // Проход 1: Обновляем существующие объекты и ищем те, что нужно удалить.
        foreach (var entity in sceneEntities.Values.ToList())
        {
            string id = entity.GetUniqueIdentifier();

            // Не трогаем игрока при обычном переходе между сценами, чтобы не "откатить" инвентарь.
            if (!isFullFileLoad && entity.gameObject.scene.buildIndex == -1)
            {
                sceneEntities.Remove(id);
                continue;
            }

            if (stateDataForScene.TryGetValue(id, out JObject objectState))
            {
                // Если объект есть в сохранении, восстанавливаем его.
                var wrapper = objectState.ToObject<Dictionary<string, JToken>>();
                var components = wrapper["components"].ToObject<Dictionary<string, JToken>>();
                foreach (var saveableComponent in entity.GetComponents<ISaveable>())
                {
                    if (components.TryGetValue(saveableComponent.GetType().ToString(), out JToken state))
                    {
                        saveableComponent.RestoreState(state);
                    }
                }
                sceneEntities.Remove(id); // Убираем из словаря, так как он обработан.
            }
        }

        // Все, что осталось в sceneEntities - это объекты, которых нет в сохранении (т.е. подобраны).
        // Уничтожаем их.
        foreach (var entity in sceneEntities.Values)
        {
            if (entity.GetComponent<ItemPickup>() != null)
            {
                Destroy(entity.gameObject);
            }
        }

        // Проход 2: Создаем объекты, которые есть в сохранении, но нет на сцене (т.е. выложены).
        if (allowCreation)
        {
            var existingIdsOnScene = new HashSet<string>(FindObjectsOfType<SaveableEntity>(false).Select(e => e.GetUniqueIdentifier()));

            foreach (var savedState in stateDataForScene)
            {
                string id = savedState.Key;
                if (existingIdsOnScene.Contains(id)) continue; // Уже существует, пропуск.

                var wrapper = savedState.Value.ToObject<Dictionary<string, JToken>>();
                var components = wrapper["components"].ToObject<Dictionary<string, JToken>>();

                if (components.ContainsKey(typeof(ItemPickup).ToString()))
                {
                    var itemPickupState = components[typeof(ItemPickup).ToString()];
                    string itemID = itemPickupState.Value<string>("itemID");
                    ItemData itemData = ItemDatabase.instance.GetItemByID(itemID);
                    if (itemData != null && itemData.prefab != null)
                    {
                        GameObject instance = Instantiate(itemData.prefab);
                        instance.GetComponent<SaveableEntity>().SetUniqueIdentifier(id);
                        foreach (var saveable in instance.GetComponents<ISaveable>())
                        {
                            string compType = saveable.GetType().ToString();
                            if (components.ContainsKey(compType))
                            {
                                saveable.RestoreState(components[compType]);
                            }
                        }
                    }
                }
            }
        }
    }

    private string GetSavePath()
    {
        return Path.Combine(Application.persistentDataPath, SaveFileName);
    }
}
