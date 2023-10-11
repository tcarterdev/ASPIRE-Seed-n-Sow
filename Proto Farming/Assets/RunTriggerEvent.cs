using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RunTriggerEvent : MonoBehaviour
{
    public bool testTriggerEvent = false;

    // Start is called before the first frame update
    void Start()
    {
        if (testTriggerEvent)
        {
            DataGathering.dataGathering.AddLine("Walk Quest Started", 123, Quest.QuestType.Walk.ToString(), 500, 0, false);
            DataGathering.dataGathering.AddLine("Collect Quest Started", 456, Quest.QuestType.Collect.ToString(), 50, 0, false);

            Invoke("DelayFakeEnd", 2);
            Invoke("TriggerEvent", 5);
        }
    }

    private void TriggerEvent() => DataGathering.dataGathering.TriggerAllFirebaseEvents();

    private void DelayFakeEnd()
    {
        DataGathering.dataGathering.AddLine("Walk Quest Completed", 123, Quest.QuestType.Walk.ToString(), 500, 500, true);
        DataGathering.dataGathering.AddLine("Collect Quest Completed", 456, Quest.QuestType.Collect.ToString(), 50, 25, false);
    }
}
