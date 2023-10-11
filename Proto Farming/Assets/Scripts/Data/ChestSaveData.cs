using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public struct ChestSaveData
{
    [Header("Chest Properties")]
    public Vector3 position;
    public Quaternion rotation;

    [Header("Slot Properties")]
    public List<ItemSaveData> listOfItems;
}
