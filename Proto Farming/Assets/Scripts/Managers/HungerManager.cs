using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HungerManager : MonoBehaviour
{
    public HungerUIManager hungerUIManager;
    public float totalHunger = 100f;

    public float currentHunger;

    private void Awake()
    {
        hungerUIManager = FindObjectOfType<HungerUIManager>();
    }

    public void Start()
    {
        hungerUIManager.InitialiseHungerBar(totalHunger);
        currentHunger = totalHunger;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.N))
        {
            IncreaseHunger(5);
        }
    }

    public void DepleteHunger(float amount)
    {
        float hungerCalculation = currentHunger -= amount;
        if (hungerCalculation <= 0)
        {
            currentHunger = 0;
            hungerUIManager.UpdateHungerUI(currentHunger);
        }
        else
        {
            hungerUIManager.UpdateHungerUI(currentHunger -= amount);
        }
    }

    public void IncreaseHunger(float amount)
    {
        float hungerCalculation = currentHunger += amount;
        if (hungerCalculation >= 100)
        {
            currentHunger = 100;
            hungerUIManager.UpdateHungerUI(currentHunger);
        }
        else
        {
            hungerUIManager.UpdateHungerUI(currentHunger += amount);
        }
    }

    public float GetCurrentHunger() => currentHunger;
}
