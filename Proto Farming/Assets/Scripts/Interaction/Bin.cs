using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bin : WorkStation
{
    [SerializeField] private GameObject binGui;

    private void Update()
    {
        if (binGui == null) { binGui = GameObject.Find("Work Station UI").transform.Find("BinFooter").gameObject; }
    }

    public override void InteractionPopUp()
    {
        gameplayUI.SetActive(true);
        interactionButton.gameObject.SetActive(true);
        

        if (inventoryManager.currentlyEquippedItem == wrench)
        {
            interactionText.SetText("Remove Bin");
        }
        else
        {
            interactionText.SetText("Open Bin");
        }
    }

    public override void InteractWithWorkStation()
    {
        if (inventoryManager.currentlyEquippedItem == wrench)
        {
            RemoveWorkStation();
        }
        else
        {
            gameplayUI.SetActive(false);
            binGui.SetActive(true);
            inventoryManager.ToggleInventory();
            flowerFooter.SetActive(false);
        }
    }
}
