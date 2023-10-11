using Firebase.Auth;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
//using static UnityEditor.VersionControl.Asset;

public class PlayerManager : MonoBehaviour
{
    public event EventHandler<string> OnPlayerDataSaved;

    [SerializeField] private PlayerData playerData = new PlayerData();
    [SerializeField] private string fileName;

    private void Start()
    {
        SetDate();

        if (SaveGameManager.Instance != null)
        {
            SaveGameManager.Instance.OnSaveGame += SaveGameManager_OnSaveGame;
        }
    }

    private void SaveGameManager_OnSaveGame(object sender, EventArgs e)
    {
        OnPlayerDataSaved?.Invoke(this, JsonUtility.ToJson(playerData));
    }

    #region Getters & Setters

    public PlayerData GetPlayerData() => playerData;

    /// <summary>
    /// Gets if the quests have been given based on the real-world time.
    /// </summary>
    /// <param name="number">the number for which quests have been given.</param>
    /// <returns>a boolean if the quest has been given or not.</returns>
    public bool GetTimeQuestGiven(int number)
    {
        bool result = true;

        // Returns the corresponding quest group.
        switch (number)
        {
            case 1:
                result = playerData.timeQuestOne;
                break;

            case 2:
                result = playerData.timeQuestTwo;
                break;

            case 3:
                result = playerData.timeQuestThree;
                break;
        }

        return result;
    }

    /// <summary>
    /// Sets whether the quests for the real-world time has been given.
    /// </summary>
    /// <param name="number">the number for which quests have been given.</param>
    /// <param name="state">true or false if a quest has been given or not.</param>
    public void SetTimeQuestGiven(int number, bool state)
    {
        // Update the corresponding quest group. 
        switch (number)
        {
            case 1:
                playerData.timeQuestOne = state;
                break;

            case 2:
                playerData.timeQuestTwo = state;
                break;

            case 3:
                playerData.timeQuestThree = state;
                break;
        }
    }

    /// <summary>
    /// Sets the player data. 
    /// </summary>
    /// <param name="walkLevel">The walk level of the player.</param>
    public void SetPlayerData(int walkLevel, int accountLevel, int currentExperience, bool timeQuestOneGiven, bool timeQuestTwoGiven, bool timeQuestThreeGiven, string oldDate, float distanceTravelledInMeters)
    {
        if (walkLevel != 0) { playerData.walkLevel = walkLevel; }

        playerData.accountLevel = accountLevel;
        playerData.currentExperience += currentExperience;

        playerData.timeQuestOne = timeQuestOneGiven;
        playerData.timeQuestTwo = timeQuestTwoGiven;
        playerData.timeQuestThree = timeQuestThreeGiven;

        if (oldDate != string.Empty)
        {
            playerData.oldDate = oldDate;
        }
        else
        {
            playerData.oldDate = DateTime.Today.ToString();
        }

        playerData.distanceTravelledInMeters = distanceTravelledInMeters;

        // Check if it is a new day
        CheckIfNewDay();
    }

    private void CheckIfNewDay()
    {
        Debug.Log($"OldDate: {playerData.oldDate}");

        // Reset booleans if it is a new day. 
        if (DateTime.Today > DateTime.Parse(playerData.oldDate))
        {
            SetTimeQuestGiven(1, false);
            SetTimeQuestGiven(2, false);
            SetTimeQuestGiven(3, false);

            // Set today's date as the old date. 
            playerData.oldDate = DateTime.Today.ToString();
        }
    }

    public void SetDate()
    {
        // Set today's date. 
        playerData.todayDate = DateTime.Today.ToString();
    }

    public void SetAccountLevel(int accountLevel) => playerData.accountLevel = accountLevel;

    public void SetCurrentExperience(int currentExperience) => playerData.currentExperience = currentExperience;

    public void SetSlowerHungerDepletion(float slowerHungerDepletion) => playerData.slowerHungerDepletion += slowerHungerDepletion;

    public int GetAccountLevel() => playerData.accountLevel;
    public int GetExperience() => playerData.currentExperience;

    public float GetDistanceTravelled() => playerData.distanceTravelledInMeters;

    #endregion
}
