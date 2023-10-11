using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class LoadingScreen : MonoBehaviour
{
    public static LoadingScreen Instance { get; private set; }

    private GameObject loadingScreenCanvas;
    private TextMeshProUGUI loadingInfoText;

    private void Awake()
    {
        if (Instance != null)
        {
            Debug.LogError($"There's more than one LoadingScreen! {transform} - {Instance}");
            Destroy(gameObject);
        }

        Instance = this;

        // Get the loading screen.
        loadingScreenCanvas = transform.GetChild(0).gameObject;
        // Get the loading info text from child. 
        loadingInfoText = transform.GetChild(0).Find("LoadingInfoText").GetComponent<TextMeshProUGUI>();
    }

    public void UpdateLoadingInfo(string description)
    {
        loadingInfoText.text = description;
    }

    public void EnableLoadingScreen(bool state, float delay)
    {
        StartCoroutine(DelayShowingLoadingScreen(state, delay));
    }

    private IEnumerator DelayShowingLoadingScreen(bool state, float delay)
    {
        if (!state)
        {
            UpdateLoadingInfo("Loading completed");
            yield return new WaitForSeconds(delay);
            loadingScreenCanvas.SetActive(state);
        }
        else
        {
            UpdateLoadingInfo("Loading level");
            loadingScreenCanvas.SetActive(state);
        }
    }

    public GameObject GetLoadingScreenCanvas() => loadingScreenCanvas;

}
