using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChestUI : MonoBehaviour
{
    // Singleton
    private static ChestUI _instance;
    public static ChestUI Instance { get { return _instance; } }

    public List<InventorySlot> chestUiSlots;
    public StorageContainer currentChest;

    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(this.gameObject);
        } else {
            _instance = this;
        }
    }

    public void LoadChestUI(List<ItemData> itemsInChest, List<int> ammountsInChest, StorageContainer chest)
    {
        currentChest = chest;

        for (int slot = 0; slot < chestUiSlots.Count; slot++)
        {
            if (itemsInChest[slot] == null)
            { 
                // Disable slot icon & number
                //slot.slotImage.enabled = false;
                Color col = chestUiSlots[slot].slotImage.color = new Color(chestUiSlots[slot].slotImage.color.r, chestUiSlots[slot].slotImage.color.g, chestUiSlots[slot].slotImage.color.b, 0f);
                chestUiSlots[slot].slotImage.color = col;
                chestUiSlots[slot].numberInSlotText.gameObject.SetActive(false);
                continue; 
            }
            else
            {
                // Update and show slot icon
                if (ammountsInChest[slot] > 1)
                {
                    chestUiSlots[slot].numberInSlot = ammountsInChest[slot];
                    chestUiSlots[slot].numberInSlotText.SetText(ammountsInChest[slot].ToString());
                    chestUiSlots[slot].numberInSlotText.gameObject.SetActive(true);
                }
                else
                {
                    chestUiSlots[slot].numberInSlotText.gameObject.SetActive(false);
                }

                chestUiSlots[slot].itemInSlot = itemsInChest[slot];
                chestUiSlots[slot].slotImage.sprite = itemsInChest[slot].itemIcon;
                Color col = chestUiSlots[slot].slotImage.color = new Color(chestUiSlots[slot].slotImage.color.r, chestUiSlots[slot].slotImage.color.g, chestUiSlots[slot].slotImage.color.b, 1f);
                chestUiSlots[slot].slotImage.color = col;
                chestUiSlots[slot].slotImage.enabled = true;
            }
        }
    }

    public void UpdateChestUI()
    {
        for (int i = 0; i < chestUiSlots.Count; i++)
        {
            if (chestUiSlots[i].itemInSlot == null)
            { 
                // Disable slot icon & number
                //slot.slotImage.enabled = false;
                Color colour = chestUiSlots[i].slotImage.color = new Color(chestUiSlots[i].slotImage.color.r, chestUiSlots[i].slotImage.color.g, chestUiSlots[i].slotImage.color.b, 0f);
                chestUiSlots[i].slotImage.color = colour;
                chestUiSlots[i].numberInSlotText.gameObject.SetActive(false);
                continue; 
            }

            // Update and show slot icon
            if (chestUiSlots[i].numberInSlot > 1)
            {
                chestUiSlots[i].numberInSlotText.SetText(chestUiSlots[i].numberInSlot.ToString());
                chestUiSlots[i].numberInSlotText.gameObject.SetActive(true);
            }
            else
            {
                chestUiSlots[i].numberInSlotText.gameObject.SetActive(false);
            }

            chestUiSlots[i].slotImage.sprite = chestUiSlots[i].itemInSlot.itemIcon;
            Color col = chestUiSlots[i].slotImage.color = new Color(chestUiSlots[i].slotImage.color.r, chestUiSlots[i].slotImage.color.g, chestUiSlots[i].slotImage.color.b, 1f);
            chestUiSlots[i].slotImage.color = col;
            chestUiSlots[i].slotImage.enabled = true;

            currentChest.itemsInChest[i] = chestUiSlots[i].itemInSlot;
            currentChest.ammountsInChest[i] = chestUiSlots[i].numberInSlot;
        }
    }

    // public void CloseChestUI()
    // {
    //     currentChest.CloseChest();  
    // }
}
