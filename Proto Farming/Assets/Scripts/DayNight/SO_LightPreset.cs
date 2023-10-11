using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(fileName = "New Lighting Preset", menuName = "World Systems/Lighting Preset", order = 1)]
public class SO_LightPreset : ScriptableObject //This Needs Renaming To Lighting Preset
{
    public Gradient ambientColour;
    public Gradient directionalColour;
    public AnimationCurve lightIntensityCurve;
    public float lightBaseIntesnity = 1;
    public Gradient skyboxColour;
    public Gradient fogColour;
    
}
