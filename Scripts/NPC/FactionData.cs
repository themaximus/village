using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "New Faction", menuName = "NPC/Faction")]
public class FactionData : ScriptableObject
{
    [Tooltip("Название фракции (например, 'Стража Города').")]
    public string factionName;

    [Tooltip("Список фракций, к которым эта фракция враждебна.")]
    public List<FactionData> HostileTo;
}

