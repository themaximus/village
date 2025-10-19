using UnityEngine;
using System;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;

[ExecuteAlways]
public class SaveableEntity : MonoBehaviour
{
    [Tooltip("���������� ID ����� �������. ������������ �������������.")]
    [SerializeField] private string uniqueId = "";

    // ������� ��� ������������ ID � �������������� ���������� � ���������
    private static Dictionary<string, SaveableEntity> globalLookup = new Dictionary<string, SaveableEntity>();

    public string GetUniqueIdentifier() => uniqueId;

    public void SetUniqueIdentifier(string id) => uniqueId = id;

    private void OnEnable()
    {
        // ������ ��� ���������, ����� �������� ���������� ��� �����������
#if UNITY_EDITOR
        if (!Application.isPlaying)
        {
            // ���� ID ������ ��� ��� ����� ������ ��������
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
        // � ������ ���� ���������� ID ������ ���� �� ������ (��� ��������)
        if (Application.isPlaying && string.IsNullOrEmpty(uniqueId))
        {
            uniqueId = Guid.NewGuid().ToString();
        }
    }

    private void OnDisable()
    {
        // ������� �� �������, ����� ID ��� ���� ���������������
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
    /// �������� ��������� ���� ISaveable ����������� �� ���� �������.
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
    /// ��������������� ��������� ���� ISaveable ����������� �� ������.
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