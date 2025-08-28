using UnityEngine;

[CreateAssetMenu(fileName = "New Gather Item Goal", menuName = "Quests/Goals/Gather Item")]
public class GatherItemGoal : QuestGoal
{
    [Header("Goal Settings")]
    public ItemData itemToGather;
    public int requiredAmount = 1;

    private int currentAmount;

    // �������������� ����� Initialize, ����� ����������� �� �������
    public override void Initialize()
    {
        base.Initialize();
        currentAmount = 0;
        // ������������� �� ���������� ������� ��������� ���������
        GameEvents.OnInventoryUpdated += CheckProgress;
        // ����� ��������� ��������� �� ������, ���� � ������ ��� ���� ������ ��������
        CheckProgress();
    }

    /// <summary>
    /// ���� ����� ����� ���������� ������������� ��� ����� ��������� � ���������.
    /// </summary>
    public override void CheckProgress()
    {
        if (isCompleted) return;

        // ������� ��������� ������
        // FindObjectOfType - ������� ������, �� ��� ������� ���� ����� ����� ������ ������ ��� ��������
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

    // ����� ���������� �� �������, ����� ����� ����������� ��� ���� �����������
    private void OnDisable()
    {
        GameEvents.OnInventoryUpdated -= CheckProgress;
    }
}
