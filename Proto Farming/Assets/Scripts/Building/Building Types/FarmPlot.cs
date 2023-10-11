using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FarmPlot : WorkStation
{
    [Header("Interaction & Gameplay")]
    [SerializeField] private PlantBuilding plantInPlot;
    public bool soilWet;
    [SerializeField] private float mostureTime;
    [SerializeField] private float soilTimer;
    [Space]
    [SerializeField] private ToolData shovel;
    [SerializeField] private ToolData wateringCan;

    [Header("Materials & Rendering")]
    [SerializeField] private MeshRenderer meshRenderer;
    [SerializeField] private Material drySoilMaterial;
    [SerializeField] private Material wetSoilMaterial;

    [Header("Sound and Effects")]
    [SerializeField] AudioClip soilwetfx;

    [Header("Data")]
    [SerializeField] private PlotData plotData = new PlotData();

    private void Start()
    {
        Debug.Log("Plot Start");
        Vector2Int posInGrid = new Vector2Int(Mathf.RoundToInt(this.transform.position.x), Mathf.RoundToInt(this.transform.position.z));
        posInFarmGrid = posInGrid;
        farmGrid.boolMap[posInGrid.x, posInGrid.y] = true;

        PlotManager.Instance.GetPlots().Add(this);
        PlotManager.Instance.GetPlotDataList().Add(plotData);
    }

    public override void OnTriggerEnter(Collider other)
    {
        if (other.gameObject != player) { return; }

        InteractionPopUp();
    }

    // This activates after the player has left the trigger (Box Collider)
    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject != player) { return; }

        if (interactionButton.gameObject.activeInHierarchy)
        {
            interactionButton.gameObject.SetActive(false);
        }
    }

    public override void InteractionPopUp()
    {   
        if (plantInPlot.readyForHarvest) //TODO: not hard coded
        { 
            interactionText.SetText("Harvest");
            interactionButton.gameObject.SetActive(true);
        }

        if (inventoryManager.currentlyEquippedItem == null) 
        { 
            return; 
        }
        else if (inventoryManager.currentlyEquippedItem.itemType == ItemType.Tool) //TODO: not hard coded
        { 
            if (inventoryManager.currentlyEquippedItem == shovel)
            {
                interactionText.SetText("Dig Up Plot");
            }
            else if (inventoryManager.currentlyEquippedItem == wateringCan)
            {
                interactionText.SetText("Water Plot");
            }
            interactionButton.gameObject.SetActive(true);
        }
        else if (inventoryManager.currentlyEquippedItem.itemType == ItemType.Seed && plantInPlot.plantData == null)
        {
            interactionText.SetText("Plant Seed");
            interactionButton.gameObject.SetActive(true);
        }
        else
        {
            interactionButton.gameObject.SetActive(false);
        }   
    }

    public override void InteractWithWorkStation()
    {
        if (plantInPlot.readyForHarvest)
        {
            HarvestPlot();
        }
        else if (inventoryManager.currentlyEquippedItem == wateringCan) //TODO: not hard coded
        { 
            WaterSoil(); 
        }
        else if (inventoryManager.currentlyEquippedItem == shovel) //TODO: not hard coded
        { 
            RemoveWorkStation();

            PlotManager.Instance.GetPlots().Remove(this);
            PlotManager.Instance.GetPlotDataList().Remove(this.plotData);

        }
        else if (inventoryManager.currentlyEquippedItem.itemType == ItemType.Seed)
        {
            PlantObjectInPlot();
        }
    }

    private void WaterSoil()
    {
        if (soilWet) { return; }

        TutorialManager tutorialmanager = GameObject.FindGameObjectWithTag("TutorialManager").GetComponent<TutorialManager>();

        if (tutorialmanager != null)
        {
            tutorialmanager.TenderLoving();
        }
        // Give XP to the player.
        if (AccountLevel.Instance != null)
        {
            AccountLevel.Instance.AddXP((int)XPValues.TEND);
        }
        

        soilWet = true;
        //meshRenderer.material = wetSoilMaterial;
        soilTimer = mostureTime;
        interactionAudioSource.PlayOneShot(soilwetfx, 1f);

        // Invoke the crop tendered quest event.
        QuestProgress.Instance.InvokeCropTendered();
    }

    private void Update()
    {
        if (!soilWet) { return; }

        // Updates the material if soilWet is true.
        meshRenderer.material = wetSoilMaterial;

        if (soilTimer <= 0) { SoilDryUp(); }
        soilTimer -= Time.deltaTime;
    }

    private void SoilDryUp()
    {
        soilWet = false;
        meshRenderer.material = drySoilMaterial;
    }

    private void PlantObjectInPlot()
    {
        // Give XP to the player.
        if (AccountLevel.Instance != null)
        {
            AccountLevel.Instance.AddXP((int)XPValues.BUILD);
        }

        // Firebase Analytics - Crop Planted tracking.
        DataGathering.dataGathering.Firebase_CropPlanted(inventoryManager.currentlyEquippedItem.plantData.plantName);

        Debug.Log("Player planted a new plant");
        inventoryManager.equippedAmmount -= 1;
        inventoryManager.UpdateInventoryDisplay();
        inventoryManager.UpdateEquippedItemDisplay();
        plantInPlot.SetPlantInPlotData(inventoryManager.currentlyEquippedItem.plantData);

    }

    private void HarvestPlot()
    {
        Debug.Log("Player planted a plant");

        // Give XP to the player.
        if (AccountLevel.Instance != null)
        {
            AccountLevel.Instance.AddXP((int)XPValues.HARVEST);
            AccountLevel.Instance.AddXP((int)XPValues.COLLECT);
        }

        //Spawn Object
        int harvestNum = Random.Range(plantInPlot.plantData.minHarvestedNum, plantInPlot.plantData.maxHarvestedNum);
        ItemData itemData = plantInPlot.plantData.harvestedItem;
        inventoryManager.AddItemToInventory(itemData, harvestNum);

        // Firebase Analytics - Crop harvested tracking.
        DataGathering.dataGathering.Firebase_CropHarvested(itemData.plantData.plantName);

        //Remove Plant
        plantInPlot.RemovePlantFromPlot();
        interactionButton.gameObject.SetActive(false);

        // Invoke item collected quest event.
        QuestProgress.Instance.InvokeItemCollected();
    }

    private void UpdateGrid()
    {
        Vector2Int posInGrid = new Vector2Int(Mathf.RoundToInt(this.transform.position.x), Mathf.RoundToInt(this.transform.position.z));
        posInFarmGrid = posInGrid;
        farmGrid.boolMap[posInGrid.x, posInGrid.y] = true;
    }

    #region Save & Load

    /// <summary>
    /// Saves the current transform, soilWet and plantInPlot to the plotData.
    /// </summary>
    public void SaveData()
    {
        plotData.position = transform.position;
        plotData.rotation = transform.rotation;

        plotData.soilWet = soilWet;
        plotData.soilTimer = soilTimer;

        plotData.plantInPlot = plantInPlot;

        if (plantInPlot.plantData != null)
        {
            plotData.plantName = plantInPlot.plantData.plantName;
            Debug.Log(plantInPlot.plantData.plantName);
        }

        plotData.readyToHarvest = plantInPlot.GetReadyToHarvest();
        plotData.growthTimer = plantInPlot.GetGrowthTimer();
        plotData.growthStage = plantInPlot.GetGrowthStage();
    }

    /// <summary>
    /// Loads the plotData and sets it to the game object. 
    /// </summary>
    public void LoadData()
    {
        transform.position = plotData.position;
        transform.rotation = plotData.rotation;

        //// Claim new grid position space.
        UpdateGrid();

        soilWet = plotData.soilWet;
        soilTimer = plotData.soilTimer;

        plantInPlot = plotData.plantInPlot;

        if (plotData.plantName != string.Empty)
        {
            plantInPlot = GetComponentInChildren<PlantBuilding>();

            // Update plant in plot variables
            plantInPlot.SetReadyToHarvest(plotData.readyToHarvest);
            plantInPlot.SetGrowthTimer(plotData.growthTimer);
            plantInPlot.SetGrowthStage(plotData.growthStage);

            plantInPlot.LoadObject(plotData.plantName);
        }
        else if (plotData.plantName == string.Empty)
        {
            plantInPlot = transform.Find("Plant in Plot").GetComponent<PlantBuilding>();

            plantInPlot.plantData = null;
            plantInPlot.SetReadyToHarvest(false);
            plantInPlot.SetGrowthTimer(0);
            plantInPlot.SetGrowthStage(0);

            transform.Find("Plant in Plot").GetComponent<MeshFilter>().mesh = null;
        }
    }

    #endregion

    #region Getters & Setters

    /// <summary>
    /// Retrieves the plotData of the plot.
    /// </summary>
    /// <returns>the plot data.</returns>
    public PlotData GetPlotData() => plotData;

    public void SetPlotData(Vector3 position, Quaternion rotation, bool soilWet, float soilTimer, PlantBuilding plantInPlot, string plantName)
    {
        plotData.position = position;
        plotData.rotation = rotation;

        plotData.soilWet = soilWet;
        plotData.soilTimer = soilTimer;

        plotData.plantInPlot = plantInPlot;
        plotData.plantName = plantName;
    }

    public void SetPlantInPlotData(bool readyToHarvest, float growthTimer, int growthStage)
    {
        plotData.readyToHarvest = readyToHarvest;

        plotData.growthTimer = growthTimer;
        plotData.growthStage = growthStage;
    }

    //public void SetPlantInPlotData(string plantName)
    //{
    //    plotData.plantName = plantInPlotData;
    //}

    #endregion  
}
