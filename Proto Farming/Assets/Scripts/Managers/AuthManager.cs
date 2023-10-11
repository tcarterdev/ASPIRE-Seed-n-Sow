using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Firebase;
using Firebase.Auth;
using Firebase.Database;
using TMPro;
using Firebase.Analytics;
using UnityEngine.Networking;
using UnityEngine.Android;
using System.Net.Mail;
using System.Threading.Tasks;
using Firebase.Extensions;

public class AuthManager : MonoBehaviour
{
    public static AuthManager Instance { get; private set; }

    public event EventHandler OnFirebaseSetup;
    public event EventHandler OnAuthStateChanged;

    [Header("Firebase")]
    [SerializeField] private DependencyStatus dependencyStatus;
    [SerializeField] private FirebaseAuth auth;
    public FirebaseUser user;

    // Login variables
    [Header("Login")]
    [SerializeField] private TMP_InputField emailLoginField;
    [SerializeField] private TMP_InputField passwordLoginField;
    [SerializeField] private TMP_Text warningLoginText;
    [SerializeField] private TMP_Text confirmLoginText;

    // Register Variables
    [Header("Register")]
    [SerializeField] private TMP_InputField usernameRegisterField;
    [SerializeField] private TMP_InputField emailRegisterField;
    [SerializeField] private TMP_InputField passwordRegisterField;
    [SerializeField] private TMP_InputField passwordReigsterVerifyField;
    [SerializeField] private TMP_Text warningRegisterText;
    [SerializeField] private TMP_Text confirmRegisterText;

    // Reset Password Variables
    [Header("Reset Password")]
    [SerializeField] private TMP_InputField resetPassEmailField;
    [SerializeField] private TextMeshProUGUI warningResetPassText;
    [SerializeField] private TextMeshProUGUI confirmResetPassText;

    private GameObject loginCanvas;
    private GameObject existingLoginUI;
    private TextMeshProUGUI existingLoginText;

    public DatabaseReference dbReference;

    // REMOVE
    [SerializeField] private GameObject registerUI;
    [SerializeField] private GameObject loginScreen;

    private SceneHandler sceneHandler;

    private bool authFlag = false;

    private string country;

    private void Awake()
    {
        if (LoadingScreen.Instance != null && SceneHandler.Instance.GetActiveSceneIndex() == (int)SceneIndexes.TITLE_SCREEN)
        {
            LoadingScreen.Instance.EnableLoadingScreen(false, 0.5f);
        }

        if (Instance != null)
        {
            Debug.LogError($"There's more than one AuthManager! {transform} - {Instance}");
            Destroy(gameObject);
        }

        Instance = this;

        sceneHandler = FindObjectOfType<SceneHandler>();
        loginCanvas = transform.parent.Find("Login Canvas").gameObject;

        existingLoginUI = loginCanvas.transform.Find("Existing Login UI").gameObject;
        existingLoginText = existingLoginUI.transform.Find("ExistingLoginText").GetComponent<TextMeshProUGUI>();

        // Check that all of the necessary dependencies for Firebas are present on the system
        FirebaseApp.CheckAndFixDependenciesAsync().ContinueWithOnMainThread(task =>
        {
            dependencyStatus = task.Result;
            if (dependencyStatus == DependencyStatus.Available)
            {
                // All dependencies were resolved, now setup Firebase Auth.
                InitialiseFirebase();
                //FirebaseAnalytics.SetAnalyticsCollectionEnabled(true);
            }
            else
            {
                Debug.LogError($"Could not resolve all Firebase dependencies: {dependencyStatus}");
            }
        });

    }

    /// <summary>
    /// Sets up Firebase Auth.
    /// </summary>
    private void InitialiseFirebase()
    {
        Debug.Log("Setting up Firebase Auth");

        // Set the authentication instance object
        auth = FirebaseAuth.DefaultInstance;
        dbReference = FirebaseDatabase.DefaultInstance.RootReference;

        auth.StateChanged += AuthStateChanged;
        OnFirebaseSetup?.Invoke(this, EventArgs.Empty);

        AuthStateChanged(this, EventArgs.Empty);
    }

    /// <summary>
    /// Track state changes of the auth object. Detects if the client has signed in or out and updates
    /// subscribed events accordingly. 
    /// </summary>
    /// <param name="sender">The object that called the function.</param>
    /// <param name="eventArgs">The argument to pass through. null or EventArgs.Empty is objects that contain no data.</param>
    private void AuthStateChanged(object sender, System.EventArgs eventArgs)
    {
        Debug.Log("AuthStateChanged called");

        if (auth.CurrentUser != null)
        {
            Debug.Log($"AUTH ID {auth.CurrentUser.UserId}");
        }

        if (auth.CurrentUser != user && auth.CurrentUser != null)
        {
            bool signedIn = user != auth.CurrentUser && auth.CurrentUser != null;

            if (!signedIn && user != null)
            {
                Debug.Log($"Signed out {user.UserId}");
            }

            user = auth.CurrentUser;
            if (signedIn)
            {
                Debug.Log($"Signed in {user.UserId}");
            }
        }

        if (auth.CurrentUser != null)
        {
            loginScreen.SetActive(false);

            if (SceneHandler.Instance.GetActiveSceneIndex() == (int)SceneIndexes.TITLE_SCREEN)
            {
                if (!registerUI.activeInHierarchy)
                {
                    // Show existing login screen. 
                    existingLoginUI.SetActive(true);

                    // Update existing login screen.
                    existingLoginText.text = $"Logged in as: {auth.CurrentUser.DisplayName}";
                }
            }
        }

        if (!authFlag)
        {
            authFlag = true;

            Debug.Log($"AuthStateChanged, CurrentUser: {user.UserId}");

            if (DataGathering.dataGathering.dataJson.UserID == string.Empty)
            {
                GetUserLocation();
                DataGathering.dataGathering.CompileBeginingOfSession(auth.CurrentUser.UserId, country);
            }

            OnAuthStateChanged?.Invoke(this, EventArgs.Empty);
        }

    }

    private void OnDestroy()
    {
        // Unsubcribe from AuthStateChanged when destroyed. 
        auth.StateChanged -= AuthStateChanged;
        auth = null;
    }

    private void GetUserLocation()
    {
        // Check if user has given permission. 
        if (!Input.location.isEnabledByUser)
        {
            Debug.LogWarning("No permission for location.");

            // Request for permission.
            Permission.RequestUserPermission(Permission.FineLocation);
        }
        else
        {
            // Permission was granted.
            Debug.Log("Permission Granted");
            StartCoroutine(GetLatLonUsingGPS());
        }
    }

    private IEnumerator GetLatLonUsingGPS()
    {
        Input.location.Start();
        int maxWait = 5;

        while (Input.location.status == LocationServiceStatus.Initializing && maxWait > 0)
        {
            yield return new WaitForSeconds(1);
            maxWait--;
        }

        Debug.LogWarning("Waiting before getting lat and lon");

        // Access grant and location value could be retrieved. 
        float longitude = Input.location.lastData.longitude;
        float latitude = Input.location.lastData.latitude;

        ReverseGeocoder rg = FindObjectOfType<ReverseGeocoder>();
        country = rg.ReverseGeocodeCountry(longitude, latitude);

        Debug.Log(country);

        // Stop retrieving location.
        Input.location.Stop();
        StopCoroutine("Start");
    }

    public void ResetPassword()
    {
        if (user != null)
        {
            auth.SendPasswordResetEmailAsync(resetPassEmailField.text).ContinueWithOnMainThread(task => {
                if (task.IsCanceled)
                {
                    Debug.LogError("SendPasswordResetEmailAsync was canceled.");
                    warningResetPassText.text = "Password reset email was cancelled. Please try again later.";
                    return;
                }
                if (task.IsFaulted)
                {
                    Debug.LogError("SendPasswordResetEmailAsync encountered an error: " + task.Exception);
                    warningResetPassText.text = "Password reset email failed. Please try again later.";
                    return;
                }

                Debug.Log("Password reset email sent successfully.");
                confirmResetPassText.text = "Email to reset password has been sent successfully.\n\nIf some time has passed and the email has not arrived in your inbox, please check your spam/junk folder.";

                // Set field to empty. 
                resetPassEmailField.text = string.Empty;
            });
        }
    }

    #region Login Methods

    /// <summary>
    /// Function for the login button. 
    /// 
    /// Calls the login function, passing in the details provided: email and password.
    /// </summary>
    public void LoginButton()
    {
        // Call the lgin coroutine, passing the email and password.
        StartCoroutine(Login(emailLoginField.text, passwordLoginField.text));
    }

    /// <summary>
    /// Logs the player in with the provided details matched against Firebase Authentication.
    /// 
    /// This function checks for registration errors, such as: missing email or password, 
    /// invalid email, user not found, etc. 
    /// </summary>
    /// <param name="email">Client's registered email.</param>
    /// <param name="password">Client's registered password.</param>
    /// <returns></returns>
    private IEnumerator Login(string email, string password)
    {
        // Call the Firebase Auth signin function, passing the email and password. 
        var loginTask = auth.SignInWithEmailAndPasswordAsync(email, password);

        // Wait until task completes. 
        yield return new WaitUntil(predicate: () => loginTask.IsCompleted);

        if (loginTask.Exception != null)
        {
            // If there are errors handle them
            Debug.LogWarning(message: $"Failed to register task with {loginTask.Exception}");
            FirebaseException firebaseException = loginTask.Exception.GetBaseException() as FirebaseException;
            AuthError errorCode = (AuthError)firebaseException.ErrorCode;

            string message = "Login failed!";
            switch (errorCode)
            {
                case AuthError.MissingEmail:
                    message = "Missing Email";
                    break;
                case AuthError.MissingPassword:
                    message = "Missing Password";
                    break;
                case AuthError.WrongPassword:
                    message = "Wrong Password";
                    break;
                case AuthError.InvalidEmail:
                    message = "Invalid Email";
                    break;
                case AuthError.UserNotFound:
                    message = "User not found";
                    break;
            }

            // Update warning text based on error.
            warningLoginText.text = message;
        }
        else
        {
            // User is now logged in
            // Now get the result
            user = loginTask.Result;
            Debug.LogFormat("User signed in successfully: {0} ({1})", user.DisplayName, user.Email);
            warningLoginText.text = "";
            confirmLoginText.text = "Logged In";

            // Set the UID for Firebase analytics.
            DataGathering.dataGathering.SetUserID(auth.CurrentUser.UserId);

            loginScreen.SetActive(false);

            // Show the loading screen. 
            LoadingScreen.Instance.EnableLoadingScreen(true, 0.5f);

            // Load the next level. 
            sceneHandler.LoadLevel();
        }
    }

    public void AlreadyLoggedIn()
    {
        existingLoginUI.SetActive(false);

        if (GameObject.Find("LoadingScreen"))
        {
            // Show the loading screen. 
            LoadingScreen.Instance.EnableLoadingScreen(true, 0.5f);
        }

        // Load the next level. 
        sceneHandler.LoadLevel();
    }

    #endregion

    #region Register Methods

    /// <summary>
    /// Function for the register button. 
    /// 
    /// Calls the register function, passing in the details provided: email, password, and username.
    /// </summary>
    public void RegisterButton()
    {
        // Call the register coroutine passing the email, password, and username. 
        StartCoroutine(Register(emailRegisterField.text, passwordRegisterField.text, usernameRegisterField.text));
    }

    /// <summary>
    /// Registers the player to Firebase Authentication with the provided details. 
    /// 
    /// This function checks for registration errors, such as: missing email or password, 
    /// email already in use, etc. 
    /// </summary>
    /// <param name="email">Client's provided email.</param>
    /// <param name="password">Client's provided password.</param>
    /// <param name="username">Client's provided username.</param>
    /// <returns></returns>
    private IEnumerator Register(string email, string password, string username)
    {
        if (username == "")
        {
            // If the username field is blank, show a warning.
            warningRegisterText.text = "Missing Username";
        }
        else if (passwordRegisterField.text != passwordReigsterVerifyField.text)
        {
            // If the passwords don't match, show a warning. 
            warningRegisterText.text = "Password Does Not Match!";
        }
        else
        {
            // Call the Firebase Auth register function, passing the email and password.
            var registerTask = auth.CreateUserWithEmailAndPasswordAsync(email, password);

            // Wait until the task completes.
            yield return new WaitUntil(predicate: () => registerTask.IsCompleted);

            if (registerTask.Exception != null)
            {
                // If there are errors, handle them.
                Debug.LogWarning(message: $"Failed to register task with {registerTask.Exception}");
                FirebaseException firebaseException = registerTask.Exception.GetBaseException() as FirebaseException;
                AuthError errorCode = (AuthError)firebaseException.ErrorCode;

                string message = "Register Failed!";
                switch (errorCode)
                {
                    case AuthError.MissingEmail:
                        message = "Missing Email";
                        break;
                    case AuthError.MissingPassword:
                        message = "Missing Password";
                        break;
                    case AuthError.WeakPassword:
                        message = "Weak Password";
                        break;
                    case AuthError.EmailAlreadyInUse:
                        message = "Email Already In Use";
                        break;
                }

                // Update the warning text in the register UI with the error message.
                warningRegisterText.text = message;
            }
            else
            {
                // User has now been created, get the result.
                user = registerTask.Result;

                if (user != null)
                {
                    // Create a user profile and set the username.
                    UserProfile profile = new UserProfile { DisplayName = username };

                    // Call the Firebase Auth update user profile function, passing the profile and username. 
                    var profileTask = user.UpdateUserProfileAsync(profile);

                    // Wait until the task completes. 
                    yield return new WaitUntil(predicate: () => profileTask.IsCompleted);

                    if (profileTask.Exception != null)
                    {
                        //If there are errors handle them
                        Debug.LogWarning(message: $"Failed to register task with {profileTask.Exception}");
                        FirebaseException firebaseEx = profileTask.Exception.GetBaseException() as FirebaseException;
                        AuthError errorCode = (AuthError)firebaseEx.ErrorCode;
                        warningRegisterText.text = "Username Set Failed!";
                    }
                    else
                    {
                        //Username is now set
                        confirmRegisterText.text = "Successfully Registered User.\n Returning to Login screen...";
                        yield return new WaitForSeconds(1f);
                        registerUI.SetActive(false);
                        loginScreen.SetActive(true);

                        // Remove message from warning and confirm texts. 
                        warningRegisterText.text = "";
                        confirmRegisterText.text = "";

                        // Set fields to empty.
                        usernameRegisterField.text = string.Empty;
                        emailRegisterField.text = string.Empty;
                        passwordRegisterField.text = string.Empty;
                        passwordReigsterVerifyField.text = string.Empty;
                    }
                }
            }
        }
    }

    #endregion

    public bool GetAuthFlag() => authFlag;
}
