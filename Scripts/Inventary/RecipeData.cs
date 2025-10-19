using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// ScriptableObject ��� �������� ������ � ������� ������.
/// </summary>
[CreateAssetMenu(fileName = "New Recipe", menuName = "Inventory/Recipe")]
public class RecipeData : ScriptableObject
{
    [Header("Crafting Recipe")]
    [Tooltip("������ ��������� (ItemData), ����������� ��� ��������.")]
    public List<ItemData> Ingredients;

    [Tooltip("������� (ItemData), ������� ����� ������ � ���������� ������.")]
    public ItemData Result;
}
