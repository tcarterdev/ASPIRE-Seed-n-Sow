using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InventoryItems : MonoBehaviour
{
    public ItemData GetItem(string itemToLoadName, string itemToLoadCategory)
    {
        if (itemToLoadCategory == ItemCategory.Ingredient.ToString()) { return GetIngredient(itemToLoadName); }
        else if (itemToLoadCategory == ItemCategory.Meal.ToString()) { return GetMeal(itemToLoadName); }
        else if (itemToLoadCategory == ItemCategory.Hybrid.ToString()) { return GetHybrid(itemToLoadName); }

        return null;
    }
    
    public ItemData GetTool(string itemToLoadName, string itemToLoadType)
    {
        return null;
    }

    public ItemData GetIngredient(string itemToLoadName)
    {
        switch (itemToLoadName)
        {
            case "Apple":
                return Resources.Load<ItemData>($"Items/ingredients/sco_apple");

            case "Beans":
                return Resources.Load<ItemData>($"Items/ingredients/sco_beans");

            case "Beef":
                return Resources.Load<ItemData>($"Items/ingredients/sco_beef");

            case "Bread":
                return Resources.Load<ItemData>($"Items/ingredients/sco_bread");

            case "Carrots":
                return Resources.Load<ItemData>($"Items/ingredients/sco_carrots");

            case "Cheese":
                return Resources.Load<ItemData>($"Items/ingredients/sco_cheese");

            case "Chicken":
                return Resources.Load<ItemData>($"Items/ingredients/sco_chicken");

            case "Chick Peas":
                return Resources.Load<ItemData>($"Items/ingredients/sco_chickpeas");

            case "Curry Powder":
                return Resources.Load<ItemData>($"Items/ingredients/sco_curry_powder");

            case "Dough":
                return Resources.Load<ItemData>($"Items/ingredients/sco_dough");

            case "Egg":
                return Resources.Load<ItemData>($"Items/ingredients/sco_egg");

            case "Eggplant":
                return Resources.Load<ItemData>($"Items/ingredients/sco_eggplant");

            case "Fish":
                return Resources.Load<ItemData>($"Items/ingredients/sco_fish");

            case "Flour":
                return Resources.Load<ItemData>($"Items/ingredients/sco_flour");

            case "Fruit Medley":
                return Resources.Load<ItemData>($"Items/ingredients/sco_fruit_medley");

            case "Ginger":
                return Resources.Load<ItemData>($"Items/ingredients/sco_ginger");

            case "Leek":
                return Resources.Load<ItemData>($"Items/ingredients/sco_leek");

            case "Lemon":
                return Resources.Load<ItemData>($"Items/ingredients/sco_lemon");

            case "Lettuce":
                return Resources.Load<ItemData>($"Items/ingredients/sco_lettuce");

            case "Milk":
                return Resources.Load<ItemData>($"Items/ingredients/sco_milk");

            case "Mushroom":
                return Resources.Load<ItemData>($"Items/ingredients/sco_mushroom");

            case "Oats":
                return Resources.Load<ItemData>($"Items/ingredients/sco_oats");

            case "Onion":
                return Resources.Load<ItemData>($"Items/ingredients/sco_onion");

            case "Orange":
                return Resources.Load<ItemData>($"Items/ingredients/sco_orange");

            case "Pasta":
                return Resources.Load<ItemData>($"Items/ingredients/sco_pasta");

            case "Peach":
                return Resources.Load<ItemData>($"Items/ingredients/sco_peach");

            case "Pepper":  // Spice
                return Resources.Load<ItemData>($"Items/ingredients/sco_pepper (spice)");

            case "Pork":
                return Resources.Load<ItemData>($"Items/ingredients/sco_pork");

            case "PO-TAY-TOE":
                return Resources.Load<ItemData>($"Items/ingredients/sco_potato");

            case "Pumpkin":
                return Resources.Load<ItemData>($"Items/ingredients/sco_pumpkin");

            case "Rice":
                return Resources.Load<ItemData>($"Items/ingredients/sco_rice");

            case "Salt":
                return Resources.Load<ItemData>($"Items/ingredients/sco_salt");

            case "Squid":
                return Resources.Load<ItemData>($"Items/ingredients/sco_squid");

            case "Strawberry":
                return Resources.Load<ItemData>($"Items/ingredients/sco_strawberry");

            case "Sugar":
                return Resources.Load<ItemData>($"Items/ingredients/sco_sugar");

            case "Tomato":
                return Resources.Load<ItemData>($"Items/ingredients/sco_tomato");

            case "Tuna":
                return Resources.Load<ItemData>($"Items/ingredients/sco_tuna");

            case "Veg Medley":
                return Resources.Load<ItemData>($"Items/ingredients/sco_veg_medley");

            case "Water":
                return Resources.Load<ItemData>($"Items/ingredients/sco_water");

            case "Wheat":
                return Resources.Load<ItemData>($"Items/ingredients/sco_wheat");

            case "Wheat Seeds":
                return Resources.Load<ItemData>($"Items/ingredients/sco_wheat_seeds");

            case "Zuccini":
                return Resources.Load<ItemData>($"Items/ingredients/sco_zuccini");
        }

        return null;
    }

    public ItemData GetMeal(string itemToLoadName)
    {
        switch (itemToLoadName)
        {
            case "BLT":
                return Resources.Load<ItemData>($"Items/Meals/{itemToLoadName}");

            case "Bolognese":
                return Resources.Load<ItemData>($"Items/Meals/{itemToLoadName}");

            case "Caesar Salad":
                return Resources.Load<ItemData>($"Items/Meals/{itemToLoadName}");

            case "Calamari":
                return Resources.Load<ItemData>($"Items/Meals/{itemToLoadName}");

            case "Carbonara":
                return Resources.Load<ItemData>($"Items/Meals/{itemToLoadName}");

            case "Carrot & Pumpkin Soup":
                return Resources.Load<ItemData>($"Items/Meals/{itemToLoadName}");

            case "Carrot Cake":
                return Resources.Load<ItemData>($"Items/Meals/{itemToLoadName}");

            case "Chili Con Carne":
                return Resources.Load<ItemData>($"Items/Meals/{itemToLoadName}");

            case "Chili Fried Rice":
                return Resources.Load<ItemData>($"Items/Meals/{itemToLoadName}");

            case "Curry":
                return Resources.Load<ItemData>($"Items/Meals/{itemToLoadName}");

            case "Egg Fried Rice":
                return Resources.Load<ItemData>($"Items/Meals/{itemToLoadName}");

            case "Egg Sandwich":
                return Resources.Load<ItemData>($"Items/Meals/{itemToLoadName}");

            case "Fish Dinner":
                return Resources.Load<ItemData>($"Items/Meals/{itemToLoadName}");

            case "French Onion Soup":
                return Resources.Load<ItemData>($"Items/Meals/{itemToLoadName}");

            case "Fruit Salad":
                return Resources.Load<ItemData>($"Items/Meals/{itemToLoadName}");

            case "Ginger Cookie":
                return Resources.Load<ItemData>($"Items/Meals/{itemToLoadName}");

            case "Hummus":
                return Resources.Load<ItemData>($"Items/Meals/{itemToLoadName}");

            case "Jacket Potato & Tuna":
                return Resources.Load<ItemData>($"Items/Meals/{itemToLoadName}");

            case "Leak & Potato Stew":
                return Resources.Load<ItemData>($"Items/Meals/{itemToLoadName}");

            case "Mushroom Risotto":
                return Resources.Load<ItemData>($"Items/Meals/{itemToLoadName}");

            case "Oat Pancakes":
                return Resources.Load<ItemData>($"Items/Meals/{itemToLoadName}");

            case "Pasta & Meatballs":
                return Resources.Load<ItemData>($"Items/Meals/{itemToLoadName}");

            case "Porridge":
                return Resources.Load<ItemData>($"Items/Meals/{itemToLoadName}");

            case "Pumpkin Pie":
                return Resources.Load<ItemData>($"Items/Meals/{itemToLoadName}");

            case "Scrambled Eggs":
                return Resources.Load<ItemData>($"Items/Meals/{itemToLoadName}");

            case "Steak Dinner":
                return Resources.Load<ItemData>($"Items/Meals/{itemToLoadName}");

            case "Strawberries & Cream":
                return Resources.Load<ItemData>($"Items/Meals/{itemToLoadName}");

            case "Tuna Pasta Bake":
                return Resources.Load<ItemData>($"Items/Meals/{itemToLoadName}");
        }

        return null;
    }

    public ItemData GetHybrid(string itemToLoadName)
    {
        switch (itemToLoadName)
        {
            case "Pepper":
                return Resources.Load<ItemData>($"Items/ingredients/sco_pepper");
        }

        return null;
    }

    public PlantData GetPlant(string plantToLoadName)
    {
        switch (plantToLoadName)
        {
            case "Wheat":
                return Resources.Load<PlantData>($"Items/Ingriedents/{plantToLoadName}");
        }

        return null;
    }

    public ItemData GetTool(string toolToLoadName)
    {
        switch (toolToLoadName)
        {
            case "Axe":
                return Resources.Load<ItemData>($"Tools/{toolToLoadName}");

            case "Shovel":
                return Resources.Load<ItemData>($"Tools/{toolToLoadName}");

            case "Watering Can":
                return Resources.Load<ItemData>($"Tools/{toolToLoadName}");

            case "Wrench":
                return Resources.Load<ItemData>($"Tools/{toolToLoadName}");
        }

        return null;
    }
}
