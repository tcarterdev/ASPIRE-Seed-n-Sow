using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AccountLevel : MonoBehaviour
{
    public static AccountLevel Instance { get; private set; }

    public event EventHandler<string> OnAccountRewardSaved;

    [SerializeField] private int maxAccountLevel = 25;
    [SerializeField] private int experienceRequired;

    [SerializeField] private int currentAccountLevel;
    private int currentExperience;
    private int experienceToAdd;

    private AccountLevelUI accountLevelUI;
    private PlayerManager playerManager;

    private float slowerHungerDepletion;

    [SerializeField] private List<bool> accountRewardClaimedList;
    [SerializeField] private string fileName;

    private void Awake()
    {
        if (Instance != null)
        {
            Debug.LogError($"There's more than one AccountLevel! {transform} - {Instance}");
            Destroy(gameObject);
        }

        Instance = this;
    }

    // Start is called before the first frame update
    void Start()
    {
        accountLevelUI = FindObjectOfType<AccountLevelUI>();
        playerManager = FindObjectOfType<PlayerManager>();

        PlayerSaveManager.Instance.OnPlayerLoaded += PlayerSaveManager_OnPlayerLoaded;

        if (SaveGameManager.Instance != null)
        {
            SaveGameManager.Instance.OnSaveGame += SaveGameManager_OnSaveGame;
        }

        InitialiseList();
    }

    public void InitialiseList()
    {
        accountRewardClaimedList = new List<bool>();

        for (int i = 0; i < maxAccountLevel + 1; i++)
        {
            accountRewardClaimedList.Add(false);
        }

        accountRewardClaimedList[0] = true;
    }

    private void PlayerSaveManager_OnPlayerLoaded(object sender, EventArgs e)
    {
        Debug.Log($"Player has been loaded, EXP is: {playerManager.GetExperience()}");

        accountLevelUI = FindObjectOfType<AccountLevelUI>();

        // Update account level and experience.
        accountLevelUI.UpdateUI(playerManager.GetAccountLevel(), playerManager.GetExperience());

        // Updated unlocked levels.
        accountLevelUI.UnlockLevel(playerManager.GetAccountLevel());

        // Set our current experience to our loaded experience.
        currentExperience = playerManager.GetExperience();

        // Set our current level to our loaded level.
        currentAccountLevel = playerManager.GetAccountLevel();

        slowerHungerDepletion = playerManager.GetPlayerData().slowerHungerDepletion;

        //if (GameObject.Find("LoadingScreen"))
        //{
        //    LoadingScreen.Instance.EnableLoadingScreen(false, 0.5f);
        //}
    }

    private void SaveGameManager_OnSaveGame(object sender, EventArgs e)
    {
        playerManager.SetAccountLevel(currentAccountLevel);
        playerManager.SetCurrentExperience(currentExperience);

        // Save account reward claimed data separately.
        // TODO: invoke save event.
        FBFileHandler.SaveToJSON<bool>(accountRewardClaimedList, fileName);
        OnAccountRewardSaved?.Invoke(this, JsonHelper.ToJson<bool>(accountRewardClaimedList.ToArray()));
    }

    // Update is called once per frame
    void Update()
    {
#if UNITY_EDITOR

        if (Input.GetKeyDown(KeyCode.Alpha8))
        {
            // Give XP
            AddXP(1000);
        }

#endif
    }

    public void AddXP(int xpToAdd)
    {
        currentExperience += xpToAdd;

        if (currentExperience == experienceRequired)
        {
            // Level up.
            currentAccountLevel++;

            // Reset current exp, as there is none left over. 
            currentExperience = 0;
        }
        else if (currentExperience > experienceRequired)
        {
            // Level up.
            currentAccountLevel++;

            // Calculate left over. 
            int leftOverXP = currentExperience - experienceRequired;

            // Set current experience back to zero, then add the left over XP.
            currentExperience = 0 + leftOverXP;
        }

        // Return early if we are at max level. 
        if (currentAccountLevel > maxAccountLevel) { return; }

        // Update the player data's account level.
        playerManager.SetAccountLevel(currentAccountLevel);

        accountLevelUI.UnlockLevel(currentAccountLevel);
        accountLevelUI.UpdateUI(currentAccountLevel, currentExperience);
    }

    public int GetAccountLevel() => currentAccountLevel;
    public int GetMaxAccountLevel() => maxAccountLevel;
    public int GetExperienceRequired() => experienceRequired;
    public int GetCurrentExperience() => currentExperience;

    public List<bool> GetAccountRewardList() => accountRewardClaimedList;
}
