using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Rotate : MonoBehaviour
{
    RectTransform rectTransform;

    private void Start()
    {
        if (GetComponent<RectTransform>() != null)
        {
            rectTransform = GetComponent<RectTransform>();
        }
    }

    void LateUpdate()
    {
        if (rectTransform != null)
        {
            rectTransform.Rotate(new Vector3(0f, 0f, -2f));
        }
        else
        {
            transform.Rotate(Vector3.up * (15f * Time.deltaTime));
        }
    }
}