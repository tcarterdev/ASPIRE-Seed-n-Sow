using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Well : WorkStation
{
    [SerializeField] private ItemData waterItemData;
    [SerializeField] private AudioSource watercollect;
    public override void InteractWithWorkStation()
    {
        inventoryManager.AddItemToInventory(waterItemData, 1);
        watercollect.Play();
    }
}
