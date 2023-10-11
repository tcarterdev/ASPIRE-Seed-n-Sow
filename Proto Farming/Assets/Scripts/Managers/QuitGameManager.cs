using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class QuitGameManager : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Application.platform == RuntimePlatform.Android)
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                Debug.Log("We are on Android, and Escape has been pressed");
                QuestSaveManager.Instance.SetIsApplicationClosing(true);
                SaveGameManager.Instance.SaveGame();

                QuestProgress.Instance.InvokeEndOfSession();

                if (SceneHandler.Instance.GetActiveSceneIndex() == (int)SceneIndexes.VENTURE_MODE)
                {
                    DistanceTravel distanceTravel = FindObjectOfType<DistanceTravel>();
                    DataGathering.dataGathering.SaveToFile(distanceTravel.GetDistanceTravelled());
                }
                else
                {
                    PlayerManager playerManager = FindObjectOfType<PlayerManager>();

                    DataGathering.dataGathering.SaveToFile(playerManager.GetDistanceTravelled());
                }
                GameObject endOfSession = GameObject.Find("EndOfSessionCanvas").transform.GetChild(0).gameObject;
                endOfSession.SetActive(true);
                endOfSession.GetComponent<EndOfSession>().UpdateEndOfSession();
            }
        }
        /// Used for debugging on PC.
        else
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                Debug.Log("We aren't on Android, and Escape has been pressed");
                QuestSaveManager.Instance.SetIsApplicationClosing(true);
                SaveGameManager.Instance.SaveGame();

                QuestProgress.Instance.InvokeEndOfSession();

                if (SceneHandler.Instance.GetActiveSceneIndex() == (int)SceneIndexes.VENTURE_MODE)
                {
                    DistanceTravel distanceTravel = FindObjectOfType<DistanceTravel>();
                    DataGathering.dataGathering.SaveToFile(distanceTravel.GetDistanceTravelled());
                }
                else
                {
                    PlayerManager playerManager = FindObjectOfType<PlayerManager>();

                    DataGathering.dataGathering.SaveToFile(playerManager.GetDistanceTravelled());
                }

                GameObject endOfSession = GameObject.Find("EndOfSessionCanvas").transform.GetChild(0).gameObject;
                endOfSession.SetActive(true);
                endOfSession.GetComponent<EndOfSession>().UpdateEndOfSession();
            }
        }
    }
}
