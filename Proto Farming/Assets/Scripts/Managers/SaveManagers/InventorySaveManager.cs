using Firebase.Auth;
using Firebase.Database;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class InventorySaveManager : BaseSaveManager
{
    public static InventorySaveManager Instance { get; private set; }

    public event EventHandler OnInventoryLoaded;

    private InventoryManager inventoryManager;

    private void Awake()
    {
        if (Instance != null)
        {
            Debug.LogError($"There's more than one InventorySaveManager! {transform} - {Instance}");
            Destroy(gameObject);
        }

        Instance = this;

        inventoryManager = FindObjectOfType<InventoryManager>();

        if (GameObject.Find("LoadingScreen"))
        {
            LoadingScreen.Instance.UpdateLoadingInfo("Loading inventory");
        }
    }

    protected override void Start()
    {
        base.Start();

        AuthManager.Instance.OnAuthStateChanged += AuthManager_OnAuthStateChanged;

        if (SceneHandler.Instance.GetActiveSceneIndex() == (int)SceneIndexes.FARM_MODE)
        {
            PlotSaveManager.Instance.OnPlotLoaded += PlotSaveManager_OnPlotLoaded;
        }
        else if (SceneHandler.Instance.GetActiveSceneIndex() == (int)SceneIndexes.VENTURE_MODE)
        {
            Debug.Log("loadingin Venturemode");
            //ARInventorySaveManager.Instance.OnVentureInventoryLoaded += ARInventorySaveManager_OnVentureInventoryLoaded;
            StartCoroutine(LoadFromDatabase());
        }

        if (inventoryManager != null)
        {
            inventoryManager.OnItemSlotSaved += InventoryManager_OnItemSlotSaved;
        }
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
    private void InventoryManager_OnItemSlotSaved(object sender, string content)
    {
        // Save the passed-through contents to the content variable
        //this.content = content;

        SaveSlotsToDatabase(content);
    }

    private void PlotSaveManager_OnPlotLoaded(object sender, EventArgs e)
    {
        if (GameObject.Find("LoadingScreen"))
        {
            LoadingScreen.Instance.UpdateLoadingInfo("Loading inventory");
        }

        inventoryManager = FindObjectOfType<InventoryManager>();

        if (SceneHandler.Instance.GetActiveSceneIndex() != (int)SceneIndexes.VENTURE_MODE)
        {
            StartCoroutine(LoadFromDatabase());
        }
        //else if (SceneHandler.Instance.GetActiveSceneIndex() == (int)SceneIndexes.VENTURE_MODE)
        //{
        //    OnInventoryLoaded?.Invoke(this, null);
        //}
    }

    private void ARInventorySaveManager_OnVentureInventoryLoaded(object sender, EventArgs e)
    {
        if (GameObject.Find("LoadingScreen"))
        {
            LoadingScreen.Instance.UpdateLoadingInfo("Loading inventory");
        }

        if (SceneHandler.Instance.GetActiveSceneIndex() != (int)SceneIndexes.FARM_MODE)
        {
            StartCoroutine(LoadFromDatabase());
        }
    }

    /// <summary>
    /// Saves the current data to the database. 
    /// Path: users/farmingMode/inventory/
    /// </summary>
    /// <param name="content">A Json string of all the inventory slots data.</param>
    public void SaveSlotsToDatabase(string content)
    {
        Debug.Log(FirebaseAuth.DefaultInstance.CurrentUser.UserId);
        AuthManager.Instance.dbReference.Child("users").Child(FirebaseAuth.DefaultInstance.CurrentUser.UserId).Child("farmingMode").Child("inventory").SetRawJsonValueAsync(content);
    }

    /// <summary>
    /// Loads the item save data from the database, and assigns the variables to the inventory slots. 
    /// </summary>
    /// <returns></returns>
    protected override IEnumerator LoadFromDatabase()
    {
        Debug.Log("Attempting to load Inventory");

        if (AuthManager.Instance.dbReference == null)
        {
            AuthManager.Instance.dbReference = FirebaseDatabase.DefaultInstance.RootReference;
        }

        // Return early if the database reference is null.
        if (!base.CheckIfDatabaseIsNull()) 
        {
            Debug.Log("Database reference is null");
            yield break; 
        }

        Debug.Log("Data reference is not null");

        Task<DataSnapshot> inventoryLoadTask = AuthManager.Instance.dbReference.
            Child("users").Child(FirebaseAuth.DefaultInstance.CurrentUser.UserId).Child("farmingMode").Child("inventory").Child("Items").GetValueAsync();

        yield return new WaitUntil(predicate: () => inventoryLoadTask.IsCompleted);

        Debug.Log("Checking for task exception");
        base.CheckForTaskException(inventoryLoadTask);

        Debug.Log("Invoking OnInventoryLoaded");
        OnInventoryLoaded?.Invoke(this, null);
    }

    /// <summary>
    /// Sets the inventory slots data to default values (empty) if the task data is null.
    /// </summary>
    protected override void TaskDataIsNull()
    {
        Debug.Log("Inventory data is null");

        List<InventorySlot> inventorySlots = inventoryManager.GetInventorySlots();

        // No data exists yet, create default ones.
        for (int i = 0; i < inventorySlots.Count; i++)
        {
            // Set data to defaults. 
            inventorySlots[i].SetSlotData("", 0, "", "");

            // Load the new default data. 
            inventorySlots[i].LoadData();
        }

        List<InventorySlot> toolSlots = inventoryManager.GetToolSlots();
        int toolSlotIndex = 0;
        for (int i = inventorySlots.Count; i < inventorySlots.Count + 4; i++)
        {
            toolSlots[toolSlotIndex].SetSlotData("", 0, "", "");

            toolSlots[toolSlotIndex].LoadData();

            toolSlotIndex++;
        }
    }

    /// <summary>
    /// Sets the inventory slots to its loaded data from the database.
    /// </summary>
    /// <param name="task">The current task we're performing.</param>
    protected override void TaskHasRetrievedData(Task<DataSnapshot> task)
    {
        Debug.Log("Inventory Data retrieved");

        // Data has been retrieved.
        // Get a snapshot of the data. 
        DataSnapshot snapshot = task.Result;

        ItemSaveData tempSlotData = new ItemSaveData();

        List<InventorySlot> inventorySlots = inventoryManager.GetInventorySlots();

        //HandleInventorySlotLoad(snapshot, inventorySlots, tempSlotData);

        for (int i = 0; i < inventorySlots.Count; i++)
        {
            string newName = snapshot.Child(i.ToString()).Child(nameof(tempSlotData.itemName)).Value.ToString();
            int newNumberInSlot = int.Parse(snapshot.Child(i.ToString()).Child(nameof(tempSlotData.numberInSlot)).Value.ToString());

            string newItemType = snapshot.Child(i.ToString()).Child(nameof(tempSlotData.itemType)).Value.ToString();
            string newItemCategory = snapshot.Child(i.ToString()).Child(nameof(tempSlotData.itemCategory)).Value.ToString();

            inventorySlots[i].SetSlotData(newName, newNumberInSlot, newItemType, newItemCategory);

            inventorySlots[i].LoadData();
        }

        HandleToolSlotLoad(snapshot, inventorySlots, tempSlotData);

        if (SceneHandler.Instance.GetActiveSceneIndex() != (int)SceneIndexes.VENTURE_MODE)
        {
            HandleEquippedItemLoad(snapshot, tempSlotData);
        }
    }

    private void HandleInventorySlotLoad(DataSnapshot snapshot, List<InventorySlot> inventorySlots, ItemSaveData tempSlotData)
    {
        for (int i = 0; i < inventorySlots.Count; i++)
        {
            string newName = snapshot.Child(i.ToString()).Child(nameof(tempSlotData.itemName)).Value.ToString();
            int newNumberInSlot = int.Parse(snapshot.Child(i.ToString()).Child(nameof(tempSlotData.numberInSlot)).Value.ToString());

            string newItemType = snapshot.Child(i.ToString()).Child(nameof(tempSlotData.itemType)).Value.ToString();
            string newItemCategory = snapshot.Child(i.ToString()).Child(nameof(tempSlotData.itemCategory)).Value.ToString();

            inventorySlots[i].SetSlotData(newName, newNumberInSlot, newItemType, newItemCategory);

            inventorySlots[i].LoadData();
        }
    }

    private void HandleToolSlotLoad(DataSnapshot snapshot, List<InventorySlot> inventorySlots, ItemSaveData tempSlotData)
    {
        List<InventorySlot> toolSlots = inventoryManager.GetToolSlots();
        int toolSlotIndex = 0;
        for (int i = inventorySlots.Count; i < inventorySlots.Count + 4; i++)
        {
            string newName = snapshot.Child(i.ToString()).Child(nameof(tempSlotData.itemName)).Value.ToString();
            int newNumberInSlot = int.Parse(snapshot.Child(i.ToString()).Child(nameof(tempSlotData.numberInSlot)).Value.ToString());

            string newItemType = snapshot.Child(i.ToString()).Child(nameof(tempSlotData.itemType)).Value.ToString();
            string newItemCategory = snapshot.Child(i.ToString()).Child(nameof(tempSlotData.itemCategory)).Value.ToString();

            toolSlots[toolSlotIndex].SetSlotData(newName, newNumberInSlot, newItemType, newItemCategory);

            toolSlots[toolSlotIndex].LoadData();

            toolSlotIndex++;
        }
    }

    private void HandleEquippedItemLoad(DataSnapshot snapshot, ItemSaveData tempSlotData)
    {
        // Equipped item
        string newEquippedItemName = snapshot.Child((snapshot.ChildrenCount - 1).ToString()).Child(nameof(tempSlotData.itemName)).Value.ToString();
        int newEquippedNumberInSlot = int.Parse(snapshot.Child((snapshot.ChildrenCount - 1).ToString()).Child(nameof(tempSlotData.numberInSlot)).Value.ToString());
        string newEquippedItemType = snapshot.Child((snapshot.ChildrenCount - 1).ToString()).Child(nameof(tempSlotData.itemType)).Value.ToString();
        string newEquippedItemCategory = snapshot.Child((snapshot.ChildrenCount - 1).ToString()).Child(nameof(tempSlotData.itemCategory)).Value.ToString();

        InventoryItems inventoryItems = FindObjectOfType<InventoryItems>();

        if (newEquippedItemType == ItemType.Tool.ToString())
        {
            Debug.Log("Loading tool");
            inventoryManager.currentlyEquippedItem = inventoryItems.GetTool(newEquippedItemName);
        }
        else
        {
            Debug.Log($"Loading: {newEquippedItemName}; Category: {newEquippedItemCategory}");

            inventoryManager.currentlyEquippedItem = inventoryItems.GetItem(newEquippedItemName, newEquippedItemCategory);
        }

        inventoryManager.equippedAmmount = newEquippedNumberInSlot;

        inventoryManager.UpdateEquippedItemDisplay();
    }

    protected override void TaskHasRetrievedData_VentureModeSaveQuests(Task<DataSnapshot> task)
    {
        throw new NotImplementedException();
    }
}