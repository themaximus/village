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

    // ���������� ��������� ��������� �� �����
    private Dictionary<string, JObject> masterStateFromFile;

    // ��������� ��������� ��������� ��� ������� ������
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
        Debug.Log($"��������� ����� '{sceneName}' ��������� �� ��������� ���������.");
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

        Debug.Log("���� ��������� � ���� (������� ��� ���������� �������).");
    }

    public void LoadFromFile()
    {
        string path = GetSavePath();
        if (!File.Exists(path))
        {
            Debug.LogWarning("���� ���������� �� ������!");
            return;
        }

        temporarySceneStates.Clear();
        isInitialLoadAfterFile = true;
        Debug.Log("��������� ��������� ���� �������. ������ �������� �� �����.");

        string jsonString = File.ReadAllText(path);
        var saveData = JsonConvert.DeserializeObject<SaveData>(jsonString);

        masterStateFromFile = saveData.sceneObjectsState.ToDictionary(
            kvp => kvp.Key,
            kvp => (JObject)kvp.Value
        );

        SceneManager.LoadScene(saveData.sceneName);
    }

    // --- ��������� �������������� ������ �������� ����� ---
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        string currentSceneName = scene.name;

        // 1. �������� "�������" ���������, ��������� ������ �� ����� � ��������� ������.
        var worldState = masterStateFromFile != null
            ? new Dictionary<string, JObject>(masterStateFromFile)
            : new Dictionary<string, JObject>();

        foreach (var sceneData in temporarySceneStates.Values)
        {
            var tempJObjectData = sceneData.ToDictionary(kvp => kvp.Key, kvp => JObject.FromObject(kvp.Value));
            foreach (var pair in tempJObjectData)
            {
                worldState[pair.Key] = pair.Value; // ��������������, ��� ��� ��������� ������ ������
            }
        }

        if (worldState.Count == 0) return;

        // 2. ��������� ������� ���������, ����� �������� ������ ������ ��� ������� ����� � "������" ��������.
        var sceneSpecificState = new Dictionary<string, JObject>();
        foreach (var pair in worldState)
        {
            var wrapper = pair.Value.ToObject<Dictionary<string, JToken>>();
            if (wrapper.TryGetValue("sceneName", out JToken sceneNameToken))
            {
                string savedSceneName = sceneNameToken.ToString();
                // ������ ����������� ���, ���� ��� ����� ��������� � ������� ��� ���� �� "������" (DontDestroyOnLoad)
                if (savedSceneName == currentSceneName || savedSceneName == "DontDestroyOnLoad")
                {
                    sceneSpecificState[pair.Key] = pair.Value;
                }
            }
        }

        // 3. ��������������� �����, ��������� ������ ��������������� ������.
        bool allowCreation = isInitialLoadAfterFile || temporarySceneStates.ContainsKey(currentSceneName);
        RestoreStateFromData(sceneSpecificState, isInitialLoadAfterFile, allowCreation);
        Debug.Log($"��������� ��� ����� '{currentSceneName}' �������������.");

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

    // --- ���������� ������ �������������� ---
    private void RestoreStateFromData(Dictionary<string, JObject> stateDataForScene, bool isFullFileLoad, bool allowCreation)
    {
        var sceneEntities = FindObjectsOfType<SaveableEntity>(false).ToDictionary(e => e.GetUniqueIdentifier());

        // ������ 1: ��������� ������������ ������� � ���� ��, ��� ����� �������.
        foreach (var entity in sceneEntities.Values.ToList())
        {
            string id = entity.GetUniqueIdentifier();

            // �� ������� ������ ��� ������� �������� ����� �������, ����� �� "��������" ���������.
            if (!isFullFileLoad && entity.gameObject.scene.buildIndex == -1)
            {
                sceneEntities.Remove(id);
                continue;
            }

            if (stateDataForScene.TryGetValue(id, out JObject objectState))
            {
                // ���� ������ ���� � ����������, ��������������� ���.
                var wrapper = objectState.ToObject<Dictionary<string, JToken>>();
                var components = wrapper["components"].ToObject<Dictionary<string, JToken>>();
                foreach (var saveableComponent in entity.GetComponents<ISaveable>())
                {
                    if (components.TryGetValue(saveableComponent.GetType().ToString(), out JToken state))
                    {
                        saveableComponent.RestoreState(state);
                    }
                }
                sceneEntities.Remove(id); // ������� �� �������, ��� ��� �� ���������.
            }
        }

        // ���, ��� �������� � sceneEntities - ��� �������, ������� ��� � ���������� (�.�. ���������).
        // ���������� ��.
        foreach (var entity in sceneEntities.Values)
        {
            if (entity.GetComponent<ItemPickup>() != null)
            {
                Destroy(entity.gameObject);
            }
        }

        // ������ 2: ������� �������, ������� ���� � ����������, �� ��� �� ����� (�.�. ��������).
        if (allowCreation)
        {
            var existingIdsOnScene = new HashSet<string>(FindObjectsOfType<SaveableEntity>(false).Select(e => e.GetUniqueIdentifier()));

            foreach (var savedState in stateDataForScene)
            {
                string id = savedState.Key;
                if (existingIdsOnScene.Contains(id)) continue; // ��� ����������, �������.

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
