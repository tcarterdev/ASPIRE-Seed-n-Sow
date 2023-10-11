using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class POI_Interact : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer == 7)
        {
            POI poi = other.GetComponent<POI>();
            if (poi.coolingDown) {return; }
            other.GetComponent<POI>().ShowCanvas();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.layer == 7)
        {            
            other.GetComponent<POI>().HideCanvas();
        }
    }
}
