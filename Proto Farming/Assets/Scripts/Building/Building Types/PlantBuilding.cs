using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlantBuilding : MonoBehaviour
{
    private FarmPlot myPlot;
    public PlantData plantData;
    public bool readyForHarvest;
    [SerializeField] private float growthTimer;
    [SerializeField] private int growthStage;

    [SerializeField] private MeshFilter meshFilter;

    void Awake()
    {
        myPlot = transform.parent.GetComponent<FarmPlot>();
        meshFilter = GetComponent<MeshFilter>();
    }

    public void SetPlantInPlotData(PlantData newPlantData)
    {
        plantData = newPlantData;
        meshFilter.mesh = plantData.growthStages[0].stageMesh;
        growthStage = 0;
        growthTimer = growthTimer = plantData.growthStages[growthStage].growthTime;
    }
    
    private void UpdatePlantInPlotData()
    {
        meshFilter.mesh = plantData.growthStages[growthStage].stageMesh;
    }

    void FixedUpdate()
    {
        if (myPlot.soilWet != true || plantData == null) { return; }

        if (growthStage == plantData.growthStages.Length - 1) { return; }

        if (growthTimer <= 0) { NextGrowthStage(); }
        growthTimer -= Time.deltaTime;
    }

    private void NextGrowthStage()
    {
        growthStage += 1;

        if (growthStage >= plantData.growthStages.Length - 1)
        {
            readyForHarvest = true;
        }
        else
        {
            readyForHarvest = false;
            meshFilter.mesh = plantData.growthStages[growthStage].stageMesh;
            growthTimer = plantData.growthStages[growthStage].growthTime;
        } 
    }

    public void RemovePlantFromPlot()
    {
        meshFilter.mesh = null;
        readyForHarvest = false;
        plantData = null;
    }

    #region Getters

    public bool GetReadyToHarvest() => readyForHarvest;
    public float GetGrowthTimer() => growthTimer;
    public int GetGrowthStage() => growthStage;

    #endregion

    #region Setters

    public void SetReadyToHarvest(bool state) => readyForHarvest = state;
    public void SetGrowthTimer(float timer) => growthTimer = timer;
    public void SetGrowthStage(int stage) => growthStage = stage;

    #endregion

    #region Load

    public void LoadObject(string plantName)
    {
        switch (plantName)
        {
            case "Wheat":
                plantData = Resources.Load<PlantData>($"Items/Ingriedents/{plantName}");
                UpdatePlantInPlotData();
                break;
        }
    }

    #endregion
}
