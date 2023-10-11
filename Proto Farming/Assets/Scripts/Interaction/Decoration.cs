using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Decoration : WorkStation
{
    public override void InteractionPopUp()
    {
        if (inventoryManager.currentlyEquippedItem == wrench)
        {
            interactionText.SetText("Remove furniture");
            gameplayUI.SetActive(true);
            interactionButton.gameObject.SetActive(true);
        }
    }

    public override void InteractWithWorkStation()
    {
        if (inventoryManager.currentlyEquippedItem == wrench)
        {
            interactionText.SetText("Remove Cooking Station");
            RemoveWorkStation();
        }
    }
}
