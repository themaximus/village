using UnityEngine;
using System;

/// <summary>
/// ���� ��������� �������� �� ����� ������ � �����, ������� ������ ���� ��������.
/// �� �������� �� ��������� � �������� ����������� �������������� (ID),
/// ������� �������������� ����� ���������� ��� ������� ���������� � �����.
/// </summary>
[ExecuteAlways] // ��������� ������� �������� ������, � �� ������ � ������ ����
public class SaveableEntity : MonoBehaviour
{
    [Tooltip("���������� ID ����� �������. ������������ �������������. �� ������� �������.")]
    [SerializeField] private string uniqueId = "";

    // ����������� ������� ��� ������������ ���� ID � �����, ����� �������� ����������
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
        // �����������: ������ ID ������������ � � ������ ����, ���� �� ������.
        // ��� ����� ��� ��������, ��������� �� �������� (��������, ����������� ���������).
        if (Application.isPlaying)
        {
            if (string.IsNullOrEmpty(uniqueId))
            {
                uniqueId = Guid.NewGuid().ToString();
            }
        }
        // ������ ��� ������ � ��������� (��� ����������� ��� �������� ����� ��������)
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
