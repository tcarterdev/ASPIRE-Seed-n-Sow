using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class SetLabelName : MonoBehaviour
{
    [SerializeField] private TMP_Text labelText;

    void Start()
    {
        labelText.SetText(this.gameObject.name);
    }
}
