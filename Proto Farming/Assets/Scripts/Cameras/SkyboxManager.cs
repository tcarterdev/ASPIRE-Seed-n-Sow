using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkyboxManager : MonoBehaviour
{
    [SerializeField] private Color outdoorSkybox;
    [SerializeField] private Color indoorSkybox;

    public void LoadOutdoorSkybox()
    {
        RenderSettings.skybox.color = outdoorSkybox;
    }

    public void LoadIndoorSkybox()
    {
        RenderSettings.skybox.color = indoorSkybox;
    }
}
