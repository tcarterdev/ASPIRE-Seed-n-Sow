using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CookingUI : MonoBehaviour
{
    private InventoryManager inventoryManager;
    [SerializeField] private Button craftButton;

    [Space]

    [SerializeField] private ItemData currentCraftableItem;
    [SerializeField] private InventorySlot[] recipeSlots;
    [SerializeField] private RecipeData[] cookingRecipes;

    private void Awake()
    {
        inventoryManager = GameObject.FindGameObjectWithTag("GameController").GetComponent<InventoryManager>();
    }

    public void UpdateCraftingSlotsUI()
    {
        int ingredientsTotal = 0;

        foreach(InventorySlot slot in recipeSlots)
        {
            if (slot.itemInSlot != null) 
            { 
                slot.slotImage.color = Color.white;
                slot.slotImage.sprite = slot.itemInSlot.itemIcon;
                ingredientsTotal += slot.itemInSlot.itemId;
            }
            else
            {   
                Color zeroAlpha = Color.white;
                zeroAlpha.a = 0f;
                slot.slotImage.color = zeroAlpha;
            }    
        }

        if (ingredientsTotal > 0)
        {
            UpdateRecipeOutput(ingredientsTotal);
        }
        else
        {
            craftButton.interactable = false;
        }

        Debug.Log("Update crafting UI");
    }

    private void UpdateRecipeOutput(int ingredientsTotal)
    {
        Debug.Log("Ingredients Total: " + ingredientsTotal.ToString());

        foreach(RecipeData recipe in cookingRecipes)
        {
            if (ingredientsTotal == recipe.itemsCombinedValue)
            {
                Debug.Log("You can craft: " + recipe.resultItem.itemName);
                currentCraftableItem = recipe.resultItem;
                craftButton.interactable = true;
                break;
            }
        }
    }

    public void CraftItem()
    {
        // Firebase Analytics - Food made tracking.
        DataGathering.dataGathering.Firebase_FoodMade(currentCraftableItem.itemName);

        inventoryManager.AddItemToInventory(currentCraftableItem, 1);
        craftButton.interactable = false;

        foreach (InventorySlot slot in recipeSlots)
        {
            slot.itemInSlot = null;
            slot.numberInSlot = 0;
        }

        inventoryManager.UpdateInventoryDisplay();

        // Invoke item cooked quest event.
        QuestProgress.Instance.InvokeItemCooked();

        // Give XP to the player.
        if (AccountLevel.Instance != null)
        {
            AccountLevel.Instance.AddXP((int)XPValues.COOK);
            AccountLevel.Instance.AddXP((int)XPValues.COLLECT);
        }
    }
}
