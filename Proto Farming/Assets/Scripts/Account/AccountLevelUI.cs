using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class AccountLevelUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI accountLevelText;

    [SerializeField] private TextMeshProUGUI accountExpText;
    [SerializeField] private Image accountExpBar;

    private AccountLevel accountLevel;

    private int exp;

    // Start is called before the first frame update
    void Start()
    {
        accountLevel = FindObjectOfType<AccountLevel>();

        InitialiseLevels();

        if (accountLevel.GetAccountLevel() != 0)
        {
            UpdateUI(accountLevel.GetAccountLevel(), accountLevel.GetCurrentExperience());
        }
        else
        {
            accountLevelText.text = $"Level: 0";
            accountExpBar.fillAmount = 0;
        }
    }

    private void InitialiseLevels()
    {
        Transform levelContainer = transform.Find("Account").Find("Levels").Find("LevelAndRewardContainer");

        for (int i = 0; i < levelContainer.childCount; i++)
        {
            levelContainer.GetChild(i).Find("Level").Find("LevelText").GetComponent<TextMeshProUGUI>().text = i.ToString();
        }

        // "Collect" level 0, as there is nothing.
        levelContainer.GetChild(0).Find("Collected").gameObject.SetActive(true);

        // Disable the reward button for level 0.
        levelContainer.GetChild(0).Find("Reward").GetChild(0).GetComponent<Button>().interactable = false;
    }

    public void UpdateUI(int currentLevel, int currentExperience)
    {
        if (currentLevel != AccountLevel.Instance.GetMaxAccountLevel())
        {
            accountLevelText.text = $"Level: {currentLevel}";
        }
        else
        {
            accountLevelText.text = $"Level: {currentLevel} (Max Level)";
        }

        if (accountExpText == null) { accountExpText = transform.Find("Account").Find("Levels").Find("XP Bar").Find("XP Bar Fill").GetComponent<TextMeshProUGUI>(); }
        if (accountExpBar == null) { accountExpBar = transform.Find("Account").Find("Levels").Find("XP Bar").Find("XP Bar Fill").GetComponent<Image>(); }

        accountExpText.text = $"{currentExperience}/{AccountLevel.Instance.GetExperienceRequired()}";
        accountExpBar.fillAmount = currentExperience / 5000.0f;
    }

    public void UnlockLevel(int levelToUnlock)
    {
        Transform levelContainer = transform.Find("Account").Find("Levels").Find("LevelAndRewardContainer");

        // Loop through and unlock ones prior to the desired unlock level.
        for (int i = 0; i < levelToUnlock; i++)
        {
            // Check to see if the reward has been collected or not.
            if (accountLevel.GetAccountRewardList()[i])
            {
                // Show the collected UI element.
                levelContainer.GetChild(i).Find("Collected").gameObject.SetActive(true);

                // Disable the reward button.
                levelContainer.GetChild(i).Find("Reward").Find("Reward Button").GetComponent<Button>().interactable = false;
            }

            // Skip the iteration if the object is already false.
            if (!levelContainer.GetChild(levelToUnlock).Find("Locked").gameObject.activeSelf) { continue; }

            // Hide the locked object.
            levelContainer.GetChild(i).Find("Locked").gameObject.SetActive(false);
        }

        // Return if the reward trying to unlock is greater than the max level.
        if (/*!levelContainer.GetChild(levelToUnlock).Find("Locked").gameObject.activeSelf && */levelToUnlock > accountLevel.GetMaxAccountLevel()) { return; }

        // Hide the locked UI. 
        levelContainer.GetChild(levelToUnlock).Find("Locked").gameObject.SetActive(false);

        // Show the collected UI.
        if (levelToUnlock != 0 && accountLevel.GetAccountRewardList()[levelToUnlock])
        {
            levelContainer.GetChild(levelToUnlock).Find("Collected").gameObject.SetActive(true);
        }
    }
}
