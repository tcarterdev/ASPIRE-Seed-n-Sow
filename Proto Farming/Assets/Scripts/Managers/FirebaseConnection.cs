using UnityEngine;
using Firebase;
using Firebase.Auth;
using Firebase.Extensions;
using UnityEngine.SceneManagement;

public class FirebaseConnection : MonoBehaviour
{
    private void Start()
    {
        // Show the loading screen. 
        LoadingScreen.Instance.EnableLoadingScreen(true, 0.5f);
        LoadingScreen.Instance.UpdateLoadingInfo("Initialising");

        CheckIfReady();
    }

    public static void CheckIfReady()
    {
        FirebaseApp.CheckAndFixDependenciesAsync().ContinueWithOnMainThread(task =>
        {
            DependencyStatus dependencyStatus = task.Result;
            if (dependencyStatus == Firebase.DependencyStatus.Available)
            {
                FirebaseApp app = FirebaseApp.DefaultInstance;
                SceneManager.LoadScene((int)SceneIndexes.TITLE_SCREEN);
                Debug.Log("Firebase is ready for use.");
            }
            else
            {
                Debug.LogError(System.String.Format("Could not resolve all firebase dependencies: {0}", dependencyStatus));
            }
        });
    }
}
