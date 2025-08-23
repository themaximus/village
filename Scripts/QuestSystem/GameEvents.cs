using System;
using UnityEngine;

/// <summary>
/// Статический класс для управления всеми глобальными игровыми событиями.
/// Это "доска объявлений", на которую могут подписываться и которую могут использовать
/// любые другие скрипты, не зная друг о друге.
/// </summary>
public static class GameEvents
{
    // --- События, связанные с боем ---

    // Событие, которое срабатывает, когда любой NPC умирает.
    // Оно передает NpcData убитого врага.
    public static event Action<NpcData> OnEnemyDied;

    // Публичный метод для вызова события смерти врага из любого другого скрипта.
    public static void ReportEnemyDied(NpcData npcData)
    {
        OnEnemyDied?.Invoke(npcData);
    }


    // --- События, связанные с инвентарем ---

    // Событие, которое срабатывает при любом изменении в инвентаре.
    public static event Action OnInventoryUpdated;

    // Публичный метод для вызова события обновления инвентаря.
    public static void ReportInventoryUpdated()
    {
        OnInventoryUpdated?.Invoke();
    }


    // В будущем сюда можно добавлять любые другие глобальные события:
    // public static event Action<string> OnLocationEntered;
    // public static void ReportLocationEntered(string locationID) { ... }
}
