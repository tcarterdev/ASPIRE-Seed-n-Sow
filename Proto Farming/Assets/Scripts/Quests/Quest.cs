using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections.Generic;
using UnityEditor;
using System.Collections;

public class Quest : MonoBehaviour
{
    [Header("Quest Properties")]
    [SerializeField] private GameObject questUI;
    [SerializeField] private Image questTypeIcon;
    [SerializeField] private TextMeshProUGUI questDescription;
    [SerializeField] private TextMeshProUGUI questTask;
    [SerializeField] private TextMeshProUGUI questTimerText;
    [SerializeField] private QuestBoard questBoard;
    [SerializeField] private Slider questCompletionBar;
    [SerializeField] private TextMeshProUGUI questCompletionAmount;
    [SerializeField] private GameObject collectRewardButton;
    [SerializeField] private TextMeshProUGUI questMode;

    [Header("Quest Timers")]
    [SerializeField] private int minQuestTimer;
    [SerializeField] private int maxQuestTimer;

    private int minWalkQuestTimer = 259200;
    private int maxWalkQuestTimer = 604800;

    [Header("Data")]
    [SerializeField] private QuestData questData = new QuestData();

    // Task amounts.
    private float currentTaskAmount;
    private float requiredTaskAmount;

    private int questTypeId;

    public float questTimer;

    private bool timerIsRunning;
    private bool questIsOpen;
    private bool isQuestCompleted;

    private GameObject questUiBG;

    [Header("Quest Type Icons")]
    [SerializeField] private Sprite buildQuestIcon;
    [SerializeField] private Sprite tendQuestIcon;
    [SerializeField] private Sprite collectQuestIcon;
    [SerializeField] private Sprite cookQuestIcon;
    [SerializeField] private Sprite walkQuestIcon;
    [SerializeField] private Sprite emptyQuestIcon;

    private string guid;
    private SaveQuests saveQuests;
    private bool hasSubscribedToEvent;

    private int sessionId;

    public enum QuestType
    {
        None,
        Build,
        Tend,
        Collect,
        Cook,
        Walk
    }

    private QuestType questType;

    private void Start()
    {
        if (GetComponent<Image>().sprite == emptyQuestIcon)
        {
            GetComponent<Button>().interactable = false;
        }

        if (SceneHandler.Instance.GetActiveSceneIndex() != (int)SceneIndexes.VENTURE_MODE)
        {
            questBoard = FindObjectOfType<QuestBoard>();
        }
        //else
        //{
        //    guid = questData.uid;
        //}

        // Disable the slider to prevent the player from modifying it. 
        questCompletionBar.interactable = false;

        saveQuests = FindObjectOfType<SaveQuests>();
        questUiBG = questUI.transform.Find("QuestUIBG").gameObject;
    }

    private void Update()
    {
        if (questType != QuestType.None) { HandleTaskCompletion(); }

        // Updates the open quests' timer.
        if (questIsOpen) { UpdateTimer(); }
    }

    #region Quest Events

    /// <summary>
    /// Triggered when the OnBuildingPlaced event is invoked. 
    /// This will be every time the player places a building/workstation/plot. 
    /// 
    /// Updates the task amount, checks if completed, and updates UI. 
    /// </summary>
    /// <param name="sender">The object that sent the invoke.</param>
    /// <param name="e">Variable pass-through, in this case it will be empty.</param>
    private void QuestBoard_OnBuildingPlaced(object sender, System.EventArgs e)
    {
        if (questType != QuestType.Build) { return; }

        if (!isQuestCompleted)
        {
            // Increase the task amount. 
            currentTaskAmount++;

            // Check if the task has been completed. 
            HandleTaskCompletion();
        }

        // Update the UI. 
        UpdateUI();
    }

    /// <summary>
    /// Triggered when the OnCropTendered event is invoked. 
    /// This will be every time the player tends to their crops, 
    /// such as watering, harvesting, etc. 
    /// 
    /// Updates the task amount, checks if completed, and updates UI. 
    /// </summary>
    /// <param name="sender">The object that sent the invoke.</param>
    /// <param name="e">Variable pass-through, in this case it will be empty.</param>
    private void QuestBoard_OnCropTendered(object sender, System.EventArgs e)
    {
        if (questType != QuestType.Tend) { return; }

        if (!isQuestCompleted)
        {
            // Increase the task amount. 
            currentTaskAmount++;

            // Check if the task has been completed. 
            HandleTaskCompletion();
        }

        // Update the UI. 
        UpdateUI();
    }

    /// <summary>
    /// Triggered when the OnItemCollected event is invoked. 
    /// This will be every time the player collects an item, 
    /// both in AR and Farming modes. 
    /// 
    /// Updates the task amount, checks if completed, and updates UI. 
    /// </summary>
    /// <param name="sender">The object that sent the invoke.</param>
    /// <param name="e">Variable pass-through, in this case it will be empty.</param>
    private void QuestBoard_OnItemCollected(object sender, System.EventArgs e)
    {
        if (questType != QuestType.Collect) { return; }

        if (!isQuestCompleted)
        {
            // Increase the task amount. 
            currentTaskAmount++;

            // Add entry to DataGathering to show the quest has been progressed. 
            DataGathering.dataGathering.AddLine("Collect Quest Progressed", sessionId, questType.ToString(), requiredTaskAmount, currentTaskAmount, false);

            // Check if the task has been completed. 
            HandleTaskCompletion();
        }

        // Update the UI. 
        UpdateUI();
    }

    /// <summary>
    /// Triggered when the OnItemCooked event is invoked. 
    /// This will be every time the player cooks an item. 
    /// 
    /// Updates the task amount, checks if completed, and updates UI. 
    /// </summary>
    /// <param name="sender">The object that sent the invoke.</param>
    /// <param name="e">Variable pass-through, in this case it will be empty.</param>
    private void QuestBoard_OnItemCooked(object sender, System.EventArgs e)
    {
        if (questType != QuestType.Cook) { return; }

        if (!isQuestCompleted)
        {
            // Increase the task amount. 
            currentTaskAmount++;

            // Check if the task has been completed. 
            HandleTaskCompletion();
        }

        // Update the UI. 
        UpdateUI();
    }

    /// <summary>
    /// Triggered when the OnDistanceTravelled event is invoked. 
    /// This will be every time the player moves within the AR mode. 
    /// 
    /// Updates the task amount, checks if completed, and updates UI. 
    /// </summary>
    /// <param name="sender">The object that sent the invoke.</param>
    /// <param name="e">Variable pass-through, in this case it will be empty.</param>
    private void QuestBoard_OnDistanceTravelled(object sender, float distanceTravelled)
    {
        if (questType != QuestType.Walk) { return; }

        if (!isQuestCompleted)
        {
            // Increase the task amount. 
            currentTaskAmount += distanceTravelled;

            // Add entry to DataGathering to show the quest has been progressed. 
            DataGathering.dataGathering.AddLine("Walk Quest Progressed", sessionId, questType.ToString(), requiredTaskAmount, currentTaskAmount, false);

            // Check if the task has been completed. 
            HandleTaskCompletion();
        }

        // Update the UI. 
        UpdateUI();
    }

    private void QuestProgress_OnEndOfSession(object sender, System.EventArgs e)
    {
        switch (questType)
        {
            //case 1:
            //    DataGathering.dataGathering.AddLine("Build Quest End of Session", sessionId, questTypeId, requiredTaskAmount, currentTaskAmount, isQuestCompleted);
            //    break;
            //case 2:
            //    DataGathering.dataGathering.AddLine("Tend Quest End of Session", sessionId, questTypeId, requiredTaskAmount, currentTaskAmount, isQuestCompleted);
            //    break;
            case QuestType.Collect:
                DataGathering.dataGathering.AddLine("Collect Quest End of Session", sessionId, questType.ToString(), requiredTaskAmount, currentTaskAmount, isQuestCompleted);
                break;
            //case 4:
            //    DataGathering.dataGathering.AddLine("Order Quest End of Session", sessionId, questTypeId, requiredTaskAmount, currentTaskAmount, isQuestCompleted);
            //    break;
            case QuestType.Walk:
                DataGathering.dataGathering.AddLine("Walk Quest End of Session", sessionId, questType.ToString(), requiredTaskAmount, currentTaskAmount, isQuestCompleted);
                break;
        }
    }

    #endregion

    /// <summary>
    /// Handles the completion of a task. Checks if the current task amount is equal to the required amount. 
    /// 
    /// Updates the quest papers on the quest board with a completed icon. 
    /// </summary>
    public void HandleTaskCompletion()
    {
        if (currentTaskAmount >= requiredTaskAmount)
        {
            
            Debug.Log("Handling Task Completion");
            // Set our current task amount to meet the required task amount. Prevents it from exceeding the value. 
            currentTaskAmount = requiredTaskAmount;

            isQuestCompleted = true;

            switch (questType)
            {
                case QuestType.Build:
                    DataGathering.dataGathering.Firebase_QuestCompleted("Build");
                    break;

                case QuestType.Tend:
                    DataGathering.dataGathering.Firebase_QuestCompleted("Tend");
                    break;

                case QuestType.Collect:
                    DataGathering.dataGathering.AddLine("Collect Quest Completed", sessionId, questType.ToString(), requiredTaskAmount, currentTaskAmount, true);
                    DataGathering.dataGathering.Firebase_QuestCompleted("Collect");
                    break;

                case QuestType.Cook:
                    DataGathering.dataGathering.Firebase_QuestCompleted("Cook");
                    break;

                case QuestType.Walk:
                    DataGathering.dataGathering.AddLine("Walk Quest Completed", sessionId, questType.ToString(), requiredTaskAmount, currentTaskAmount, true);
                    DataGathering.dataGathering.Firebase_QuestCompleted("Walk");
                    break;
            }

            if (SceneHandler.Instance.GetActiveSceneIndex() != (int)SceneIndexes.VENTURE_MODE)
            {
                questBoard = FindObjectOfType<QuestBoard>();

                // Quest completed. 
                for (int i = 0; i < questBoard.GetQuestPapers().Count; i++)
                {
                    if (this.gameObject == questBoard.GetQuests()[i] && 
                        this.gameObject.GetComponent<Quest>().currentTaskAmount >= this.gameObject.GetComponent<Quest>().requiredTaskAmount)
                    {
                        // If the timer has expired, we ignore the completion. 
                        if (questBoard.GetQuests()[i].GetComponent<Quest>().questTimer <= 0) { continue; }

                        questBoard.GetQuestPapers()[i].GetComponentInChildren<Image>().sprite = questBoard.questCompleteIcon;
                    }
                }
            }

            UnsubscribeToQuestEvent();

            EnableQuestReward();
        }
    }

    /// <summary>
    /// Enables the quest reward for the player to receive when ready. 
    /// 
    /// On click of the button, the reward will be granted and the quest will be removed. 
    /// </summary>
    private void EnableQuestReward()
    {
        if (questIsOpen)
        {
            //guid = null;

            // Hide UI elements.
            questTimerText.gameObject.SetActive(false);
            questCompletionBar.gameObject.SetActive(false);

            // Show collect reward button. 
            collectRewardButton.SetActive(true);
            questTask.text = "Quest Completed!";
        }
    }

    /// <summary>
    /// Handles the quest timer, but removing it from the quests UI and quest board when the timer has expired.
    /// </summary>
    public void HandleQuestTimer()
    {
        if (timerIsRunning)
        {
            if (questTimer <= 0)
            {
                guid = string.Empty;

                UnsubscribeToQuestEvent();

                switch (questType)
                {
                    case QuestType.Build:
                        DataGathering.dataGathering.Firebase_QuestLapsed("Build", requiredTaskAmount, currentTaskAmount);
                        break;
                    case QuestType.Tend:
                        DataGathering.dataGathering.Firebase_QuestLapsed("Tend", requiredTaskAmount, currentTaskAmount);
                        break;
                    case QuestType.Collect:
                        DataGathering.dataGathering.Firebase_QuestLapsed("Collect", requiredTaskAmount, currentTaskAmount);
                        break;
                    case QuestType.Cook:
                        DataGathering.dataGathering.Firebase_QuestLapsed("Order", requiredTaskAmount, currentTaskAmount);
                        break;
                    case QuestType.Walk:
                        DataGathering.dataGathering.Firebase_QuestLapsed("Walk", requiredTaskAmount, currentTaskAmount);
                        break;
                }

                if (questUiBG == null) { questUiBG = questUI.transform.Find("QuestUIBG").gameObject; }

                // Hide Quest UI.
                questUiBG.SetActive(false);
  
                // Reset values. 
                questTimerText.text = "Time Left: 0:00:00";
                questIsOpen = false;
                currentTaskAmount = 0;
                requiredTaskAmount = 0;
                isQuestCompleted = false;
                GetComponent<Button>().interactable = false;

                // Update quest slots UI. 
                // Loop through quests.
                int questIndexToStartFrom = 0;
                List<GameObject> quests = questBoard.GetQuests();
                GameObject tempQuest = null;
                for (int i = 0; i < quests.Count; i++)
                {
                    // If the sprites between the quest board and quest UI match, and the timer is zero, update quest UI. 
                    if (quests[i].GetComponentInChildren<Image>().sprite == questBoard.GetQuestPapers()[i].GetComponentInChildren<Image>().sprite
                        && quests[i].GetComponent<Quest>().questTimer <= 0)
                    {
                        // Index is used to update the positions of the quest boards that are after the expired quest.
                        questIndexToStartFrom = i;

                        // Minus from the corresponding counter
                        questBoard.MinusFromCounter(quests[i].GetComponent<Quest>().questType);

                        // Set quest ID to zero.
                        quests[i].GetComponent<Quest>().questType = QuestType.None;
                        quests[i].GetComponent<Quest>().timerIsRunning = false;

                        // Reset sprite for quests slots UI. 
                        questBoard.GetQuestSlots().transform.GetChild(i).GetComponent<Image>().sprite = questBoard.emptyQuestIcon;

                        // Set as last sibling to update quest slots UI. 
                        questBoard.GetQuestSlots().transform.GetChild(i).SetAsLastSibling();

                        // Hide object from Quest Board. 
                        questBoard.GetQuestPapers()[questBoard.GetQuestPapers().Count - 1].SetActive(false);
                    }
                }

                // Update positions of the quests on Quest Board. 
                List<GameObject> questPapers = questBoard.GetQuestPapers();
                for (int i = questIndexToStartFrom; i < questBoard.GetQuestPapers().Count; i++)
                {
                    if (i + 1 < questBoard.GetQuests().Count)
                    {
                        questPapers[i].GetComponentInChildren<Image>().sprite = questBoard.GetQuests()[i + 1].GetComponent<Image>().sprite;
                    }
                }

                // Hide empty quests on the Quest Board. 
                for (int i = 0; i < questPapers.Count; i++)
                {
                    // Continue through the loop until the sprite matches the emptyQuestIcon sprite.
                    if (questPapers[i].GetComponentInChildren<Image>().sprite != questBoard.emptyQuestIcon) { continue; }

                    // Hide quest paper.
                    questPapers[i].SetActive(false);
                }

                // The expired quest we want to remove and re-add.
                tempQuest = questBoard.GetQuestSlots().transform.GetChild(questBoard.GetQuestPapers().Count - 1).gameObject;

                // Remove and then re-add the quest. 
                questBoard.GetQuests().Remove(tempQuest);
                questBoard.GetQuests().Add(tempQuest);

                // Check for task completion upon updating the positions of the quest papers. 
                // This will update their icons if they had been completed prior to the re-positioning. 
                for (int i = 0; i < questPapers.Count; i++)
                {
                    if (questBoard.GetQuests()[i].GetComponent<Quest>().currentTaskAmount == questBoard.GetQuests()[i].GetComponent<Quest>().requiredTaskAmount)
                    {
                        questBoard.GetQuests()[i].GetComponent<Quest>().HandleTaskCompletion();
                    }
                }
            }
        }
    }

    /// <summary>
    /// Sets up the quest before updating the UI. 
    /// </summary>
    public void SetUpQuests()
    {
        // If there is no UID, and there is a valid questTypeID...
        if (guid == string.Empty && questType != QuestType.None)
        {
            // Call the Firebase Quest Accepted event. 
            switch (questType)
            {
                case QuestType.Build:
                    DataGathering.dataGathering.Firebase_QuestAccepted("Build");
                    break;
                case QuestType.Tend:
                    DataGathering.dataGathering.Firebase_QuestAccepted("Tend");
                    break;
                case QuestType.Collect:
                    DataGathering.dataGathering.Firebase_QuestAccepted("Collect");
                    break;
                case QuestType.Cook:
                    DataGathering.dataGathering.Firebase_QuestAccepted("Cook");
                    break;
                case QuestType.Walk:
                    DataGathering.dataGathering.Firebase_QuestAccepted("Walk");
                    break;
            }

            // Generate a new UID. 
            guid = System.Guid.NewGuid().ToString();
        }

        // Ensure the isQuestCompleted is false when setting up initial quests. 
        isQuestCompleted = false;

        // Generate a new quest timer value.
        questTimer = Random.Range(minQuestTimer, maxQuestTimer);

        if (questType == QuestType.Walk)
        {
            questTimer = Random.Range(minWalkQuestTimer, maxWalkQuestTimer);
        }

        // Generate a new task amount value. 
        SetRandomTaskAmountValue();

        // Update slider to show correct values. 
        questCompletionBar.maxValue = requiredTaskAmount;

        UpdateUI();

        // Start the timer. 
        timerIsRunning = true;

        // Enable the button. 
        GetComponent<Button>().interactable = true;

        if (GetComponent<Image>().sprite == questBoard.emptyQuestIcon)
        {
            GetComponent<Button>().interactable = false;
        }

        if (!hasSubscribedToEvent) { SubscribeToQuestEvent(); }
    }

    public void SubscribeToQuestEvent()
    {
        if (questType != QuestType.None)
        {
            hasSubscribedToEvent = true;
        }

        // Subscribe to relevant event based on quest type.
        switch (questType)
        {
            case QuestType.Build:
                QuestProgress.Instance.OnBuildingPlaced += QuestBoard_OnBuildingPlaced;
                break;
            case QuestType.Tend:
                QuestProgress.Instance.OnCropTendered += QuestBoard_OnCropTendered;
                break;
            case QuestType.Collect:
                QuestProgress.Instance.OnItemCollected += QuestBoard_OnItemCollected;
                break;
            case QuestType.Cook:
                QuestProgress.Instance.OnItemCooked += QuestBoard_OnItemCooked;
                break;
            case QuestType.Walk:
                QuestProgress.Instance.OnDistanceTravelled += QuestBoard_OnDistanceTravelled;
                break;
        }

        QuestProgress.Instance.OnEndOfSession += QuestProgress_OnEndOfSession;
    }

    public void UnsubscribeToQuestEvent()
    {
        hasSubscribedToEvent = false;

        // Subscribe to relevant event based on quest type.
        switch (questType)
        {
            case QuestType.Build:
                QuestProgress.Instance.OnBuildingPlaced -= QuestBoard_OnBuildingPlaced;
                break;
            case QuestType.Tend:
                QuestProgress.Instance.OnCropTendered -= QuestBoard_OnCropTendered;
                break;
            case QuestType.Collect:
                QuestProgress.Instance.OnItemCollected -= QuestBoard_OnItemCollected;
                break;
            case QuestType.Cook:
                QuestProgress.Instance.OnItemCooked -= QuestBoard_OnItemCooked;
                break;
            case QuestType.Walk:
                QuestProgress.Instance.OnDistanceTravelled -= QuestBoard_OnDistanceTravelled;
                break;
        }
    }

    /// <summary>
    /// Sets the task amount value to a random value. 
    /// </summary>
    private void SetRandomTaskAmountValue()
    {
        requiredTaskAmount = Random.Range(4, 16);
        
        if (questType == QuestType.Walk)
        {
            switch (questBoard.GetWalkLevel())
            {
                case 1:
                    // 500m
                    requiredTaskAmount = 500;
                    break;
                case 2:
                    // 1.5km
                    requiredTaskAmount = 1500;
                    break;
                case 3:
                    // 2.6km
                    requiredTaskAmount = 2600;
                    break;
                case 4:
                    // 3.7km
                    requiredTaskAmount = 3700;
                    break;
                case 5:
                    // 4.8km 
                    requiredTaskAmount = 4800;
                    break;
            }
        }
    }

    /// <summary>
    /// Toggles the QuestUI to true and updates the UI
    /// </summary>
    public void ToggleQuestUI()
    {
        if (SceneHandler.Instance.GetActiveSceneIndex() != (int)SceneIndexes.VENTURE_MODE)
        {
            for (int i = 0; i < questBoard.GetQuests().Count; i++)
            {
                questBoard.GetQuests()[i].GetComponent<Quest>().questIsOpen = false;
            }
        }
        else if (SceneHandler.Instance.GetActiveSceneIndex() == (int)SceneIndexes.VENTURE_MODE)
        {
            for (int i = 0; i < saveQuests.GetQuests().Count; i++)
            {
                saveQuests.GetQuests()[i].GetComponent<Quest>().questIsOpen = false;
            }
        }

        questIsOpen = true;

        UpdateUI();
        questUiBG.SetActive(true);
    }

    /// <summary>
    /// Sets the properties for the quest that was randomised from the Quest Board.
    /// </summary>
    /// <param name="id">the id of the type of quest.</param>
    public void SetQuestType(QuestType newQuestType)
    {
        questType = newQuestType;
    }

    /// <summary>
    /// Updates the quest UI description, task and icon of the quest type.
    /// </summary>
    private void UpdateUI()
    {
        if (SceneHandler.Instance.GetActiveSceneIndex() == (int)SceneIndexes.VENTURE_MODE)
        {
            UpdateUI_VentureMode();
        }

        // Update timer UI. 
        UpdateTimer();

        if (questIsOpen)
        {
            if (!isQuestCompleted)
            {
                // Hide the collect reward button, and show the task timer and completion bar.
                collectRewardButton.SetActive(false);
                questTimerText.gameObject.SetActive(true);
                questCompletionBar.gameObject.SetActive(true);
            }

            // Update completion bar. 
            questCompletionBar.maxValue = requiredTaskAmount;
            questCompletionBar.value = currentTaskAmount;

            // Update completion text. 
            if (questType != QuestType.Walk)
            {
                questCompletionAmount.text = $"{currentTaskAmount}/{requiredTaskAmount}";
            }
            else if (questType == QuestType.Walk)
            {
                questCompletionAmount.text = $"{(currentTaskAmount / 1000f).ToString("0.##")}/{(requiredTaskAmount / 1000f).ToString("0.##")}";
            }

            // Change the quest UI icon based on the quest type.
            switch (questType)
            {
                case QuestType.Build:
                    questTypeIcon.sprite = buildQuestIcon;

                    questDescription.text = $"You have been given the task of building!";
                    questTask.text = $"Build/Place {requiredTaskAmount} items";
                    questMode.text = "Farming mode only";
                    break;
                case QuestType.Tend:
                    questTypeIcon.sprite = tendQuestIcon;

                    questDescription.text = $"You have been given the task of tending your crops!";
                    questTask.text = $"Tend {requiredTaskAmount} crops";
                    questMode.text = "Farming mode only";
                    break;
                case QuestType.Collect:
                    questTypeIcon.sprite = collectQuestIcon;

                    questDescription.text = $"You have been given the task of collecting items!";
                    questTask.text = $"Collect {requiredTaskAmount} items";
                    questMode.text = "Farming and/or Venture mode";
                    break;
                case QuestType.Cook:
                    questTypeIcon.sprite = cookQuestIcon;

                    questDescription.text = $"You have been given the task of cooking!";
                    questTask.text = $"Cook {requiredTaskAmount} items";
                    questMode.text = "Farming mode only";
                    break;
                case QuestType.Walk:
                    questTypeIcon.sprite = walkQuestIcon;

                    questDescription.text = $"You have been given the task of walking!";
                    questTask.text = $"Walk {requiredTaskAmount / 1000f}km";
                    questMode.text = "Venture mode only";
                    break;
            }
        }
    }

    private void UpdateUI_VentureMode()
    {
        Sprite sprite = null;
        switch (questType)
        {
            case QuestType.Build:
                sprite = buildQuestIcon;
                break;
            case QuestType.Tend:
                sprite = tendQuestIcon;
                break;
            case QuestType.Collect:
                sprite = collectQuestIcon;
                break;
            case QuestType.Cook:
                sprite = cookQuestIcon;
                break;
            case QuestType.Walk:
                sprite = walkQuestIcon;
                break;
        }

        GetComponent<Image>().sprite = sprite;
    }

    /// <summary>
    /// Updates the quest timer text with correct formatting. 
    /// </summary>
    private void UpdateTimer()
    {
        float days = Mathf.FloorToInt(questTimer / 86400) % 365;
        float hours = Mathf.FloorToInt(questTimer / 3600) % 24;
        float minutes = Mathf.FloorToInt(questTimer / 60) % 60;
        float seconds = Mathf.FloorToInt(questTimer % 60);

        questTimerText.text = "Time Left: ";
        if (days > 0) { questTimerText.text += days + "d "; }
        if (hours > 0) { questTimerText.text += hours + "h "; }
        if (minutes > 0) { questTimerText.text += minutes + "m "; }
        if (seconds > 0) { questTimerText.text += seconds + "s "; } 
    }

    #region Save & Load

    /// <summary>
    /// Saves the quest data.
    /// </summary>
    public void SaveData()
    {
        questData.questType = questType.ToString();
        
        if (guid != null)
        {
            questData.uid = guid.ToString();
        }

        if (QuestSaveManager.Instance.GetIsApplicationClosing())
        {
            questData.sessionId = 0;
        }
        else
        {
            questData.sessionId = sessionId;
        }

        questData.timer = questTimer;
        questData.amount = currentTaskAmount;
        questData.requiredAmount = requiredTaskAmount;
    }

    /// <summary>
    /// Loads the questData and sets it to the game object. 
    /// </summary>
    public void LoadData()
    {
        questBoard = FindObjectOfType<QuestBoard>();

        if (questData.questType != string.Empty)
        {
            questType = (QuestType)System.Enum.Parse(typeof(QuestType), questData.questType);
        }

        guid = questData.uid;
        sessionId = questData.sessionId;

        questTimer = questData.timer;
        currentTaskAmount = questData.amount;
        requiredTaskAmount = questData.requiredAmount;

        // Update the UI once the data has been loaded. 
        UpdateUI();

        if (SceneHandler.Instance.GetActiveSceneIndex() != (int)SceneIndexes.VENTURE_MODE)
        { 
            // Update quest icons to match the loaded quests.
            questBoard.UpdateQuestIcons();
        }
    }

    #endregion

    #region Getters & Setters

    /// <summary>
    /// Gets the quest type ID.
    /// </summary>
    /// <returns>An int that resembles the quest type ID.</returns>
    public QuestType GetQuestType() => questType;

    /// <summary>
    /// Retrieves the questData of the plot.
    /// </summary>
    /// <returns>The quest data.</returns>
    public QuestData GetQuestData() => questData;

    /// <summary>
    /// Gets the isQuestCompleted bool.
    /// </summary>
    /// <returns>A bool for if the quest is completed or not.</returns>
    public bool GetIsQuestCompleted() => isQuestCompleted;

    /// <summary>
    /// Sets the quest data, called when loading the quest data.
    /// </summary>
    /// <param name="questType">The type of quest.</param>
    /// <param name="timer">The quest timer at the time of saving.</param>
    /// <param name="amount">the current progress towards the task.</param>
    /// <param name="requiredAmount">the required amount for the task.</param>
    public void SetQuestData(QuestType questType, string uid, int sessionId, float timer, float amount, float requiredAmount)
    {
        questData.questType = questType.ToString();
        questData.uid = uid;
        questData.sessionId = sessionId;
        questData.timer = timer;
        questData.amount = amount;
        questData.requiredAmount = requiredAmount;
    }

    public void UpdateQuestType(QuestType newQuestType) => questType = newQuestType;

    public void SetIsTimerRunning(bool state) => timerIsRunning = state;

    public float GetCurrentTaskAmount() => currentTaskAmount;

    public float GetRequiredTaskAmount() => requiredTaskAmount;

    public Sprite GetEmptyQuestIcon() => emptyQuestIcon;

    public string GetUID() => guid;
    public void SetUID(string newUID) => guid = newUID; 

    public int GetSessionID() => sessionId;
    public void SetSessionID(int id)
    {
        sessionId = id;
    }

    public bool GetHasSubscribedToEvent() => hasSubscribedToEvent;

    public void SetIsQuestOpen(bool state) => questIsOpen = state;
    public bool GetIsQuestOpen() => questIsOpen;

    public void SetCurrentTaskAmount(float amount) => currentTaskAmount = amount;
    public void SetRequiredTaskAmount(float amount) => requiredTaskAmount = amount;

    public void SetTimerAmount(float amount) => questTimer = amount;

    #endregion
}
