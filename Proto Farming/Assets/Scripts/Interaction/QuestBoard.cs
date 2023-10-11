using System.CodeDom.Compiler;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class QuestBoard : WorkStation
{
    public event System.EventHandler<string> OnQuestSaved;

    [Header("Quest Board Properties")]
    [SerializeField] private GameObject questBoardUI;
    [SerializeField] private GameObject questBoardBG;
    [SerializeField] private GameObject questSlots;
    [SerializeField] private GameObject questPapersObj;

    [Tooltip("The quests in the UI")]
    [SerializeField] private List<GameObject> quests;

    [Tooltip("The quest papers that are on the Quest Board.")]
    [SerializeField] private List<GameObject> questPapers;

    [Header("Quest Type Icons")]
    [SerializeField] private Sprite buildQuestIcon;
    [SerializeField] private Sprite tendQuestIcon;
    [SerializeField] private Sprite collectQuestIcon;
    [SerializeField] private Sprite cookQuestIcon;
    [SerializeField] private Sprite walkQuestIcon;
    public Sprite emptyQuestIcon;
    public Sprite questCompleteIcon;

    [SerializeField] private GameObject questUI;

    // Quest Type Counters.
    private int buildQuestCounter;
    private int tendQuestCounter;
    private int collectQuestCounter;
    private int cookQuestCounter;
    private int walkQuestCounter;

    [SerializeField] private string fileName = "quests.json";

    [Header("Quest Data")]
    [SerializeField] private List<QuestData> questDataList;

    [Header("Quest Rewards")]
    [SerializeField] private List<ItemData> questRewards;

    private int walkLevel = 1;

    // Quests bsaed on time
    [SerializeField] private float timeBetweenQuestCheck = 60.0f;
    private float questCheckTimer;
    private bool timeQuestGivenOne;
    private bool timeQuestGivenTwo;
    private bool timeQuestGivenThree;
    private bool startQuestCheckTimer;

    private GameObject questUiBG;

    private PlayerManager playerManager;

    private bool canUpdateTimers;

    public override void InteractionPopUp()
    {
        interactionButton.gameObject.SetActive(true);
        interactionText.SetText(buttonPrompt);
    }

    public override void InteractWithWorkStation()
    {
        interactionButton.gameObject.SetActive(false);
        questBoardBG.SetActive(true);
        flowerFooter.SetActive(false);
    }

    private void Start()
    {
        canUpdateTimers = false;

        GetReferences();

        quests.Clear();

        // Loop through quest slots.
        for (int i = 0; i < questSlots.transform.childCount; i++)
        {
            // Add the quest slots' children to quests list.
            quests.Add(questSlots.transform.GetChild(i).gameObject);
        }

        questCheckTimer = 0.5f;

        if (QuestSaveManager.Instance != null)
        {
            QuestSaveManager.Instance.OnQuestLoadCompleted += QuestSaveManager_OnQuestLoadCompleted;
        }

        playerManager = FindObjectOfType<PlayerManager>();
    }

    private void GetReferences()
    {
        questBoardUI = GameObject.Find("Quest Board UI");
        questBoardBG = questBoardUI.transform.GetChild(0).gameObject;

        questSlots = questBoardUI.transform.GetChild(0).Find("Quest Slots").gameObject;

        questUI = GameObject.Find("Quest UI");
        questUiBG = questUI.transform.Find("QuestUIBG").gameObject;

        questPapersObj = transform.GetChild(0).Find("Quest Papers").gameObject;
    }

    private void Update()
    {
        /// Used for debugging. 
        if (Input.GetKeyDown(KeyCode.Q))
        {
            GenerateQuests(3);
        }

        /// Used for debugging.
        if (Input.GetKeyDown(KeyCode.Alpha1)) { QuestProgress.Instance.InvokeBuildingPlaced(); }
        if (Input.GetKeyDown(KeyCode.Alpha2)) { QuestProgress.Instance.InvokeCropTendered(); }
        if (Input.GetKeyDown(KeyCode.Alpha3)) { QuestProgress.Instance.InvokeItemCollected(); }
        if (Input.GetKeyDown(KeyCode.Alpha4)) { QuestProgress.Instance.InvokeItemCooked(); }


        if (canUpdateTimers)
        {
            // Update quest timers. 
            UpdateQuestTimers();
        }

        // Reduce checkTimeTimer every second if startQuestCheckTimer is true.
        if (startQuestCheckTimer) { questCheckTimer -= Time.deltaTime; }

        // If the timer has reached 0 or below, check if more quests can be added.
        if (questCheckTimer <= 0)
        {
            CheckRealWorldTime();
            questCheckTimer = timeBetweenQuestCheck;
        }
    }

    public void CheckRealWorldTime()
    {
        Debug.Log("checking real world time");

        // The hour of the current day, in real life.
        int sysHour = System.DateTime.Now.Hour;

        Debug.Log($"System Time: {sysHour}");

        // Generate quests based on the current hour of the day, in real life.
        // If it is past 7am, generate 3 more quests.
        if (sysHour > 7 && !playerManager.GetTimeQuestGiven(1))
        {
            playerManager.SetTimeQuestGiven(1, true);
            GenerateQuests(3);
        }

        // If it is past 12pm, generate 3 more quests. 
        if (sysHour > 12 && !playerManager.GetTimeQuestGiven(2))
        {
            playerManager.SetTimeQuestGiven(2, true);
            GenerateQuests(3);
        }
        
        // If it is past 11pm, generate 3 more quests.
        if (sysHour > 23 && !playerManager.GetTimeQuestGiven(3))
        {
            playerManager.SetTimeQuestGiven(3, true);
            GenerateQuests(3);
        }
        
        // If it is past 12am, reset the quest booleans.
        if (sysHour > 00 && playerManager.GetTimeQuestGiven(1) && playerManager.GetTimeQuestGiven(2) && playerManager.GetTimeQuestGiven(3))
        {
            playerManager.SetTimeQuestGiven(1, false);
            playerManager.SetTimeQuestGiven(2, false);
            playerManager.SetTimeQuestGiven(3, false);
        }
    }

    public void SaveQuestBoard()
    {
        // Go through each quest and save their data to the questData.
        foreach (var quest in quests)
        {
            quest.GetComponent<Quest>().SaveData();
        }

        // Clear the quest data list before adding more.
        questDataList.Clear();

        // Retrieve the questData from the quests 
        for (int i = 0; i < quests.Count; i++)
        {
            questDataList.Add(quests[i].GetComponent<Quest>().GetQuestData());
        }

        FBFileHandler.SaveToJSON<QuestData>(questDataList, fileName);
        OnQuestSaved?.Invoke(this, JsonHelper.ToJson<QuestData>(questDataList.ToArray()));
    }

    public void LoadQuestBoard()
    {
        // Make sure references have been retrieved.
        questBoardUI = GameObject.Find("Quest Board UI");
        questBoardBG = questBoardUI.transform.GetChild(0).gameObject;

        questSlots = questBoardUI.transform.GetChild(0).Find("Quest Slots").gameObject;

        questUI = GameObject.Find("Quest UI");
        questUiBG = questUI.transform.Find("QuestUIBG").gameObject;

        questPapersObj = transform.GetChild(0).Find("Quest Papers").gameObject;

        quests.Clear();

        // Loop through quest slots.
        for (int i = 0; i < questSlots.transform.childCount; i++)
        {
            // Add the quest slots' children to quests list.
            quests.Add(questSlots.transform.GetChild(i).gameObject);
        }

        // Load through each quest and update their data. 
        foreach (var quest in quests)
        {
            quest.GetComponent<Quest>().LoadData();
        }

        canUpdateTimers = true;
    }

    private void QuestSaveManager_OnQuestLoadCompleted(object sender, System.EventArgs e)
    {
        Debug.Log("OnQuestLoadCompleted");

        startQuestCheckTimer = true;

        canUpdateTimers = true;

        //// Loop through 
        //for (int i = 0; i < quests.Count; i++)
        //{
        //    Debug.Log($"{quests[i].name} Type: {quests[i].GetComponent<Quest>().GetQuestType()}");
        //    if (quests[i].GetComponent<Quest>().GetQuestType().ToString() != Quest.QuestType.None.ToString()) { continue; }

        //    quests[i].SetActive(false);
        //}
    }

    /// <summary>
    /// Checks if the quest board is full by checking if the object is active.
    /// </summary>
    /// <returns>a bool. True if full, false if not.</returns>
    private bool IsQuestBoardFull()
    {
        int isActiveCounter = 0;

        // Loop through quest papers and check if they are active or not.
        for (int i = 0; i < questPapers.Count; i++)
        {
            if (questPapers[i].activeInHierarchy)
            {
                // Increase active counter. 
                isActiveCounter++;
            }
        }

        return isActiveCounter == 9 ? true : false;
    }

    /// <summary>
    /// Calculates how much space is left on the quest board. 
    /// </summary>
    /// <returns>An int of how many spaces are left. </returns>
    private int CalculateSpaceLeftForQuests()
    {
        int isActiveCounter = 0;

        // Loop through quest papers and check if they are active or not.
        for (int i = 0; i < questPapers.Count; i++)
        {
            if (questPapers[i].activeInHierarchy)
            {
                // Increase active counter.
                isActiveCounter++;
            }
        }

        // The amount of space left.
        int spaceLeft = questPapers.Count - isActiveCounter;

        return spaceLeft;
    }

    /// <summary>
    /// Adds the new quest.
    /// </summary>
    /// <param name="questType">Enum of the type of quest, chosen within the GenerateQuests function.</param>
    private void AddNewQuest(Quest.QuestType questType)
    {
        // The index of where to start from within the list. 
        int questPaperIndex = 0;

        // Loop through the quest papers and find the first inactive quest paper.
        for (int i = 0; i < questPapers.Count; i++)
        {
            if (!questPapers[i].activeSelf)
            {
                questPaperIndex = i;
                break;
            }
        }
        
        // If the quest paper index is out of bounds, return out.
        if (questPaperIndex < 0 || questPaperIndex > 9) { return; }

        // Set the quest icon based on the quest type ID.
        Sprite questIcon = null;
        switch (questType)
        {
            case Quest.QuestType.Build:
                questIcon = buildQuestIcon;
                break;
            case Quest.QuestType.Tend:
                questIcon = tendQuestIcon;
                break;
            case Quest.QuestType.Collect:
                questIcon = collectQuestIcon;
                break;
            case Quest.QuestType.Cook:
                questIcon = cookQuestIcon;
                break;
            case Quest.QuestType.Walk:
                questIcon = walkQuestIcon;
                break;
            default:
                questIcon = emptyQuestIcon;
                break;
        }

        // Check if the slot is definitely false, and if it is, update it with the relevenat data and enable it.
        if (!questPapers[questPaperIndex].activeSelf)
        {
            questPapers[questPaperIndex].SetActive(true);
            questPapers[questPaperIndex].GetComponentInChildren<Image>().sprite = questIcon;

            // Updates the quest UI sprites to show the correct icon.
            questSlots.transform.GetChild(questPaperIndex).GetComponent<Image>().sprite = questIcon;
            questSlots.transform.GetChild(questPaperIndex).gameObject.SetActive(true);

            // Give GUID
            quests[questPaperIndex].GetComponent<Quest>().SetUID(System.Guid.NewGuid().ToString());

            // Updates the quest properties, so the UI displays the correct information for the quest.
            quests[questPaperIndex].GetComponent<Quest>().SetQuestType(questType);

            // Generate a new timer for the newly generated quest. 
            //quests[questPaperIndex].GetComponent<Quest>().SetUpQuests();
        }
    }

    /// <summary>
    /// Generates the players' quest by going through each quest and giving them a type. 
    /// </summary>
    private void GenerateQuests(int questsToGenerate)
    {
        // If the quest board is full, don't generate more. 
        if (IsQuestBoardFull()) 
        {
            return;
        }

        // Determines how much space is left.
        int spaceLeft = CalculateSpaceLeftForQuests();

        // If the space left is between 0 and 3, handle it by generating enough quests to not go out of bounds, 
        // or returning early if none need to be generated.
        if (spaceLeft < 4)
        {
            if (spaceLeft == 3)
            {
                questsToGenerate = 3;
            }
            else if (spaceLeft == 2)
            {
                questsToGenerate = 2;
            }
            else if (spaceLeft == 1)
            {
                questsToGenerate = 1;
            }
            else
            {
                return;
            }
        }



        // Loop through depending on how many quests we're trying to generate. 
        for (int i = 0; i < questsToGenerate; i++)
        {
            // Random number to determine the quest type.
            Quest.QuestType questType = (Quest.QuestType)Random.Range(1, 6);

            // If we have more of one type than we want, go back an iteration and continue until we find a space. 
            if (questType == Quest.QuestType.Build && buildQuestCounter == 2)
            {
                i--;
                continue;
            }
            else if (questType == Quest.QuestType.Tend && tendQuestCounter == 2)
            {
                i--;
                continue;
            }
            else if (questType == Quest.QuestType.Collect && collectQuestCounter == 2)
            {
                i--;
                continue;
            }
            else if (questType == Quest.QuestType.Cook && cookQuestCounter == 2)
            {
                i--;
                continue;
            }
            else if (questType == Quest.QuestType.Walk && walkQuestCounter == 2)
            {
                i--;
                continue;
            }

            // Set the quest icon to resemble the quest type, and increase the corresponding counter.
            switch (questType)
            {
                case Quest.QuestType.Build:
                    AddNewQuest(questType);

                    buildQuestCounter++;
                    break;
                case Quest.QuestType.Tend:
                    AddNewQuest(questType);

                    tendQuestCounter++;
                    break;
                case Quest.QuestType.Collect:
                    AddNewQuest(questType);

                    collectQuestCounter++;
                    break;
                case Quest.QuestType.Cook:
                    AddNewQuest(questType);

                    cookQuestCounter++;
                    break;
                case Quest.QuestType.Walk:
                    AddNewQuest(questType);

                    walkQuestCounter++;
                    break;
            }
        }
       
        SetUpQuests();
    }

    /// <summary>
    /// Sets up the quests by looping through the quests and calling their own SetUpQuests function.
    /// </summary>
    private void SetUpQuests()
    {
        // Loop through quests.
        for (int i = 0; i < quests.Count; i++)
        {
            // Sets up the quest.
            quests[i].GetComponent<Quest>().SetUpQuests();
        }
    }

    /// <summary>
    /// Updates the quest timers by looping through the quests and calling their own HandleQuestTimer() function. 
    /// 
    /// Ensures it is updated even if the gameObject isn't active.
    /// </summary>
    private void UpdateQuestTimers()
    {
        // Loop through quests.
        for (int i = 0; i < quests.Count; i++)
        {
            if (quests[i].GetComponent<Quest>().questTimer > 0 && !quests[i].GetComponent<Quest>().GetIsQuestCompleted() && quests[i].GetComponent<Quest>().GetQuestType() != Quest.QuestType.None)
            {
                // Only count the timer down if the quest is not completed, and the quest ID is not equal to 0.
                quests[i].GetComponent<Quest>().questTimer -= Time.deltaTime;
            }
            else if (quests[i].GetComponent<Quest>().questTimer <= 0 && quests[i].GetComponent<Quest>().GetQuestType() != Quest.QuestType.None)
            {
                quests[i].GetComponent<Quest>().HandleQuestTimer();
            }
        }
    }

    /// <summary>
    /// Minuses from the corresponding counter.
    /// </summary>
    /// <param name="questType">An enum of the questType to minus the counter from.</param>
    public void MinusFromCounter(Quest.QuestType questType)
    {
        switch (questType)
        {
            case Quest.QuestType.Build:
                buildQuestCounter--;
                break;
            case Quest.QuestType.Tend:
                tendQuestCounter--;
                break;
            case Quest.QuestType.Collect:
                collectQuestCounter--;
                break;
            case Quest.QuestType.Cook:
                cookQuestCounter--;
                break;
            case Quest.QuestType.Walk:
                walkQuestCounter--;
                break;
        }
    }

    public void UpdateQuestIcons()
    {
        if (questBoardUI == null)
        {
            questBoardUI = GameObject.Find("Quest Board UI");
        }

        questSlots = questBoardUI.transform.GetChild(0).Find("Quest Slots").gameObject;

        quests = new List<GameObject>();
        for (int i = 0; i < questSlots.transform.childCount; i++)
        {
            quests.Add(questSlots.transform.GetChild(i).gameObject);
        }

        questPapersObj = this.transform.GetChild(0).Find("Quest Papers").gameObject;

        questPapers = new List<GameObject>();
        for (int i = 0; i < questPapersObj.transform.childCount; i++)
        {
            questPapers.Add(questPapersObj.transform.GetChild(i).gameObject);
        }

        for (int i = 0; i < quests.Count; i++)
        {
            Quest currentQuest = quests[i].GetComponent<Quest>();

            // Skip an iteration if the ID is none.
            if (currentQuest.GetQuestType() == Quest.QuestType.None) { continue; }

            // Update Quest Board Paper to match the quest. 
            questPapers[i].GetComponentInChildren<Image>().sprite = GetQuestTypeIcon(currentQuest.GetQuestType());
            questPapers[i].SetActive(true);

            // Update Quest UI icons.
            quests[i].GetComponent<Image>().sprite = GetQuestTypeIcon(currentQuest.GetQuestType());

            currentQuest.HandleTaskCompletion();
        }
    }

    public void GiveQuestReward()
    {
        int randomIndex = Random.Range(0, questRewards.Count);
        ItemData randomItem = questRewards[randomIndex];
        
        // Add 10 of a random seed.
        inventoryManager.AddItemToInventory(randomItem, 10);

        if (AccountLevel.Instance != null) { AccountLevel.Instance.AddXP((int)XPValues.QUEST); }

        // Hide the Quest UI.
        questUiBG.SetActive(false);

        int questIndexToStartFrom = 0;
        GameObject tempQuest = null;
        for (int i = 0; i < quests.Count; i++)
        {
            Quest currentQuest = quests[i].GetComponent<Quest>();
            if (currentQuest.GetIsQuestCompleted() && currentQuest.GetIsQuestOpen())
            {
                // Index is used to update the positions of the quest boards that are after the expired quest.
                questIndexToStartFrom = i;

                // Minus from the corresponding counter
                MinusFromCounter(currentQuest.GetQuestType());

                // Reset Quest
                currentQuest.SetIsQuestOpen(false);
                currentQuest.UpdateQuestType(Quest.QuestType.None);
                currentQuest.SetIsTimerRunning(false);
                currentQuest.SetCurrentTaskAmount(0);
                currentQuest.SetRequiredTaskAmount(0);
                currentQuest.SetUID(string.Empty);

                // Reset sprite for quests slots UI. 
                questSlots.transform.GetChild(i).GetComponent<Image>().sprite = emptyQuestIcon;
                questSlots.transform.GetChild(i).gameObject.SetActive(false);

                // Set as last sibling to update quest slots UI. 
                questSlots.transform.GetChild(i).SetAsLastSibling();

                // Hide object from Quest Board. 
                questPapers[questPapers.Count - 1].SetActive(false);
            }
        }


        // Update positions of the quests on Quest Board. 
        for (int i = questIndexToStartFrom; i < questPapers.Count; i++)
        {
            if (i + 1 < quests.Count)
            {
                questPapers[i].GetComponentInChildren<Image>().sprite = quests[i + 1].GetComponent<Image>().sprite;
            }
        }

        // Hide empty quests on the Quest Board. 
        for (int i = 0; i < questPapers.Count; i++)
        {
            // Continue through the loop until the sprite matches the emptyQuestIcon sprite.
            if (questPapers[i].GetComponentInChildren<Image>().sprite != emptyQuestIcon) { continue; }

            // Hide quest paper.
            questPapers[i].SetActive(false);
        }

        // The expired quest we want to remove and re-add.
        tempQuest = questSlots.transform.GetChild(questPapers.Count - 1).gameObject;

        // Remove and then re-add the quest. 
        quests.Remove(tempQuest);
        quests.Add(tempQuest);

        // Check for task completion upon updating the positions of the quest papers. 
        // This will update their icons if they had been completed prior to the re-positioning. 
        for (int i = 0; i < questPapers.Count; i++)
        {
            if (quests[i].GetComponent<Quest>().GetCurrentTaskAmount() == quests[i].GetComponent<Quest>().GetRequiredTaskAmount())
            {
                quests[i].GetComponent<Quest>().HandleTaskCompletion();
            }
        }
    }

    #region Getters

    /// <summary>
    /// Returns the sprite for the corresponding quest type ID.
    /// </summary>
    /// <param name="id">the ID of the quest type.</param>
    /// <returns>a sprite corresponding to the quest type.</returns>
    public Sprite GetQuestTypeIcon(Quest.QuestType questType)
    {
        switch (questType)
        {
            case Quest.QuestType.Build:
                return buildQuestIcon;
            case Quest.QuestType.Tend:
                return tendQuestIcon;
            case Quest.QuestType.Collect:
                return collectQuestIcon;
            case Quest.QuestType.Cook:
                return cookQuestIcon;
            case Quest.QuestType.Walk:
                return walkQuestIcon;
            default:
                return null;
        }
    }

    /// <summary>
    /// Gets the list of quests from the UI. 
    /// </summary>
    /// <returns>a list of gameObjects (the quests from the UI).</returns>
    public List<GameObject> GetQuests()
    {
        GetReferences();

        quests.Clear();
        
        for (int i = 0; i < questSlots.transform.childCount; i++)
        {
            quests.Add(questSlots.transform.GetChild(i).gameObject);
        }

        return quests;
    }


    /// <summary>
    /// Gets the list of quest papers on the quest board.
    /// </summary>
    /// <returns>A list of gameObjects (the quest papers on quest board).</returns>
    public List<GameObject> GetQuestPapers() => questPapers;

    /// <summary>
    /// Gets the Quest Slots gameObject that contains the quests UI. 
    /// </summary>
    /// <returns>A gameObject that has quests as its children.</returns>
    public GameObject GetQuestSlots() => questSlots;

    public int GetWalkLevel() => walkLevel;

    public GameObject GetQuestBoardBG() => questBoardBG;

    #endregion
}
