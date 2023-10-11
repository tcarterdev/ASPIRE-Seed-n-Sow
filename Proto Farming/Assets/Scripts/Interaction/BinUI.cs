using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class BinUI : MonoBehaviour, IDropHandler
{
    private InventoryManager inventoryManager;
    [SerializeField] private AudioClip binmunch;
    [SerializeField] private AudioSource binaudio;

    private void Awake()
    {
        inventoryManager = GameObject.FindGameObjectWithTag("GameController").GetComponent<InventoryManager>();
    }

    public void OnDrop(PointerEventData eventData)
    {
        Debug.Log("You dropped: " + inventoryManager.draggingItem.itemName + " into the bin.");
        inventoryManager.startingSlot.numberInSlot = 0;
        binaudio.PlayOneShot(binmunch, 1);
        inventoryManager.startingSlot.itemInSlot = null;
        inventoryManager.draggingItem = null;
        inventoryManager.dragAmmount = 0;
        inventoryManager.UpdateInventoryDisplay();

        // Update the inventory slot save data. 
        for (int i = 0; i < inventoryManager.GetInventorySlots().Count; i++)
        {
            inventoryManager.GetInventorySlots()[i].SaveData();
        }
    }
}
