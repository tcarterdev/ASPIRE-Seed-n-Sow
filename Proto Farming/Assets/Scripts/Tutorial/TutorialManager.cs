using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TutorialManager : MonoBehaviour
{

    public GameObject[] TutorialElements;
    public void GettingStarted()
    {
        TutorialElements[0].SetActive(false);
    }

    public void DigOfTheStump()
    {
        TutorialElements[1].SetActive(false);
        
    }

    public void PlottingNotScheming()
    {
        TutorialElements[2].SetActive(false);
        
    }

    public void SeedsBeforeSow()
    {
        TutorialElements[3].SetActive(false);
    }

    public void TenderLoving()
    {
        TutorialElements[3].SetActive(false);
        TutorialElements[4].SetActive(true);

    }

    public void HarvestTime()
    {
        TutorialElements[4].SetActive(false);
        TutorialElements[5].SetActive(true);

    }

    public void TheGreatAspireBake()
    {
        TutorialElements[5].SetActive(false);
        TutorialElements[6].SetActive(true);
    }
}
