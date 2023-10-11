using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ToolPickUp : ItemPickUp
{
    [SerializeField] private ToolData toolData;

    private void OnTriggerEnter(Collider other)
    {
        //Debug.Log("Hello");

        if (other.gameObject != player) { return; }

        inventoryManager.AddToolToToolbelt(toolData);

        // If we are in Farming Mode, and there is a UniqueID attached, add to the collected items list.
        if (SceneHandler.Instance.GetActiveSceneIndex() == (int)SceneIndexes.FARM_MODE && GetComponent<UniqueID>() != null)
        {
            ItemSaveManager.itemSaveData.collectedItems.Add(uid);
        }

        Destroy(this.gameObject);
    }
}
