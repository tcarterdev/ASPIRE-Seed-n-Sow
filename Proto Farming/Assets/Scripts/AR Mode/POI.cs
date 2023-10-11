using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class POI : MonoBehaviour
{
    public bool coolingDown;
    public Canvas canvas;
    public POI_Menu poiMenu;
    private string poiName;

    [SerializeField] private Animator animator;
    [SerializeField] private ParticleSystem vfx;
    [SerializeField] private ParticleSystem collected;
    [SerializeField] private ItemData[] possibleItems;
    [SerializeField] private Button interactButton;
    [SerializeField] private TMP_Text buttonText;
    [SerializeField] private SkinnedMeshRenderer smkRenderer;
    [SerializeField] private Material[] coolDownMats;

    private void Start()
    {
        poiMenu = GameObject.FindGameObjectWithTag("POI").GetComponent<POI_Menu>();
        canvas.worldCamera = Camera.main;
        poiName = gameObject.name;
    } 

    public void ShowCanvas()
    {
        canvas.gameObject.SetActive(true);
        animator.SetFloat("Blend", 1);
        vfx.gameObject.SetActive(true);
        vfx.Play();
    }   

    public void HideCanvas()
    {
        canvas.gameObject.SetActive(false);
        animator.SetFloat("Blend", 0);
        vfx.gameObject.SetActive(false);
        vfx.Stop(true, ParticleSystemStopBehavior.StopEmitting);
    }

    public void OpenPoiMenu()
    {
        poiMenu.rewardItem = PickRandomItem();
        poiMenu.TogglePoiMenu(poiName);
        poiMenu.poi = this;
    }

    private ItemData PickRandomItem()
    {
        int randomIndex = Random.Range(0, possibleItems.Length);
        ItemData randomItem = possibleItems[randomIndex];
        return randomItem;
    }

    public void StartCoolDown()
    {
        Debug.Log("Started cool down on: " + this.gameObject.name);
        coolingDown = true;
        //renderer.materials = coolDownMats;
        interactButton.interactable = false;
        buttonText.SetText("Cooling down");
        vfx.gameObject.SetActive(false);
        //collected.Play();
    }
}