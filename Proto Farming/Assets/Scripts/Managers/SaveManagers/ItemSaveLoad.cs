using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Events;
using Firebase.Auth;

public class ItemSaveLoad : MonoBehaviour
{
    private static string directory = "/SaveData/";
    private static string fileName = "item_data.txt";

    public static UnityAction<SaveData> OnItemLoad;

    private void Start()
    {
        if (InventorySaveManager.Instance != null)
        {
            InventorySaveManager.Instance.OnInventoryLoaded += InventorySaveManager_OnInventoryLoaded;
        }
    }

    private void InventorySaveManager_OnInventoryLoaded(object sender, EventArgs e)
    {
        Load();
    }

    public static void Save(SaveData data)
    {
        string dir = Application.persistentDataPath + directory;

        if (!Directory.Exists(dir)) { Directory.CreateDirectory(dir); }

        string json = JsonUtility.ToJson(data, true);
        File.WriteAllText(dir + $"{FirebaseAuth.DefaultInstance.CurrentUser.UserId}_" + fileName, json);
    }

    public static SaveData Load()
    {
        string fullPath = Application.persistentDataPath + directory + $"{FirebaseAuth.DefaultInstance.CurrentUser.UserId}" + fileName;
        SaveData data = new SaveData();

        if (File.Exists(fullPath))
        {
            string json = File.ReadAllText(fullPath);
            data = JsonUtility.FromJson<SaveData>(json);

            // Invoke load event.
            OnItemLoad?.Invoke(data);
        }

        return data;
    }
}
