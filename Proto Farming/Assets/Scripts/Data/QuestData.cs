using System;
using UnityEngine;

[Serializable]
public struct QuestData
{
    [Header("Quest Properties")]
    public string questType;
    public string uid;
    public int sessionId;

    [Header("Task Properties")]
    public float timer;
    public float amount;
    public float requiredAmount;
}
