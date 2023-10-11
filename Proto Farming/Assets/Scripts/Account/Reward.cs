using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Level Reward", menuName = "Account/Level Reward")]
public class Reward : ScriptableObject
{
    public enum RewardType 
    { 
        None,
        Item,           // Seeds and food.
        Tool,           // Tools.
        Passive        // Slower hunger depletion rate.
    }

    [Range(0, 25)]
    public int levelToReceive;

    public RewardType rewardType;

    [Header("Item Rewards")]
    public List<ItemData> itemRewards;

    [Header("Tool Rewards")]
    public List<ToolData> toolRewards;

    [Header("Passive Rewards")]
    public float slowerHungerDepletion;
}
