using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HelpBook : MonoBehaviour
{
    public GameObject helpbookobj;



    public GameObject page1;

    public GameObject page2;

    public GameObject page3;

    public GameObject page4;

    public GameObject page5;
    public void HelpBookFunc()
    {
        helpbookobj.SetActive(true);

        DataGathering.dataGathering.Firebase_TutorialOpened();
    }

    public void CloseHelpBook()
    {
        helpbookobj.SetActive(false);

        DataGathering.dataGathering.Firebase_TutorialClosed();
    }



    public void Page1()
    {
        helpbookobj.SetActive(false);
        page1.SetActive(true);

    }

    public void Page2()
    {
        page1.SetActive(false);
        page2.SetActive(true);
    }

    public void Page3()
    {
        page2.SetActive(false);
        page3.SetActive(true);
    }

    public void Page4()
    {
        page3.SetActive(false);
        page4.SetActive(true);
    }

    public void Page5()
    {
        page4.SetActive(false);
        page5.SetActive(true);
    }

}
