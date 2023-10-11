using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SaveData
{
    public List<string> collectedItems; // Items that have been collected. 
    public List<string> activeItems;    // Items that can be collected.

    public SaveData()
    {
        collectedItems = new List<string>();
        activeItems = new List<string>();
    }
}
