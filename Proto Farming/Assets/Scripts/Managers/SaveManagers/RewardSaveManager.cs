using Firebase.Auth;
using Firebase.Database;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class RewardSaveManager : BaseSaveManager
{
    public static RewardSaveManager Instance { get; private set; }

    public event EventHandler OnAccountRewardLoaded;

    private void Awake()
    {
        if (Instance != null)
        {
            Debug.LogError($"There's more than one RewardSaveManager! {transform} - {Instance}");
            Destroy(gameObject);
        }

        Instance = this;
    }

    // Start is called before the first frame update
    protected override void Start()
    {
        base.Start();

        AuthManager.Instance.OnAuthStateChanged += AuthManager_OnAuthStateChanged;

        if (SceneHandler.Instance.GetActiveSceneIndex() != (int)SceneIndexes.TITLE_SCREEN)
        {
            PlayerSaveManager.Instance.OnPlayerLoaded += PlayerSaveManager_OnPlayerLoaded;
        }

        SaveGameManager.Instance.OnLoadGame += SaveGameManager_OnLoadGame;

        if (AccountLevel.Instance != null)
        {
            AccountLevel.Instance.OnAccountRewardSaved += AccountLevel_OnAccountRewardSaved;
        }
    }

    #region Events

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
    /// Invoked from SaveGameManager.
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

    /// <summary>
    /// 
    /// </summary>
    /// <param name="sender">The object that sent the invoke.</param>
    /// <param name="content">Contents of the data.</param>
    private void PlayerSaveManager_OnPlayerLoaded(object sender, EventArgs e)
    {
        Debug.Log("OnPlayerLoaded received");

        if (GameObject.Find("LoadingScreen"))
        {
            LoadingScreen.Instance.UpdateLoadingInfo("Loading player rewards");
        }

        StartCoroutine(LoadFromDatabase());
    }

    private void AccountLevel_OnAccountRewardSaved(object sender, string content)
    {
        AuthManager.Instance.dbReference.Child("users").Child(FirebaseAuth.DefaultInstance.CurrentUser.UserId).Child("accountRewards").SetRawJsonValueAsync(content);
    }

    #endregion

    protected override IEnumerator LoadFromDatabase()
    {
        Debug.Log("Attempting to load Rewards");

        // Return early if the database reference is null.
        if (!base.CheckIfDatabaseIsNull()) { yield break; }

        Task<DataSnapshot> rewardLoadTask = AuthManager.Instance.dbReference.Child("users").Child(AuthManager.Instance.user.UserId).Child("accountRewards").Child("Items").GetValueAsync();

        yield return new WaitUntil(predicate: () => rewardLoadTask.IsCompleted);

        base.CheckForTaskException(rewardLoadTask);

        // TODO: Invoke player reward loaded.
        OnAccountRewardLoaded?.Invoke(this, null);
    }

    protected override void TaskDataIsNull()
    {
        AccountLevel.Instance.InitialiseList();

        for (int i = 0; i < AccountLevel.Instance.GetMaxAccountLevel(); i++)
        {
            AccountLevel.Instance.GetAccountRewardList()[i] = false;
        }
    }

    protected override void TaskHasRetrievedData(Task<DataSnapshot> task)
    {
        // Data has been retrieved. 
        DataSnapshot snapshot = task.Result;

        AccountLevel.Instance.InitialiseList();

        for (int i = 0; i < snapshot.ChildrenCount; i++)
        {
            AccountLevel.Instance.GetAccountRewardList()[i] = bool.Parse(snapshot.Child(i.ToString()).Value.ToString());
        }
    }

    protected override void TaskHasRetrievedData_VentureModeSaveQuests(Task<DataSnapshot> task)
    {
        throw new System.NotImplementedException();
    }




}
