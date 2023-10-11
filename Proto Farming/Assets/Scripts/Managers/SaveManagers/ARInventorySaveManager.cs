using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Firebase.Auth;
using Firebase.Database;
using UnityEngine;

public class ARInventorySaveManager : BaseSaveManager
{
    public static ARInventorySaveManager Instance { get; private set; }

    public event EventHandler OnVentureInventoryLoaded;

    private AR_Inventory ventureInventory;

    private void Awake()
    {
        if (Instance != null)
        {
            Debug.LogWarning($"There's more than one ARInventorySaveManager! {transform} - {Instance}");
            Destroy(gameObject);
        }

        Instance = this;

        ventureInventory = FindObjectOfType<AR_Inventory>();
        ventureInventory.OnVentureInventorySaved += VentureInventory_OnVentureInventorySaved;
        
        AuthManager.Instance.OnAuthStateChanged += AuthManager_OnAuthStateChanged;

        //if (GameObject.Find("LoadingScreen"))
        //{
        //    LoadingScreen.Instance.UpdateLoadingInfo("Loading inventory");
        //}
    }

    protected override void Start()
    {
        base.Start();

        StartCoroutine(LoadFromDatabase());
    }

    /// <summary>
    /// Invoked from the AuthManager script.
    /// </summary>
    /// <param name="sender">The object that sent the invoke.</param>
    /// <param name="e">Variable pass-through, in this case it will be empty.</param>
    private void AuthManager_OnAuthStateChanged(object sender, EventArgs e)
    {
        //Debug.Log("AuthStateChanged");

        if (SceneHandler.Instance.GetActiveSceneIndex() != (int)SceneIndexes.TITLE_SCREEN)
        {
            // If the authentication state has changed, load the data in case its a new user. 
            //HandleSlotLoad();
        }
    }

    /// <summary>
    /// Invoked when the InventoryManager has saved all the inventory slots' data. 
    /// </summary>
    /// <param name="sender">The object that sent the invoke.</param>
    /// <param name="content">A Json string of all the plots data.</param>
    private void VentureInventory_OnVentureInventorySaved(object sender, string content)
    {
        Debug.Log("OnVentureInventorySaved called");

        // Save the passed-through contents to the content variable
        //this.content = content;

        //SaveInventoryToDatabase(content);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="content">A Json string of all the inventory data.</param>
    public void SaveInventoryToDatabase(string content)
    {
        FirebaseDatabase.DefaultInstance.RootReference.Child("users").Child(FirebaseAuth.DefaultInstance.CurrentUser.UserId).Child("ventureMode").Child("inventory").SetRawJsonValueAsync(content);
    }

    protected override IEnumerator LoadFromDatabase()
    {
        Debug.Log("Loading from database");

        // Return early if the database reference is null.
        if (!base.CheckIfDatabaseIsNull()) { yield break; }

        Task<DataSnapshot> inventoryLoadTask = FirebaseDatabase.DefaultInstance.RootReference.
            Child("users").Child(FirebaseAuth.DefaultInstance.CurrentUser.UserId).Child("ventureMode").Child("inventory").Child("Items").GetValueAsync();

        yield return new WaitUntil(predicate: () => inventoryLoadTask.IsCompleted);

        base.CheckForTaskException(inventoryLoadTask);

        // Invoke inventory load event for LoadingScreen.
        OnVentureInventoryLoaded?.Invoke(this, EventArgs.Empty);
    }

    protected override void TaskDataIsNull()
    {
        // Clear the CollectedItems list as there is no data.
        ventureInventory.GetCollectedItemsList().Clear();
    }

    protected override void TaskHasRetrievedData(Task<DataSnapshot> task)
    {
        DataSnapshot snapshot = task.Result;

        ItemSaveData tempSaveData = new ItemSaveData();

        for (int i = 0; i < snapshot.ChildrenCount; i++)
        {
            string newItemName = snapshot.Child(i.ToString()).Child(nameof(tempSaveData.itemName)).Value.ToString();
            int newNumberInSlot = int.Parse(snapshot.Child(i.ToString()).Child(nameof(tempSaveData.numberInSlot)).Value.ToString());

            tempSaveData.itemName = newItemName;
            tempSaveData.numberInSlot = newNumberInSlot;

            ventureInventory.LoadCollectedItem(tempSaveData);
        }
    }

    protected override void TaskHasRetrievedData_VentureModeSaveQuests(Task<DataSnapshot> task)
    {
        throw new System.NotImplementedException();
    }
}
