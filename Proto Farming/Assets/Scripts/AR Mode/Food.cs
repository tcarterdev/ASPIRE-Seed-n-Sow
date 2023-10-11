using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Food : MonoBehaviour
{
    [SerializeField] private InventorySlot inventorySlot;

    private HungerManager hungerManager;

    // Start is called before the first frame update
    void Start()
    {
        hungerManager = FindObjectOfType<HungerManager>();
    }

    public void EatFood()
    {
        if (!inventorySlot.itemInSlot.canEat)
        {
            Debug.Log("Can't be eaten. Returning early!");
            return;
        }

        // Increase hunger. 
        hungerManager.IncreaseHunger(inventorySlot.itemInSlot.replenishAmount);

        // Take away an item from the stack.
        inventorySlot.numberInSlot--;

        // Remove item if there are none left.
        if (inventorySlot.numberInSlot == 0)
        {
            List<InventorySlot> inventorySlots = inventorySlot.GetInventoryManager().GetInventorySlots();

            for (int i = 0; i < inventorySlots.Count; i++)
            {
                if (inventorySlot == inventorySlots[i])
                {
                    // Set the itemInSlot to null, as there is nothing left to eat.
                    inventorySlots[i].itemInSlot = null;

                    // Update the inventory display. 
                    inventorySlot.GetInventoryManager().UpdateInventoryDisplay();
                }
            }
        }
    }
}
