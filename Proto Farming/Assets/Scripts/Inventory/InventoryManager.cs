using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class InventoryManager : MonoBehaviour
{
    [SerializeField] FarmingMovement playerMovement;
    [SerializeField] CameraBlender cameraBlender;

    [Header("Equipped Items")]
    public ItemData currentlyEquippedItem;
    public int equippedAmmount;
    [SerializeField] private Image equippedIcon;
    [SerializeField] private TMP_Text equippedNumberText;

    [Header("Inventory")]
    public bool inventoryOpen;
    public bool chestOpen;
    public StorageContainer currentChest;
    public bool cookingPotOpen;
    [SerializeField] private List<InventorySlot> inventorySlots = new List<InventorySlot>();
    [SerializeField] private GameObject inventorySlotsParent;

    [Header("Tools")]
    [SerializeField] private List<InventorySlot> toolSlots = new List<InventorySlot>();

    [Header("Drag & Drop")]
    public ItemData draggingItem;
    public InventorySlot startingSlot;
    public int dragAmmount;
    public bool dragging;

    [Header("Other UI Elements")]
    [SerializeField] private GameObject inventoryObject;
    [SerializeField] private GameObject flowerFooter;
    [SerializeField] private GameObject binUI;
    [SerializeField] private CookingUI cookingPot;
    [SerializeField] private ChestUI chestUI;
    [SerializeField] private GameObject chestIconsParent;
    [SerializeField] private GameObject craftingSlotParents;
    [SerializeField] private GameObject questBoardUI;

    [SerializeField] private List<ItemSaveData> itemSaveDataList;
    [SerializeField] private string fileName;

    [Header("Audio")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip selectfx;
    public event EventHandler<string> OnItemSlotSaved;

    [Header("Particles")]
    [SerializeField] private ParticleSystem itemEquipped;

    private GameObject inventorySlotsObj;
    private GameObject farmingGameplay;

    private void Start()
    {
        if (SaveGameManager.Instance != null)
        {
            SaveGameManager.Instance.OnSaveGame += SaveGameManager_OnSaveGame;
        }

        farmingGameplay = GameObject.Find("Farming Gameplay");

        if (SceneHandler.Instance.GetActiveSceneIndex() == (int)SceneIndexes.FARM_MODE)
        {
            inventorySlotsParent = farmingGameplay.transform.Find("Inventory Parent").gameObject;
        }
        else if (SceneHandler.Instance.GetActiveSceneIndex() == (int)SceneIndexes.VENTURE_MODE)
        {
            inventorySlotsParent = GameObject.Find("AR Game Logic").transform.Find("UI").Find("Inventory Parent").gameObject;

        }

        inventorySlotsObj = inventorySlotsParent.transform.Find("Inventory Slots").gameObject;
        inventorySlotsParent.SetActive(false);

        CloseInventory();
    }

    public void ToggleInventory()
    {
        Debug.Log("Toggled Inventory");

        if (inventoryOpen)
        { CloseInventory(); }
        else
        { OpenInventory(); }
    }

    private void OpenInventory()
    {
        inventoryOpen = true;
        inventoryObject.SetActive(true);
        UpdateInventoryDisplay();

        if (SceneHandler.Instance.GetActiveSceneIndex() == (int)SceneIndexes.FARM_MODE)
        {
            playerMovement.enabled = false;
            flowerFooter.SetActive(false);
            cameraBlender.BlendToInventoryCam();

            if (chestOpen)
            {
                chestUI.gameObject.SetActive(true);
                chestIconsParent.gameObject.SetActive(true);
            }
            else
            {
                chestUI.gameObject.SetActive(false);
                chestIconsParent.gameObject.SetActive(false);
            }
        }

        audioSource.PlayOneShot(selectfx, 1);
    }

    private void CloseInventory()
    {
        inventoryOpen = false;
        inventoryObject.SetActive(false);

        if (SceneHandler.Instance.GetActiveSceneIndex() == (int)SceneIndexes.FARM_MODE)
        {
            playerMovement.enabled = true;
            flowerFooter.SetActive(true);
            cameraBlender.BlendToMovementCam();
            binUI.SetActive(false);
            chestUI.gameObject.SetActive(false);

            if (chestOpen)
            {
                chestOpen = false;
                currentChest.CloseChest();
            }
            
            cookingPot.gameObject.SetActive(false);
            craftingSlotParents.SetActive(false);
        }

        Debug.Log("Closed Inventory");
    }

    public void EquipItem(InventorySlot itemSlot)
    {
        ItemData itemToSwap = null;
        int numToSwap = 0;

        if (currentlyEquippedItem != null)
        {
            itemToSwap = currentlyEquippedItem;
            numToSwap = equippedAmmount;
            
        }
        
        currentlyEquippedItem = itemSlot.itemInSlot;
        equippedAmmount = itemSlot.numberInSlot;

        if (itemToSwap != null)
        {
            itemSlot.itemInSlot = itemToSwap;
            itemSlot.numberInSlot = numToSwap;
            
        }
        else
        {
            
            itemSlot.numberInSlot = 0;
            itemSlot.itemInSlot = null;
        }
        itemEquipped.Play();
        equippedIcon.sprite = currentlyEquippedItem.itemIcon;
        equippedNumberText.SetText("x" + equippedAmmount.ToString());
        UpdateInventoryDisplay();
        CloseInventory();
    }

    public void AddItemToInventory(ItemData item, int numberOfItems)
    {
        bool addedToInventory = false;
        int remainingToAdd = numberOfItems;

        //Add Items to existing slots of the same type
        foreach (InventorySlot slot in inventorySlots)
        {
            if (slot.itemInSlot == item  && slot.numberInSlot < item.maxInStack)
            {
                int numToAdd = item.maxInStack - slot.numberInSlot;
                numToAdd = Mathf.Clamp(numToAdd, 0, remainingToAdd);
                slot.numberInSlot += numToAdd;
                remainingToAdd -= numToAdd;
                remainingToAdd = Mathf.Clamp(remainingToAdd, 0, item.maxInStack);

                numToAdd = 0; //reset ready for next slot;
            }     
        }

        //Add Remainder
        foreach (InventorySlot slot in inventorySlots)
        {
            if (remainingToAdd <= 0) { addedToInventory = true; break; }

            if (slot.itemInSlot == null)
            {
                slot.itemInSlot = item;
                slot.numberInSlot += remainingToAdd;
                addedToInventory = true;
                Debug.Log("Item: " + item.itemName + " was added to inventory.");
                
                break;  
            }
        }
        
        UpdateInventoryDisplay();
        
        if (addedToInventory == false)
        {
            Debug.Log("Item: " + item.itemName + " was not picked up.  Inventory might be full?");
        }
    }

    public void AddToolToToolbelt(ToolData tool)
    {
        bool addedToInventory = false;

        //Add Remainder
        foreach (InventorySlot slot in toolSlots)
        {
            if (slot.itemInSlot == null)
            {
                slot.itemInSlot = tool;
                slot.numberInSlot = 1;
                addedToInventory = true;
                Debug.Log("Item: " + tool.itemName + " was added to inventory.");
                
                break;  
            }
        }
        
        UpdateInventoryDisplay();
        
        if (addedToInventory == false)
        {
            Debug.Log("Item: " + tool.itemName + " was not picked up.  Toolbelt might be full?");
        }
    }

    public void UpdateInventoryDisplay()
    {
        foreach (InventorySlot slot in inventorySlots)
        {
            if (slot.itemInSlot == null)
            {
                // Disable slot icon & number
                Color col = slot.slotImage.color = new Color(slot.slotImage.color.r, slot.slotImage.color.g, slot.slotImage.color.b, 0f);
                slot.slotImage.color = col;

                if (slot.numberInSlotText != null)
                { slot.numberInSlotText.enabled = false; }
                continue; 
            }
            else
            {
                // Update and show slot icon
                slot.slotImage.sprite = slot.itemInSlot.itemIcon;
                Color col = slot.slotImage.color = new Color(slot.slotImage.color.r, slot.slotImage.color.g, slot.slotImage.color.b, 1f);
                slot.slotImage.color = col;

                // Update and show number in slot unless item is not stackable
                if (slot.itemInSlot.canStack != false)
                {
                    slot.numberInSlotText.SetText(slot.numberInSlot.ToString());
                    slot.numberInSlotText.enabled = true;
                }
                else
                {
                    slot.numberInSlotText.enabled = false;
                }
            }
        }

        foreach (InventorySlot slot in toolSlots)
        {
            if (slot.itemInSlot == null)
            { 
                // Disable slot icon & number
                //slot.slotImage.enabled = false;
                Color col = slot.slotImage.color = new Color(slot.slotImage.color.r, slot.slotImage.color.g, slot.slotImage.color.b, 0f);
                slot.slotImage.color = col;
                continue; 
            }
            else
            {
                // Update and show slot icon
                slot.slotImage.sprite = slot.itemInSlot.itemIcon;
                Color col = slot.slotImage.color = new Color(slot.slotImage.color.r, slot.slotImage.color.g, slot.slotImage.color.b, 1f);
                slot.slotImage.color = col;
                slot.slotImage.enabled = true;
            }
        }

        if (chestOpen == true)
        {
            chestUI.UpdateChestUI();
        }
        if (cookingPotOpen)
        {
            cookingPot.UpdateCraftingSlotsUI();
        }
        
        Debug.Log("Updated Inventory & Tools Display");
    }
    public void DropItem(ItemData item, int numOfItems, Vector3 dropPoint)
    {
        if (item == null) { return; } 

        Vector3 randomOffset = new Vector3(UnityEngine.Random.Range(-0.75f, 0.75f), 0f, UnityEngine.Random.Range(-0.75f, -0.25f));
        GameObject droppedObject = Instantiate(item.droppedItemPrefab, dropPoint + randomOffset, Quaternion.identity);
        droppedObject.GetComponent<Collider>().isTrigger = false;
        droppedObject.GetComponent<Rigidbody>().AddExplosionForce(50f, dropPoint + Vector3.up, 1f, 1f, ForceMode.Force);
        droppedObject.GetComponent<Collider>().isTrigger = true;
    }

    public void UpdateEquippedItemDisplay()
    {
        // Return early if there is no equipped item.
        if (currentlyEquippedItem == null) { return; }

        equippedIcon.sprite = currentlyEquippedItem.itemIcon;
        equippedNumberText.SetText("x" + equippedAmmount.ToString());
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

        // Go through each inventory slot and save their data to the struct
        foreach (InventorySlot slot in inventorySlots)
        {
            slot.SaveData();
        }

        // Go through each tool slot and save their data to the struct.
        foreach (InventorySlot slot in toolSlots)
        {
            slot.SaveData();
        }

        // Retrieve the itemSaveData from the slots
        for (int i = 0; i < inventorySlots.Count; i++)
        {
            itemSaveDataList.Add(inventorySlots[i].GetItemSaveData());
        }

        //Tool slots
        int toolSlotIndex = 0;
        Debug.Log($"InvSlots: {inventorySlots.Count} and ItemSaveData: {itemSaveDataList.Count}");
        for (int i = inventorySlots.Count; i < inventorySlots.Count + 4; i++)
        {
            itemSaveDataList.Add(toolSlots[toolSlotIndex].GetItemSaveData());
            toolSlotIndex++;
        }

        if (currentlyEquippedItem != null && SceneHandler.Instance.GetActiveSceneIndex() != (int)SceneIndexes.VENTURE_MODE)
        {
            // Save equipped item data.
            ItemSaveData equippedSaveData = new ItemSaveData();
            equippedSaveData.itemName = currentlyEquippedItem.itemName;
            equippedSaveData.itemType = currentlyEquippedItem.itemType.ToString();
            equippedSaveData.itemCategory = currentlyEquippedItem.itemCategory.ToString();
            equippedSaveData.numberInSlot = equippedAmmount;

            // Equipped item
            itemSaveDataList.Add(equippedSaveData);
        }

        FBFileHandler.SaveToJSON<ItemSaveData>(itemSaveDataList, fileName);
        OnItemSlotSaved?.Invoke(this, JsonHelper.ToJson<ItemSaveData>(itemSaveDataList.ToArray()));
    }


    #endregion

    #region Getters & Setters

    public List<InventorySlot> GetInventorySlots() => inventorySlots;

    public List<InventorySlot> GetToolSlots() => toolSlots;

    public List<ItemSaveData> GetItemSaveDataList() => itemSaveDataList;

    public void SetInventoryOpen(bool state) => inventoryOpen = state;

    #endregion
}
