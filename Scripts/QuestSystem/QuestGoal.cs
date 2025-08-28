using UnityEngine;

/// <summary>
/// Абстрактный базовый класс для всех целей квеста.
/// </summary>
public abstract class QuestGoal : ScriptableObject
{
    [Header("Goal Information")]
    [TextArea] public string description; // Описание цели, например, "Убейте 5 волков"

    public bool isCompleted { get; protected set; }

    public virtual void Initialize()
    {
        isCompleted = false;
    }

    /// <summary>
    /// Метод, который будет проверять, выполнены ли условия цели.
    /// </summary>
    public abstract void CheckProgress();

    /// <summary>
    /// Возвращает строку, описывающую текущий прогресс цели (например, "2 / 5").
    /// </summary>
    public abstract string GetProgressText();
}
