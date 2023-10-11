using Firebase.Auth;
using Firebase.Database;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class PlayerSaveManager : BaseSaveManager
{
    public static PlayerSaveManager Instance { get; private set; }

    public event EventHandler OnPlayerLoaded;

    private PlayerManager playerManager;

    private void Awake()
    {
        if (Instance != null)
        {
            Debug.LogError($"There's more than one PlayerSaveManager! {transform} - {Instance}");
            Destroy(gameObject);
        }

        Instance = this;

        playerManager = FindObjectOfType<PlayerManager>();
    }

    // Start is called before the first frame update
    protected override void Start()
    {
        base.Start();

        AuthManager.Instance.OnAuthStateChanged += AuthManager_OnAuthStateChanged;

        if (SceneHandler.Instance.GetActiveSceneIndex() != (int)SceneIndexes.TITLE_SCREEN)
        {
            playerManager.OnPlayerDataSaved += PlayerManager_OnPlayerDataSaved;
        }

        SaveGameManager.Instance.OnLoadGame += SaveGameManager_OnLoadGame;

        if (SceneHandler.Instance.GetActiveSceneIndex() == (int)SceneIndexes.VENTURE_MODE)
        {
            InventorySaveManager.Instance.OnInventoryLoaded += InventorySaveManager_OnInventoryLoaded;
        }
    }

    /// <summary>
    /// Invoked from the AuthManager script.
    /// </summary>
    /// <param name="sender">The object that sent the invoke.</param>
    /// <param name="e">Variable pass-through, in this case it will be empty.</param>
    private void AuthManager_OnAuthStateChanged(object sender, EventArgs e)
    {
        if (SceneHandler.Instance.GetActiveSceneIndex() != (int)SceneIndexes.TITLE_SCREEN)
        {
            // If the authentication state has changed, load the data in case its a new user. 
            StartCoroutine(LoadFromDatabase());
        }
        
        //if (SceneHandler.Instance.GetActiveSceneIndex() == (int)SceneIndexes.VENTURE_MODE) { OnPlayerLoaded?.Invoke(this, null); }
    }

    /// <summary>
    /// Invoked from SaveGameManager. The start of the loading sequence.
    /// </summary>
    /// <param name="sender">The object that sent the invoke.</param>
    /// <param name="e">Variable pass-through, in this case it will be empty.</param>
    private void SaveGameManager_OnLoadGame(object sender, EventArgs e)
    {
        // If the loading screen exists, update the information text. 
        if (GameObject.Find("LoadingScreen"))
        {
            // Plot Loaded
            LoadingScreen.Instance.UpdateLoadingInfo("Loading player data");
        }

        StartCoroutine(LoadFromDatabase());
    }

    private void InventorySaveManager_OnInventoryLoaded(object sender, EventArgs e)
    {
        if (GameObject.Find("LoadingScreen"))
        {
            LoadingScreen.Instance.UpdateLoadingInfo("Loading player data");
        }

        StartCoroutine(LoadFromDatabase());
    }

    /// <summary>
    /// Saves the current data to the database.
    /// </summary>
    /// <param name="sender">The object that sent the invoke.</param>
    /// <param name="content">Contents of the data.</param>
    private void PlayerManager_OnPlayerDataSaved(object sender, string content)
    {
        AuthManager.Instance.dbReference.Child("users").Child(FirebaseAuth.DefaultInstance.CurrentUser.UserId).
            Child("properties").SetRawJsonValueAsync(content);
    }

    protected override IEnumerator LoadFromDatabase()
    {
        // Return early if the database reference is null.
        if (!base.CheckIfDatabaseIsNull()) { yield break; }

        //if (SceneHandler.Instance.GetActiveSceneIndex() == (int)SceneIndexes.VENTURE_MODE) 
        //{
        //    OnPlayerLoaded?.Invoke(this, null);

        //    yield break;
        //}

        Task<DataSnapshot> playerLoadTask = AuthManager.Instance.dbReference.Child("users").Child(AuthManager.Instance.user.UserId).Child("properties").GetValueAsync();

        yield return new WaitUntil(predicate: () => playerLoadTask.IsCompleted);

        base.CheckForTaskException(playerLoadTask);

        Debug.Log("Invoking OnPlayerLoaded");
        OnPlayerLoaded?.Invoke(this, EventArgs.Empty);
    }

    protected override void TaskDataIsNull()
    {
        // No data exists, create default ones.
        playerManager.SetPlayerData(1, 0, 0, false, false, false, DateTime.Today.AddDays(-1).ToString(), 0);
    }

    protected override void TaskHasRetrievedData(Task<DataSnapshot> task)
    {
        Debug.Log("Player Data retrieved");

        // Data has been retrieved. 
        DataSnapshot snapshot = task.Result;

        PlayerData tempPlayerData = new PlayerData();

        int newWalkLevel = int.Parse(snapshot.Child(nameof(tempPlayerData.walkLevel)).Value.ToString());

        int newAccountLevel = int.Parse(snapshot.Child(nameof(tempPlayerData.accountLevel)).Value.ToString());
        int newCurrentExperience = int.Parse(snapshot.Child(nameof(tempPlayerData.currentExperience)).Value.ToString());

        Debug.Log($"{newAccountLevel}, {newCurrentExperience}");

        bool newTimeQuestOne = bool.Parse(snapshot.Child(nameof(tempPlayerData.timeQuestOne)).Value.ToString());
        bool newTimeQuestTwo = bool.Parse(snapshot.Child(nameof(tempPlayerData.timeQuestTwo)).Value.ToString());
        bool newTimeQuestThree = bool.Parse(snapshot.Child(nameof(tempPlayerData.timeQuestThree)).Value.ToString());

        // Dates
        string newOldDate = null;
        if (snapshot.Child(nameof(tempPlayerData.oldDate)).Value != null)
        {
            // If there is data, set the old date with the new date.
            newOldDate = snapshot.Child(nameof(tempPlayerData.oldDate)).Value.ToString();
        }
        else
        {
            // Assume its first play-through, so set the current date as their old date. 
            newOldDate = DateTime.Today.ToString();
        }

        float newDistanceTravelledInMeters = 0f;
        if (snapshot.Child(nameof(tempPlayerData.distanceTravelledInMeters)).Exists)
        {
            newDistanceTravelledInMeters = float.Parse(snapshot.Child(nameof(tempPlayerData.distanceTravelledInMeters)).Value.ToString());
        }

        playerManager.SetPlayerData(newWalkLevel, newAccountLevel, newCurrentExperience, newTimeQuestOne, newTimeQuestTwo, newTimeQuestThree, newOldDate, newDistanceTravelledInMeters);
    }

    protected override void TaskHasRetrievedData_VentureModeSaveQuests(Task<DataSnapshot> task)
    {
        throw new NotImplementedException();
    }
}
