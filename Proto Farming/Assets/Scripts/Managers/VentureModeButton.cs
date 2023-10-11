using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class VentureModeButton : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        GetComponent<Button>().onClick.AddListener(() => 
        {
            SaveGameManager.Instance.SaveGame();
            GetComponent<SceneChanger>().ChangeScene();
        });
    }
}
