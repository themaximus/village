using UnityEngine;
using System;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;

[ExecuteAlways]
public class SaveableEntity : MonoBehaviour
{
    [Tooltip("Уникальный ID этого объекта. Генерируется автоматически.")]
    [SerializeField] private string uniqueId = "";

    // Словарь для отслеживания ID и предотвращения дубликатов в редакторе
    private static Dictionary<string, SaveableEntity> globalLookup = new Dictionary<string, SaveableEntity>();

    public string GetUniqueIdentifier() => uniqueId;

    public void SetUniqueIdentifier(string id) => uniqueId = id;

    private void OnEnable()
    {
        // Логика для редактора, чтобы избежать дубликатов при копировании
#if UNITY_EDITOR
        if (!Application.isPlaying)
        {
            // Если ID пустой или уже занят другим объектом
            if (string.IsNullOrEmpty(uniqueId) || (globalLookup.ContainsKey(uniqueId) && globalLookup[uniqueId] != this))
            {
                uniqueId = Guid.NewGuid().ToString();
            }
            globalLookup[uniqueId] = this;
            return;
        }
#endif
    }

    private void Awake()
    {
        // В режиме игры генерируем ID только если он пустой (для префабов)
        if (Application.isPlaying && string.IsNullOrEmpty(uniqueId))
        {
            uniqueId = Guid.NewGuid().ToString();
        }
    }

    private void OnDisable()
    {
        // Убираем из словаря, чтобы ID мог быть переиспользован
#if UNITY_EDITOR
        if (!Application.isPlaying)
        {
            if (globalLookup.ContainsKey(uniqueId) && globalLookup[uniqueId] == this)
            {
                globalLookup.Remove(uniqueId);
            }
        }
#endif
    }

    /// <summary>
    /// Собирает состояние всех ISaveable компонентов на этом объекте.
    /// </summary>
    public object CaptureStateWrapper()
    {
        var componentStates = new Dictionary<string, object>();
        foreach (var saveableComponent in GetComponents<ISaveable>())
        {
            object state = saveableComponent.CaptureState();
            if (state != null)
            {
                componentStates[saveableComponent.GetType().ToString()] = state;
            }
        }

        return new Dictionary<string, object>
        {
            { "sceneName", gameObject.scene.name },
            { "components", componentStates }
        };
    }

    /// <summary>
    /// Восстанавливает состояние всех ISaveable компонентов из данных.
    /// </summary>
    public void RestoreStateWrapper(JObject objectData)
    {
        var components = objectData["components"]?.ToObject<Dictionary<string, JToken>>();
        if (components == null) return;

        foreach (var saveableComponent in GetComponents<ISaveable>())
        {
            if (components.TryGetValue(saveableComponent.GetType().ToString(), out JToken state))
            {
                saveableComponent.RestoreState(state);
            }
        }
    }
}