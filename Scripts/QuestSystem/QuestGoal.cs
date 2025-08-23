using UnityEngine;

/// <summary>
/// ����������� ������� ����� ��� ���� ����� ������.
/// </summary>
public abstract class QuestGoal : ScriptableObject
{
    [Header("Goal Information")]
    [TextArea] public string description; // �������� ����, ��������, "������ 5 ������"

    public bool isCompleted { get; protected set; }

    public virtual void Initialize()
    {
        isCompleted = false;
    }

    /// <summary>
    /// �����, ������� ����� ���������, ��������� �� ������� ����.
    /// </summary>
    public abstract void CheckProgress();

    /// <summary>
    /// ���������� ������, ����������� ������� �������� ���� (��������, "2 / 5").
    /// </summary>
    public abstract string GetProgressText();
}
