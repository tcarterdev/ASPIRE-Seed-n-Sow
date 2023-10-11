using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class SaveGameManager : MonoBehaviour
{
    public static SaveGameManager Instance { get; private set; }

    public event EventHandler OnSaveGame;
    public event EventHandler OnLoadGame;

    public bool saveOnExit;

    private void Awake()
    {
        if (Instance != null)
        {
            Debug.LogError($"There's more than one scr_save_game! {transform} - {Instance}");
            Destroy(gameObject);
        }

        Instance = this;
    }

    private void Update()
    {
#if UNITY_EDITOR
        if (Input.GetKeyDown(KeyCode.S)) { SaveGame(); }
#endif
    }

    public void SaveGame() => OnSaveGame?.Invoke(this, EventArgs.Empty);

    public void LoadGame() => OnLoadGame?.Invoke(this, EventArgs.Empty);

    void OnApplicationFocus(bool hasFocus)
    {
#if !UNITY_EDITOR
        if (!hasFocus)
        {
            SaveGame();
        }
#endif
    }
}
