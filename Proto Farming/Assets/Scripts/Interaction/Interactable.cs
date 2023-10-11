using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Interactable : MonoBehaviour
{   
    [HideInInspector] public FarmGrid farmGrid;
    /*[HideInInspector] */public GameObject player;
    [HideInInspector] public InventoryManager inventoryManager;
    [HideInInspector] public GameObject flowerFooter;
    [HideInInspector] public AudioSource interactionAudioSource;

    private void Awake()
    {
        player = GameObject.FindGameObjectWithTag("Player");
        inventoryManager = GameObject.FindGameObjectWithTag("GameController").GetComponent<InventoryManager>();
        flowerFooter = GameObject.FindGameObjectWithTag("FlowerFooter");
        
        farmGrid = GameObject.FindGameObjectWithTag("FarmManager").GetComponent<FarmGrid>();
        interactionAudioSource = player.GetComponent<AudioSource>();
    }

    private void Start()
    {
        Vector2Int posInGrid = new Vector2Int(Mathf.RoundToInt(this.transform.position.x), Mathf.RoundToInt(this.transform.position.z));
        
        if (farmGrid != null) 
        { 
            if (posInGrid.x > farmGrid.boolMap.GetLength(0) || posInGrid.x < 0 || posInGrid.y > farmGrid.boolMap.GetLength(1) || posInGrid.y < 0) { return; }

            farmGrid.boolMap[posInGrid.x, posInGrid.y] = true;
        }
    }

    private void FixedUpdate()
    {
        if (inventoryManager == null && player == null)
        {
            inventoryManager = GameObject.FindGameObjectWithTag("GameController").GetComponent<InventoryManager>();
            player = GameObject.FindGameObjectWithTag("Player");
        }

        if (flowerFooter == null) { flowerFooter = GameObject.Find("FlowerFooter"); }
    }

    private void OnDestroy()
    {
        Vector2Int posInGrid = new Vector2Int(Mathf.RoundToInt(this.transform.position.x), Mathf.RoundToInt(this.transform.position.z));

        // if (farmGrid != null) 
        // { 
        //     if (posInGrid.x > farmGrid.boolMap.GetLength(0) || posInGrid.x < 0 || posInGrid.y > farmGrid.boolMap.GetLength(1) || posInGrid.y < 0) { return; }

        //     farmGrid.boolMap[posInGrid.x, posInGrid.y] = false;   
        // }
    }
}