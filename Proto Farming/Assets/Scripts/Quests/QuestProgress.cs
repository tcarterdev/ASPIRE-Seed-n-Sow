using System;
using UnityEngine;

public class QuestProgress : MonoBehaviour
{
    public static QuestProgress Instance { get; private set; }

    public event EventHandler OnEndOfSession;

    // Quest Events
    public event EventHandler OnBuildingPlaced;
    public event EventHandler OnCropTendered;
    public event EventHandler OnItemCollected;
    public event EventHandler OnItemCooked;
    public event EventHandler<float> OnDistanceTravelled;

    private void Awake()
    {
        if (Instance != null)
        {
            Debug.LogError($"There's more than one QuestProgress! {transform} - {Instance}");
            Destroy(gameObject);
        }

        Instance = this;
    }

    private void Update()
    {
        if (SceneHandler.Instance.GetActiveSceneIndex() == (int)SceneIndexes.VENTURE_MODE && Input.GetKeyDown(KeyCode.H)) { InvokeItemCollected(); }
        
        if (Input.GetKeyDown(KeyCode.K))
        {
            // AddLine for Quests.
            InvokeEndOfSession();

            DataGathering.dataGathering.SaveToFile(10);
            GameObject endOfSession = GameObject.Find("EndOfSessionCanvas").transform.GetChild(0).gameObject;
            endOfSession.SetActive(true);
            endOfSession.GetComponent<EndOfSession>().UpdateEndOfSession();
        }
        //if (SceneHandler.Instance.GetActiveSceneIndex() == (int)SceneIndexes.VENTURE_MODE && Input.GetKeyDown(KeyCode.J)) { InvokeDistanceTravelled(); }
    }

    /// <summary>
    /// Invokes the OnBuildingPlaced event. 
    /// 
    /// This should be called each time a building is placed in Farming Mode. 
    /// </summary>
    public void InvokeBuildingPlaced() => OnBuildingPlaced?.Invoke(this, null);

    /// <summary>
    /// Invokes the OnCropTendered event.
    /// 
    /// This should be called each time a crop has been tendered n Farming Mode. 
    /// </summary>
    public void InvokeCropTendered() => OnCropTendered?.Invoke(this, null);

    /// <summary>
    /// Invokes the OnItemCollected event. 
    /// 
    /// This should be called each time an item has been collected in Farming Mode or Venture Mode.
    /// </summary>
    public void InvokeItemCollected() => OnItemCollected?.Invoke(this, null);

    /// <summary>
    /// Invokes the OnItemCooked event.
    /// 
    /// This should be called each time an item has been cooked in Farming Mode.
    /// </summary>
    public void InvokeItemCooked() => OnItemCooked?.Invoke(this, null);

    /// <summary>
    /// Invokes the Distance Travelled event.
    /// 
    /// This should be called each time the player moves in Venture Mode.
    /// </summary>
    public void InvokeDistanceTravelled(float distanceTravelled) => OnDistanceTravelled?.Invoke(this, distanceTravelled);

    public void InvokeEndOfSession() => OnEndOfSession?.Invoke(this, null);
}
