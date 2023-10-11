using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class EndOfSession : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI timeFinishedText;

    [SerializeField] private TextMeshProUGUI totalDistanceTravelledText;
    [SerializeField] private TextMeshProUGUI totalItemsCollectedText;
    [SerializeField] private TextMeshProUGUI totalQuestsProgressedText;
    [SerializeField] private TextMeshProUGUI totalQuestsCompletedText;

    public void Start()
    {
        GetComponentInChildren<Button>().onClick.AddListener(() =>
        {
            if (Application.platform == RuntimePlatform.Android)
            {
                // Quit Game
                Application.Quit();
            }
            else
            {
                Debug.Log("Application Quit");
            }
        });
    }

    public void UpdateEndOfSession()
    {
        timeFinishedText.text = $"Time Finished: {DataGathering.dataGathering.dataJson.EndOfSessionReport.TimeFinished}";

        // Total distance travelled.
        totalDistanceTravelledText.text = DataGathering.dataGathering.dataJson.EndOfSessionReport.TotalDistanceTravelled.ToString();

        // Total number of items collected.
        totalItemsCollectedText.text = DataGathering.dataGathering.dataJson.EndOfSessionReport.TotalCollected.ToString();

        // Adds the total quests progressed on from walking and collecting together.
        totalQuestsProgressedText.text = 
            DataGathering.dataGathering.dataJson.EndOfSessionReport.WalkingQuests_Stats.TotalProgressedOn + 
            DataGathering.dataGathering.dataJson.EndOfSessionReport.CollectingQuests_Stats.TotalProgressedOn.ToString();

        // Adds the total quests completed on from walking and collecting together.
        totalQuestsCompletedText.text = 
            DataGathering.dataGathering.dataJson.EndOfSessionReport.WalkingQuests_Stats.TotalCompleted +
            DataGathering.dataGathering.dataJson.EndOfSessionReport.CollectingQuests_Stats.TotalCompleted.ToString();
    }
}
