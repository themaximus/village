using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Linq;
using System.Text; // Добавлено для StringBuilder

/// <summary>
/// Управляет интерфейсом окна крафта.
/// </summary>
public class CraftingUI : MonoBehaviour
{
    [Header("UI Components")]
    [Tooltip("Список слотов для ингредиентов.")]
    public List<UniversalSlotUI> ingredientSlots;
    [Tooltip("Слот для отображения результата крафта.")]
    public UniversalSlotUI resultSlot;
    [Tooltip("Кнопка для создания предмета.")]
    public Button craftButton;

    private List<RecipeData> availableRecipes;
    private Inventory playerInventory;
    private RecipeData currentRecipe;

    private void Awake()
    {
        playerInventory = FindObjectOfType<Inventory>();
        if (playerInventory == null)
        {
            Debug.LogError("[CraftingUI] Не найден инвентарь игрока (Inventory) в сцене!");
        }

        if (craftButton != null)
        {
            craftButton.onClick.AddListener(CraftItem);
        }
    }

    /// <summary>
    /// Открывает окно крафта и загружает в него доступные рецепты.
    /// </summary>
    public void Open(List<RecipeData> recipes)
    {
        if (recipes == null || recipes.Count == 0)
        {
            Debug.LogWarning("[CraftingUI] Верстак открыт, но ему не передано ни одного рецепта.");
            this.availableRecipes = new List<RecipeData>();
        }
        else
        {
            this.availableRecipes = recipes;
            Debug.Log($"[CraftingUI] Окно крафта открыто. Загружено {recipes.Count} рецептов.");
        }

        gameObject.SetActive(true);
        foreach (var slot in ingredientSlots)
        {
            slot.InitializeCraftingSlot(this);
        }
        ClearAllSlots();
    }

    /// <summary>
    /// Закрывает окно крафта и возвращает предметы в инвентарь.
    /// </summary>
    public void Close()
    {
        ReturnItemsToInventory();
        gameObject.SetActive(false);
        Debug.Log("[CraftingUI] Окно крафта закрыто.");
    }

    /// <summary>
    /// Проверяет, соответствуют ли предметы в слотах какому-либо рецепту.
    /// </summary>
    public void CheckRecipe()
    {
        Debug.Log("--- [CraftingUI] Запущена проверка рецепта ---");
        currentRecipe = null;
        if (resultSlot != null) resultSlot.ClearSlot();
        if (craftButton != null) craftButton.interactable = false;

        if (availableRecipes == null)
        {
            Debug.LogError("[CraftingUI] Ошибка: список доступных рецептов пуст (null).");
            return;
        }

        List<ItemData> currentIngredients = new List<ItemData>();
        foreach (var slot in ingredientSlots)
        {
            if (!slot.IsEmpty())
            {
                currentIngredients.Add(slot.Item);
            }
        }

        if (currentIngredients.Count == 0)
        {
            Debug.Log("[CraftingUI] В слотах нет ингредиентов. Проверка завершена.");
            return;
        }

        Debug.Log($"[CraftingUI] Ингредиентов в слотах: {currentIngredients.Count}. Начинаем сравнение с {availableRecipes.Count} рецептами.");

        foreach (var recipe in availableRecipes)
        {
            Debug.Log($"--> Проверяем рецепт для: '{recipe.Result.itemName}'");
            if (RecipeMatches(recipe, currentIngredients))
            {
                Debug.Log($"<color=green>УСПЕХ! Найден подходящий рецепт: {recipe.name}</color>");
                currentRecipe = recipe;
                if (resultSlot != null) resultSlot.AddItem(recipe.Result);
                if (craftButton != null) craftButton.interactable = true;
                return; // Выходим из цикла, так как рецепт найден
            }
        }
        Debug.Log("<color=yellow>ПРЕДУПРЕЖДЕНИЕ: Ни один из рецептов не подошел.</color>");
    }

    /// <summary>
    /// Сравнивает ингредиенты в слотах с рецептом, игнорируя порядок.
    /// </summary>
    private bool RecipeMatches(RecipeData recipe, List<ItemData> ingredients)
    {
        if (recipe.Ingredients == null || ingredients == null)
        {
            Debug.LogWarning($"[RecipeMatches] Ошибка: список ингредиентов в рецепте '{recipe.name}' или в слотах не инициализирован.");
            return false;
        }

        if (recipe.Ingredients.Count != ingredients.Count)
        {
            Debug.Log($"[RecipeMatches] Несовпадение по количеству: в рецепте {recipe.Ingredients.Count}, в слотах {ingredients.Count}.");
            return false;
        }

        var recipeIngredientIDs = recipe.Ingredients.Select(i => i.itemID).OrderBy(id => id).ToList();
        var currentIngredientIDs = ingredients.Select(i => i.itemID).OrderBy(id => id).ToList();

        // Для детальной отладки выводим списки ID
        StringBuilder recipeSb = new StringBuilder("ID в рецепте: ");
        foreach (var id in recipeIngredientIDs) recipeSb.Append(id + ", ");
        Debug.Log(recipeSb.ToString());

        StringBuilder currentSb = new StringBuilder("ID в слотах: ");
        foreach (var id in currentIngredientIDs) currentSb.Append(id + ", ");
        Debug.Log(currentSb.ToString());

        bool isEqual = recipeIngredientIDs.SequenceEqual(currentIngredientIDs);
        Debug.Log($"[RecipeMatches] Результат сравнения списков: {isEqual}");

        return isEqual;
    }

    /// <summary>
    /// Создает предмет по текущему рецепту.
    /// </summary>
    private void CraftItem()
    {
        Debug.Log("[CraftingUI] Нажата кнопка 'Создать'.");
        if (currentRecipe == null || playerInventory == null)
        {
            Debug.LogError("[CraftingUI] Невозможно создать предмет: рецепт или инвентарь не найдены!");
            return;
        }

        if (playerInventory.AddItem(currentRecipe.Result))
        {
            Debug.Log($"<color=cyan>ПРЕДМЕТ '{currentRecipe.Result.itemName}' УСПЕШНО СОЗДАН и добавлен в инвентарь.</color>");
            foreach (var slot in ingredientSlots)
            {
                slot.ClearSlot();
            }
            CheckRecipe();
        }
        else
        {
            Debug.LogError("[CraftingUI] Не удалось создать предмет: в инвентаре нет места!");
            // Здесь можно добавить UI сообщение для игрока
        }
    }

    private void ReturnItemsToInventory()
    {
        if (playerInventory == null) return;
        foreach (var slot in ingredientSlots)
        {
            if (!slot.IsEmpty())
            {
                playerInventory.AddItem(slot.Item);
                slot.ClearSlot();
            }
        }
    }

    private void ClearAllSlots()
    {
        foreach (var slot in ingredientSlots)
        {
            slot.ClearSlot();
        }
        if (resultSlot != null) resultSlot.ClearSlot();
        CheckRecipe();
    }
}

