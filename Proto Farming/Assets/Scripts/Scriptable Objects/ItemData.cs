using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum ItemType { Item, Seed, Building, Tool };
public enum ItemCategory { Ingredient, Meal, Hybrid };

[CreateAssetMenu(fileName = "Item Data", menuName = "Inventory/ItemData", order = 1)]
public class ItemData : ScriptableObject
{
    [Header("Key Details")]
    public string itemName;
    public int itemId;
    public Sprite itemIcon;
    [TextArea] public string itemDescription;
    [Tooltip("Used for checking if can interact with diff building types")] public ItemType itemType;
    public ItemCategory itemCategory;
    public GameObject droppedItemPrefab;

    [Header("Stacking Details")]
    public bool canStack;
    public int maxInStack;

    [Header("Building & Plant Data")]
    public GameObject buildPrefab;
    public PlantData plantData;
    public Mesh buildPreview;

    [Header("Food Values")]
    [Tooltip("Used within Venture Mode to replenish hunger")]
    public bool canEat;
    public float replenishAmount;

    [Header("Let Him Cook")]
    public Sprite[] ingridents;

}
