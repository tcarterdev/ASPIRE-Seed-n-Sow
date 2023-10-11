using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class POI_Menu : MonoBehaviour
{
    private AR_Inventory ar_Inventory;

    [SerializeField] private GameObject poiMenu;
    [SerializeField] private TMP_Text poiNameText;
    [SerializeField] private Image rewardImage;
    [SerializeField] private TMP_Text rewardItemText;
    [SerializeField] private TMP_Text warningText;

    [Space]
    public POI poi;
    public ItemData rewardItem;

    private InventoryManager inventoryManager;

    private void Awake()
    {
        ar_Inventory = GetComponent<AR_Inventory>();

        inventoryManager = FindObjectOfType<InventoryManager>();
    }

    public void TogglePoiMenu(string poiName)
    {
        if (poiMenu.activeInHierarchy)
        {
            poiMenu.SetActive(false);
            rewardItem = null;
        }
        else
        {
            if (rewardImage != null)
            {
                rewardImage.sprite = rewardItem.itemIcon;
                rewardImage.gameObject.SetActive(true);
                rewardItemText.SetText(rewardItem.itemName);
                rewardItemText.gameObject.SetActive(true);
            }
            else
            {
                rewardItemText.gameObject.SetActive(false);
                rewardImage.gameObject.SetActive(false);
            }

            poiMenu.SetActive(true);
            poiNameText.SetText(poiName);
        }
    }

    public void CollectReward()
    {
        // Get HungerManager
        HungerManager hungerManager = FindObjectOfType<HungerManager>();

        warningText.text = string.Empty;

        // Return early if the player has no hunger.
        if (hungerManager.GetCurrentHunger() <= 0) 
        {
            warningText.text = "You have no hunger, unable to collect reward.";
            return; 
        }

        bool doubleReward = false;

        ItemSaveData newItemSaveData = new ItemSaveData();

        // Set up new item.
        newItemSaveData.itemName = rewardItem.itemName;

        int xpToAdd = 0;
        // Check hunger state
        if (hungerManager.GetCurrentHunger() >= 75)
        {
            Debug.Log("Double Rewards!");

            doubleReward = true;

            if (rewardItem.canStack)
            {
                // Extra reward for hunger level.
                newItemSaveData.numberInSlot++;
            }

            // Increase item collected quest(s).
            QuestProgress.Instance.InvokeItemCollected();

            // More XP
            xpToAdd += (int)XPValues.COLLECT;
        }
        else if (hungerManager.GetCurrentHunger() >= 50 && hungerManager.GetCurrentHunger() < 75)
        {
            Debug.Log("More XP!");

            // More XP
            xpToAdd += (int)XPValues.COLLECT / 2;
        }

        // Standard reward.
        xpToAdd += (int)XPValues.COLLECT;
        newItemSaveData.numberInSlot++;

        // If the item can stack, add double to the inventory
        if (rewardItem.canStack)
        {
            // Add collected item.
            ar_Inventory.collectedItems.Add(newItemSaveData);

            // Add to inventory. 
            inventoryManager.AddItemToInventory(rewardItem, newItemSaveData.numberInSlot);
        }
        else
        {
            // If it's a double reward, add an extra item.
            if (doubleReward)
            {
                // Add collected item.
                ar_Inventory.collectedItems.Add(newItemSaveData);

                // Add to inventory. 
                inventoryManager.AddItemToInventory(rewardItem, 1);
            }

            // Add collected item.
            ar_Inventory.collectedItems.Add(newItemSaveData);

            // Add to inventory. 
            inventoryManager.AddItemToInventory(rewardItem, 1);
        }

        inventoryManager.UpdateInventoryDisplay();

        // Add EXP for collecting items.
        AccountLevel.Instance.AddXP(xpToAdd);

        rewardItem = null;
        TogglePoiMenu(null);
        poi.StartCoolDown();

        // Increase item collected quest(s).
        QuestProgress.Instance.InvokeItemCollected();
    }
}