using ICSharpCode.SharpZipLib.Zip;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneChanger : MonoBehaviour
{
    [SerializeField] private int sceneIndex;

    public void StartChangeSceneDelay() { StartCoroutine(DelaySceneChange()); }

    private IEnumerator DelaySceneChange()
    {
        yield return new WaitForSeconds(2.0f);
        ChangeScene();
    }

    public void ChangeScene()
    {
        if (SceneManager.GetActiveScene().buildIndex == (int)SceneIndexes.VENTURE_MODE)
        {
            if (GameObject.Find("LoadingScreen"))
            {
                LoadingScreen.Instance.EnableLoadingScreen(true, 0.5f);
            }

            if (GameObject.Find("GameCore"))
            {
                GameCore.Instance.gameObject.SetActive(true);
            }

            StartCoroutine(ChangeSceneAfterSave((int)SceneIndexes.FARM_MODE));
        }
        else
        {
            if (GameObject.Find("LoadingScreen"))
            {
                LoadingScreen.Instance.EnableLoadingScreen(true, 0.5f);
            }

            StartCoroutine(ChangeSceneAfterSave((int)SceneIndexes.VENTURE_MODE));

            GameCore.Instance.gameObject.SetActive(false);
            SceneManager.LoadScene(sceneIndex);
        }
        //Destroy(GameCore.Instance.gameObject);
    }

    IEnumerator ChangeSceneAfterSave(int sceneIndex)
    {
        yield return new WaitForSeconds(3.0f);
        SceneManager.LoadScene(sceneIndex);
    }
}
