using UnityEngine;
using System;

/// <summary>
/// Этот компонент вешается на любой объект в сцене, который должен быть сохранен.
/// Он отвечает за генерацию и хранение уникального идентификатора (ID),
/// который гарантированно будет уникальным для каждого экземпляра в сцене.
/// </summary>
[ExecuteAlways] // Позволяет скрипту работать всегда, а не только в режиме игры
public class SaveableEntity : MonoBehaviour
{
    [Tooltip("Уникальный ID этого объекта. Генерируется автоматически. Не меняйте вручную.")]
    [SerializeField] private string uniqueId = "";

    // Статический словарь для отслеживания всех ID в сцене, чтобы избежать дубликатов
    private static readonly System.Collections.Generic.Dictionary<string, SaveableEntity> globalLookup = new System.Collections.Generic.Dictionary<string, SaveableEntity>();

    public string GetUniqueIdentifier()
    {
        return uniqueId;
    }

    public void SetUniqueIdentifier(string id)
    {
        UnregisterId();
        uniqueId = id;
        RegisterId();
    }

    private void Awake()
    {
        // ИСПРАВЛЕНИЕ: Теперь ID генерируется и в режиме игры, если он пустой.
        // Это нужно для объектов, созданных из префабов (например, выброшенных предметов).
        if (Application.isPlaying)
        {
            if (string.IsNullOrEmpty(uniqueId))
            {
                uniqueId = Guid.NewGuid().ToString();
            }
        }
        // Логика для работы в редакторе (при копировании или создании новых объектов)
        else
        {
            if (string.IsNullOrEmpty(uniqueId) || !IsUnique(uniqueId))
            {
                uniqueId = Guid.NewGuid().ToString();
            }
        }

        RegisterId();
    }

    private void OnDestroy()
    {
        UnregisterId();
    }

    private bool IsUnique(string id)
    {
        if (globalLookup.ContainsKey(id) && globalLookup[id] != this)
        {
            return false;
        }
        return true;
    }

    private void RegisterId()
    {
        if (!string.IsNullOrEmpty(uniqueId) && !globalLookup.ContainsKey(uniqueId))
        {
            globalLookup.Add(uniqueId, this);
        }
    }

    private void UnregisterId()
    {
        if (!string.IsNullOrEmpty(uniqueId) && globalLookup.ContainsKey(uniqueId))
        {
            globalLookup.Remove(uniqueId);
        }
    }
}
