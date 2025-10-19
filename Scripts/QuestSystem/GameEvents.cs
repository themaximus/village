using System;
using UnityEngine;

/// <summary>
/// ����������� ����� ��� ���������� ����� ����������� �������� ���������.
/// ��� "����� ����������", �� ������� ����� ������������� � ������� ����� ������������
/// ����� ������ �������, �� ���� ���� � �����.
/// </summary>
public static class GameEvents
{
    // --- �������, ��������� � ���� ---

    // �������, ������� �����������, ����� ����� NPC �������.
    // ��� �������� NpcData ������� �����.
    public static event Action<NpcData> OnEnemyDied;

    // ��������� ����� ��� ������ ������� ������ ����� �� ������ ������� �������.
    public static void ReportEnemyDied(NpcData npcData)
    {
        OnEnemyDied?.Invoke(npcData);
    }


    // --- �������, ��������� � ���������� ---

    // �������, ������� ����������� ��� ����� ��������� � ���������.
    public static event Action OnInventoryUpdated;

    // ��������� ����� ��� ������ ������� ���������� ���������.
    public static void ReportInventoryUpdated()
    {
        OnInventoryUpdated?.Invoke();
    }


    // � ������� ���� ����� ��������� ����� ������ ���������� �������:
    // public static event Action<string> OnLocationEntered;
    // public static void ReportLocationEntered(string locationID) { ... }
}
