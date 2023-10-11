using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class HouseDoor : WorkStation
{
    [SerializeField] private int sceneToLoad;
    [SerializeField] private Vector3 posToTele;
    [SerializeField] private bool loadIndoors;

    [SerializeField] private Color outdoorColour;
    [SerializeField] private Color indoorColour;

    private void Start()
    {
        if (SceneManager.GetActiveScene().buildIndex == (int)SceneIndexes.FARM_HOUSE_INTERIOR && GameObject.Find("LoadingScreen"))
        {
            LoadingScreen.Instance.EnableLoadingScreen(false, 0.5f);
        }
    }

    public override void InteractionPopUp()
    {
        interactionButton.gameObject.SetActive(true);
    }

    public override void InteractWithWorkStation()
    {
        if (SceneHandler.Instance.GetActiveSceneIndex() == (int)SceneIndexes.FARM_HOUSE_INTERIOR)
        {
            if (GameObject.Find("LoadingScreen"))
            {
                LoadingScreen.Instance.EnableLoadingScreen(true, 0.5f);
                LoadingScreen.Instance.UpdateLoadingInfo("Loading farm");
            }

            SceneManager.LoadScene((int)SceneIndexes.FARM_MODE);
        }
        else
        {
            if (GameObject.Find("LoadingScreen"))
            {
                LoadingScreen.Instance.EnableLoadingScreen(true, 0.5f);
                LoadingScreen.Instance.UpdateLoadingInfo("Loading house");
            }

            SceneManager.LoadScene((int)SceneIndexes.FARM_HOUSE_INTERIOR);
        }

        player.transform.position = posToTele;

        if (sceneToLoad == (int)SceneIndexes.FARM_MODE)
        {
            Camera.main.backgroundColor = outdoorColour;
        }
        else
        {
            Camera.main.backgroundColor = indoorColour;
        }
    }
}
