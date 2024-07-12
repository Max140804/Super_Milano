using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Firebase.Auth;
using Firebase.Extensions;
using Firebase;
using Firebase.Database;
using System.Collections;
using System.Collections.Generic;
using System;
using Google;
using System.Threading.Tasks;
using TMPro;

public class MainMenu : MonoBehaviourPunCallbacks
{
    
    public string usernamee;

    // Main Menu UI elements
    [Header("Main Menu UI")]
    public Toggle acceptTermsToggle;
    public Toggle approveAgeToggle;
    public GameObject loginPage;
    public GameObject loginEmailPage;
    public GameObject loadingScreen;
    public GameObject loginPanel;
    public GameObject mainMenuPanel;
    public GameObject roomGO1v1;
    public GameObject roomGO2v2;
    public TextMeshProUGUI roomGo1v1RoomName;
    public TextMeshProUGUI roomGo2v2RoomName;


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

    [Header("Room Data")]
    public List<PlayerDisplay> playersInRoom = new List<PlayerDisplay>();
    public PlayerDisplay playerPrefab;
    public Transform displayParent1v1;
    public Transform displayParent2v2;
    public GameObject playButton1v1;
    public GameObject playButton2v2;

    // Firebase
    private FirebaseAuth auth;
    private bool isFirebaseLoaded = false;

    // Photon
    private bool isPhotonLoaded = false;
    private int gameNumber;
    private string _roomName;


    public FirebaseApp app;

    public DatabaseReference databaseReference;


    public string playerId;
    public float coins;

    public Text coins_text;
    public Text profile_coins_text;
    public Text username;
    public DailyRewards daily;


    int dfdf;

    public float unobid;
    public float dominobid;

    public Text XP_text;
    public Text XP_text2;

    public float XP;
    public float Level;
    public int gameplayed;
    public int gamewin;
    public int gamelose;
    public Text gameplayed_Text;
    public Text gamewin_Text;
    public Text gamelose_Text;
    public float currentbid;
    public int unoplayers;
    public int players;

    public int privGame;
    public string phoneNumber = "+9647853121381";

    public GameObject errorpanel;
    public Text errorpanel_text;

    public GameObject errorpanel2;
    public Text errorpanel_text2;
    public InputField privateinput;
    public Text usernameText;

    public Toggle AIuno;
    public Toggle AIdomino;

    public UISwitcher.UISwitcher music;
    public UISwitcher.UISwitcher sound;
    public GameObject musicbackground;
    public GameObject soundbackground;

    public InputField sendcoins;
    public InputField sendtowho;

    public InputField resetpassword;

    public bool isai;

    private void Awake()
    {

        if (PlayerPrefs.GetInt("music") == 1)
        {
            music.isOn = false;
        }
        else
        {
            music.isOn = true;

        }
        if (PlayerPrefs.GetInt("sound") == 1)
        {
            sound.isOn = false;
        }
        else
        {
            sound.isOn = true;

        }

        FirebaseApp.CheckAndFixDependenciesAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsCanceled)
            {
                errorpanel2.SetActive(true);
                errorpanel_text2.text = "CheckAndFixDependenciesAsync was canceled.";
            }
            if (task.IsFaulted)
            {
                errorpanel2.SetActive(true);
                errorpanel_text2.text = "CheckAndFixDependenciesAsync encountered an error: " + task.Exception;
            }
            if (task.Result == DependencyStatus.Available)
            {

                InitializeFirebase();
                PlayerPrefs.SetInt("ai", 0);

                PhotonNetwork.ConnectUsingSettings();
                auth = FirebaseAuth.DefaultInstance;

                //LogoutUser();

                if (auth.CurrentUser != null)
                {
                    playerId = auth.CurrentUser.UserId;

                    CheckEmailVerification();


                }



            }
            else
            {
                errorpanel2.SetActive(true);
                errorpanel_text2.text = "Could not resolve all Firebase dependencies: " + task.Result;

            }
        });
    }

    public void GoogleSignIn()
    {
        Credential credential = GoogleAuthProvider.GetCredential("981360111670-hhs97pmq28h6o3jrbnv5ft9l31jh04ar.apps.googleusercontent.com", "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");

        auth.SignInAndRetrieveDataWithCredentialAsync(credential).ContinueWith(task =>
        {
            if (task.IsCanceled)
            {
                Debug.LogError("Google sign-in was canceled.");
                return;
            }
            if (task.IsFaulted)
            {
                Debug.LogError("Google sign-in encountered an error: " + task.Exception);
                return;
            }

            // Successfully signed in
            AuthResult result = task.Result;
            Debug.LogFormat("User signed in successfully: {0} ({1})", result.User.DisplayName, result.User.UserId);
        });
    }

    public void SendPasswordResetEmail()
    {
        StartCoroutine(SendPasswordResetEmailCoroutine(resetpassword.text));
    }

    private IEnumerator SendPasswordResetEmailCoroutine(string emailAddress)
    {
        var passwordResetTask = auth.SendPasswordResetEmailAsync(emailAddress);
        yield return new WaitUntil(() => passwordResetTask.IsCompleted);

        if (passwordResetTask.Exception != null)
        {
            errorpanel2.SetActive(true);
            errorpanel_text2.text = $"Failed to send password reset email: {passwordResetTask.Exception}";
        }
        else
        {
            errorpanel2.SetActive(true);
            errorpanel_text2.text = "Password reset email sent successfully.";
        }
    }


    public void sendcoin()
    {
        StartCoroutine(Senplayercoin(float.Parse(sendcoins.text), sendtowho.text));
    }
    public IEnumerator Senplayercoin(float howmuch, string sendtowhom)
    {
        var task = databaseReference.Child(HelperClass.Encrypt("players", playerId)).Child(HelperClass.Encrypt(sendtowhom, playerId)).Child(HelperClass.Encrypt("coins", playerId)).GetValueAsync();
        yield return new WaitUntil(() => task.IsCompleted);

        if (task.Exception != null)
        {
            Debug.LogError($"Failed to get player coins: {task.Exception}");
        }
        else if (task.Result.Value == null)
        {
            Debug.Log("Player not found or coins not set.");
        }
        else
        {

            float othercoins = float.Parse(HelperClass.Decrypt(task.Result.Value.ToString(), playerId));

            StartCoroutine(UpdateCoinsCoroutine(playerId, coins - howmuch));

            StartCoroutine(UpdateCoinsCoroutine(sendtowhom, othercoins + howmuch));

            Debug.Log($"Player has {coins} coins.");
            // Do something with the retrieved coins, e.g., update UI
        }
    }
    public void setpriv(int privat)
    {
        privGame = privat;
    }
    public void OpenWhatsAppLink()
    {
        string url = "https://wa.me/" + phoneNumber;
        Application.OpenURL(url);
    }
    public void Openprivacypolicy()
    {
        string url = "https://www.freeprivacypolicy.com/live/e74e9d1f-3629-4188-a17f-5cbf942df8ea";
        Application.OpenURL(url);
    }
    public void domino()
    {
        currentbid = dominobid;
    }
    public void uno()
    {
        currentbid = unobid;
    }
    public void setuno_bid(int bid)
    {
        if (bid == 42)
        {
            unobid = 0.05f;
        }
        else
        {
            unobid = bid;
        }
    }
    public void SendMail()
    {
        // Construct the mailto URL with just the recipient's address
        string mailto = string.Format("mailto:{0}", "ibrahimrateb2@gmail.com");

        // Open the default mail client with the constructed URL
        Application.OpenURL(mailto);
    }
    public void copyID()
    {
        TextEditor te = new TextEditor();
        te.text = playerId;
        te.SelectAll();
        te.Copy();
    }

    public void setdomino_bid(int bid)
    {
        if (bid == 42)
        {
            dominobid = 0.05f;
        }
        else
        {
            dominobid = bid;
        }
    }
    public void set_bid(int bid)
    {
        if (bid == 42)
        {
            currentbid = 0.05f;
        }
        else
        {
            currentbid = bid;

        }
    }
    public void setplayersunoplayers()
    {
        players = unoplayers;
    }
    public void setuno_players(int playersss)
    {
        unoplayers = playersss;
    }
    public void setplayers(int playersss)
    {
        players = playersss;
    }

    void Start()
    {



    }
    public void UpdatePlayerCoins(string playerId, float coins)
    {
        StartCoroutine(UpdateCoinsCoroutine(playerId, coins));
    }

    private void InitializeFirebase()
    {

        app = FirebaseApp.DefaultInstance;

        // Ensure that the DatabaseURL is set
        if (string.IsNullOrEmpty(app.Options.DatabaseUrl?.ToString()))
        {
            Uri databaseUrl = new Uri("https://super-milano-default-rtdb.firebaseio.com/");
            app.Options.DatabaseUrl = databaseUrl;
        }


        // Initialize the database reference after Firebase is initialized
        databaseReference = FirebaseDatabase.GetInstance(app).RootReference;

    }

    private IEnumerator UpdateCoinsCoroutine(string playerId, float coins)
    {
        var task = databaseReference.Child(HelperClass.Encrypt("players", playerId)).Child(HelperClass.Encrypt(playerId, playerId)).Child(HelperClass.Encrypt("coins", playerId)).SetValueAsync(HelperClass.Encrypt(coins.ToString(), playerId));
        yield return new WaitUntil(() => task.IsCompleted);

        if (task.Exception != null)
        {
            Debug.LogError($"Failed to update coins: {task.Exception}");
        }
        else
        {
            Debug.Log("Coins updated successfully");
        }
    }

    private IEnumerator Updatexp(string playerId, float xp)
    {
        var task = databaseReference.Child(HelperClass.Encrypt("players", playerId)).Child(HelperClass.Encrypt(playerId, playerId)).Child(HelperClass.Encrypt("XP", playerId)).SetValueAsync(HelperClass.Encrypt(xp.ToString(), playerId));
        yield return new WaitUntil(() => task.IsCompleted);

        if (task.Exception != null)
        {
            Debug.LogError($"Failed to update coins: {task.Exception}");
        }
        else
        {
            Debug.Log("Coins updated successfully");
        }
    }

    public IEnumerator Getxp(string playerId)
    {
        var task = databaseReference.Child(HelperClass.Encrypt("players", playerId)).Child(HelperClass.Encrypt(playerId, playerId)).Child(HelperClass.Encrypt("XP", playerId)).GetValueAsync();
        yield return new WaitUntil(() => task.IsCompleted);

        if (task.Exception != null)
        {
            Debug.LogError($"Failed to get player coins: {task.Exception}");
        }
        else if (task.Result.Value == null)
        {
            Debug.Log("Player not found or coins not set.");
        }
        else
        {
            XP = float.Parse(HelperClass.Decrypt(task.Result.Value.ToString(), playerId));
            // Do something with the retrieved coins, e.g., update UI
        }
    }
    public void UpdateStatics(string playerId, float played, float win, float lose)
    {
        StartCoroutine(Updateplayed(playerId, played));
        StartCoroutine(Updatewin(playerId, win));
        StartCoroutine(Updatelose(playerId, lose));

    }
    private IEnumerator Updateplayed(string playerId, float played)
    {
        var task = databaseReference.Child(HelperClass.Encrypt("players", playerId)).Child(HelperClass.Encrypt(playerId.ToString(), playerId)).Child(HelperClass.Encrypt("played", playerId)).SetValueAsync(HelperClass.Encrypt(played.ToString(), playerId));
        yield return new WaitUntil(() => task.IsCompleted);
    }
    private IEnumerator Updatewin(string playerId, float win)
    {
        var task = databaseReference.Child(HelperClass.Encrypt("players", playerId)).Child(HelperClass.Encrypt(playerId.ToString(), playerId)).Child(HelperClass.Encrypt("win", playerId)).SetValueAsync(HelperClass.Encrypt(win.ToString(), playerId));
        yield return new WaitUntil(() => task.IsCompleted);

    }
    private IEnumerator UpdateEmail(string playerId)
    {
        var task = databaseReference.Child(HelperClass.Encrypt("players", playerId)).Child(HelperClass.Encrypt(playerId.ToString(), playerId)).Child(HelperClass.Encrypt("email", playerId)).SetValueAsync(HelperClass.Encrypt(auth.CurrentUser.Email, playerId));
        yield return new WaitUntil(() => task.IsCompleted);

    }
    private IEnumerator Updateusername(string playerId)
    {
        var task = databaseReference.Child(HelperClass.Encrypt("players", playerId)).Child(HelperClass.Encrypt(playerId.ToString(), playerId)).Child(HelperClass.Encrypt("username", playerId)).SetValueAsync(HelperClass.Encrypt(auth.CurrentUser.DisplayName, playerId));
        yield return new WaitUntil(() => task.IsCompleted);

    }
    private IEnumerator Updatelose(string playerId, float lose)
    {
        var task = databaseReference.Child(HelperClass.Encrypt("players", playerId)).Child(HelperClass.Encrypt(playerId.ToString(), playerId)).Child(HelperClass.Encrypt("lose", playerId)).SetValueAsync(HelperClass.Encrypt(lose.ToString(), playerId));
        yield return new WaitUntil(() => task.IsCompleted);
    }
    public IEnumerator Getstatics(string playerId)
    {
        var task = databaseReference.Child(HelperClass.Encrypt("players", playerId)).Child(HelperClass.Encrypt(playerId, playerId)).Child(HelperClass.Encrypt("played", playerId)).GetValueAsync();
        yield return new WaitUntil(() => task.IsCompleted);
        var task2 = databaseReference.Child(HelperClass.Encrypt("players", playerId)).Child(HelperClass.Encrypt(playerId, playerId)).Child(HelperClass.Encrypt("win", playerId)).GetValueAsync();
        yield return new WaitUntil(() => task2.IsCompleted);
        var task3 = databaseReference.Child(HelperClass.Encrypt("players", playerId)).Child(HelperClass.Encrypt(playerId, playerId)).Child(HelperClass.Encrypt("lose", playerId)).GetValueAsync();
        yield return new WaitUntil(() => task3.IsCompleted);

        if (task.Exception != null && task2.Exception != null && task3.Exception != null)
        {
            Debug.LogError($"Failed to get player coins: {task.Exception}");
        }
        else if (task.Result.Value == null && task2.Result.Value == null && task3.Result.Value == null)
        {
            Debug.Log("Player not found or coins not set.");
        }
        else
        {
            gameplayed = int.Parse(HelperClass.Decrypt(task.Result.Value.ToString(), playerId));
            gamewin = int.Parse(HelperClass.Decrypt(task2.Result.Value.ToString(), playerId));
            gamelose = int.Parse(HelperClass.Decrypt(task3.Result.Value.ToString(), playerId));
            // Do something with the retrieved coins, e.g., update UI
        }
    }
    public IEnumerator UpdateTime(string playerId, string time, int streak)
    {
        var task = databaseReference.Child(HelperClass.Encrypt("players", playerId)).Child(HelperClass.Encrypt(playerId, playerId)).Child(HelperClass.Encrypt("LastReward", playerId)).SetValueAsync(HelperClass.Encrypt(time, playerId));
        yield return new WaitUntil(() => task.IsCompleted);
        var task2 = databaseReference.Child(HelperClass.Encrypt("players", playerId)).Child(HelperClass.Encrypt(playerId, playerId)).Child(HelperClass.Encrypt("DailyStreak", playerId)).SetValueAsync(HelperClass.Encrypt(streak.ToString(), playerId));
        yield return new WaitUntil(() => task2.IsCompleted);

        if (task.Exception != null)
        {
            Debug.LogError($"Failed to update coins: {task.Exception}");
        }
        else
        {
            Debug.Log("Coins updated successfully");
        }


        if (task2.Exception != null)
        {
            Debug.LogError($"Failed to update coins: {task.Exception}");
        }
        else
        {
            Debug.Log("Coins updated successfully");
        }
    }
    public IEnumerator GetTime(string playerId)
    {
        var task = databaseReference.Child(HelperClass.Encrypt("players", playerId)).Child(HelperClass.Encrypt(playerId, playerId)).Child(HelperClass.Encrypt("LastReward", playerId)).GetValueAsync();
        yield return new WaitUntil(() => task.IsCompleted);
        var task2 = databaseReference.Child(HelperClass.Encrypt("players", playerId)).Child(HelperClass.Encrypt(playerId, playerId)).Child(HelperClass.Encrypt("DailyStreak", playerId)).GetValueAsync();
        yield return new WaitUntil(() => task2.IsCompleted);
        if (task.Exception != null)
        {
            Debug.LogError($"Failed to get player coins: {task.Exception}");
        }
        else if (task.Result.Value == null)
        {
            Debug.Log("Player not found or coins not set.");
        }
        else
        {
            long temp = Convert.ToInt64(HelperClass.Decrypt(task.Result.Value.ToString(), playerId));
            daily.lastRewardTime = DateTime.FromBinary(temp);
            daily.currentStreak = PlayerPrefs.GetInt(HelperClass.Decrypt(task2.Result.Value.ToString(), playerId));



        }
    }
    private IEnumerator UpdateLevelCoroutine(string playerId, int level)
    {
        var task = databaseReference.Child(HelperClass.Encrypt("players", playerId)).Child(HelperClass.Encrypt(playerId, playerId)).Child(HelperClass.Encrypt("level", playerId)).SetValueAsync(HelperClass.Encrypt(level.ToString(), playerId));
        yield return new WaitUntil(() => task.IsCompleted);

        if (task.Exception != null)
        {
            Debug.LogError($"Failed to update coins: {task.Exception}");
        }
        else
        {
            Debug.Log("Coins updated successfully");
        }
    }
    public IEnumerator GetPlayerLevel(string playerId)
    {
        var task = databaseReference.Child(HelperClass.Encrypt("players", playerId)).Child(HelperClass.Encrypt(playerId, playerId)).Child(HelperClass.Encrypt("coins", playerId)).GetValueAsync();
        yield return new WaitUntil(() => task.IsCompleted);

        if (task.Exception != null)
        {
            Debug.LogError($"Failed to get player coins: {task.Exception}");
        }
        else if (task.Result.Value == null)
        {
            Debug.Log("Player not found or coins not set.");
        }
        else
        {
            Level = float.Parse(HelperClass.Decrypt(task.Result.Value.ToString(), playerId));
            Debug.Log($"Player has {coins} coins.");
            // Do something with the retrieved coins, e.g., update UI
        }
    }
    public IEnumerator GetPlayerCoins(string playerId)
    {
        var task = databaseReference.Child(HelperClass.Encrypt("players", playerId)).Child(HelperClass.Encrypt(playerId, playerId)).Child(HelperClass.Encrypt("coins", playerId)).GetValueAsync();
        yield return new WaitUntil(() => task.IsCompleted);

        if (task.Exception != null)
        {
            Debug.LogError($"Failed to get player coins: {task.Exception}");
        }
        else if (task.Result.Value == null)
        {
            Debug.Log("Player not found or coins not set.");
        }
        else
        {
            coins = float.Parse(HelperClass.Decrypt(task.Result.Value.ToString(), playerId));
            Debug.Log($"Player has {coins} coins.");
            // Do something with the retrieved coins, e.g., update UI
        }
    }
    public void GetUsername(string playerId)
    {
        FirebaseUser user = auth.CurrentUser;
        if (user != null)
        {
            usernamee = user.DisplayName;
            usernameText.text = usernamee;
            if (PhotonNetwork.LocalPlayer != null)
            {
                PhotonNetwork.LocalPlayer.NickName = usernamee;
                Debug.Log("Username: " + usernamee);
            }
            else
            {
                return;
            }
            Debug.Log("Username: " + username);
        }
        else
        {
            Debug.Log("No user is signed in.");
        }


    }
    public void LogoutUser()
    {
        auth.SignOut();
        //SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
    public async void RegisterUser()
    {
        string email = regEmailInputField.text;
        string password = regPasswordInputField.text;
        string repeatPassword = regRepeatPasswordInputField.text;
        string username = regUsernameInputField.text;

        if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password) || string.IsNullOrEmpty(repeatPassword) || string.IsNullOrEmpty(username))
        {
            errorpanel2.SetActive(true);
            errorpanel_text2.text = "Please fill all the fields.";
            return;
        }

        if (password != repeatPassword)
        {
            errorpanel2.SetActive(true);
            errorpanel_text2.text = "Passwords do not match.";
            return;
        }

        try
        {
            FirebaseUser user = (await auth.CreateUserWithEmailAndPasswordAsync(email, password)).User;

            UserProfile profile = new UserProfile { DisplayName = username };
            await user.UpdateUserProfileAsync(profile);

            errorpanel2.SetActive(true);
            errorpanel_text2.text = "Registration successful Please check your email for verification.";
            await SendEmailVerification();
        }
        catch (System.Exception ex)
        {
            errorpanel2.SetActive(true);
            errorpanel_text2.text = "Registration failed: " + ex.Message;
        }
    }

    public async void LoginUser()
    {
        string email = loginEmailInputField.text;
        string password = loginPasswordInputField.text;

        if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
        {
            errorpanel2.SetActive(true);
            errorpanel_text2.text = "Please enter email and password.";
            return;
        }

        try
        {
            FirebaseUser user = (await auth.SignInWithEmailAndPasswordAsync(email, password)).User;
            await CheckEmailVerification();
        }
        catch (System.Exception ex)
        {
            errorpanel2.SetActive(true);
            errorpanel_text2.text = "Login failed: " + ex.Message;
        }
    }

    private async System.Threading.Tasks.Task SendEmailVerification()
    {
        FirebaseUser user = auth.CurrentUser;
        try
        {
            await user.SendEmailVerificationAsync();
            errorpanel2.SetActive(true);
            errorpanel_text2.text = "Email verification sent Please check your email.";
        }
        catch (System.Exception ex)
        {
            errorpanel2.SetActive(true);
            errorpanel_text2.text = "Failed to send email verification.";
        }
    }

    private async System.Threading.Tasks.Task CheckEmailVerification()
    {
        FirebaseUser user = auth.CurrentUser;
        await user.ReloadAsync();

        if (user.IsEmailVerified)
        {
            isFirebaseLoaded = true;
            LoadMainScene();
        }
        else
        {
            await SendEmailVerification();

            errorpanel2.SetActive(true);
            errorpanel_text2.text = "Email is not verified Please check your email.";

        }
    }

    public void LoadMainScene()
    {

        playerId = auth.CurrentUser.UserId;
        StartCoroutine(GetPlayerCoins(playerId));
        GetUsername(playerId);
        StartCoroutine(Getxp(playerId));
        StartCoroutine(Getstatics(playerId));
        StartCoroutine(UpdateEmail(playerId));
        StartCoroutine(Updateusername(playerId));
        mainMenuPanel.SetActive(true);
        loginPanel.SetActive(false);
        loadingScreen.SetActive(false);

    }

    public void Trying()
    {
        mainMenuPanel.SetActive(true);
        loginPanel.SetActive(false);
        loadingScreen.SetActive(false);

    }

    public void PlayGame()
    {
        PhotonNetwork.JoinRandomRoom();
    }

    public override void OnConnected()
    {
        PhotonNetwork.AutomaticallySyncScene = true;
    }

    public override void OnConnectedToMaster()
    {
        loadingScreen.SetActive(false);
        loginPanel.SetActive(false);
        mainMenuPanel.SetActive(true);
        Debug.Log("Connected to Photon Master Server.");
        isPhotonLoaded = true;
        PhotonNetwork.JoinLobby();
        PhotonNetwork.AutomaticallySyncScene = true;
    }

    public void JoinRandomRoom(int num)
    {
        StartCoroutine(JoinRandomRoomCoroutine(num));
    }

    private IEnumerator JoinRandomRoomCoroutine(int num)
    {
        if (num == 1)
        {
            if (AIdomino.isOn)
            {
                PlayerPrefs.SetInt("ai", 1);
                createroomforai(num);
            }
            else
            {
                yield return StartCoroutine(GetPlayerCoins(playerId));
                if (coins - currentbid >= 0)
                {
                    gameNumber = num;
                    JoinRandomRoom((byte)num);
                }
                else
                {
                    errorpanel.SetActive(true);
                    errorpanel_text.text = "You don't have enough coins";
                }
                PlayerPrefs.SetInt("ai", 0);
            }
        }
        else
        {
            if (AIuno.isOn)
            {
                PlayerPrefs.SetInt("ai", 1);
                createroomforai(num);
            }
            else
            {
                yield return StartCoroutine(GetPlayerCoins(playerId));
                if (coins - currentbid >= 0)
                {
                    gameNumber = num;
                    JoinRandomRoom((byte)num);
                }
                else
                {
                    errorpanel.SetActive(true);
                    errorpanel_text.text = "You don't have enough coins";
                }
                PlayerPrefs.SetInt("ai", 0);
            }
        }
    }

    public void JoinRandomRoom(byte mapCode)
    {
        var customProperties = new ExitGames.Client.Photon.Hashtable
        {
            ["Bid"] = currentbid,
            ["Scene"] = gameNumber,
            ["unoplayer"] = players
        };

        PhotonNetwork.JoinRandomRoom(customProperties, players);
    }

    public override void OnCreateRoomFailed(short returnCode, string message)
    {

        errorpanel.SetActive(true);
        errorpanel_text.text = "Room Creation Failed: " + message;

    }

    public void LeaveRoom()
    {
        PhotonNetwork.LeaveRoom();
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
        StartCoroutine(GetPlayerCoins(playerId));
        if (gameNumber == 1)
        {
            if (AIdomino.isOn)
            {

                PlayerPrefs.SetInt("ai", 1);
                createroomforai(gameNumber);
            }
            else
            {
                PlayerPrefs.SetInt("ai", 0);
            }
        }
        else
        {
            if (AIuno.isOn)
            {
                PlayerPrefs.SetInt("ai", 1);
                createroomforai(gameNumber);

            }
            else
            {
                PlayerPrefs.SetInt("ai", 0);
            }
        }
        if (coins - currentbid >= 0)
        {
            var customProperties = new ExitGames.Client.Photon.Hashtable
            {
                ["Bid"] = currentbid,
                ["Scene"] = gameNumber,
                ["unoplayer"] = players
            };

            var roomOptions = new RoomOptions
            {
                CustomRoomProperties = customProperties,
                IsVisible = true,
                IsOpen = true,
                MaxPlayers = players,
                CleanupCacheOnLeave = false
            };

            string roomName = UnityEngine.Random.Range(0, 100000).ToString();
            roomOptions.CustomRoomPropertiesForLobby = new[] { "Scene", "unoplayer", "Bid" };

            PhotonNetwork.CreateRoom(roomName, roomOptions);
        }
        else
        {
            errorpanel.SetActive(true);
            errorpanel_text.text = "you don't have enough coins";
        }

    }
    public void createroomforai(int num)
    {
        gameNumber = num;
        isai = true;
        var customProperties = new ExitGames.Client.Photon.Hashtable
        {
            ["Bid"] = currentbid,
            ["Scene"] = gameNumber,
            ["unoplayer"] = players
        };
        string roomName = UnityEngine.Random.Range(0, 100000).ToString();
        var roomOptions = new RoomOptions
        {
            CustomRoomProperties = customProperties,
            MaxPlayers = 1,
            IsVisible = false,
            IsOpen = false
        };
        roomOptions.CustomRoomPropertiesForLobby = new[] { "Scene", "unoplayer", "Bid" };

        PhotonNetwork.CreateRoom(roomName, roomOptions);

    }


    public void CreateAndJoinPrivateRoom(int num)
    {
        StartCoroutine(GetPlayerCoins(playerId));

        if (num == 1)
        {
            if (AIdomino.isOn)
            {
                PlayerPrefs.SetInt("ai", 1);
                createroomforai(num);

            }
            else
            {
                PlayerPrefs.SetInt("ai", 0);

                if (coins - currentbid >= 0)
                {
                    gameNumber = num;
                    var customProperties = new ExitGames.Client.Photon.Hashtable
                    {
                        ["Bid"] = currentbid,
                        ["Scene"] = gameNumber,
                        ["unoplayer"] = players
                    };
                    string roomName = UnityEngine.Random.Range(0, 100000).ToString();
                    _roomName = roomName;
                    var roomOptions = new RoomOptions
                    {
                        CustomRoomProperties = customProperties,
                        MaxPlayers = 2, //players,
                        IsVisible = false
                    };
                    roomOptions.CustomRoomPropertiesForLobby = new[] { "Scene", "unoplayer", "Bid" };
                    Debug.Log(roomName);
                    PhotonNetwork.CreateRoom(roomName, roomOptions);
                }
                else
                {
                    errorpanel.SetActive(true);
                    errorpanel_text.text = "you don't have enough coins";
                }
            }
        }
        else
        {
            if (AIuno.isOn)
            {
                PlayerPrefs.SetInt("ai", 1);
                createroomforai(num);

            }
            else
            {
                PlayerPrefs.SetInt("ai", 0);

                if (coins - currentbid >= 0)
                {
                    gameNumber = num;
                    var customProperties = new ExitGames.Client.Photon.Hashtable
                    {
                        ["Bid"] = currentbid,
                        ["Scene"] = gameNumber,
                        ["unoplayer"] = players
                    };
                    string roomName = UnityEngine.Random.Range(0, 100000).ToString();
                    var roomOptions = new RoomOptions
                    {
                        CustomRoomProperties = customProperties,
                        MaxPlayers = players,
                        IsVisible = false
                    };
                    roomOptions.CustomRoomPropertiesForLobby = new[] { "Scene", "unoplayer", "Bid" };

                    PhotonNetwork.CreateRoom(roomName, roomOptions);
                }
                else
                {
                    errorpanel.SetActive(true);
                    errorpanel_text.text = "you don't have enough coins";
                }
            }
        }
    }

    public void JoinPrivateRoom()
    {
        StartCoroutine(GetPlayerCoins(playerId));

        if (coins - currentbid >= 0)
        {
            PhotonNetwork.JoinRoom(privateinput.text);
        }
        else
        {
            errorpanel.SetActive(true);
            errorpanel_text.text = "you don't have enough coins";
        }
    }

    public override void OnJoinedRoom()
    {

        PlayerPrefs.SetString("playerid", playerId);
        PlayerPrefs.SetFloat("bid", currentbid);
        PlayerPrefs.SetInt("played", gameplayed + 1);
        PlayerPrefs.SetInt("win", gamewin);
        PlayerPrefs.SetInt("lose", gamelose + 1);

        if (isai == false)
        {

            UpdateStatics(playerId, gameplayed + 1, gamewin, gamelose + 1);

            StartCoroutine(UpdateCoinsCoroutine(playerId, coins - currentbid));
            coins -= currentbid;
        }

        if (players == 2)
        {
            roomGO1v1.SetActive(true);
            roomGo1v1RoomName.text = "Room Name: " + PhotonNetwork.CurrentRoom.Name;
        }
        else if (players == 4)
        {
            roomGO2v2.SetActive(true);
            roomGo2v2RoomName.text = "Room Name: " + PhotonNetwork.CurrentRoom.Name;
        }

        UpdatePlayerList();

        //PhotonNetwork.LoadLevel(gameNumber);
        Debug.Log("Joined room: " + PhotonNetwork.CurrentRoom.Name);
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
        else
        {
            errorpanel2.SetActive(true);
            errorpanel_text2.text = "please accept Terms and approve that you're above 18";
        }
    }

    void Update()
    {


        if (music.isOn)
        {
            PlayerPrefs.SetInt("music", 0);
            musicbackground.SetActive(true);
        }
        else
        {
            PlayerPrefs.SetInt("music", 1);
            musicbackground.SetActive(false);
        }
        if (sound.isOn)
        {
            PlayerPrefs.SetInt("sound", 0);
            soundbackground.SetActive(true);
        }
        else
        {
            PlayerPrefs.SetInt("sound", 1);
            soundbackground.SetActive(false);
        }

        XP_text.text = "Level " + XP.ToString();
        XP_text2.text = "Level " + XP.ToString();
        gameplayed_Text.text = gameplayed.ToString();
        gamewin_Text.text = gamewin.ToString();
        gamelose_Text.text = gamelose.ToString();
        coins_text.text = coins.ToString();
        profile_coins_text.text = coins.ToString();

        if (isPhotonLoaded && !isFirebaseLoaded && dfdf == 0)
        {
            loadingScreen.SetActive(false);
            loginPanel.SetActive(true);
            dfdf = 1;
        }
        else if (isPhotonLoaded && isFirebaseLoaded)
        {
            loadingScreen.SetActive(false);
            loginPanel.SetActive(false);
            mainMenuPanel.SetActive(true);
        }

        if (PhotonNetwork.IsMasterClient && PhotonNetwork.CurrentRoom.PlayerCount == players)
        {
            if (players == 2)
            {
                playButton1v1.SetActive(true);
            }
            else if (players == 4)
            {
                playButton2v2.SetActive(true);
            }
        }
        else
        {
            if (players == 2)
            {
                playButton1v1.SetActive(false);
            }
            else if (players == 4)
            {
                playButton2v2.SetActive(false);
            }
        }
    }

    public override void OnLeftRoom()
    {
        if (players == 2)
        {
            roomGO1v1.SetActive(false);
            Debug.Log("Left Room");
        }
        else if (players == 4)
        {
            roomGO2v2.SetActive(false);
            Debug.Log("Left Room");
        }
    }

    void UpdatePlayerList()
    {
        foreach (PlayerDisplay item in playersInRoom)
        {
            Destroy(item.gameObject);
        }
        playersInRoom.Clear();

        if (PhotonNetwork.CurrentRoom == null)
        {
            return;
        }

        foreach (KeyValuePair<int, Player> player in PhotonNetwork.CurrentRoom.Players)
        {
            PlayerDisplay newPlayer = null;

            if (players == 2)
            {
                newPlayer = Instantiate(playerPrefab, displayParent1v1);
                newPlayer.SetPlayerInfo(player.Value);
            }
            else if (players == 4)
            {
                newPlayer = Instantiate(playerPrefab, displayParent2v2);
                newPlayer.SetPlayerInfo(player.Value);
            }

            playersInRoom.Add(newPlayer);
        }
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        UpdatePlayerList();
    }
    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        UpdatePlayerList();
    }

    public void OnClickPlayByMaster()
    {
        PhotonNetwork.LoadLevel(gameNumber);
    }
}
