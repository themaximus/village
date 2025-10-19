using UnityEngine;

/// <summary>
/// ���������-������, ������� ���������� �������������� ������� � �������.
/// �������� ���� ��������� �� ��� ������� NPC � �� ������.
/// </summary>
public class FactionIdentity : MonoBehaviour
{
    [Tooltip("����� �������, � ������� ����������� ���� ������.")]
    [SerializeField] private FactionData faction;

    /// <summary>
    /// ���������� ������ � ������� ����� �������.
    /// </summary>
    public FactionData Faction => faction;

    /// <summary>
    /// ��������� �������� ������� ������� �� ����� ���� (��������, �� ������).
    /// </summary>
    /// <param name="newFaction">����� �������.</param>
    public void SetFaction(FactionData newFaction)
    {
        faction = newFaction;
    }
}

