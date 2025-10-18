using UnityEngine;

/// <summary>
/// Абстрактный базовый класс для всех действий, которые может выполнять предмет.
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
    /// Главный метод, который будет выполнять логику действия.
    /// </summary>
    /// <param name="performer">Объект, который использует предмет (например, игрок).</param>
    public abstract void Execute(GameObject performer);
}
