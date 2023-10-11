using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class DragDrop : MonoBehaviour, IPointerClickHandler, IBeginDragHandler, IDragHandler, IDropHandler, IEndDragHandler
{
    private Transform player;
    private InventoryManager inventoryManager;
    private RectTransform rectTransform;
    private CanvasGroup canvasGroup;
    public InventorySlot mySlot;
    private Vector2 startingPosition;

    private void Awake()
    {
        player = GameObject.FindGameObjectWithTag("Player").transform;
        inventoryManager = GameObject.FindGameObjectWithTag("GameController").GetComponent<InventoryManager>();
        rectTransform = GetComponent<RectTransform>();
        canvasGroup = GetComponent<CanvasGroup>();
    }

    public void OnPointerClick(PointerEventData pointerEventData)
    {
        Debug.Log("On Pointer Down");
        if (mySlot.itemInSlot == null || inventoryManager.dragging) { return; }

        inventoryManager.EquipItem(mySlot);
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (mySlot.itemInSlot == null) { return; }

        Debug.Log("On Begin Drag:" + mySlot.itemInSlot.itemName);
        inventoryManager.dragging = true;
        inventoryManager.startingSlot = mySlot;
        inventoryManager.draggingItem = this.mySlot.itemInSlot;
        inventoryManager.dragAmmount = this.mySlot.numberInSlot;

        //Set the canvas group to non-interactable to stop the dragging item blocking raycast
        canvasGroup.alpha = 0.5f;
        canvasGroup.blocksRaycasts = false;
        canvasGroup.interactable = false;

        //Set the start pos to tele back if not dropped on a new slow
        startingPosition = rectTransform.position;
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (mySlot.itemInSlot == null) { return; }

        //Set the canvas group to non-interactable to stop the dragging item blocking raycast
        canvasGroup.alpha = 0.5f;
        canvasGroup.blocksRaycasts = false;
        canvasGroup.interactable = false;

        //rectTransform.anchoredPosition += eventData.delta;
        transform.position = Input.mousePosition;
    }

    public void OnDrop(PointerEventData eventData)
    {
        Debug.Log("On Drop" + inventoryManager.draggingItem + " on to " + this.gameObject.name);
        inventoryManager.dragging = false;

        if (eventData.pointerDrag != null)
        {
            eventData.pointerDrag.GetComponent<RectTransform>().anchoredPosition = GetComponent<RectTransform>().anchoredPosition;

            if (mySlot.itemInSlot != null)
            {
                //Swap Items around
                Debug.Log("I have a " + mySlot.itemInSlot.name + " in my slot");
                ItemData cachedItemData = mySlot.itemInSlot;
                int cachedAmmount = mySlot.numberInSlot;

                mySlot.itemInSlot = inventoryManager.draggingItem;
                mySlot.numberInSlot = inventoryManager.dragAmmount;

                inventoryManager.startingSlot.itemInSlot = cachedItemData;
                inventoryManager.startingSlot.numberInSlot = cachedAmmount;
            }
            else
            {
                //Add items to my slot and clear the other one
                Debug.Log("I have nada in my slot");
                mySlot.itemInSlot = inventoryManager.draggingItem;
                mySlot.numberInSlot = inventoryManager.dragAmmount;

                inventoryManager.startingSlot.itemInSlot = null;
                inventoryManager.startingSlot.numberInSlot = 0;
            }

            inventoryManager.UpdateInventoryDisplay();
        }


    }

    public void OnEndDrag(PointerEventData eventData)
    {   
        Debug.Log("HELLLO");
        inventoryManager.dragging = false;
        this.rectTransform.position = startingPosition;

        canvasGroup.alpha = 1f;
        canvasGroup.blocksRaycasts = true;
        canvasGroup.interactable = true;

        inventoryManager.DropItem(mySlot.itemInSlot, mySlot.numberInSlot, (player.position + player.forward));
        mySlot.itemInSlot = null;
        mySlot.numberInSlot = 0;
        inventoryManager.UpdateInventoryDisplay();

        // Update the inventory slot save data. 
        for (int i = 0; i < inventoryManager.GetInventorySlots().Count; i++)
        {
            inventoryManager.GetInventorySlots()[i].SaveData();
        }
    }
}