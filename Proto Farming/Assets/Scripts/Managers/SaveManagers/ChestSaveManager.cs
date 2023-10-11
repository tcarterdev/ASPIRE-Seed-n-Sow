using System;
using System.Linq;
using Firebase.Auth;
using Firebase.Database;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class ChestSaveManager : BaseSaveManager
{
    public static ChestSaveManager Instance { get; private set; }

    public event EventHandler OnChestsLoaded;

    private List<StorageContainer> storageContainers;

    private List<ChestSaveData> chestSaveDataList;

    [SerializeField] private GameObject chestPrefab;

    private bool chestEventTriggered; 

    private void Awake()
    {
        if (Instance != null)
        {
            Debug.LogWarning($"There's more than one ChestSaveManager! {transform} - {Instance}");
            Destroy(gameObject);
        }

        Instance = this;
    }

    // Start is called before the first frame update
    protected override void Start()
    {
        base.Start();

        if (InventorySaveManager.Instance != null)
        {
            InventorySaveManager.Instance.OnInventoryLoaded += InventorySaveManager_OnInventoryLoaded;
        }

        chestSaveDataList = new List<ChestSaveData>();
        storageContainers = FindObjectsOfType<StorageContainer>().ToList();
    }

    private void InventorySaveManager_OnInventoryLoaded(object sender, EventArgs e)
    {
        if (GameObject.Find("LoadingScreen"))
        {
            LoadingScreen.Instance.UpdateLoadingInfo("Loading chests");
        }

        if (SceneHandler.Instance.GetActiveSceneIndex() != (int)SceneIndexes.VENTURE_MODE)
        {
            StartCoroutine(LoadFromDatabase());
        }
    }

    public void AddChestToList(ChestSaveData chestToAdd)
    {
        chestSaveDataList.Clear();
        chestSaveDataList.Add(chestToAdd);

        if (chestSaveDataList.Count == storageContainers.Count) { SaveToDatabase(); }
    }

    private void SaveToDatabase()
    {
        AuthManager.Instance.dbReference.Child("users").Child(FirebaseAuth.DefaultInstance.CurrentUser.UserId).
            Child("farmingMode").Child("chests").SetRawJsonValueAsync(JsonHelper.ToJson<ChestSaveData>(chestSaveDataList.ToArray()));
    }

    protected override void TaskDataIsNull()
    {
        Debug.Log("No chest data");

        //// Chest data is null
        //// Destroy chests as there is no data. 
        //for (int i = 0; i < storageContainers.Count; i++)
        //{
        //    Destroy(storageContainers[i].gameObject);;
        //}

        //// Clear the data. 
        //chestSaveDataList.Clear();
        //storageContainers.Clear();
    }

    protected override void TaskHasRetrievedData(Task<DataSnapshot> task)
    {
        Debug.Log("Chest data retrieved");

        DataSnapshot snapshot = task.Result;

        // The number of chests within the database. 
        long numberOfChests = snapshot.ChildrenCount;

        // If the amount of chests in the scene doesn't equal to the chests in the database. 
        if (storageContainers.Count != numberOfChests)
        {
            long result = storageContainers.Count - numberOfChests;
            if (result < 0)
            {
                // Number of chests needed to create.
                // Converts the number to a positive. 
                CreateChests((int)MathF.Abs(result));
                Debug.Log($"Creating {(int)MathF.Abs(result)} chests");
            }
            else if (result > 0)
            {
                // Number of chests to remove.
                RemoveChests((int)Mathf.Abs(result));
                Debug.Log($"Removing {(int)MathF.Abs(result)} chests");
            }
        }

        // Load the chest data. 
        LoadChestData(storageContainers, snapshot);

        chestSaveDataList.Clear();

        for (int i = 0; i < storageContainers.Count; i++)
        {
            chestSaveDataList.Add(storageContainers[i].GetChestSaveData());
        }
    }

    /// <summary>
    /// Creates the number of plots needed to match the data from the database.
    /// E.g., If there are 6 plots in the database, but only 2 in the game scene, 
    /// then 4 plots will be created. 
    /// </summary>
    /// <param name="numberOfChestsToCreate">The number of plots to create.</param>
    private void CreateChests(int numberOfChestsToCreate)
    {
        // If there are no plots in the world, create them.
        for (int i = 0; i < numberOfChestsToCreate; i++)
        {
            // Create the plot. 
            GameObject chest = Instantiate(chestPrefab, transform.position, Quaternion.identity);

            StorageContainer chestToAdd = chest.GetComponent<StorageContainer>();

            // Add to the chest data. 
            storageContainers.Add(chestToAdd);
            chestSaveDataList.Add(chestToAdd.GetChestSaveData());
        }
    }

    /// <summary>
    /// Removes the number of excess plots to match the data from the database.
    /// E.g., If there are 3 plots in the database, but there are 5 in the game scene, 
    /// then 2 plots will be removed. 
    /// </summary>
    /// <param name="numberOfChestsToRemove">The number of plots to remove.</param>
    private void RemoveChests(int numberOfChestsToRemove)
    {
        // Delete extra plots. 
        // Remove from PlotManager's list.
        for (int i = numberOfChestsToRemove; i > 0; i--)
        {
            // Destroy the plot.
            Destroy(storageContainers[i].gameObject);

            // Remove the plot from both the plot and plotData lists. 
            storageContainers.RemoveAt(i);
        }
    }

    /// <summary>
    /// Loads data from the database and sets it to the plots. 
    /// </summary>
    /// <param name="plots">The list of plots that are within the scene.</param>
    /// <param name="snapshot">The snapshot of the data we're loading.</param>
    private void LoadChestData(List<StorageContainer> chests, DataSnapshot snapshot)
    {
        ChestSaveData tempChestData = storageContainers[0].GetChestSaveData();

        // Load data
        for (int i = 0; i < storageContainers.Count; i++)
        {
            // Position.
            Vector3 newPosition = new Vector3();
            newPosition.x = float.Parse(snapshot.Child(i.ToString()).Child(nameof(tempChestData.position)).Child("x").Value.ToString());
            newPosition.y = float.Parse(snapshot.Child(i.ToString()).Child(nameof(tempChestData.position)).Child("y").Value.ToString());
            newPosition.z = float.Parse(snapshot.Child(i.ToString()).Child(nameof(tempChestData.position)).Child("z").Value.ToString());

            // Rotation.
            Quaternion newRotation = new Quaternion();
            newRotation = Quaternion.Euler
                (float.Parse(snapshot.Child(i.ToString()).Child(nameof(tempChestData.rotation)).Child("x").Value.ToString()),
                float.Parse(snapshot.Child(i.ToString()).Child(nameof(tempChestData.rotation)).Child("y").Value.ToString()),
                float.Parse(snapshot.Child(i.ToString()).Child(nameof(tempChestData.rotation)).Child("z").Value.ToString())
                );

            ItemSaveData tempItemSaveData = new ItemSaveData();
            List<ItemSaveData> tempItemSaveDataList = new List<ItemSaveData>();
            
            DataSnapshot chestItemSaveData = snapshot.Child(i.ToString()).Child("itemSaveDatas");
            for (int j = 0; j < chestItemSaveData.ChildrenCount; j++)
            {
                string newItemName = chestItemSaveData.Child(i.ToString()).Child(nameof(tempItemSaveData.itemName)).Value.ToString();
                int newNumberInSlot = int.Parse(chestItemSaveData.Child(i.ToString()).Child(nameof(tempItemSaveData.numberInSlot)).Value.ToString());

                string newItemType = chestItemSaveData.Child(i.ToString()).Child(nameof(tempItemSaveData.itemType)).Value.ToString();
                string newItemCategory = chestItemSaveData.Child(i.ToString()).Child(nameof(tempItemSaveData.itemCategory)).Value.ToString();

                tempItemSaveData.itemName = newItemName;
                tempItemSaveData.numberInSlot = newNumberInSlot;
                tempItemSaveData.itemType = newItemType;
                tempItemSaveData.itemCategory = newItemCategory;

                tempItemSaveDataList.Add(tempItemSaveData);
            }

            storageContainers[i].SetChestSaveData(newPosition, newRotation, tempItemSaveDataList);
            storageContainers[i].LoadData();
        }
    }

    protected override void TaskHasRetrievedData_VentureModeSaveQuests(Task<DataSnapshot> task)
    {
        throw new System.NotImplementedException();
    }

    protected override IEnumerator LoadFromDatabase()
    {
        // Return early if the database reference is null.
        if (!base.CheckIfDatabaseIsNull()) { yield break; }

        Task<DataSnapshot> chestLoadTask = AuthManager.Instance.dbReference.
            Child("users").Child(FirebaseAuth.DefaultInstance.CurrentUser.UserId).Child("farmingMode").Child("chests").Child("Items").GetValueAsync();

        yield return new WaitUntil(predicate: () => chestLoadTask.IsCompleted);

        base.CheckForTaskException(chestLoadTask);

        if (!chestEventTriggered) 
        {
            chestEventTriggered = true;
            OnChestsLoaded?.Invoke(this, EventArgs.Empty);
        }
    }
}
