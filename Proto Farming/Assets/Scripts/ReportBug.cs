using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ReportBug : MonoBehaviour
{
    public void OpenBugReport()
    {
        Application.OpenURL("https://forms.office.com/e/1dguyipsc7");
    }
}
