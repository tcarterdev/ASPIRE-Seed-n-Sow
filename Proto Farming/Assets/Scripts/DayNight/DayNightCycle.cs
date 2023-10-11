using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DayNightCycle : MonoBehaviour
{
    [SerializeField] private Light directionalLight; 
    [SerializeField] private SO_LightPreset ligthingPreset;
    [SerializeField, Range(0, 60)] private int second;
    [SerializeField, Range(0, 60)] public int minute;
    [SerializeField, Range(0, 24)] public int hour;
    [SerializeField, Range(0, 10)] private int timeMultipler;

    private void FixedUpdate() //Fixed update for performance
    {
        OnCalendarTick();

        //if not ligththing preset do not try to update ligthting
        if (ligthingPreset == null)
        { return; } 

        //if the game is running
        if (Application.isPlaying)
        {
            //divde hour by 24 to get a value from 0-1
            UpdateLighting(hour / 24f);
        }
    }

    private void UpdateLighting(float timePercent)
    {
        //set ambient light & fog colour based off time percent
        RenderSettings.ambientLight = ligthingPreset.ambientColour.Evaluate(timePercent);
        //RenderSettings.skybox.color = ligthingPreset.ambientColour.Evaluate(timePercent);

        if (Camera.main != null)
        {
            Camera.main.backgroundColor = ligthingPreset.skyboxColour.Evaluate(timePercent);
        }

        RenderSettings.fogColor = ligthingPreset.fogColour.Evaluate(timePercent);


        //make sure there is a sun light
        if (directionalLight != null)
        {
            //Update light colour
            directionalLight.color = ligthingPreset.directionalColour.Evaluate(timePercent);
            directionalLight.intensity = (ligthingPreset.lightIntensityCurve.Evaluate(timePercent) * ligthingPreset.lightBaseIntesnity);

            //update the rotation of the directional light
            directionalLight.transform.localRotation = Quaternion.Euler(new Vector3((timePercent * 360f)- 90f, 170f, 0f));
        }
    }

    private void OnCalendarTick()
    {
        second += 1 * timeMultipler;
		if (second > 59)
		{
			second = 0;
			minute += 1;
			if (minute > 59)
			{
				minute = 0;
				hour += 1;
				if (hour > 23) // ITS A BRAND NEW DAY!
				{
					hour = 0;
				}
			}
		}
    }
}
