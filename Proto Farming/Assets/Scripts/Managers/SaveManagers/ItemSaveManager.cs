using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Events;

public class ItemSaveManager : MonoBehaviour
{
    public static SaveData itemSaveData;

    private void Start()
    {
        itemSaveData = new SaveData();
        ItemSaveLoad.OnItemLoad += ItemSaveLoad_OnItemLoad;

        if (SaveGameManager.Instance != null)
        {
            SaveGameManager.Instance.OnSaveGame += SaveGameManager_OnSaveGame;
        }
    }

    public void SaveGameManager_OnSaveGame(object sender, EventArgs e)
    {
        var saveData = itemSaveData;

        ItemSaveLoad.Save(saveData);
    }

    public void ItemSaveLoad_OnItemLoad(SaveData data)
    {
        itemSaveData = data;
    }
}
