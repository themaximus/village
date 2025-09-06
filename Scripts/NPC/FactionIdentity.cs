using UnityEngine;

/// <summary>
/// Компонент-маркер, который определяет принадлежность объекта к фракции.
/// Добавьте этот компонент на все префабы NPC и на игрока.
/// </summary>
public class FactionIdentity : MonoBehaviour
{
    [Tooltip("Ассет фракции, к которой принадлежит этот объект.")]
    [SerializeField] private FactionData faction;

    /// <summary>
    /// Возвращает данные о фракции этого объекта.
    /// </summary>
    public FactionData Faction => faction;

    /// <summary>
    /// Позволяет изменить фракцию объекта во время игры (например, по квесту).
    /// </summary>
    /// <param name="newFaction">Новая фракция.</param>
    public void SetFaction(FactionData newFaction)
    {
        faction = newFaction;
    }
}

