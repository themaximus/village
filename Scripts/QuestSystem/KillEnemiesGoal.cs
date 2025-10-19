using UnityEngine;

[CreateAssetMenu(fileName = "New Kill Enemies Goal", menuName = "Quests/Goals/Kill Enemies")]
public class KillEnemiesGoal : QuestGoal
{
    [Header("Goal Settings")]
    public NpcData enemyToKill;
    public int requiredAmount = 1;

    private int currentAmount;

    // Переопределяем метод Initialize, чтобы подписаться на события
    public override void Initialize()
    {
        base.Initialize();
        currentAmount = 0;
        // Подписываемся на глобальное событие смерти врага
        GameEvents.OnEnemyDied += OnEnemyDied;
    }

    // Этот метод будет вызываться автоматически, когда ЛЮБОЙ враг в игре умирает
    private void OnEnemyDied(NpcData killedNpcData)
    {
        if (isCompleted) return;

        // Если это тот самый враг, который нам нужен, увеличиваем счетчик
        if (killedNpcData == enemyToKill)
        {
            currentAmount++;
            CheckProgress();
        }
    }

    public override void CheckProgress()
    {
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
        GameEvents.OnEnemyDied -= OnEnemyDied;
    }
}
