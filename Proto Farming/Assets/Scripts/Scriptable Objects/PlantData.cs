using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Plant Data", menuName = "Plants & Crops/plantData", order = 1)]
public class PlantData : ScriptableObject
{
    public string plantName;

    public GrowthStage[] growthStages;
    public ItemData harvestedItem;
    public int minHarvestedNum;
    public int maxHarvestedNum;
}

[System.Serializable] public struct GrowthStage 
{
    public string stageName;
    public float growthTime;
    public Mesh stageMesh;
}