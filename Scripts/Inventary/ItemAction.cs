using UnityEngine;

/// <summary>
/// ����������� ������� ����� ��� ���� ��������, ������� ����� ��������� �������.
/// </summary>
public abstract class ItemAction : ScriptableObject
{
    [Header("Action Animation")]
    public Sprite[] animationNorth;
    public Sprite[] animationEast;
    public Sprite[] animationSouth;
    public Sprite[] animationWest;

    [Header("Animation Settings")]
    public float animationFrameRate = 0.1f;
    public float animationDuration = 0.5f;

    /// <summary>
    /// ������� �����, ������� ����� ��������� ������ ��������.
    /// </summary>
    /// <param name="performer">������, ������� ���������� ������� (��������, �����).</param>
    public abstract void Execute(GameObject performer);
}
