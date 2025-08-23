using UnityEngine;

[CreateAssetMenu(fileName = "New Gather Item Goal", menuName = "Quests/Goals/Gather Item")]
public class GatherItemGoal : QuestGoal
{
    [Header("Goal Settings")]
    public ItemData itemToGather;
    public int requiredAmount = 1;

    private int currentAmount;

    // Переопределяем метод Initialize, чтобы подписаться на события
    public override void Initialize()
    {
        base.Initialize();
        currentAmount = 0;
        // Подписываемся на глобальное событие изменения инвентаря
        GameEvents.OnInventoryUpdated += CheckProgress;
        // Сразу проверяем инвентарь на случай, если у игрока уже есть нужные предметы
        CheckProgress();
    }

    /// <summary>
    /// Этот метод будет вызываться автоматически при любом изменении в инвентаре.
    /// </summary>
    public override void CheckProgress()
    {
        if (isCompleted) return;

        // Находим инвентарь игрока
        // FindObjectOfType - простой способ, но для больших сцен лучше иметь прямую ссылку или синглтон
        Inventory playerInventory = Object.FindObjectOfType<Inventory>();
        if (playerInventory != null)
        {
            currentAmount = playerInventory.GetItemCount(itemToGather);
        }

        if (currentAmount >= requiredAmount)
        {
            currentAmount = requiredAmount;
            isCompleted = true;
            Debug.Log("Goal '" + description + "' completed!");
        }
    }

    public override string GetProgressText()
    {
        return $"{currentAmount} / {requiredAmount}";
    }

    // Важно отписаться от события, когда квест завершается или игра закрывается
    private void OnDisable()
    {
        GameEvents.OnInventoryUpdated -= CheckProgress;
    }
}
