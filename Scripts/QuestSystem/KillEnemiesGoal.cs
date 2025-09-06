using UnityEngine;

[CreateAssetMenu(fileName = "New Kill Enemies Goal", menuName = "Quests/Goals/Kill Enemies")]
public class KillEnemiesGoal : QuestGoal
{
    [Header("Goal Settings")]
    public NpcData enemyToKill;
    public int requiredAmount = 1;

    private int currentAmount;

    // �������������� ����� Initialize, ����� ����������� �� �������
    public override void Initialize()
    {
        base.Initialize();
        currentAmount = 0;
        // ������������� �� ���������� ������� ������ �����
        GameEvents.OnEnemyDied += OnEnemyDied;
    }

    // ���� ����� ����� ���������� �������������, ����� ����� ���� � ���� �������
    private void OnEnemyDied(NpcData killedNpcData)
    {
        if (isCompleted) return;

        // ���� ��� ��� ����� ����, ������� ��� �����, ����������� �������
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

    // ����� ���������� �� �������, ����� ����� ����������� ��� ���� �����������
    private void OnDisable()
    {
        GameEvents.OnEnemyDied -= OnEnemyDied;
    }
}
