using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;


public class Builder : MonoBehaviour
{
    private FarmingMovement playerMovement;
    private InventoryManager inventoryManager;
    private GameObject farmManager;
    private FarmGrid farmGrid;
    private int buildMenuOpenint;
    
    public bool buildMenuOpen;
    [SerializeField] private GameObject buildMenuParent;
    [SerializeField] private GameObject farmMenuParent;
    [SerializeField] private BuildPreview buildPreview;
    [SerializeField] private Vector3 bulidPreviewStartingOffset;
    [SerializeField] private PlayerInput playerInput;
    [SerializeField] private bool previewMoveCoolDown;
    [SerializeField] private float moveCoolDownTime;
    [SerializeField] private Material cantBuildMat;
    [SerializeField] private Material canBuildMat;
    [SerializeField] private Vector2Int buildDir = Vector2Int.down;

    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip selectfx;
    [SerializeField] private AudioClip plantplot;
    [SerializeField] private GameObject recipiesUI;

    [SerializeField] private ParticleSystem itemNeeded;
    bool firstTimeOpenBuildMode;
    [SerializeField] private GameObject pns_TT_UI;
    [SerializeField] private GameObject tutorial_buildmode;

    private void Awake()
    {
        inventoryManager = GameObject.FindGameObjectWithTag("GameController").GetComponent<InventoryManager>();
        farmManager = inventoryManager.transform.GetChild(0).gameObject;
        farmGrid = farmManager.GetComponent<FarmGrid>();
        playerInput = GetComponent<PlayerInput>();
        playerMovement = GetComponent<FarmingMovement>();
        
        
        
       
    }

    private void Update()
    {
        if (!buildMenuOpen) { return; }

        BuildMode();
    }


    public void OpenBuildMenu()
    {
        buildMenuOpen = true;
        buildMenuParent.SetActive(true);
        farmMenuParent.SetActive(false);
        playerMovement.enabled = false;
        EnterBuildMode();
        audioSource.PlayOneShot(selectfx, 1);
        buildMenuOpenint++;

        if (buildMenuOpenint == 1)
        {
            tutorial_buildmode.SetActive(true);
        }
        
    }

    public void CloseBuildMenu()
    {
        buildMenuOpen = false;
        buildMenuParent.SetActive(false);
        farmMenuParent.SetActive(true);
        playerMovement.enabled = true;
        ExitBuildMode();
        audioSource.PlayOneShot(selectfx, 1);
    }

    private void EnterBuildMode()
    {
        if (inventoryManager.currentlyEquippedItem == null)
        {
            itemNeeded.Play();
            Debug.LogWarning("There is nothing equppied!");
            ExitBuildMode();
            CloseBuildMenu();
            return;
        }

        if (firstTimeOpenBuildMode == true)
        {
            pns_TT_UI.SetActive(true);
        }    

        buildPreview.gameObject.SetActive(true);
        buildPreview.GetComponent<MeshRenderer>().material = canBuildMat;
        Vector3 startingPos = transform.position - bulidPreviewStartingOffset;
        startingPos.x = Mathf.Round(startingPos.x);
        startingPos.y = 0f;
        startingPos.z = Mathf.Round(startingPos.z);

        buildPreview.transform.position = startingPos;
        buildPreview.meshFilter.mesh = inventoryManager.currentlyEquippedItem.buildPreview;
    }

    private void BuildMode()
    {
        Vector2 input = playerInput.actions["Move"].ReadValue<Vector2>();

        if (input.magnitude >= 0.1f  && !previewMoveCoolDown)
        {
            previewMoveCoolDown = true;
            Vector3 moveDir = new Vector3(input.x, 0, input.y);
            moveDir.x = Mathf.Round(moveDir.x);
            moveDir.z = Mathf.Round(moveDir.z);

            buildPreview.transform.position += moveDir;
            
            StartCoroutine(PreviewMoveCoolDown());

            buildDir.x = (int)moveDir.x;
            buildDir.y = (int)moveDir.z;
        }
    }

    public void OpenRecipies()
    {
        recipiesUI.SetActive(true);
    }
    public void CloseRecipies()
    {
        recipiesUI.SetActive(false);
    }

    IEnumerator PreviewMoveCoolDown()
    {
        CanBuildHereCheck();

        yield return new WaitForSeconds(moveCoolDownTime);
        previewMoveCoolDown = false;
    }

    private void ExitBuildMode()
    {
        buildPreview.gameObject.SetActive(false);
    }

    public void BuildObject()
    {  
        if (farmGrid.boolMap[(int) buildPreview.transform.position.x, (int) buildPreview.transform.position.z])
        {
            Debug.LogWarning("There is already a building here!");
            return;
        }

        GameObject newBuilding = Instantiate(inventoryManager.currentlyEquippedItem.buildPrefab);
        audioSource.PlayOneShot(plantplot, 2);
        newBuilding.transform.parent = farmManager.transform;
        newBuilding.transform.position = buildPreview.transform.position;
        
        int a = (int) buildPreview.transform.position.x;
        int b = (int) buildPreview.transform.position.z;

        farmGrid.boolMap[a, b] = true;

        newBuilding.GetComponent<WorkStation>().posInFarmGrid = new Vector2Int(a, b);

        buildPreview.transform.position = new Vector3(buildPreview.transform.position.x + buildDir.x, buildPreview.transform.position.y, buildPreview.transform.position.z + buildDir.y); 
        CanBuildHereCheck();

        // Invoke building placed quest event.
        QuestProgress.Instance.InvokeBuildingPlaced();
    }
    private void CanBuildHereCheck()
    {
        // Stop outta bounds, outta pocket
        if ((int)buildPreview.transform.position.x <= 0 || buildPreview.transform.position.x >= farmGrid.boolMap.GetLength(0) - 1)
        {
            buildPreview.GetComponent<MeshRenderer>().material = cantBuildMat;
            return;
        } else if ((int)buildPreview.transform.position.z <= 0 || buildPreview.transform.position.z >= farmGrid.boolMap.GetLength(1) - 1)
        {
            buildPreview.GetComponent<MeshRenderer>().material = cantBuildMat;
            return;
        }


        if (farmGrid.boolMap[(int) buildPreview.transform.position.x, (int) buildPreview.transform.position.z])
        {
            buildPreview.GetComponent<MeshRenderer>().material = cantBuildMat;
            return;
        }
        else
        {
            buildPreview.GetComponent<MeshRenderer>().material = canBuildMat;
            return;
        }
    }
}