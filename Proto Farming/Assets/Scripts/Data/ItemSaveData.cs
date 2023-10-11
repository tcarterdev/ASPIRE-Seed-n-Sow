using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public struct ItemSaveData
{
    [Header("Slot Properties")]
    public string itemName;
    public int numberInSlot;
    public string itemType;
    public string itemCategory;
}
