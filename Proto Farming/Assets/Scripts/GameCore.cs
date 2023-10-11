using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameCore : MonoBehaviour
{
    private static GameCore _instance;
    public static GameCore Instance { get { return _instance; } }


    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(this.gameObject);
        } else {
            _instance = this;
        }

    }

    void Start()
    {
        DontDestroyOnLoad(this.gameObject);
        Application.targetFrameRate = 120;
    }

    private void OnEnable()
    {
        // Detects when the scene has been loaded.
        SceneManager.sceneLoaded += SceneManager_OnSceneLoaded;
    }

    private void SceneManager_OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.buildIndex == (int)SceneIndexes.FARM_MODE)
        {
            gameObject.SetActive(true);
            StartCoroutine(LoadGame());
        }
    }

    private IEnumerator LoadGame()
    {
        yield return null;
        SaveGameManager.Instance.LoadGame();
    }
}
