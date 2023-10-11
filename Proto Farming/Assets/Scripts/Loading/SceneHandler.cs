using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneHandler : MonoBehaviour
{
    public static SceneHandler Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null)
        {
            Debug.LogError($"There's more than one SceneHandler! {transform} - {Instance}");
            Destroy(gameObject);
        }

        Instance = this;
    }

    public void LoadLevel()
    {
        // Load the next scene. 
        SceneManager.LoadScene((int)SceneIndexes.FARM_MODE);
    }

    public int GetActiveSceneIndex()
    {
        return SceneManager.GetActiveScene().buildIndex;
    }

    public string GetActiveSceneName()
    {
        return SceneManager.GetActiveScene().name;
    }
}
