using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CookingPot : WorkStation
{
    [SerializeField] private GameObject cookingUi;
    [SerializeField] private GameObject craftingItemSlots;

    private void Update()
    {
        if (cookingUi == null) { cookingUi = GameObject.Find("Work Station UI").transform.Find("CookingStation").gameObject; }

        if (craftingItemSlots == null) { craftingItemSlots = GameObject.Find("Farming Gameplay").transform.Find("Inventory Parent").transform.Find("Crafting Slots").gameObject; }
    }

    public override void InteractionPopUp()
    {
        gameplayUI.SetActive(true);
        interactionButton.gameObject.SetActive(true);

        if (inventoryManager.currentlyEquippedItem == wrench)
        {
            interactionText.SetText("Remove Cooking Station");
        }
        else
        {
            interactionText.SetText("Open Cooking Station");
        }
    }

    public override void InteractWithWorkStation()
    {
        if (inventoryManager.currentlyEquippedItem == wrench)
        {
            interactionText.SetText("Remove Cooking Station");
            RemoveWorkStation();
        }
        else
        {
            interactionText.SetText("Open Cooking Station");
            inventoryManager.ToggleInventory();
            cookingUi.SetActive(true);
            craftingItemSlots.SetActive(true);
            inventoryManager.cookingPotOpen = true;
        } 
    }

    public void StopCooking()
    {
        cookingUi.SetActive(false);
        inventoryManager.ToggleInventory();
        craftingItemSlots.SetActive(false);
        inventoryManager.cookingPotOpen = false;
    }
}
