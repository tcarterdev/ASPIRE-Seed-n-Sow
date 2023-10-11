using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Firebase.Auth;

public class DistanceTravel : MonoBehaviour
{
    [SerializeField] private Transform playerTransform;

    [SerializeField] private bool checkingDist = false;
    [SerializeField] private float totalDistTraveledInMeters;
    [SerializeField] private float totalDistTraveledInKm;

    [SerializeField] private float moveDistanceThreshold;
    [SerializeField] private float hungerDepletionAmount = 0.05f;

    private HungerManager hungerManager;
    private AccountLevel accountLevel;
    private PlayerManager playerManager;

    Vector3 lastPos;

    private void Start()
    {
        hungerManager = FindObjectOfType<HungerManager>();

        checkingDist = false;

        totalDistTraveledInMeters = 0;
        StartCoroutine(PostStartPauseCoroutine());

        accountLevel = FindObjectOfType<AccountLevel>();
        playerManager = FindObjectOfType<PlayerManager>();
    }

    void FixedUpdate()
    {
        if (!checkingDist) { return; }

        float distFromLastPos = Vector3.Distance(playerTransform.position, lastPos);

        //Debug.Log(distFromLastPos);

        // Deplete hunger if we have moved more than the threshold.
        if (distFromLastPos != 0 && distFromLastPos > moveDistanceThreshold)
        {
            hungerManager.DepleteHunger(hungerDepletionAmount - playerManager.GetPlayerData().slowerHungerDepletion);

            totalDistTraveledInMeters += distFromLastPos;

            //Debug.Log($"Adding: {Mathf.RoundToInt(distFromLastPos * xpOnWalkMultiplier)}XP");
            accountLevel.AddXP(Mathf.RoundToInt(distFromLastPos * (float)XPValues.WALK));

            // Invoke quest event.
            QuestProgress.Instance.InvokeDistanceTravelled(distFromLastPos);
        }
                                                 
        totalDistTraveledInKm = Mathf.Clamp(totalDistTraveledInMeters / 1000f, 0, Mathf.Infinity);
        lastPos = playerTransform.position;
    }

    IEnumerator PostStartPauseCoroutine()
    {
        yield return new WaitForSeconds(1f);
        totalDistTraveledInMeters = 0f;
        checkingDist = true;
        lastPos = playerTransform.position;
    }

    public float GetDistanceTravelled() => totalDistTraveledInMeters;

    public void SaveDistanceTravelled()
    {
        AuthManager.Instance.dbReference.Child("users").
            Child(FirebaseAuth.DefaultInstance.CurrentUser.UserId).
            Child("properties").Child("distanceTravelledInMeters").SetValueAsync(totalDistTraveledInMeters);
    }
}
