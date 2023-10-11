using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlotManager : MonoBehaviour
{
    public static PlotManager Instance { get; private set; }

    public event EventHandler<string> OnPlotSaved;

    [SerializeField] private List<FarmPlot> plots;
    [SerializeField] private List<PlotData> plotDataList;

    [SerializeField] private string fileName;

    private void Awake()
    {
        if (Instance != null)
        {
            Debug.LogError($"There's more than one scr_plot_manager! {transform} - {Instance}");
            Destroy(gameObject);
        }

        Instance = this;
    }

    // Start is called before the first frame update
    void Start()
    {
        SaveGameManager.Instance.OnSaveGame += SaveGame_OnSaveGame;   
    }   

    private void SaveGame_OnSaveGame(object sender, EventArgs e)
    {
        if (SceneHandler.Instance.GetActiveSceneIndex() == (int)SceneIndexes.FARM_MODE)
        {
            // Go through each plot and save their data to the plotData.
            foreach (var plot in plots)
            {
                plot.SaveData();
            }

            // Retrieve the plotData from the plots 
            for (int i = 0; i < plotDataList.Count; i++)
            {
                plotDataList[i] = plots[i].GetPlotData();
            }

            FBFileHandler.SaveToJSON<PlotData>(plotDataList, fileName);
            OnPlotSaved?.Invoke(this, JsonHelper.ToJson<PlotData>(plotDataList.ToArray()));
        }
    }

    public List<FarmPlot> GetPlots() => plots;
    public List<PlotData> GetPlotDataList() => plotDataList;
}
