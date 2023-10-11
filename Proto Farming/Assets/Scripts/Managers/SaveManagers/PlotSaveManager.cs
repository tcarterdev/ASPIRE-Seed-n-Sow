using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Firebase.Auth;
using Firebase.Database;
using System;
using System.Threading.Tasks;

public class PlotSaveManager : BaseSaveManager
{
    public static PlotSaveManager Instance { get; private set; }

    [SerializeField] private GameObject plotPrefab;

    public event EventHandler OnPlotLoaded;

    private void Awake()
    {
        if (Instance != null)
        {
            Debug.LogError($"There's more than one PlotSaveManager! {transform} - {Instance}");
            Destroy(gameObject);
        }

        Instance = this;
    }

    protected override void Start()
    {
        base.Start();

        // Subscribe to events. 
        //AuthManager.Instance.OnFirebaseSetup += AuthManager_OnFirebaseSetup;
        AuthManager.Instance.OnAuthStateChanged += AuthManager_OnAuthStateChanged;
        PlotManager.Instance.OnPlotSaved += PlotManager_OnPlotSaved;

        if (RewardSaveManager.Instance != null)
        {
            RewardSaveManager.Instance.OnAccountRewardLoaded += RewardSaveManager_OnAccountRewardLoaded;
        }
    }

    /// <summary>
    /// Invoked when the PlotManager has saved all the plots data. 
    /// </summary>
    /// <param name="sender">The object that sent the invoke.</param>
    /// <param name="content">A Json string of all the plots data.</param>
    private void PlotManager_OnPlotSaved(object sender, string content)
    {
        // Save the passed-through contents to the content variable
        //this.content = content;

        SavePlotsToDatabase(content);
    }

    /// <summary>
    /// Invoked from the AuthManager script.
    /// </summary>
    /// <param name="sender">The object that sent the invoke.</param>
    /// <param name="e">Variable pass-through, in this case it will be empty.</param>
    private void AuthManager_OnAuthStateChanged(object sender, EventArgs e)
    {
        //Debug.Log("AuthStateChanged");

        if (SceneHandler.Instance.GetActiveSceneIndex() != (int)SceneIndexes.TITLE_SCREEN)
        {
            // If the authentication state has changed, load the data in case its a new user. 
            //HandlePlotLoad();
        }
    }

    /// <summary>
    /// Invoked from the PlayerSaveManager script.
    /// </summary>
    /// <param name="sender">The object that sent the invoke.</param>
    /// <param name="e">Variable pass-through, in this case it will be empty.</param>
    private void RewardSaveManager_OnAccountRewardLoaded(object sender, EventArgs e)
    {
        if (SceneHandler.Instance.GetActiveSceneIndex() != (int)SceneIndexes.VENTURE_MODE)
        {
            if (GameObject.Find("LoadingScreen"))
            {
                LoadingScreen.Instance.UpdateLoadingInfo("Loading plots");
            }

            StartCoroutine(LoadFromDatabase());
        }
        else if (SceneHandler.Instance.GetActiveSceneIndex() == (int)SceneIndexes.VENTURE_MODE)
        {
            OnPlotLoaded?.Invoke(this, null);
        }
    }

    /// <summary>
    /// Saves the current data to the database. 
    /// Path: users/farmingMode/plots/
    /// </summary>
    /// <param name="content">A Json string of all the plots data.</param>
    public void SavePlotsToDatabase(string content)
    {
        Debug.Log(FirebaseAuth.DefaultInstance.CurrentUser.UserId);
        if (AuthManager.Instance == null) { return; }
        AuthManager.Instance.dbReference.Child("users").Child(FirebaseAuth.DefaultInstance.CurrentUser.UserId).Child("farmingMode").Child("plots").SetRawJsonValueAsync(content);
    }

    /// <summary>
    /// Starts the coroutine to load the plot data from the database.
    /// 
    /// Also used for the load button in the UI.
    /// </summary>
    public void HandlePlotLoad()
    {
        StartCoroutine(LoadFromDatabase());
    }

    /// <summary>
    /// Loads data from the database.
    /// </summary>
    /// <returns></returns>
    protected override IEnumerator LoadFromDatabase()
    {
        List<FarmPlot> plots = PlotManager.Instance.GetPlots();
        for (int i =0; i < plots.Count;i++)
        {
            // Free up grid spaces before plot load. 
            // The grid gets updated as the plots are loaded in.
            plots[i].farmGrid.boolMap[plots[i].posInFarmGrid.x, plots[i].posInFarmGrid.y] = false;
        }

        // Return early if the database reference is null.
        if (!base.CheckIfDatabaseIsNull()) { yield break; }

        Task<DataSnapshot> plotLoadTask = AuthManager.Instance.dbReference.Child("users").Child(AuthManager.Instance.user.UserId).Child("farmingMode").Child("plots").Child("Items").GetValueAsync();

        yield return new WaitUntil(predicate: () => plotLoadTask.IsCompleted);

        // Check the task status. 
        base.CheckForTaskException(plotLoadTask);

        OnPlotLoaded?.Invoke(this, null);
    }

    /// <summary>
    /// Sets the plot data to default values (empty) if the task data is null.
    /// </summary>
    protected override void TaskDataIsNull()
    {
        // Get the plots list.
        List<FarmPlot> plots = PlotManager.Instance.GetPlots();
        FarmGrid farmGrid = GameObject.FindGameObjectWithTag("FarmManager").GetComponent<FarmGrid>();

        // Destroy all plots, as there is no data. 
        for (int i = 0; i < plots.Count; i++)
        {
            // Free up space in grid position.
            farmGrid.boolMap[plots[i].posInFarmGrid.x, plots[i].posInFarmGrid.y] = false;

            Destroy(plots[i].gameObject);
        }

        // Clear all plots from list.
        plots.Clear();

        // Clear Plot data list.
        PlotManager.Instance.GetPlotDataList().Clear();
    }

    /// <summary>
    /// Sets the farm plots to its loaded data from the database.
    /// </summary>
    /// <param name="task">The current task we're performing.</param>
    protected override void TaskHasRetrievedData(Task<DataSnapshot> task)
    {
        List<FarmPlot> plots = PlotManager.Instance.GetPlots();

        // Data has been retrieved.
        // Get a snapshot of the data. 
        DataSnapshot snapshot = task.Result;

        // The number of plots within the database. 
        long numberOfPlotsToCreate = snapshot.ChildrenCount;

        // If the amount of plots in the scene doesn't equal to the plots in the database.
        if (plots.Count != numberOfPlotsToCreate)
        {
            long result = plots.Count - numberOfPlotsToCreate;
            if (result < 0)
            {
                // Number of plots needed to create.
                // Converts the number to a positive. 
                CreatePlots((int)MathF.Abs(result));
            }
            else if (result > 0)
            {
                // Number of plots to remove.
                RemovePlots((int)Mathf.Abs(result));
            }
        }

        // Clear the list. 
        PlotManager.Instance.GetPlotDataList().Clear();

        // Add the plots' data that didn't get removed to the plot data list.
        for (int i = 0; i < plots.Count; i++)
        {
            PlotManager.Instance.GetPlotDataList().Add(plots[i].GetPlotData());
        }

        // Load the plot data.
        LoadPlotData(plots, snapshot);
    }

    /// <summary>
    /// Creates the number of plots needed to match the data from the database.
    /// E.g., If there are 6 plots in the database, but only 2 in the game scene, 
    /// then 4 plots will be created. 
    /// </summary>
    /// <param name="numberOfPlotsToCreate">The number of plots to create.</param>
    private void CreatePlots(int numberOfPlotsToCreate)
    {
        // If there are no plots in the world, create them.
        for (int i = 0; i < numberOfPlotsToCreate; i++)
        {
            // Create the plot. 
            GameObject plot = Instantiate(plotPrefab, transform.position, Quaternion.identity);

            // Add to the PlotManager's plot and plotData list. 
            PlotManager.Instance.GetPlots().Add(plot.GetComponent<FarmPlot>());
            PlotManager.Instance.GetPlotDataList().Add(plot.GetComponent<FarmPlot>().GetPlotData());
        }
    }

    /// <summary>
    /// Removes the number of excess plots to match the data from the database.
    /// E.g., If there are 3 plots in the database, but there are 5 in the game scene, 
    /// then 2 plots will be removed. 
    /// </summary>
    /// <param name="numberOfPlotsToRemove">The number of plots to remove.</param>
    private void RemovePlots(int numberOfPlotsToRemove)
    {
        List<FarmPlot> plots = PlotManager.Instance.GetPlots();

        // Delete extra plots. 
        // Remove from PlotManager's list.
        for (int i = numberOfPlotsToRemove; i > 0; i--)
        {
            // Destroy the plot.
            Destroy(plots[i].gameObject);
            
            // Remove the plot from both the plot and plotData lists. 
            plots.RemoveAt(i);
        }
    }

    /// <summary>
    /// Loads data from the database and sets it to the plots. 
    /// </summary>
    /// <param name="plots">The list of plots that are within the scene.</param>
    /// <param name="snapshot">The snapshot of the data we're loading.</param>
    private void LoadPlotData(List<FarmPlot> plots, DataSnapshot snapshot)
    {
        PlotData tempPlotData = plots[0].GetPlotData();

        // Load data
        for (int i = 0; i < plots.Count; i++)
        {
            // Position
            Vector3 newPosition = new Vector3();
            newPosition.x = float.Parse(snapshot.Child(i.ToString()).Child(nameof(tempPlotData.position)).Child("x").Value.ToString());
            newPosition.y = float.Parse(snapshot.Child(i.ToString()).Child(nameof(tempPlotData.position)).Child("y").Value.ToString());
            newPosition.z = float.Parse(snapshot.Child(i.ToString()).Child(nameof(tempPlotData.position)).Child("z").Value.ToString());

            // Rotation
            Quaternion newRotation = new Quaternion();
            newRotation = Quaternion.Euler
                (float.Parse(snapshot.Child(i.ToString()).Child(nameof(tempPlotData.rotation)).Child("x").Value.ToString()),
                float.Parse(snapshot.Child(i.ToString()).Child(nameof(tempPlotData.rotation)).Child("y").Value.ToString()),
                float.Parse(snapshot.Child(i.ToString()).Child(nameof(tempPlotData.rotation)).Child("z").Value.ToString())
                );

            // Plot Properties.
            bool newSoilWet = (bool)snapshot.Child(i.ToString()).Child(nameof(tempPlotData.soilWet)).Value;
            float newSoilTimer = float.Parse(snapshot.Child(i.ToString()).Child(nameof(tempPlotData.soilTimer)).Value.ToString());

            // Plant in Plot.
            string newPlantName = snapshot.Child(i.ToString()).Child(nameof(tempPlotData.plantName)).Value.ToString();

            if (newPlantName == string.Empty)
            {
                plots[i].SetPlotData(newPosition, newRotation, newSoilWet, newSoilTimer, null, string.Empty);
            }
            else
            {
                plots[i].SetPlotData(newPosition, newRotation, newSoilWet, newSoilTimer, null, newPlantName);
            }

            // Plant in Plot properties. 
            bool newReadyToHarvest = (bool)snapshot.Child(i.ToString()).Child(nameof(tempPlotData.readyToHarvest)).Value;
            float newGrowthTimer = float.Parse(snapshot.Child(i.ToString()).Child(nameof(tempPlotData.growthTimer)).Value.ToString());
            int newGrowthStage = int.Parse(snapshot.Child(i.ToString()).Child(nameof(tempPlotData.growthStage)).Value.ToString());

            plots[i].SetPlantInPlotData(newReadyToHarvest, newGrowthTimer, newGrowthStage);

            plots[i].LoadData();
        }
    }

    protected override void TaskHasRetrievedData_VentureModeSaveQuests(Task<DataSnapshot> task)
    {
        throw new NotImplementedException();
    }
}
