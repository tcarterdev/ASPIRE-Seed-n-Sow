using Firebase.Database;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public abstract class BaseSaveManager : MonoBehaviour
{
    protected virtual void Start()
    {
        //SaveGameManager.Instance.OnLoadGame += SaveGameManager_OnLoadGame;

        //SaveGameManager_OnLoadGame(null, null);
    }

    private void SaveGameManager_OnLoadGame(object sender, EventArgs e)
    {
        //StartCoroutine(LoadFromDatabase());
    }

    /// <summary>
    /// Checks for exceptions with the task. 
    /// </summary>
    /// <param name="task">The current task being performed.</param>
    public virtual void CheckForTaskException(Task<DataSnapshot> task)
    {
        if (task.Exception != null)
        {
            Debug.LogWarning(message: $"Failed to register task with {task.Exception}");
        }
        else if (task.Result.Value == null)
        {
            Debug.LogWarning(message: $"No data exists yet, defaulting values");

            // If the data result is null.
            TaskDataIsNull();
        }
        else
        {
            // If the data has been retrieved.
            TaskHasRetrievedData(task);
        }
    }

    public virtual void CheckForTaskException_VentureMode(Task<DataSnapshot> task)
    {
        if (task.Exception != null)
        {
            Debug.LogWarning(message: $"Failed to register task with {task.Exception}");
        }
        else if (task.Result.Value == null)
        {
            Debug.LogWarning(message: $"No data exists yet, defaulting values");

            // If the data result is null.
            TaskDataIsNull();
        }
        else
        {
            //Debug.Log(message: "Data has been retrieved");

            // If the data has been retrieved.
            TaskHasRetrievedData_VentureModeSaveQuests(task);
        }
    }

    /// <summary>
    /// Checks if the database is null or not.
    /// </summary>
    /// <returns>False if it is null, true if it isn't null.</returns>
    public bool CheckIfDatabaseIsNull() => AuthManager.Instance.dbReference == null ? false : true;

    /// <summary>
    /// Calls the children's TaskDataIsNull() function to do their own logic.
    /// </summary>
    protected abstract void TaskDataIsNull();

    /// <summary>
    /// Calls the children's TaskHasRetrieved() function to do their own logic.
    /// </summary>
    /// <param name="task">The current task being performed.</param>
    protected abstract void TaskHasRetrievedData(Task<DataSnapshot> task);

    protected abstract void TaskHasRetrievedData_VentureModeSaveQuests(Task<DataSnapshot> task);

    /// <summary>
    /// Calls the children's LoadFromDatabase() coroutine to do their own logic. 
    /// </summary>
    /// <returns></returns>
    protected abstract IEnumerator LoadFromDatabase();
}
