using UnityEngine;

// ���� ������� ��������� ����� ��� �������� ������ "�������" � ���� Assets -> Create
[CreateAssetMenu(fileName = "New Stat Sheet", menuName = "Stats/Character Stat Sheet")]
public class CharacterStatSheet : ScriptableObject
{
    [Header("Base Stat Values")]
    public float maxHealth = 100f;
    public float maxHunger = 100f;
    public float maxThirst = 100f;
    public float maxSanity = 100f;
    public float maxVigor = 100f; // ������������ ��������

    [Header("Passive Stat Decay Rates")]
    [Tooltip("������� ������ ������ �������� � �������.")]
    public float hungerDecayRate = 1f;
    [Tooltip("������� ������ ����� �������� � �������.")]
    public float thirstDecayRate = 2f;
    // ����� �������� � ������ ��������� ���������, ��������, �������������� �������� �� ���.
}
