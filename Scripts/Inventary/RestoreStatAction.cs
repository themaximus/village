using UnityEngine;

// ���� ������� ������� ����� ��� �������� ������ "��������" � ���� Assets -> Create
[CreateAssetMenu(fileName = "New Restore Stat Action", menuName = "Item Actions/Restore Stat")]
public class RestoreStatAction : ItemAction
{
    // ������������ ��� ������, ����� ������ �������������� �� ����� ������������.
    // ��� ������� ������� ���������� ������ � ����������.
    public enum StatToRestore { Health, Hunger, Thirst, Sanity, Vigor }

    [Header("Action Settings")]
    public StatToRestore statToRestore;
    public float amount = 10f; // ����������, �� ������� ����� ������������

    /// <summary>
    /// ��������� ������ �������������� ��������������.
    /// </summary>
    public override void Execute(GameObject performer)
    {
        // �������� ����� ��������� StatController �� �������, ������� ����������� �������
        StatController statController = performer.GetComponent<StatController>();
        if (statController == null)
        {
            Debug.LogWarning("StatController not found on " + performer.name);
            return;
        }

        // ���������� ����������� switch ��� �����������, ����� �������������� ����� ���������
        switch (statToRestore)
        {
            case StatToRestore.Health:
                statController.Health.Add(amount);
                Debug.Log("Restored " + amount + " Health.");
                break;
            case StatToRestore.Hunger:
                statController.Hunger.Add(amount);
                Debug.Log("Restored " + amount + " Hunger.");
                break;
            case StatToRestore.Thirst:
                statController.Thirst.Add(amount);
                Debug.Log("Restored " + amount + " Thirst.");
                break;
            case StatToRestore.Sanity:
                statController.Sanity.Add(amount);
                Debug.Log("Restored " + amount + " Sanity.");
                break;
            case StatToRestore.Vigor:
                statController.Vigor.Add(amount);
                Debug.Log("Restored " + amount + " Vigor.");
                break;
        }
    }
}
