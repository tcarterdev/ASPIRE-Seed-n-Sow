using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DataGathering_SceneChangeManager : MonoBehaviour
{
    public string SceneIdentifier;
    // Start is called before the first frame update
    void Start()
    {

        StartCoroutine(SceneOpened());
    }

    void OnDestroy()
    {
        DataGathering.dataGathering.Firebase_LeavingGameMode(SceneIdentifier);
    }
    public IEnumerator SceneOpened() 
    {
        yield return new WaitForSecondsRealtime(0.1f);
        DataGathering.dataGathering.Firebase_EnteringGameMode(SceneIdentifier);
    }

}
