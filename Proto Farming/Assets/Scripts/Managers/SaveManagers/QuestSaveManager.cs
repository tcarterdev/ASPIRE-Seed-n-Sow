using Firebase.Auth;
using Firebase.Database;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class QuestSaveManager : BaseSaveManager
{
    public static QuestSaveManager Instance { get; private set; }

    public event EventHandler OnQuestLoadCompleted;

    private QuestBoard questBoard;

    // AR Mode
    private GameObject questUiBG;
    private GameObject questSlots;

    private SaveQuests saveQuests;

    private bool isApplicationClosing;

    private void Awake()
    {
        if (Instance != null)
        {
            Debug.LogError($"There's more than one QuestSaveManager! {transform} - {Instance}");
            Destroy(gameObject);
        }

        Instance = this;
    }

    protected override void Start()
    {
        base.Start();

        if (SceneHandler.Instance.GetActiveSceneIndex() != (int)SceneIndexes.TITLE_SCREEN)
        {
            questUiBG = GameObject.Find("Quest Board UI").transform.GetChild(0).gameObject;
            questSlots = questUiBG.transform.GetChild(0).gameObject;
        }


        saveQuests = FindObjectOfType<SaveQuests>();

        // Subscribe to events
        AuthManager.Instance.OnAuthStateChanged += AuthManager_OnAuthStateChanged;

        if (SceneHandler.Instance.GetActiveSceneIndex() == (int)SceneIndexes.FARM_MODE)
        {
            if (ChestSaveManager.Instance != null) { ChestSaveManager.Instance.OnChestsLoaded += ChestSaveManager_OnChestsLoaded; }
        }

        if (SceneHandler.Instance.GetActiveSceneIndex() != (int)SceneIndexes.VENTURE_MODE)
        {
            questBoard = FindObjectOfType<QuestBoard>();
            questBoard.OnQuestSaved += QuestBoard_OnQuestSaved;
        }
        else if (SceneHandler.Instance.GetActiveSceneIndex() == (int)SceneIndexes.VENTURE_MODE)
        {
            //HandleQuestLoad();
            saveQuests.OnQuestSaved += SaveQuests_OnQuestSaved;

            if (RewardSaveManager.Instance != null) { RewardSaveManager.Instance.OnAccountRewardLoaded += RewardSaveManager_OnAccountRewardLoaded; }
        }
    }

    private void Update()
    {
        // TEMP
        if (Input.GetKeyDown(KeyCode.Alpha5))
        {
            questUiBG.SetActive(true);
        }
        
        //// Debugger for adding quest entries to DataGathering.
        //if (Input.GetKeyDown(KeyCode.J))
        //{
        //    for (int i = 0; i < questBoard.GetQuests().Count; i++)
        //    {
        //        Quest currentLoadingQuest = questBoard.GetQuests()[i].GetComponent<Quest>();

        //        if (currentLoadingQuest.GetQuestID() == 3)
        //        {
        //            DataGathering.dataGathering.AddLine("Collect Quest Started", currentLoadingQuest.GetSessionID(),
        //                currentLoadingQuest.GetQuestID(), currentLoadingQuest.GetRequiredTaskAmount(), currentLoadingQuest.GetCurrentTaskAmount(), false);
        //        }
        //    }
        //}
    }

    /// <summary>
    /// Invoked from the AuthManager script.
    /// </summary>
    /// <param name="sender">The object that sent the invoke.</param>
    /// <param name="e">Variable pass-through, in this case it will be empty.</param>
    private void AuthManager_OnAuthStateChanged(object sender, EventArgs e)
    {
        Debug.Log("AuthStateChanged");

        if (!AuthManager.Instance.GetAuthFlag())
        {
            // If the authentication state has changed, load the data in case its a new user. 
            HandleQuestLoad();
        }
    }

    private void ChestSaveManager_OnChestsLoaded(object sender, EventArgs e)
    {
        if (GameObject.Find("LoadingScreen"))
        {
            LoadingScreen.Instance.UpdateLoadingInfo("Loading quests");
        }

        Debug.Log("ChestSaveManager_OnChestsLoaded: Starting Quest Load");
        StartCoroutine(LoadFromDatabase());
    }

    private void RewardSaveManager_OnAccountRewardLoaded(object sender, EventArgs e)
    {
        if (GameObject.Find("LoadingScreen"))
        {
            LoadingScreen.Instance.UpdateLoadingInfo("Loading quests");
        }

        Debug.Log("Rewards loaded, now loading Quests");
        StartCoroutine(LoadFromDatabase());
    }

    private void QuestBoard_OnQuestSaved(object sender, string content)
    {
        SaveQuestsToDatabase(content);
    }

    private void SaveQuests_OnQuestSaved(object sender, string content)
    {
        // Handle saving differently in Venture Mode.
        if (SceneHandler.Instance.GetActiveSceneIndex() == (int)SceneIndexes.VENTURE_MODE && !isApplicationClosing)
        {
            // Load quest data temporarily
            StartCoroutine(CheckQuestsInDatabase());
        }
        else if (SceneHandler.Instance.GetActiveSceneIndex() == (int)SceneIndexes.VENTURE_MODE && isApplicationClosing)
        {
            // Load quest data temporarily
            StartCoroutine(CheckQuestsInDatabase());

            // Resetting Quest UIDs. 
            ResetSession.Instance.ResetUIDs();
        }
    }

    private void SaveQuestsToDatabase(string content)
    {
        if (SceneHandler.Instance.GetActiveSceneIndex() != (int)SceneIndexes.VENTURE_MODE)
        {
            AuthManager.Instance.dbReference.Child("users").Child(FirebaseAuth.DefaultInstance.CurrentUser.UserId).Child("farmingMode").Child("quests").SetRawJsonValueAsync(content);
        }
    }

    private IEnumerator CheckQuestsInDatabase()
    {
        Task<DataSnapshot> questLoadTask = AuthManager.Instance.dbReference.Child("users").
            Child(FirebaseAuth.DefaultInstance.CurrentUser.UserId).Child("farmingMode").Child("quests").Child("Items").GetValueAsync();

        yield return new WaitUntil(predicate: () => questLoadTask.IsCompleted);

        // Check the task status. 
        base.CheckForTaskException_VentureMode(questLoadTask);
    }

    private void HandleQuestLoad() => StartCoroutine(LoadFromDatabase());

    protected override IEnumerator LoadFromDatabase()
    {
        // Return early if database reference is null. 
        if (!base.CheckIfDatabaseIsNull()) { yield break; }

        Task<DataSnapshot> questLoadTask = AuthManager.Instance.dbReference.Child("users").
            Child(FirebaseAuth.DefaultInstance.CurrentUser.UserId).Child("farmingMode").Child("quests").Child("Items").GetValueAsync();

        yield return new WaitUntil(predicate: () => questLoadTask.IsCompleted);

        // Check the task status. 
        base.CheckForTaskException(questLoadTask);

        // Load completed 
        OnQuestLoadCompleted?.Invoke(this, null);

        if (GameObject.Find("LoadingScreen"))
        {
            if (SceneHandler.Instance.GetActiveSceneIndex() == (int)SceneIndexes.VENTURE_MODE)
            {
                // Disable loading screen. 
                LoadingScreen.Instance.EnableLoadingScreen(false, 3.0f);    // Extra delay to allow scene load.
            }
            else
            {
                // Disable loading screen. 
                LoadingScreen.Instance.EnableLoadingScreen(false, 0.5f);
            }
        }
    }

    protected override void TaskDataIsNull()
    {
        questBoard.CheckRealWorldTime();
    }

    /// <summary>
    /// Sets the quests to its loaded data from the database
    /// </summary>
    /// <param name="task">The current task we're performing.</param>
    protected override void TaskHasRetrievedData(Task<DataSnapshot> task)
    {
        Debug.Log("Quest Data retrieved");

        // If we are in Venture Mode, we do specific loading.
        if (SceneManager.GetActiveScene().buildIndex == (int)SceneIndexes.VENTURE_MODE)
        {
            TaskHasRetrievedData_VentureModeLoading(task);
            return;
        }

        // Get the Quests
        List<GameObject> quests = questBoard.GetQuests();

        // Data has been received.
        // Get snapshot of the data.
        DataSnapshot snapshot = task.Result;

        // Check how many quest papers are active
        int numberOfActiveQuestPapers = 0;
        List<GameObject> questPapers = questBoard.GetQuestPapers();
        for (int i = 0; i < questPapers.Count; i++)
        {
            if (questPapers[i].activeInHierarchy)
            {
                // Increase active counter.
                numberOfActiveQuestPapers++;
            }
        }

        questBoard = FindObjectOfType<QuestBoard>();
        quests = questBoard.GetQuests();

        // Check how many valid quests there are.
        QuestData tempQuestData = quests[0].GetComponent<Quest>().GetQuestData();
        long numberOfValidQuests = 0;
        for (int i = 0; i < snapshot.ChildrenCount; i++)
        {
            if (snapshot.Child(i.ToString()).Child(nameof(tempQuestData.questType)).Value.ToString() != Quest.QuestType.None.ToString())
            {
                numberOfValidQuests++;
            }
        }

        Debug.Log($"Num of Valid Quests: {numberOfValidQuests}");

        // If the number of active quests does not equal the number of valid quests in the database, 
        // then activate or deactivate as needed. 
        if (numberOfActiveQuestPapers != numberOfValidQuests)
        {
            long result = numberOfActiveQuestPapers - numberOfValidQuests;

            if (result < 0)
            {
                // Number of quests to activate.
                ActivateQuestPapers((int)Mathf.Abs(result), numberOfActiveQuestPapers);
            }
            else if (result > 0)
            {
                // Number of quests to deactivate.
                DeactivateQuestPapers((int)Mathf.Abs(result), numberOfActiveQuestPapers);
            }
        }

        LoadQuestData(quests, snapshot);
    }

    protected override void TaskHasRetrievedData_VentureModeSaveQuests(Task<DataSnapshot> task)
    {
        Debug.Log("VentureModeSaveQuests");

        // Data has been received.
        // Get snapshot of the data.
        DataSnapshot snapshot = task.Result;

        SaveQuests saveQuests = FindObjectOfType<SaveQuests>();
        QuestData tempQuestData = saveQuests.GetQuests()[0].GetComponent<Quest>().GetQuestData();

        // Store UIDs from in-game.
        List<string> inGameUIDs = new List<string>();
        for (int i = 0; i < saveQuests.GetQuests().Count; i++)
        {
            if (saveQuests.GetQuests()[i].GetComponent<Quest>().GetUID() != "")
            {
                inGameUIDs.Add(saveQuests.GetQuests()[i].GetComponent<Quest>().GetUID());
            }
        }

        // Store UIDs from Database. 
        List<string> databaseUIDs = new List<string>();
        for (int i = 0; i < snapshot.ChildrenCount; i++)
        {
            databaseUIDs.Add(snapshot.Child(i.ToString()).Child(nameof(tempQuestData.uid)).Value.ToString());
        }
        
        // X is in-game
        // Y is database
        for (int i = 0; i < inGameUIDs.Count; i++)
        {
            // Skip if the ID is empty
            if (inGameUIDs[i] == "") { continue; }

            for (int j = 0; j < databaseUIDs.Count; j++)
            {
                if (inGameUIDs[i] == databaseUIDs[j])
                {
                    // Reset UID
                    saveQuests.GetQuests()[i].GetComponent<Quest>().SetUID(string.Empty);

                    // Save data. 
                    saveQuests.GetQuests()[i].GetComponent<Quest>().SaveData();

                    // Amount
                    AuthManager.Instance.dbReference.Child("users").
                        Child(FirebaseAuth.DefaultInstance.CurrentUser.UserId).
                        Child("farmingMode").Child("quests").Child("Items").
                        Child(j.ToString()).Child("amount").SetValueAsync(saveQuests.GetQuests()[i].GetComponent<Quest>().GetQuestData().amount);

                    // Required Amount
                    AuthManager.Instance.dbReference.Child("users").
                        Child(FirebaseAuth.DefaultInstance.CurrentUser.UserId).
                        Child("farmingMode").Child("quests").Child("Items").
                        Child(j.ToString()).Child("requiredAmount").SetValueAsync(saveQuests.GetQuests()[i].GetComponent<Quest>().GetQuestData().requiredAmount);

                    // Quest Type
                    AuthManager.Instance.dbReference.Child("users").
                        Child(FirebaseAuth.DefaultInstance.CurrentUser.UserId).
                        Child("farmingMode").Child("quests").Child("Items").
                        Child(j.ToString()).Child("questType").SetValueAsync(saveQuests.GetQuests()[i].GetComponent<Quest>().GetQuestData().questType);

                    // Timer
                    AuthManager.Instance.dbReference.Child("users").
                        Child(FirebaseAuth.DefaultInstance.CurrentUser.UserId).
                        Child("farmingMode").Child("quests").Child("Items").
                        Child(j.ToString()).Child("timer").SetValueAsync(saveQuests.GetQuests()[i].GetComponent<Quest>().GetQuestData().timer);

                    if (saveQuests.GetQuests()[i].GetComponent<Quest>().GetQuestData().questType == Quest.QuestType.None.ToString())
                    {
                        // Update UID
                        AuthManager.Instance.dbReference.Child("users").
                            Child(FirebaseAuth.DefaultInstance.CurrentUser.UserId).
                            Child("farmingMode").Child("quests").Child("Items").
                            Child(j.ToString()).Child("uid").SetValueAsync(saveQuests.GetQuests()[i].GetComponent<Quest>().GetQuestData().uid);
                    }
                }
            }
        }
    }

    private void ActivateQuestPapers(int numberOfQuestPapersToActivate, int numberOfActiveQuestPapers)
    {
        // Loop through the quest papers an deactive the required amount. 
        for (int i = 0; i < numberOfQuestPapersToActivate; i++)
        {
            questBoard.GetQuestPapers()[numberOfActiveQuestPapers].SetActive(true);
            numberOfActiveQuestPapers++;
        }
    }

    private void DeactivateQuestPapers(int numberOfQuestPapersToDeactivate, int numberOfActiveQuestPapers)
    {
        // Minus 1 from the active quest papers to get the right index value. 
        numberOfActiveQuestPapers--;

        // Loop through the quest papers and deactivate the required amount.
        for (int i = 0; i < numberOfQuestPapersToDeactivate; i++)
        {
            questBoard.GetQuestPapers()[numberOfActiveQuestPapers].SetActive(false);
            numberOfActiveQuestPapers--;
        }
    }

    private void LoadQuestData(List<GameObject> UiQuests, DataSnapshot snapshot)
    {
        // Create a temp quest data for access to the variables inside. 
        QuestData tempQuestData = UiQuests[0].GetComponent<Quest>().GetQuestData();

        int questIndexToStartFrom = 0;

        // Loop through the Quests in the UI. 
        for (int i = 0; i < UiQuests.Count; i++)
        {
            // Current loading quest type.
            Quest.QuestType newQuestType = (Quest.QuestType)Enum.Parse(typeof(Quest.QuestType), snapshot.Child(i.ToString()).Child(nameof(tempQuestData.questType)).Value.ToString());

            // If the quest type is none, get the current index. 
            if (newQuestType.ToString() == Quest.QuestType.None.ToString())
            {
                // The index of the current quest. 
                questIndexToStartFrom = i;

                // Set the Quest in the UI at the current index as the last sibling. This will push the rest of the Quests forward.
                UiQuests[questIndexToStartFrom].transform.SetAsLastSibling();
            }

            Debug.Log($"Loading Quest Type: {newQuestType}");

            // If we are in the Venture Mode, skip all quests that aren't the collect or wakling quest. 
            if (SceneManager.GetActiveScene().buildIndex == (int)SceneIndexes.VENTURE_MODE)
            {
                if (newQuestType != Quest.QuestType.Collect && newQuestType != Quest.QuestType.Walk) 
                {
                    continue;
                }
            }

            // The current quest we are loading.
            Quest currentLoadingQuest = UiQuests[i].GetComponent<Quest>();

            // If the quest is subcribed to an event, unsubscribe. 
            if (currentLoadingQuest.GetHasSubscribedToEvent())
            {
                currentLoadingQuest.UnsubscribeToQuestEvent();
            }

            // If a uid exists, load it.
            string newUid = "";
            if (snapshot.Child(i.ToString()).Child(nameof(tempQuestData.uid)).Exists)
            {
                // Load the UID that is in the database.
                newUid = snapshot.Child(i.ToString()).Child(nameof(tempQuestData.uid)).Value.ToString();
            }

            // If no sessionId exists, generate a new one and set it as the newSessionId.
            int newSessionId = 0;
            if (!snapshot.Child(i.ToString()).Child(nameof(tempQuestData.sessionId)).Exists)
            {
                currentLoadingQuest.SetSessionID(DataGathering.dataGathering.ReturnUniqueQuestID());

                // Override new session ID.
                newSessionId = currentLoadingQuest.GetSessionID();
            }
            else
            {
                // Otherwise, load the Session ID that is in the Database.
                newSessionId = int.Parse(snapshot.Child(i.ToString()).Child(nameof(tempQuestData.sessionId)).Value.ToString());
            }

            // If the Session ID is 0, generate a new one.
            // Else, we just ignore and keep the one that is in the database, meaning it is the same session.
            if (newSessionId == 0)
            {
                currentLoadingQuest.SetSessionID(DataGathering.dataGathering.ReturnUniqueQuestID());

                // Override new session ID.
                newSessionId = currentLoadingQuest.GetSessionID();
            }

            float newTimer = float.Parse(snapshot.Child(i.ToString()).Child(nameof(tempQuestData.timer)).Value.ToString());
            float newAmount = float.Parse(snapshot.Child(i.ToString()).Child(nameof(tempQuestData.amount)).Value.ToString());
            float newRequiredAmount = float.Parse(snapshot.Child(i.ToString()).Child(nameof(tempQuestData.requiredAmount)).Value.ToString());

            currentLoadingQuest.SetQuestData(newQuestType, newUid, newSessionId, newTimer, newAmount, newRequiredAmount);
            currentLoadingQuest.LoadData();

            // Subscribe to quest event after data has been loaded.
            if (!currentLoadingQuest.GetHasSubscribedToEvent()/* && SceneManager.GetActiveScene().buildIndex == (int)SceneIndexes.VENTURE_MODE*/)
            {
                // Subscribe here
                currentLoadingQuest.SubscribeToQuestEvent();
            }

            // If we are in the farming mode, add the collect and walking quest entries to DataGathering,
            // so we can track the users' progress. 
            if (SceneManager.GetActiveScene().buildIndex == (int)SceneIndexes.FARM_MODE)
            {
                if (currentLoadingQuest.GetQuestType() == Quest.QuestType.Collect)
                {
                    // Collect Quest.
                    DataGathering.dataGathering.AddLine("Collect Quest Started", currentLoadingQuest.GetSessionID(),
                        currentLoadingQuest.GetQuestType().ToString(), currentLoadingQuest.GetRequiredTaskAmount(), currentLoadingQuest.GetCurrentTaskAmount(), false);
                }
                else if (currentLoadingQuest.GetQuestType() == Quest.QuestType.Walk)
                {
                    // Walking Quest
                    DataGathering.dataGathering.AddLine("Walking Quest Started", currentLoadingQuest.GetSessionID(),
                        currentLoadingQuest.GetQuestType().ToString(), currentLoadingQuest.GetRequiredTaskAmount(), currentLoadingQuest.GetCurrentTaskAmount(), false);
                }
            }
        }

        if (SceneHandler.Instance.GetActiveSceneIndex() == (int)SceneIndexes.VENTURE_MODE) { return; }

        //// Update positions of the quests on Quest Board. 
        //for (int i = questIndexToStartFrom; i < questBoard.GetQuestPapers().Count; i++)
        //{
        //    if (i + 1 < UiQuests.Count)
        //    {
        //        questBoard.GetQuestPapers()[i].GetComponentInChildren<Image>().sprite = UiQuests[i + 1].GetComponent<Image>().sprite;
        //    }
        //}

        //// Hide empty quests on the Quest Board. 
        //for (int i = 0; i < questBoard.GetQuestPapers().Count; i++)
        //{
        //    // If the UI Quest doesn't have an empty UID, skip.
        //    if (UiQuests[i].GetComponent<Quest>().GetUID() != null) { continue; }

        //    // Else, deactivate the quest paper in the same index.
        //    questBoard.GetQuestPapers()[i].SetActive(false);
        //}

        HandleQuestBoard();

        for (int i = 0; i < UiQuests.Count; i++)
        {
            if (UiQuests[i].GetComponent<Quest>().GetQuestType().ToString() != Quest.QuestType.None.ToString()) { continue; }

            UiQuests[i].SetActive(false);
        }
    }

    private void HandleQuestBoard()
    {


        // Clear QuestBoard papers.
        for (int i = 0; i < questBoard.GetQuestPapers().Count; i++)
        {
            var currentQuestPaper = questBoard.GetQuestPapers()[i];

            // Set QuestPaper icons to empty.
            currentQuestPaper.GetComponentInChildren<Image>().sprite = questBoard.GetQuests()[i].GetComponent<Quest>().GetEmptyQuestIcon();

            // Deactivate QuestPapers.
            currentQuestPaper.SetActive(false);
        }

        // Update QuestPapers to match Quests.
        for (int i = 0; i < questBoard.GetQuestPapers().Count; i++)
        {
            var currentQuestPaper = questBoard.GetQuestPapers()[i];

            // Set QuestPaper icons to empty.
            currentQuestPaper.GetComponentInChildren<Image>().sprite = questBoard.GetQuests()[i].GetComponent<Image>().sprite;

            if (currentQuestPaper.GetComponentInChildren<Image>().sprite != questBoard.GetQuests()[i].GetComponent<Quest>().GetEmptyQuestIcon() &&
                questBoard.GetQuests()[i].GetComponent<Quest>().GetUID() != string.Empty)
            {
                // Activate QuestPapers.
                currentQuestPaper.SetActive(true);
            }
        }
    }

    #region Venture Mode Loading

    private void TaskHasRetrievedData_VentureModeLoading(Task<DataSnapshot> task)
    {
        // Get the Quests
        List<GameObject> quests = new List<GameObject>();

        for (int i = 0; i < questSlots.transform.childCount; i++)
        {
            quests.Add(questSlots.transform.GetChild(i).gameObject);
        }

        // Data has been received.
        // Get snapshot of the data.
        DataSnapshot snapshot = task.Result;

        LoadQuestData(quests, snapshot);

        // Loop through quests...
        List<Transform> emptyQuests = new List<Transform>();
        for (int i = 0; i < questSlots.transform.childCount; i++)
        {
            Transform currentQuest = questSlots.transform.GetChild(i);
            // If their sprite is null...
            if (currentQuest.GetComponent<Image>().sprite == quests[0].GetComponent<Quest>().GetEmptyQuestIcon() ||
                currentQuest.GetComponent<Image>().sprite == null)
            {
                // Add the empty quests to a separate list. 
                emptyQuests.Add(currentQuest);
            }
        }

        // Loop through the empty quests and set them as the last sibling.
        for (int i = 0; i < emptyQuests.Count; i++)
        {
            emptyQuests[i].SetAsLastSibling();
        }

        SaveQuests saveQuests = FindObjectOfType<SaveQuests>();

        saveQuests.GetQuests().Clear();

        for (int i = 0; i < questSlots.transform.childCount; i++)
        {
            saveQuests.GetQuests().Add(questSlots.transform.GetChild(i).gameObject);
        }

        saveQuests.SetCanUpdateTimers(true);
    }

    #endregion

    public void SetIsApplicationClosing(bool state) => isApplicationClosing = state;
    public bool GetIsApplicationClosing() => isApplicationClosing;

    public GameObject GetQuestSlots() => questSlots;
    public QuestBoard GetQuestBoard() => questBoard;
}
