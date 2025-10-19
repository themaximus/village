using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Linq;
using System.Text; // ��������� ��� StringBuilder

/// <summary>
/// ��������� ����������� ���� ������.
/// </summary>
public class CraftingUI : MonoBehaviour
{
    [Header("UI Components")]
    [Tooltip("������ ������ ��� ������������.")]
    public List<UniversalSlotUI> ingredientSlots;
    [Tooltip("���� ��� ����������� ���������� ������.")]
    public UniversalSlotUI resultSlot;
    [Tooltip("������ ��� �������� ��������.")]
    public Button craftButton;

    private List<RecipeData> availableRecipes;
    private Inventory playerInventory;
    private RecipeData currentRecipe;

    private void Awake()
    {
        playerInventory = FindObjectOfType<Inventory>();
        if (playerInventory == null)
        {
            Debug.LogError("[CraftingUI] �� ������ ��������� ������ (Inventory) � �����!");
        }

        if (craftButton != null)
        {
            craftButton.onClick.AddListener(CraftItem);
        }
    }

    /// <summary>
    /// ��������� ���� ������ � ��������� � ���� ��������� �������.
    /// </summary>
    public void Open(List<RecipeData> recipes)
    {
        if (recipes == null || recipes.Count == 0)
        {
            Debug.LogWarning("[CraftingUI] ������� ������, �� ��� �� �������� �� ������ �������.");
            this.availableRecipes = new List<RecipeData>();
        }
        else
        {
            this.availableRecipes = recipes;
            Debug.Log($"[CraftingUI] ���� ������ �������. ��������� {recipes.Count} ��������.");
        }

        gameObject.SetActive(true);
        foreach (var slot in ingredientSlots)
        {
            slot.InitializeCraftingSlot(this);
        }
        ClearAllSlots();
    }

    /// <summary>
    /// ��������� ���� ������ � ���������� �������� � ���������.
    /// </summary>
    public void Close()
    {
        ReturnItemsToInventory();
        gameObject.SetActive(false);
        Debug.Log("[CraftingUI] ���� ������ �������.");
    }

    /// <summary>
    /// ���������, ������������� �� �������� � ������ ������-���� �������.
    /// </summary>
    public void CheckRecipe()
    {
        Debug.Log("--- [CraftingUI] �������� �������� ������� ---");
        currentRecipe = null;
        if (resultSlot != null) resultSlot.ClearSlot();
        if (craftButton != null) craftButton.interactable = false;

        if (availableRecipes == null)
        {
            Debug.LogError("[CraftingUI] ������: ������ ��������� �������� ���� (null).");
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
            Debug.Log("[CraftingUI] � ������ ��� ������������. �������� ���������.");
            return;
        }

        Debug.Log($"[CraftingUI] ������������ � ������: {currentIngredients.Count}. �������� ��������� � {availableRecipes.Count} ���������.");

        foreach (var recipe in availableRecipes)
        {
            Debug.Log($"--> ��������� ������ ���: '{recipe.Result.itemName}'");
            if (RecipeMatches(recipe, currentIngredients))
            {
                Debug.Log($"<color=green>�����! ������ ���������� ������: {recipe.name}</color>");
                currentRecipe = recipe;
                if (resultSlot != null) resultSlot.AddItem(recipe.Result);
                if (craftButton != null) craftButton.interactable = true;
                return; // ������� �� �����, ��� ��� ������ ������
            }
        }
        Debug.Log("<color=yellow>��������������: �� ���� �� �������� �� �������.</color>");
    }

    /// <summary>
    /// ���������� ����������� � ������ � ��������, ��������� �������.
    /// </summary>
    private bool RecipeMatches(RecipeData recipe, List<ItemData> ingredients)
    {
        if (recipe.Ingredients == null || ingredients == null)
        {
            Debug.LogWarning($"[RecipeMatches] ������: ������ ������������ � ������� '{recipe.name}' ��� � ������ �� ���������������.");
            return false;
        }

        if (recipe.Ingredients.Count != ingredients.Count)
        {
            Debug.Log($"[RecipeMatches] ������������ �� ����������: � ������� {recipe.Ingredients.Count}, � ������ {ingredients.Count}.");
            return false;
        }

        var recipeIngredientIDs = recipe.Ingredients.Select(i => i.itemID).OrderBy(id => id).ToList();
        var currentIngredientIDs = ingredients.Select(i => i.itemID).OrderBy(id => id).ToList();

        // ��� ��������� ������� ������� ������ ID
        StringBuilder recipeSb = new StringBuilder("ID � �������: ");
        foreach (var id in recipeIngredientIDs) recipeSb.Append(id + ", ");
        Debug.Log(recipeSb.ToString());

        StringBuilder currentSb = new StringBuilder("ID � ������: ");
        foreach (var id in currentIngredientIDs) currentSb.Append(id + ", ");
        Debug.Log(currentSb.ToString());

        bool isEqual = recipeIngredientIDs.SequenceEqual(currentIngredientIDs);
        Debug.Log($"[RecipeMatches] ��������� ��������� �������: {isEqual}");

        return isEqual;
    }

    /// <summary>
    /// ������� ������� �� �������� �������.
    /// </summary>
    private void CraftItem()
    {
        Debug.Log("[CraftingUI] ������ ������ '�������'.");
        if (currentRecipe == null || playerInventory == null)
        {
            Debug.LogError("[CraftingUI] ���������� ������� �������: ������ ��� ��������� �� �������!");
            return;
        }

        if (playerInventory.AddItem(currentRecipe.Result))
        {
            Debug.Log($"<color=cyan>������� '{currentRecipe.Result.itemName}' ������� ������ � �������� � ���������.</color>");
            foreach (var slot in ingredientSlots)
            {
                slot.ClearSlot();
            }
            CheckRecipe();
        }
        else
        {
            Debug.LogError("[CraftingUI] �� ������� ������� �������: � ��������� ��� �����!");
            // ����� ����� �������� UI ��������� ��� ������
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

