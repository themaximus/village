using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// ScriptableObject для хранения данных о рецепте крафта.
/// </summary>
[CreateAssetMenu(fileName = "New Recipe", menuName = "Inventory/Recipe")]
public class RecipeData : ScriptableObject
{
    [Header("Crafting Recipe")]
    [Tooltip("Список предметов (ItemData), необходимых для создания.")]
    public List<ItemData> Ingredients;

    [Tooltip("Предмет (ItemData), который будет создан в результате крафта.")]
    public ItemData Result;
}
