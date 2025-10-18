using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "New Faction", menuName = "NPC/Faction")]
public class FactionData : ScriptableObject
{
    [Tooltip("�������� ������� (��������, '������ ������').")]
    public string factionName;

    [Tooltip("������ �������, � ������� ��� ������� ���������.")]
    public List<FactionData> HostileTo;
}

