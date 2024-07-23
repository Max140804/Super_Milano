using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Firebase;
using Firebase.Database;
using Firebase.Extensions;
using System;
using UnityEditor;
using UnityEngine.UI;
using System.Security.Cryptography;
using TMPro;

public class Events_Manager : MonoBehaviour
{
    string playerId = "234353423";

    public Tournament tournamentPrefab;
    List<Tournament> tourPrefList = new List<Tournament>();
    public GameObject parent;
    public GameObject CurrentTourParent;
    public GameObject content;

    public GameObject tourr;
    public TextMeshProUGUI tourrName;
    public MainMenu menu;

    public List<Text> names16UI;
    public List<Text> names8UI;
    public List<Text> names4UI;
    public List<Text> names2UI;

    private List<string> names16 = new List<string>();
    private List<string> names8 = new List<string>();
    private List<string> names4 = new List<string>();
    private List<string> names2 = new List<string>();

    public List<Text> ready16;
    public List<Text> ready8;
    public List<Text> ready4;
    public List<Text> ready2;

    public string currenttour;
    public string currentusername;

    public InputField tournamentname;
    public string tournamenttype;
    public int tournamentplayers;
    public float tournamentbid;

    int playersInRoom = 0;
    bool playerAlreadyInTour;
    float timeBtwUpdt = 1.5f;
    float nextUpdtTime;

    public void changetype(string typ)
    {
        tournamenttype = typ;
    }
    public void changeplayers(int pla)
    {
        tournamentplayers = pla;
    }
    public void changebid(int bi)
    {
        if (bi == 42)
        {
            tournamentbid = 0.05f;
        }
        else
        {
            tournamentbid = bi;
        }
    }
    public void GetCurrentTournamentsData()
    {
        CurrentTourParent.GetComponent<RectTransform>().sizeDelta = new Vector2(CurrentTourParent.GetComponent<RectTransform>().sizeDelta.x, 0);

        foreach (Transform child in CurrentTourParent.transform)
        {
            Destroy(child.gameObject);
        }

        menu.databaseReference.Child("Tournaments").GetValueAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsFaulted)
            {
                Debug.LogError("Error getting data: " + task.Exception);
            }
            else if (task.IsCompleted)
            {
                DataSnapshot snapshot = task.Result;
                foreach (DataSnapshot childSnapshot in snapshot.Children)
                {
                    Debug.Log(childSnapshot.Key);
                    CurrentTourParent.GetComponent<RectTransform>().sizeDelta = new Vector2(CurrentTourParent.GetComponent<RectTransform>().sizeDelta.x, CurrentTourParent.GetComponent<RectTransform>().sizeDelta.y + 350);
                }
            }
        });
    }
    public void GetTournamentsData()
    {
        parent.GetComponent<RectTransform>().sizeDelta = new Vector2(parent.GetComponent<RectTransform>().sizeDelta.x, 0);
        foreach (Transform child in parent.transform)
        {
            Destroy(child.gameObject);
        }

        menu.databaseReference.Child("Tournaments").GetValueAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsFaulted)
            {
                Debug.LogError("Error getting data: " + task.Exception);
            }
            else if (task.IsCompleted)
            {
                DataSnapshot snapshot = task.Result;

                foreach (DataSnapshot childSnapshot in snapshot.Children)
                {
                    parent.GetComponent<RectTransform>().sizeDelta = new Vector2(parent.GetComponent<RectTransform>().sizeDelta.x, parent.GetComponent<RectTransform>().sizeDelta.y + 350);

                    string tournamentName = childSnapshot.Key;
                    int tournamentplayers = int.Parse(childSnapshot.Child("players").Value.ToString());
                    float tournamentbid = float.Parse(childSnapshot.Child("bid").Value.ToString());
                    string tournamenttype = childSnapshot.Child("type").Value.ToString();
                }

                Canvas.ForceUpdateCanvases();
                LayoutRebuilder.ForceRebuildLayoutImmediate(content.GetComponent<RectTransform>());
            }
        });
    }
    IEnumerator tour(string key, Tournament tournamentComponent)
    {
        // Wait until data is fetched
        yield return StartCoroutine(FetchTournamentDetails(key, tournamentComponent));
    }
    IEnumerator FetchTournamentDetails(string key, Tournament tournamentComponent)
    {
        string name = "Failed To Load";
        string type = "Uno";
        int players = 2;
        float bid = 0.5f;

        // Fetch name asynchronously
        yield return StartCoroutine(FetchNameAsync(key));

        // Fetch other tournament details
        var typeTask = menu.databaseReference.Child(HelperClass.Encrypt("Tournments", playerId)).Child(key).Child(HelperClass.Encrypt("type", playerId)).GetValueAsync();
        yield return new WaitUntil(() => typeTask.IsCompleted);

        if (typeTask.Exception != null)
        {
            Debug.LogError($"Failed to get tournament type: {typeTask.Exception}");
        }
        else if (typeTask.Result.Value != null)
        {
            type = typeTask.Result.Value.ToString();
        }

        var playersTask = menu.databaseReference.Child(HelperClass.Encrypt("Tournments", playerId)).Child(key).Child(HelperClass.Encrypt("players", playerId)).GetValueAsync();
        yield return new WaitUntil(() => playersTask.IsCompleted);

        if (playersTask.Exception != null)
        {
            Debug.LogError($"Failed to get tournament players: {playersTask.Exception}");
        }
        else if (playersTask.Result.Value != null)
        {
            players = int.Parse(playersTask.Result.Value.ToString());
        }

        var bidTask = menu.databaseReference.Child(HelperClass.Encrypt("Tournments", playerId)).Child(key).Child(HelperClass.Encrypt("bid", playerId)).GetValueAsync();
        yield return new WaitUntil(() => bidTask.IsCompleted);

        if (bidTask.Exception != null)
        {
            Debug.LogError($"Failed to get tournament bid: {bidTask.Exception}");
        }
        else if (bidTask.Result.Value != null)
        {
            bid = float.Parse(bidTask.Result.Value.ToString());
        }

        tournamentComponent.SetData(name, type, players, bid);
    }
    IEnumerator FetchNameAsync(string key)
    {
        var nameTask = menu.databaseReference.Child(HelperClass.Encrypt("Tournments", playerId)).Child(key).Child(HelperClass.Encrypt("name", playerId)).GetValueAsync();
        yield return new WaitUntil(() => nameTask.IsCompleted);

        if (nameTask.Exception != null)
        {
            Debug.LogError($"Failed to fetch tournament name: {nameTask.Exception}");
            yield return "Failed To Load";
        }
        else if (nameTask.Result.Value != null)
        {
            yield return HelperClass.Decrypt(nameTask.Result.Value.ToString(), playerId);
        }
        else
        {
            yield return "Name Not Found";
        }
    }
    public IEnumerator tourrr(string key, Tournament tournamentComponent)
    {
        string name = "Failed To load";
        int players = 2;
        float bid = 0.5f;
        string type = "Uno";

        var typetask = menu.databaseReference.Child(HelperClass.Encrypt("Tournments", playerId)).Child(key).Child(HelperClass.Encrypt("type", playerId)).GetValueAsync();
        yield return new WaitUntil(() => typetask.IsCompleted);

        if (typetask.Exception != null)
        {
            Debug.LogError($"Failed to get player coins: {typetask.Exception}");
        }
        else if (typetask.Result.Value == null)
        {
            Debug.Log("Player not found or coins not set.");
        }
        else
        {
            type = typetask.Result.Value.ToString();
            // Do something with the retrieved coins, e.g., update UI
        }

        name = HelperClass.Decrypt(key, playerId);

        var playerstask = menu.databaseReference.Child(HelperClass.Encrypt("Tournments", playerId)).Child(key).Child(HelperClass.Encrypt("players", playerId)).GetValueAsync();
        yield return new WaitUntil(() => playerstask.IsCompleted);

        if (playerstask.Exception != null)
        {
            Debug.LogError($"Failed to get player coins: {playerstask.Exception}");
        }
        else if (playerstask.Result.Value == null)
        {
            Debug.Log("Player not found or coins not set.");
        }
        else
        {
            players = int.Parse(playerstask.Result.Value.ToString());
            // Do something with the retrieved coins, e.g., update UI
        }

        var bidtask = menu.databaseReference.Child(HelperClass.Encrypt("Tournments", playerId)).Child(key).Child(HelperClass.Encrypt("bid", playerId)).GetValueAsync();
        yield return new WaitUntil(() => bidtask.IsCompleted);

        if (bidtask.Exception != null)
        {
            Debug.LogError($"Failed to get player coins: {bidtask.Exception}");
        }
        else if (bidtask.Result.Value == null)
        {
            Debug.Log("Player not found or coins not set.");
        }
        else
        {
            bid = float.Parse(bidtask.Result.Value.ToString());
            // Do something with the retrieved coins, e.g., update UI
        }

        // Assuming 'SetData' is a method that updates the tournament's UI or data
        tournamentComponent.SetData(name, type, players, bid);
    }
    public void Create()
    {
        string key = GenerateUniqueKey();
        currenttour = key;
        string encryptedKey = HelperClass.Encrypt(key, playerId);

        var tournamentData = new Dictionary<string, object>
        {
            { HelperClass.Encrypt("name", playerId), HelperClass.Encrypt(tournamentname.text, playerId) },
            { HelperClass.Encrypt("type", playerId), tournamenttype },
            { HelperClass.Encrypt("players", playerId), tournamentplayers },
            { HelperClass.Encrypt("bid", playerId), tournamentbid }
        };

        menu.databaseReference.Child(HelperClass.Encrypt("Tournments", playerId)).Child(encryptedKey).SetValueAsync(tournamentData).ContinueWithOnMainThread(task =>
        {
            if (task.IsCompleted)
            {
                Debug.Log("Tournament created successfully");
                Join(key);
            }
            else
            {
                Debug.Log("Error creating tournament: " + task.Exception);
            }
        });
    }
    public void create_tour(string key)
    {
        var tournamentData = new Dictionary<string, object>
        {
            { HelperClass.Encrypt("name", playerId), HelperClass.Encrypt(tournamentname.text, playerId) },
            { HelperClass.Encrypt("type", playerId), tournamenttype },
            { HelperClass.Encrypt("players", playerId), tournamentplayers },
            { HelperClass.Encrypt("bid", playerId), tournamentbid }
        };

        menu.databaseReference.Child(HelperClass.Encrypt("Tournments", playerId)).Child(HelperClass.Encrypt(key, playerId)).SetValueAsync(tournamentData).ContinueWithOnMainThread(task =>
        {
            if (task.IsCompleted)
            {
                // Tournament created successfully
                Debug.Log("Tournament created successfully");

                // Join the tournament immediately after creating it
                Join(key);
            }
            else
            {
                // Handle errors
                Debug.LogError("Error creating tournament: " + task.Exception);
            }
        });
    }
    public void Join(string key)
    {
        Debug.Log("Joining tournament with key: " + key);
        currenttour = key;

        // Set the tournament GameObject active
        tourr.SetActive(true);

        // Update UI elements with tournament details
        tourrName.text = HelperClass.Decrypt(key, playerId);

        // Fetch player name (or use a placeholder)
        string playerName = currentusername; // This should be set to the actual player's name

        // Add the player's name to the names16UI list
        for (int i = 0; i < names16UI.Count; i++)
        {
            if (string.IsNullOrEmpty(names16UI[i].text))
            {
                names16UI[i].text = playerName;
                break;
            }
        }
    }

    public static string GenerateUniqueKey(int length = 8)
    {
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
        var random = new System.Random();
        var result = new char[length];
        for (int i = 0; i < length; i++)
        {
            result[i] = chars[random.Next(chars.Length)];
        }
        return new string(result);
    }
}
