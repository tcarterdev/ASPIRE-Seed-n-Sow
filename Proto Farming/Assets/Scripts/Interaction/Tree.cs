using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tree : WorkStation
{
    [SerializeField] private ToolData axe;

     private void Start()
    {
        Vector2Int posInGrid = new Vector2Int(Mathf.RoundToInt(this.transform.position.x), Mathf.RoundToInt(this.transform.position.z));
        farmGrid.boolMap[posInGrid.x, posInGrid.y] = true;

        
        //PlotManager.Instance.GetPlots().Add(this);
        //PlotManager.Instance.GetPlotDataList().Add(plotData);
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
        if (inventoryManager.currentlyEquippedItem == null) 
        { 
            interactionButton.gameObject.SetActive(false);
            return; 
        }
        else if (inventoryManager.currentlyEquippedItem == axe) //TODO: not hard coded
        { 
            interactionText.SetText("Cut Tree");
            interactionButton.gameObject.SetActive(true);
        }  
    }

    public override void InteractWithWorkStation()
    {
        Debug.Log("FLJKA:SLD");
        RemoveWorkStation();
    }
}
