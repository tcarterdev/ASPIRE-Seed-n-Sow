using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public struct PlayerData
{
    [Tooltip("The player's walk level. Changes the distance they are asked to walk in the quests.")]
    public int walkLevel;

    public int accountLevel;
    public int currentExperience;

    // Rewards
    public float slowerHungerDepletion;

    // The three booleans that check if the groups of three quests are given based on real-world time.
    public bool timeQuestOne;
    public bool timeQuestTwo;
    public bool timeQuestThree;

    public string oldDate;
    public string todayDate;

    public float distanceTravelledInMeters;
}
