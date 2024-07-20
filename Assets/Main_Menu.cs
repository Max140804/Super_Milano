using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Firebase.Auth;
using Firebase.Extensions;
using System.Net;

public class Main_Menu : MonoBehaviourPunCallbacks
{
    // Main Menu UI elements
    [Header("Main Menu UI")]
    public Text statusText;
    public Toggle acceptTermsToggle;
    public Toggle approveAgeToggle;
    public GameObject loginPage;
    public GameObject loginEmailPage;
    public GameObject loadingScreen;
    public GameObject loginPanel;
    public GameObject mainMenuPanel;

    // Registration fields
    [Header("Registration Fields")]
    public InputField regEmailInputField;
    public InputField regPasswordInputField;
    public InputField regRepeatPasswordInputField;
    public InputField regUsernameInputField;

    // Login fields
    [Header("Login Fields")]
    public InputField loginEmailInputField;
    public InputField loginPasswordInputField;

    // Verification panel
    [Header("Verification Panel")]
    public Text verificationText;


    // Photon variables
    private int gameNumber;
    private int loadedPhoton;


    // Firebase
    private FirebaseAuth auth;
    private int loadedFirebase;

    [Header("Private Game Fields")]
    public InputField _roomName;

    // Start is called before the first frame update
    void Start()
    {
        auth = FirebaseAuth.DefaultInstance;
        auth.SignOut();

        if (auth.CurrentUser != null)
        {
            CheckEmailVerification();
        }
    }

    public void LogoutUser()
    {
        auth.SignOut();
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public async void RegisterUser()
    {
        string email = regEmailInputField.text;
        string password = regPasswordInputField.text;
        string repeatPassword = regRepeatPasswordInputField.text;
        string username = regUsernameInputField.text;

        if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password) || string.IsNullOrEmpty(repeatPassword) || string.IsNullOrEmpty(username))
        {
            statusText.text = "Please fill all the fields.";
            return;
        }

        if (password != repeatPassword)
        {
            statusText.text = "Passwords do not match.";
            return;
        }

        try
        {
            FirebaseUser user = (await auth.CreateUserWithEmailAndPasswordAsync(email, password)).User;

            UserProfile profile = new UserProfile { DisplayName = username };
            await user.UpdateUserProfileAsync(profile);

            Debug.Log("User registration successful.");
            statusText.text = "Registration successful. Please check your email for verification.";
            SendEmailVerification();
        }
        catch (System.Exception ex)
        {
            Debug.LogError("User registration failed: " + ex.Message);
            statusText.text = "Registration failed: " + ex.Message;
        }
    }

    public async void LoginUser()
    {
        string email = loginEmailInputField.text;
        string password = loginPasswordInputField.text;

        if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
        {
            statusText.text = "Please enter email and password.";
            return;
        }

        try
        {
            FirebaseUser user = (await auth.SignInWithEmailAndPasswordAsync(email, password)).User;
            CheckEmailVerification();
        }
        catch (System.Exception ex)
        {
            Debug.LogError("User login failed: " + ex.Message);
            statusText.text = "Login failed: " + ex.Message;
        }
    }

    private async void SendEmailVerification()
    {
        FirebaseUser user = auth.CurrentUser;
        try
        {
            await user.SendEmailVerificationAsync();
            Debug.Log("Email verification sent.");
            verificationText.text = "Email verification sent. Please check your email.";
        }
        catch (System.Exception ex)
        {
            Debug.LogError("Failed to send email verification: " + ex.Message);
            verificationText.text = "Failed to send email verification.";
        }
    }

    public async void CheckEmailVerification()
    {
        FirebaseUser user = auth.CurrentUser;

        await user.ReloadAsync();
        if (user.IsEmailVerified)
        {
            Debug.Log("Email is verified.");
            verificationText.text = "Email is verified!";
            LoadMainScene();
        }
        else
        {
            Debug.Log("Email is not verified.");
            verificationText.text = "Email is not verified. Please check your email.";
        }
    }

    private void LoadMainScene()
    {
        mainMenuPanel.SetActive(true);
        loginPanel.SetActive(false);
        loadingScreen.SetActive(false);
    }

    public void PlayGame()
    {
        PhotonNetwork.JoinRandomRoom();
    }

    public override void OnConnectedToMaster()
    {
        loadedPhoton = 1;
        Debug.Log("Connected to Photon Master Server.");
        PhotonNetwork.JoinLobby();
        PhotonNetwork.AutomaticallySyncScene = true;
    }

    public void JoinRandomRoom(int num)
    {
        gameNumber = num;
        JoinRandomRoom((byte)num);
    }

    public void JoinRandomRoom(byte mapCode)
    {
        var customProperties = new ExitGames.Client.Photon.Hashtable
        {
            ["Scene"] = gameNumber,
            ["unoplayer"] = 4
        };

        PhotonNetwork.JoinRandomRoom(customProperties, 4);
    }

    public override void OnCreateRoomFailed(short returnCode, string message)
    {
        Debug.LogError("Room Creation Failed: " + message);
    }

    public void LeaveRoom()
    {
        PhotonNetwork.LeaveRoom();
    }

    public void StartGame()
    {
        PhotonNetwork.LoadLevel(1); // Consider using a named scene
    }

    public override void OnJoinRandomFailed(short returnCode, string message)
    {
        CreateAndJoinRoom();
    }

    public override void OnJoinRoomFailed(short returnCode, string message)
    {
        CreateAndJoinRoom();
    }

    private void CreateAndJoinRoom()
    {
        var customProperties = new ExitGames.Client.Photon.Hashtable
        {
            ["Scene"] = gameNumber,
            ["unoplayer"] = 2
        };

        var roomOptions = new RoomOptions
        {
            CustomRoomProperties = customProperties,
            IsVisible = true,
            IsOpen = true,
            MaxPlayers = 2,
            CleanupCacheOnLeave = false
        };

        string roomName = UnityEngine.Random.Range(0, 100000).ToString();

        roomOptions.CustomRoomPropertiesForLobby = new[] { "Scene", "unoplayer" };

        PhotonNetwork.CreateRoom(roomName, roomOptions);
    }

    public void CreateAndJoinPrivateRoom()
    {
        //string roomName = UnityEngine.Random.Range(0, 100000).ToString();
        string roomName = _roomName.text;

        var roomOptions = new RoomOptions
        {
            MaxPlayers = 1,
            IsVisible = false
        };

        PhotonNetwork.CreateRoom(roomName, roomOptions);
    }

    public void JoinPrivateRoom()
    {
        PhotonNetwork.JoinRoom("join");
        PhotonNetwork.JoinRoom(_roomName.text);
    }

    public override void OnJoinedRoom()
    {
        //PhotonNetwork.LoadLevel(gameNumber);
        Debug.Log("Joined room successfully. Loading game scene.");
    }

    public override void OnJoinedLobby()
    {
        Debug.Log("Joined lobby.");
    }

    public void EmailLogin()
    {
        if (acceptTermsToggle.isOn && approveAgeToggle.isOn)
        {
            loginPage.SetActive(false);
            loginEmailPage.SetActive(true);
        }
    }

    private void Update()
    {
        statusText.text = $"{loadedPhoton}{loadedFirebase}";
        if (loadedPhoton == 1 && loadedFirebase == 0)
        {
            loadingScreen.SetActive(false);
            loginPanel.SetActive(true);
            loadedPhoton = 2;
        }
        if (loadedPhoton == 1 && loadedFirebase == 1)
        {
            loadingScreen.SetActive(false);
            loginPanel.SetActive(false);
            mainMenuPanel.SetActive(true);
            loadedPhoton = 2;
            loadedFirebase = 2;
        }
        if (loadedFirebase == 1)
        {
            loadingScreen.SetActive(false);
            loginPanel.SetActive(false);
            mainMenuPanel.SetActive(true);
            loadedPhoton = 2;
            loadedFirebase = 2;
        }
    }
}
