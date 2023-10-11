using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TreeStump : WorkStation
{
    [SerializeField] private ToolData shovel;

    private void Start()
    {
        Vector2Int posInGrid = new Vector2Int(Mathf.RoundToInt(this.transform.position.x), Mathf.RoundToInt(this.transform.position.z));
        farmGrid.boolMap[posInGrid.x, posInGrid.y] = true;
    }

    public override void OnTriggerEnter(Collider other)
    {
        if (other.gameObject != player) { return; }

        InteractionPopUp();
    }

    // This activates after the player has left the trigger (Box Collider)
    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject != player) { return; }

        if (interactionButton.gameObject.activeInHierarchy)
        {
            interactionButton.gameObject.SetActive(false);
        }
    }

    public override void InteractionPopUp()
    {   
        if (inventoryManager.currentlyEquippedItem == shovel) 
        { 
            interactionText.SetText("Dig up stump");
            interactionButton.gameObject.SetActive(true);
        }
    }

    public override void InteractWithWorkStation()
    {
        RemoveWorkStation(); 
    }
}
