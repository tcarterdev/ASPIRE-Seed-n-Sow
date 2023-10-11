using System;
using System.IO;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StorageContainer : WorkStation
{
    public List<ItemData> itemsInChest = new List<ItemData>();
    public List<int> ammountsInChest = new List<int>();
    public InventorySlot[] chestUISlots;
    [SerializeField] private ChestUI chestUi;

    [SerializeField] private ChestSaveData chestSaveData;

    [SerializeField] private string fileName;

    public event EventHandler<string> OnChestSaved;

    private InventoryItems inventoryItems;

    private void Start()
    {
        chestSaveData = new ChestSaveData();

        chestSaveData.listOfItems = new List<ItemSaveData>();

        fileName = $"{fileName}_{Guid.NewGuid()}.json";

        if (SaveGameManager.Instance != null)
        {
            SaveGameManager.Instance.OnSaveGame += SaveGameManager_OnSaveGame;
        }
    }

    private void Update()
    {
        if (chestUi == null) { chestUi = GameObject.Find("Work Station UI").transform.Find("Chest GUI").GetComponent<ChestUI>(); }
    }

    public override void InteractionPopUp()
    {
        interactionButton.gameObject.SetActive(true);
        interactionText.SetText(buttonPrompt);

        if (inventoryManager.currentlyEquippedItem == wrench)
        {
            interactionText.SetText("Remove chest");
        }
        else
        {
            interactionText.SetText("Open chest");
        }
    }

    public override void InteractWithWorkStation()
    {
        if (inventoryManager.currentlyEquippedItem == wrench)
        {
            SpewOutItems();
            RemoveWorkStation();
        }
        else    
        {      
            inventoryManager.chestOpen = true;
            inventoryManager.currentChest = this;
            interactionButton.gameObject.SetActive(false);
            chestUi.gameObject.SetActive(true);
            inventoryManager.ToggleInventory();
            flowerFooter.SetActive(false);
            chestUi.LoadChestUI(itemsInChest, ammountsInChest, this);
        }
    }

    public void CloseChest()
    {
        for (int i = 0; i < itemsInChest.Count; i++)
        {
            itemsInChest[i] = ChestUI.Instance.chestUiSlots[i].itemInSlot;
            ammountsInChest[i] = ChestUI.Instance.chestUiSlots[i].numberInSlot;
        }

        inventoryManager.chestOpen = false;
    }

    private void SpewOutItems()
    {
        for (int i = 0; i < itemsInChest.Count; i++)
        {
            if (itemsInChest[i] != null)
            {
                inventoryManager.DropItem(itemsInChest[i], ammountsInChest[i], this.transform.position);
                ammountsInChest[i] = 0;
                itemsInChest[i] = null;
            }
        }
    }

    #region Save & Load

    /// <summary>
    /// Invoked from SaveGameManager, saves the data for the inventory slots. 
    /// </summary>
    /// <param name="sender">The object that sent the invoke.</param>
    /// <param name="e">Variable pass-through, in this case it will be empty.</param>
    private void SaveGameManager_OnSaveGame(object sender, EventArgs e)
    {
        Debug.Log("Saving slots");

        chestSaveData.listOfItems.Clear();

        // Save item name and amount.
        for (int i = 0; i < itemsInChest.Count; i++)
        {
            // Continue if there is no item. 
            if (itemsInChest[i] == null) { continue; }

            // Create a temp item data. 
            ItemSaveData itemData = new ItemSaveData();

            // Update the item name and number in slot. 
            itemData.itemName = itemsInChest[i].itemName;
            itemData.numberInSlot = ammountsInChest[i];
            itemData.itemType = itemsInChest[i].itemType.ToString();
            itemData.itemCategory = itemsInChest[i].itemCategory.ToString();

            // Add item data to the list.
            chestSaveData.listOfItems.Add(itemData);
        }

        chestSaveData.position = transform.position;
        chestSaveData.rotation = transform.rotation;

        List<ChestSaveData> tempList = new List<ChestSaveData>();
        tempList.Add(chestSaveData);

        ChestSaveManager.Instance.AddChestToList(chestSaveData);

        //FBFileHandler.SaveToJSON<ChestSaveData>(tempList, fileName);
        //OnChestSaved?.Invoke(this, JsonHelper.ToJson<ChestSaveData>(tempList.ToArray()));
    }

    #endregion

    public void LoadData()
    {
        // Update position and rotation. 
        transform.position = chestSaveData.position;
        transform.rotation = chestSaveData.rotation;

        for (int i = 0; i < chestSaveData.listOfItems.Count; i++)
        {
            if (itemsInChest[i].itemName != "")
            {
                // Load the items. 
                if (chestSaveData.listOfItems[i].itemType == ItemType.Tool.ToString())
                {
                    itemsInChest[i] = inventoryItems.GetTool(chestSaveData.listOfItems[i].itemName);
                }
                else
                {
                    itemsInChest[i] = inventoryItems.GetItem(chestSaveData.listOfItems[i].itemName, chestSaveData.listOfItems[i].itemCategory);
                }
            }

            // Load the amounts.
            ammountsInChest[i] = chestSaveData.listOfItems[i].numberInSlot;
        }
    }

    #region Getters & Setters

    public ChestSaveData GetChestSaveData() => chestSaveData;

    public void SetChestSaveData(Vector3 position, Quaternion rotation, List<ItemSaveData> listOfItems)
    {
        chestSaveData.position = position;
        chestSaveData.rotation = rotation;

        chestSaveData.listOfItems = listOfItems;
    }

    #endregion
}