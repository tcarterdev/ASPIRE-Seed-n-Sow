using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class QuestReward : MonoBehaviour
{
    private GameObject questUiBG;
    private GameObject questBoardUI;
    private GameObject questSlots;

    private SaveQuests saveQuests;

    [Header("Quest Rewards")]
    [SerializeField] private List<ItemData> questRewards;

    private InventoryManager inventoryManager;

    // Start is called before the first frame update
    void Start()
    {
        questUiBG = GameObject.Find("Quest UI").transform.Find("QuestUIBG").gameObject;
        questBoardUI = GameObject.Find("Quest Board UI");
        questSlots = questBoardUI.transform.GetChild(0).Find("Quest Slots").gameObject;

        saveQuests = questBoardUI.GetComponent<SaveQuests>();

        inventoryManager = FindObjectOfType<InventoryManager>();
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

        List<GameObject> quests = saveQuests.GetQuests();

        int questIndexToStartFrom = 0;
        GameObject tempQuest = null;
        for (int i = 0; i < questSlots.transform.childCount; i++)
        {
            Quest currentQuest = quests[i].GetComponent<Quest>();
            if (currentQuest.GetIsQuestCompleted() && currentQuest.GetIsQuestOpen())
            {
                // Index is used to update the positions of the quest boards that are after the expired quest.
                questIndexToStartFrom = i;

                Debug.Log($"Quest Collected from {i}");

                // TODO: Implement for Quest Board in Farming Mode.
                //// Minus from the corresponding counter
                //MinusFromCounter(currentQuest.GetQuestType());

                // Reset Quest
                currentQuest.SetIsQuestOpen(false);
                currentQuest.UpdateQuestType(Quest.QuestType.None);
                currentQuest.SetIsTimerRunning(false);
                currentQuest.SetCurrentTaskAmount(0);
                currentQuest.SetRequiredTaskAmount(0);
                currentQuest.SetTimerAmount(0);
                //currentQuest.SetUID(string.Empty);

                // Reset sprite for quests slots UI. 
                questSlots.transform.GetChild(i).GetComponent<Image>().sprite = currentQuest.GetEmptyQuestIcon();
                questSlots.transform.GetChild(i).gameObject.SetActive(false);

                // Set as last sibling to update quest slots UI. 
                questSlots.transform.GetChild(i).SetAsLastSibling();
            }
        }

        // The expired quest we want to remove and re-add.
        tempQuest = questSlots.transform.GetChild(quests.Count - 1).gameObject;

        // Remove and then re-add the quest. 
        quests.Remove(tempQuest);
        quests.Add(tempQuest);


        //// Update positions of the quests on Quest Board. 
        //for (int i = questIndexToStartFrom; i < questPapers.Count; i++)
        //{
        //    if (i + 1 < quests.Count)
        //    {
        //        questPapers[i].GetComponentInChildren<Image>().sprite = quests[i + 1].GetComponent<Image>().sprite;
        //    }
        //}

        //// Hide empty quests on the Quest Board. 
        //for (int i = 0; i < questPapers.Count; i++)
        //{
        //    // Continue through the loop until the sprite matches the emptyQuestIcon sprite.
        //    if (questPapers[i].GetComponentInChildren<Image>().sprite != emptyQuestIcon) { continue; }

        //    // Hide quest paper.
        //    questPapers[i].SetActive(false);
        //}

        //// The expired quest we want to remove and re-add.
        //tempQuest = questSlots.transform.GetChild(questPapers.Count - 1).gameObject;

        //// Remove and then re-add the quest. 
        //quests.Remove(tempQuest);
        //quests.Add(tempQuest);

        //// Check for task completion upon updating the positions of the quest papers. 
        //// This will update their icons if they had been completed prior to the re-positioning. 
        //for (int i = 0; i < questPapers.Count; i++)
        //{
        //    if (quests[i].GetComponent<Quest>().GetCurrentTaskAmount() == quests[i].GetComponent<Quest>().GetRequiredTaskAmount())
        //    {
        //        quests[i].GetComponent<Quest>().HandleTaskCompletion();
        //    }
        //}
    }
}

