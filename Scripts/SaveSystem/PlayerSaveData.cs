using System.Collections.Generic; // ���������� ��� ������������� Dictionary

/// <summary>
/// ���� ������� ��������� Unity ������ � ������������ ���� �����,
/// ���� ���� �� �� �������� ����������� (MonoBehaviour).
/// </summary>
[System.Serializable]
public class PlayerSaveData
{
    // ���� ��� �������� ������� ������.
    // �� ���������� ������ float[3], ��� ��� Vector3 �� ������ ������ �������������.
    public float[] position;

    // ������� ��� �������� ������� �������� ���� �������������.
    // ���� - ��� �������� �������������� (��������, "Health"), �������� - �� ������� ����������.
    public Dictionary<string, float> stats;

    /// <summary>
    /// ����������� ��� �������� �������� ������� � �������.
    /// </summary>
    public PlayerSaveData()
    {
        position = new float[3];
        stats = new Dictionary<string, float>();
    }
}
