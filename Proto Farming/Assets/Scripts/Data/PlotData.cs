using System;
using UnityEngine;

[Serializable]
public struct PlotData
{
    [Header("Position & Rotation")]
    public Vector3 position;
    public Quaternion rotation;

    [Header("Plot Properties")]
    public bool soilWet;
    public float soilTimer;

    public PlantBuilding plantInPlot;

    [Header("Plant in Plot Properties")]
    public string plantName;
    public bool readyToHarvest;
    public float growthTimer;
    public int growthStage;
}
