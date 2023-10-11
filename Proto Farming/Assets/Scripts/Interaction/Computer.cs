using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Computer : WorkStation
{
    [SerializeField] private GameObject computerUI;
    [SerializeField] private GameObject gamePlayUI;

    public override void InteractionPopUp()
    {
        gameplayUI.SetActive(true);
        interactionButton.gameObject.SetActive(true);

        if (inventoryManager.currentlyEquippedItem == wrench)
        {
            interactionText.SetText("Remove Computer");
        }
        else
        {
            interactionText.SetText("Use Computer");
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
            interactionText.SetText("Use Computer");
            gamePlayUI.SetActive(false);
            computerUI.SetActive(true);
        } 
    }
}
