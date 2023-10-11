using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public abstract class WorkStation : Interactable
{
    public Button interactionButton;
    public TMP_Text interactionText;
    [SerializeField] public GameObject gameplayUI;
    public string buttonPrompt;
    [Space]
    public ToolData wrench;
    [Space]
    public Vector2Int posInFarmGrid;

    public virtual void OnTriggerEnter(Collider other)
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

    public virtual void InteractionPopUp()
    {
        interactionButton.gameObject.SetActive(true);
    }

    public virtual void InteractionPopDown()
    {
        interactionButton.gameObject.SetActive(true);
    }

    public virtual void InteractWithWorkStation()
    {
        Debug.LogError("Interact with work station called the virutal not an override");
    }

    public virtual void RemoveWorkStation()
    {
        Destroy(this.gameObject);
        FarmGrid farmGrid = GameObject.FindGameObjectWithTag("FarmManager").GetComponent<FarmGrid>();
        farmGrid.boolMap[posInFarmGrid.x, posInFarmGrid.y] = false;
    }
}
