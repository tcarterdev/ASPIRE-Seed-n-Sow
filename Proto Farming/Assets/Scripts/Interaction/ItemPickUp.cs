using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(UniqueID))]
public class ItemPickUp : Interactable
{
    public ItemData itemData;
    public int numberOfItems = 1;

    protected string uid;

    private void Start()
    {
        // Cache the item's uid.
        uid = GetComponent<UniqueID>().GetUID();

        // Subscribe to OnItemLoad event.
        ItemSaveLoad.OnItemLoad += ItemSaveLoad_OnItemLoad;

        // Add item to active items list.
        ItemSaveManager.itemSaveData.activeItems.Add(uid);
    }

    /// <summary>
    /// Checks if the item has been collected already, if it has, delete it.
    /// </summary>
    /// <param name="data"></param>
    private void ItemSaveLoad_OnItemLoad(SaveData data)
    { 
        if (data.collectedItems.Contains(uid)) { Destroy(this.gameObject); }
    }

    private void OnDestroy()
    {
        // If the item is in active items, remove it. 
        if (ItemSaveManager.itemSaveData.activeItems.Contains(uid)) { ItemSaveManager.itemSaveData.activeItems.Remove(uid); }

        // Unsubcribe to item load event.
        ItemSaveLoad.OnItemLoad -= ItemSaveLoad_OnItemLoad;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject != player) { return; }

        inventoryManager.AddItemToInventory(itemData, numberOfItems);

        // If we are in Farming Mode, and there is a UniqueID attached, add to the collected items list.
        if (SceneHandler.Instance.GetActiveSceneIndex() == (int)SceneIndexes.FARM_MODE && GetComponent<UniqueID>() != null)
        {
            ItemSaveManager.itemSaveData.collectedItems.Add(uid);
        }

        Destroy(this.gameObject);
    }
}
