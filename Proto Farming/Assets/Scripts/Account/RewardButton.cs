using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class RewardButton : MonoBehaviour
{
    [SerializeField] private Reward reward;
    [SerializeField] private TextMeshProUGUI rewardText;
    [SerializeField] private Button rewardButton;

    [SerializeField] private GameObject collectedObj;

    private PlayerManager playerManager;
    private InventoryManager inventoryManager;
    private InventoryItems inventoryItems;

    private void Start()
    {
        playerManager = FindObjectOfType<PlayerManager>();
        inventoryManager = FindObjectOfType<InventoryManager>();
        inventoryItems = FindObjectOfType<InventoryItems>();

        if (reward == null)
        {
            rewardText.text = "";
            return;
        }

        string tempText = "";
        switch (reward.rewardType)
        {
            case Reward.RewardType.Item:
                tempText = "Random Item";
                break;

            case Reward.RewardType.Tool:
                tempText = "Random Tool";
                break;

            case Reward.RewardType.Passive:
                // Increase slower hunger depletion rate. 
                tempText = "Slower Hunger Depletion Rate";
                break;
            default:
                tempText = "";
                break;
        }

        // Update reward text.
        rewardText.text = tempText;
    }


    public void GiveReward()
    {
        if (reward.rewardType == Reward.RewardType.None)
        {
            // Set reward has been collected.
            AccountLevel.Instance.GetAccountRewardList()[0] = true;

            DisableButton();
            return;
        }

        switch (reward.rewardType)
        {
            case Reward.RewardType.Item:
                inventoryManager.AddItemToInventory(inventoryItems.GetItem(reward.itemRewards[0].itemName, reward.itemRewards[0].itemCategory.ToString()), 1);
                break;

            case Reward.RewardType.Tool:
                inventoryManager.AddItemToInventory(inventoryItems.GetTool(reward.toolRewards[0].itemName), 1);
                break;

            case Reward.RewardType.Passive:
                // Increase slower hunger depletion rate. 
                playerManager.SetSlowerHungerDepletion(reward.slowerHungerDepletion);
                break;
        }

        // Set reward has been collected.
        AccountLevel.Instance.GetAccountRewardList()[reward.levelToReceive] = true;

        DisableButton();
    }

    private void DisableButton()
    {
        collectedObj.SetActive(true);
        rewardButton.interactable = false;
    }
}
