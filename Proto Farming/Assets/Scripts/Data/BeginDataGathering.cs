using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Firebase.Auth;

public class BeginDataGathering : MonoBehaviour
{
    public int questID;

    private void Start()
    {
        Debug.Log("Begin Data Gathering");
        DataGathering.dataGathering.CompileBeginingOfSession("id", "UK");    // TODO: Add location to where UK is

        questID = DataGathering.dataGathering.ReturnUniqueQuestID();    // TODO: Generate when quests are in Venture Mode.
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.U))
        {
            DataGathering.dataGathering.AddLine("Quest Started", questID, Quest.QuestType.Walk.ToString(), 72, 0, false);
        }

        if (Input.GetKeyDown(KeyCode.I))
        {
            DataGathering.dataGathering.AddLine("Quest Finished", questID, Quest.QuestType.Walk.ToString(), 72, 72, true);
        }

        //if (Input.GetKeyDown(KeyCode.O))
        //{
        //    DataGathering.dataGathering.SaveToFile();
        //}
    }
}
