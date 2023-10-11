using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class ComptuerGUI : MonoBehaviour
{
    [SerializeField] private GameObject gameplayGUI;
    [SerializeField] private DayNightCycle clock;
    [Space]
    [Header("Home Screen")]
    [SerializeField] private TMP_Text clockText;

    private void Update()
    {
        if (clockText.gameObject.activeInHierarchy)
        {
            string hour;
            if (clock.hour <= 9)
            {
                hour =  "0" + clock.hour.ToString();
            }
            else
            {
                hour = clock.hour.ToString();
            }

            string min;
            if (clock.minute <= 9)
            {
                min =  "0" + clock.minute.ToString();
            }
            else
            {
                min = clock.minute.ToString();
            }

            clockText.SetText(hour + ":" + min); 
        }
    }

    public void CloseComputer()
    {
        this.gameObject.SetActive(false);
        gameplayGUI.SetActive(true);
    }
}
