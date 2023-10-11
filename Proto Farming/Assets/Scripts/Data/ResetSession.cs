using Firebase.Auth;
using Firebase.Database;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class ResetSession : MonoBehaviour
{
    private List<GameObject> quests;

    public static ResetSession Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null)
        {
            Debug.LogError($"There's more than one ResetSession! {transform} - {Instance}");
            Destroy(gameObject);
        }

        Instance = this;
    }

    public void ResetUIDs()
    {
        Debug.Log("Resetting UIDs");
        for (int i = 0; i < 9; i++)
        {
            // Amount
            AuthManager.Instance.dbReference.Child("users").
                Child(FirebaseAuth.DefaultInstance.CurrentUser.UserId).
                Child("farmingMode").Child("quests").Child("Items").
                Child(i.ToString()).Child("sessionId").SetValueAsync(null);
        }
    }

    //private IEnumerator LoadQuestFromDatabase()
    //{
    //    Debug.Log("loading from database");

    //    // Return early if database reference is null. 
    //    if (!CheckIfDatabaseIsNull()) { yield break; }

    //    Task<DataSnapshot> questLoadTask = AuthManager.Instance.dbReference.Child("users").
    //        Child(FirebaseAuth.DefaultInstance.CurrentUser.UserId).Child("farmingMode").Child("quests").Child("Items").GetValueAsync();

    //    yield return new WaitUntil(predicate: () => questLoadTask.IsCompleted);

    //    // Check the task status. 
    //    CheckForTaskException(questLoadTask);
    //}

    ///// <summary>
    ///// Checks if the database is null or not.
    ///// </summary>
    ///// <returns>False if it is null, true if it isn't null.</returns>
    //private bool CheckIfDatabaseIsNull() => AuthManager.Instance.dbReference == null ? false : true;

    ///// <summary>
    ///// Checks for exceptions with the task. 
    ///// </summary>
    ///// <param name="task">The current task being performed.</param>
    //public virtual void CheckForTaskException(Task<DataSnapshot> task)
    //{
    //    if (task.Exception != null)
    //    {
    //        Debug.LogWarning(message: $"Failed to register task with {task.Exception}");
    //    }
    //    else if (task.Result.Value == null)
    //    {
    //        Debug.LogWarning(message: $"No data exists yet, defaulting values");

    //        // If the data result is null.
    //        //TaskDataIsNull();
    //    }
    //    else
    //    {
    //        Debug.Log(message: "Data has been retrieved");

    //        // If the data has been retrieved.
    //        SetUIDsToNull(task);
    //    }
    //}

    //private void SetUIDsToNull(Task<DataSnapshot> task)
    //{
    //    Debug.Log("SetUIDsToNull");

    //    quests = new List<GameObject>();

    //    DataSnapshot snapshot = task.Result;

    //    for (int i = 0; i < snapshot.ChildrenCount; i++)
    //    {
    //        // Amount
    //        AuthManager.Instance.dbReference.Child("users").
    //            Child(FirebaseAuth.DefaultInstance.CurrentUser.UserId).
    //            Child("farmingMode").Child("quests").Child("Items").
    //            Child(i.ToString()).Child("sessionId").SetValueAsync(null);
    //    }
    //}
}
