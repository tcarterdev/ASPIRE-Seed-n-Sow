using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AR_Inventory : MonoBehaviour
{
    public List<ItemData> collectedItemsOld = new List<ItemData>();
    public List<ItemSaveData> collectedItems = new List<ItemSaveData>();
    [SerializeField] private List<ItemSaveData> itemSaveDataList;

    public event EventHandler<string> OnVentureInventorySaved;

    [SerializeField] private string fileName;

    private void Start()
    {
        SaveGameManager.Instance.OnSaveGame += SaveGameManager_OnSaveGame;

        itemSaveDataList = new List<ItemSaveData>();
    }

    private void SaveGameManager_OnSaveGame(object sender, EventArgs e)
    {
        Debug.Log("Saving AR Inventory Slots");

        // Clear list before saving new data.
        itemSaveDataList.Clear();

        ItemSaveData itemSaveData = new ItemSaveData();
        for (int i = 0; i < collectedItems.Count; i++)
        {
            itemSaveData.itemName = collectedItems[i].itemName;
            itemSaveData.numberInSlot = collectedItems[i].numberInSlot;
            itemSaveDataList.Add(itemSaveData);
        }

        FBFileHandler.SaveToJSON<ItemSaveData>(itemSaveDataList, fileName);
        OnVentureInventorySaved?.Invoke(this, JsonHelper.ToJson<ItemSaveData>(itemSaveDataList.ToArray()));
    }

    public void LoadCollectedItem(ItemSaveData itemToLoad)
    {
        collectedItems.Add(itemToLoad);
    }

    public List<ItemSaveData> GetCollectedItemsList() => collectedItems;
}
