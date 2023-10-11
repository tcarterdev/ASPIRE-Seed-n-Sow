using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;

public class InventorySlot : MonoBehaviour
{
    [SerializeField] private InventoryManager inventoryManager;

    [Header("Item Details")]
    public ItemData itemInSlot;
    public int numberInSlot;

    [Header("Slot Object References")]
    public Image slotImage;
    public TMP_Text numberInSlotText;

    [SerializeField] private ItemSaveData itemSaveData = new ItemSaveData();

    private void Awake()
    {
        if (SceneHandler.Instance.GetActiveSceneIndex() == (int)SceneIndexes.FARM_MODE)
        {
            inventoryManager = GameObject.FindGameObjectWithTag("GameController").GetComponent<InventoryManager>();
        }
        else if (SceneHandler.Instance.GetActiveSceneIndex() == (int)SceneIndexes.VENTURE_MODE)
        {
            inventoryManager = GameObject.Find("AR Game Logic").GetComponent<InventoryManager>();
        }
    }

    private void Start()
    {
        //inventoryManager.GetInventorySlots().Add(this);
        //inventoryManager.GetItemSaveDataList().Add(itemSaveData);
    }

    #region Save & Load

    /// <summary>
    /// Saves the item name and amount to itemSaveData. 
    /// </summary>
    public void SaveData()
    {
        Debug.Log("Saving slot data");

        if (itemInSlot != null) 
        { 
            itemSaveData.itemName = itemInSlot.itemName;
            itemSaveData.itemType = itemInSlot.itemType.ToString();
            itemSaveData.itemCategory = itemInSlot.itemCategory.ToString();
        }
        else
        {
            itemSaveData.itemName = string.Empty;
            itemSaveData.itemType = string.Empty;
            itemSaveData.itemCategory = string.Empty;
        }

        itemSaveData.numberInSlot = numberInSlot;
    }

    /// <summary>
    /// Loads the itemSaveData and sets it to the game object.
    /// </summary>
    public void LoadData()
    {
        InventoryItems inventoryItems = FindObjectOfType<InventoryItems>();

        if (itemSaveData.itemType == ItemType.Tool.ToString())
        {
            Debug.Log("Loading tool");
            itemInSlot = inventoryItems.GetTool(itemSaveData.itemName);
        }
        else
        {
            Debug.Log($"Loading: {itemSaveData.itemName}; Category: {itemSaveData.itemCategory}");
      
            itemInSlot = inventoryItems.GetItem(itemSaveData.itemName, itemSaveData.itemCategory);
        }

        numberInSlot = itemSaveData.numberInSlot;

        inventoryManager.UpdateInventoryDisplay();
    }

    #endregion

    #region Getters & Setters

    /// <summary>
    /// Retrieves the itemSaveData of the inventory slot.
    /// </summary>
    /// <returns></returns>
    public ItemSaveData GetItemSaveData() => itemSaveData;

    public void SetSlotData(string name, int number, string itemType, string itemCategory)
    {
        itemSaveData.itemName = name;
        itemSaveData.numberInSlot = number;

        itemSaveData.itemType = itemType;
        itemSaveData.itemCategory = itemCategory;
    }

    public InventoryManager GetInventoryManager() => inventoryManager;

    #endregion

}