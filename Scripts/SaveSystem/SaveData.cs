using System.Collections.Generic;

/// <summary>
/// ������� �����-���������, ������� ������ ��� ������ ��� ������ ����������.
/// </summary>
[System.Serializable]
public class SaveData
{
    // ��� �����, � ������� ���� ������� ����������
    public string sceneName;

    // �������, �������� ��������� ���� ����������� �������� �� ���� �����
    public Dictionary<string, object> sceneObjectsState;

    public SaveData()
    {
        sceneObjectsState = new Dictionary<string, object>();
    }
}
