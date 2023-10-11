using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SaveQuests : MonoBehaviour
{
    public event System.EventHandler<string> OnQuestSaved;

    [Tooltip("The quests in the UI")]
    [SerializeField] private List<GameObject> quests;
    private Transform questSlots;
    private GameObject questUiBG;

    [Header("Quest Data")]
    [SerializeField] private List<QuestData> questDataList;

    [SerializeField] private string fileName = "quests.json";

    private bool canUpdateTimers;

    // Start is called before the first frame update
    void Start()
    {
        canUpdateTimers = false;

        SaveGameManager.Instance.OnSaveGame += SaveGame_OnSaveGame;
        SaveGameManager.Instance.OnLoadGame += SaveGame_OnLoadGame;

        if (SceneHandler.Instance.GetActiveSceneIndex() == (int)SceneIndexes.VENTURE_MODE)
        {
            questSlots = transform.Find("QuestBoardBG").Find("Quest Slots");
            questUiBG = transform.Find("Quest UI").Find("QuestUIBG").gameObject;

            for (int i = 0; i < questSlots.childCount; i++)
            {
                quests.Add(questSlots.GetChild(i).gameObject);
            }
        }
    }

    private void Update()
    {
        if (canUpdateTimers)
        {
            UpdateQuestTimers();
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
            if (quests[i].GetComponent<Quest>().questTimer > 0 && !quests[i].GetComponent<Quest>().GetIsQuestCompleted() && quests[i].GetComponent<Quest>().GetQuestType() != 0)
            {
                // Only count the timer down if the quest is not completed, and the quest ID is not equal to 0.
                quests[i].GetComponent<Quest>().questTimer -= Time.deltaTime;
            }
            else if (quests[i].GetComponent<Quest>().questTimer <= 0 && quests[i].GetComponent<Quest>().GetQuestType() != 0)
            {
                quests[i].GetComponent<Quest>().HandleQuestTimer();
            }
        }
    }

    private void SaveGame_OnSaveGame(object sender, System.EventArgs e)
    {
        if (SceneHandler.Instance.GetActiveSceneIndex() == (int)SceneIndexes.VENTURE_MODE)
        {
            OnQuestSaved?.Invoke(this, null);
            return;
        }

        GetComponent<QuestBoard>().SaveQuestBoard();
    }

    private void SaveGame_OnLoadGame(object sender, System.EventArgs e)
    {
        //if (SceneHandler.Instance.GetActiveSceneIndex() == (int)SceneIndexes.VENTURE_MODE)
        //{
        //    HandleVentureModeLoad();
        //    return;
        //}

        //GetComponent<QuestBoard>().LoadQuestBoard();
    }

    public void GiveQuestReward()
    {
        Debug.Log("Player received reward (to be implemented)");
        //questUiBG.SetActive(false);

        int questIndexToStartFrom = 0;
        GameObject tempQuest = null;
        for (int i = 0; i < quests.Count; i++)
        {
            Debug.Log("For loop");
            if (quests[i].GetComponent<Quest>().GetIsQuestCompleted())
            {
                // Index is used to update the positions of the quest boards that are after the expired quest.
                questIndexToStartFrom = i;

                // Minus from the corresponding counter
                //MinusFromCounter(quests[i].GetComponent<Quest>().GetQuestID());

                // Set quest ID to zero.
                quests[i].GetComponent<Quest>().UpdateQuestType(0);
                quests[i].GetComponent<Quest>().SetIsTimerRunning(false);

                // Reset sprite for quests slots UI. 
                questSlots.transform.GetChild(i).GetComponent<Image>().sprite = quests[i].GetComponent<Quest>().GetEmptyQuestIcon();
                questSlots.transform.GetChild(i).gameObject.SetActive(false);

                // Set as last sibling to update quest slots UI. 
                questSlots.transform.GetChild(i).SetAsLastSibling();

                // Hide object from Quest Board. 
                //questPapers[questPapers.Count - 1].SetActive(false);
            }
        }

        // The expired quest we want to remove and re-add.
        tempQuest = questSlots.transform.GetChild(quests.Count - 1).gameObject;

        Debug.Log("About to clear quests");
        quests.Clear();
        Debug.Log("Quests cleared");

        //for (int i = 0; i < questSlots.childCount; i++)
        //{
        //    quests.Add(questSlots.GetChild(i).gameObject);
        //}

        //// Remove and then re-add the quest. 
        //quests.Remove(tempQuest);
        //quests.Add(tempQuest);

        // Check for task completion upon updating the positions of the quest papers. 
        // This will update their icons if they had been completed prior to the re-positioning. 
        for (int i = 0; i < quests.Count; i++)
        {
            if (quests[i].GetComponent<Quest>().GetCurrentTaskAmount() == quests[i].GetComponent<Quest>().GetRequiredTaskAmount())
            {
                quests[i].GetComponent<Quest>().HandleTaskCompletion();
            }
        }
    }

    #region Venture Mode Saving & Loading

    #endregion

    #region Getters & Setters
    public List<GameObject> GetQuests() => quests;

    public void SetCanUpdateTimers(bool state) => canUpdateTimers = state;

    #endregion

    private void OnDisable()
    {
        SaveGameManager.Instance.OnSaveGame -= SaveGame_OnSaveGame;
        SaveGameManager.Instance.OnLoadGame -= SaveGame_OnLoadGame;
    }
}
