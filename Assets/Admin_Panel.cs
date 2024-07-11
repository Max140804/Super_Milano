using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Firebase.Auth;
using Firebase.Extensions;
using Firebase;
using Firebase.Database;
using System.Collections;
using System;
using System.Data.SqlTypes;
using UnityEditor;
using static UnityEngine.UIElements.UxmlAttributeDescription;

public class Admin_Panel : MonoBehaviour
{

    private FirebaseApp app;

    private DatabaseReference databaseReference;

    public GameObject loadingScreen;
    public GameObject mainMenuPanel;

    public InputField playerid;
    public InputField coinstoadd; 
    public InputField playerid1;
    public InputField coinstoadd2;

    public InputField playeridfordelete;


    public InputField playeridfordetails;

    public float coins; 
    private FirebaseAuth auth;

    public Text muchcoin;
    public Text email;
    public Text username;
    public InputField Email;
    public InputField Password;

    public GameObject loginpage;

    public string ema;
    public string passs;
    // Start is called before the first frame update
    void Start()
    {
        FirebaseApp.CheckAndFixDependenciesAsync().ContinueWithOnMainThread(task =>
        {

            if (task.Result == DependencyStatus.Available)
            {

                InitializeFirebase();
                auth = FirebaseAuth.DefaultInstance;

            }
        });
    }



    public void Login()
    {
        if(Email.text == ema && Password.text == passs)
        {
            loginpage.SetActive(false);
            mainMenuPanel.SetActive(true);
        }
    }


    public void addcoins()
    {
        StartCoroutine(GetPlayerCoins(0));


    }


    public void playerdetails()
    {
        StartCoroutine(Getusername());
        StartCoroutine(Getemail());
        StartCoroutine(Getcoins());


    }


    public void removecoins()
    {
        StartCoroutine(GetPlayerCoins(1));


    }

    public IEnumerator Getusername()
    {
        var task = databaseReference.Child(HelperClass.Encrypt("players", playeridfordetails.text)).Child(HelperClass.Encrypt(playeridfordetails.text, playeridfordetails.text)).Child(HelperClass.Encrypt("username", playeridfordetails.text)).GetValueAsync();
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

            username.text = "username: " + HelperClass.Decrypt(task.Result.Value.ToString(), playeridfordetails.text);
            Debug.Log($"Player has {coins} coins.");
            // Do something with the retrieved coins, e.g., update UI
        }
    }

    public IEnumerator Getemail()
    {
        var task = databaseReference.Child(HelperClass.Encrypt("players", playeridfordetails.text)).Child(HelperClass.Encrypt(playeridfordetails.text, playeridfordetails.text)).Child(HelperClass.Encrypt("email",playeridfordetails.text)).GetValueAsync();
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

            email.text = "Email : " + HelperClass.Decrypt(task.Result.Value.ToString(), playeridfordetails.text);
            Debug.Log($"Player has {coins} coins.");
            // Do something with the retrieved coins, e.g., update UI
        }
    }
    public IEnumerator Getcoins()
    {
        var task = databaseReference.Child(HelperClass.Encrypt("players", playeridfordetails.text)).Child(HelperClass.Encrypt(playeridfordetails.text, playeridfordetails.text)).Child(HelperClass.Encrypt("coins", playeridfordetails.text)).GetValueAsync();
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

            muchcoin.text = "coins: " + HelperClass.Decrypt(task.Result.Value.ToString(), playeridfordetails.text);
            Debug.Log($"Player has {coins} coins.");
            // Do something with the retrieved coins, e.g., update UI
        }
    }

    public IEnumerator GetPlayerCoins(float addorremove)
    {
        var task = databaseReference.Child(HelperClass.Encrypt("players", playerid.text)).Child(HelperClass.Encrypt(playerid.text,playerid.text)).Child(HelperClass.Encrypt("coins",playerid.text)).GetValueAsync();
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
 
            coins = float.Parse(HelperClass.Decrypt(task.Result.Value.ToString(), playerid.text));

            if (addorremove == 0)
            {
                StartCoroutine(addCoinsCoroutine());

            }
            if (addorremove == 1)
            {
                StartCoroutine(removeCoinsCoroutine());

            }
            Debug.Log($"Player has {coins} coins.");
            // Do something with the retrieved coins, e.g., update UI
        }
    }
    public void DeleteUserById()
    {
        if (playeridfordelete.text != "")
        {
            // Get a reference to the user's data
            DatabaseReference userRef = databaseReference.Child(HelperClass.Encrypt("players", playeridfordelete.text)).Child(HelperClass.Encrypt(playeridfordelete.text, playeridfordelete.text));

            // Delete the user
            userRef.RemoveValueAsync().ContinueWith(task =>
            {
                if (task.IsCompleted)
                {
                    Debug.Log("User deleted successfully.");
                }
                else
                {
                    Debug.LogError("Failed to delete user: " + task.Exception);
                }
            });
        }
    }

    private IEnumerator removeCoinsCoroutine()
    {
        var task = databaseReference.Child(HelperClass.Encrypt("players", playerid1.text)).Child(HelperClass.Encrypt(playerid1.text, playerid1.text)).Child(HelperClass.Encrypt("coins", playerid1.text)).SetValueAsync(HelperClass.Encrypt((coins - float.Parse(coinstoadd2.text)).ToString(), playerid1.text));
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
    private IEnumerator addCoinsCoroutine()
    {
        var task = databaseReference.Child(HelperClass.Encrypt("players", playerid.text)).Child(HelperClass.Encrypt(playerid.text, playerid.text)).Child(HelperClass.Encrypt("coins", playerid.text)).SetValueAsync(HelperClass.Encrypt((coins + float.Parse(coinstoadd.text)).ToString(), playerid.text));
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
        loginpage.SetActive(true);
        loadingScreen.SetActive(false);
    }
    // Update is called once per frame
    void Update()
    {
        
    }
}
