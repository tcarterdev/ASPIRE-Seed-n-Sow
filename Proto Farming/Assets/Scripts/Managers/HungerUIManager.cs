using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HungerUIManager : MonoBehaviour
{
    [SerializeField] private Slider hungerBar;
    [SerializeField] private float sliderSmoothTime = 1f;
    [SerializeField] private bool decreaseSliderSlowly;
    [SerializeField] private GameObject noHungerPopUp;

    private HungerManager hungerManager;
    private bool noHungerPopUpShown;

    // Start is called before the first frame update
    void Start()
    {
        hungerBar = GetComponentInChildren<Slider>();
        hungerManager = FindObjectOfType<HungerManager>();

        UpdateHungerUI(hungerManager.GetCurrentHunger());
    }

    public void InitialiseHungerBar(float totalHunger) => hungerBar.value = totalHunger;

    public void UpdateHungerUI(float currentHunger)
    {
        hungerBar.value = currentHunger / 100f;

        if (!noHungerPopUpShown && hungerBar.value <= 0)
        {
            noHungerPopUpShown = true;
            noHungerPopUp.SetActive(true);
        }
    }
}
