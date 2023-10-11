using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TutorialItemPickuP : Interactable
{
    [SerializeField] private ToolData toolData;
    public ItemData itemData;
    public int numberOfItems = 1;
    public GameObject digofthestumpTT;

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject == player) {
            Debug.Log("wallahi");
            digofthestumpTT.SetActive(true);
        }

        inventoryManager.AddToolToToolbelt(toolData);
        Destroy(this.gameObject);
    }
}
