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
using Photon.Pun;
using Photon.Realtime;
using ExitGames.Client.Photon;

public class Events_Manager : MonoBehaviourPunCallbacks, IOnEventCallback
{
    string playerId = "234353423";

    public GameObject tournamentPrefab;
    public GameObject parent;
    public GameObject CurrentTourParent;
    public GameObject content;

    public GameObject tourr;
    public MainMenu menu;

    public List<Text> names16;
    public List<Text> names8;
    public List<Text> names4;
    public List<Text> names2;

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

    private const byte TournamentMatchEndEventCode = 1;

    private void Awake()
    {
    }

    void Start()
    {
        PhotonNetwork.AddCallbackTarget(this);
    }

    void OnDestroy()
    {
        PhotonNetwork.RemoveCallbackTarget(this);
    }

    public void OnEvent(EventData photonEvent)
    {
        byte eventCode = photonEvent.Code;

        if (eventCode == TournamentMatchEndEventCode)
        {
            object[] data = (object[])photonEvent.CustomData;
            string[] winners = (string[])data[0];

            // Handle the winners of the tournament match
            OnMatchCompleted(new List<string>(winners));
        }
    }

    public void OnMatchCompleted(List<string> winners)
    {
        // Handle logic for match completion, e.g., move winners to the next round
        Debug.Log("Match completed with winners: " + string.Join(", ", winners));
    }

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

                    // Instantiate a GameObject for each tournament
                    GameObject tournamentObject = Instantiate(tournamentPrefab);
                    tournamentObject.transform.SetParent(CurrentTourParent.transform);

                    // Set data in the instantiated GameObject
                    Tournament tournamentComponent = tournamentObject.GetComponent<Tournament>();
                    StartCoroutine(tour(childSnapshot.Key, tournamentComponent));
                }

                Canvas.ForceUpdateCanvases();
                LayoutRebuilder.ForceRebuildLayoutImmediate(content.GetComponent<RectTransform>());
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
                    // Assuming each child has some data you want to use
                    string tournamentName = childSnapshot.Key;
                    int tournamentplayers = int.Parse(childSnapshot.Child("players").Value.ToString());
                    float tournamentbid = float.Parse(childSnapshot.Child("bid").Value.ToString());
                    string tournamenttype = childSnapshot.Child("type").Value.ToString();

                    // Instantiate a GameObject for each tournament
                    GameObject tournamentObject = Instantiate(tournamentPrefab);
                    tournamentObject.transform.SetParent(parent.transform);

                    // Set data in the instantiated GameObject
                    Tournament tournamentComponent = tournamentObject.GetComponent<Tournament>();
                    if (tournamentComponent != null)
                    {
                        tournamentComponent.SetData(tournamentName, tournamenttype, tournamentplayers, tournamentbid);
                    }
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

        tournamentComponent.SetData(name, type, players, bid);
    }

    public void CreateTournament()
    {
        var name = tournamentname.text;

        string TournamentId = HelperClass.Encrypt(name, playerId);

        // Create a new tournament in the database
        Tournament tournamentData = new Tournament
        {
            name = HelperClass.Encrypt(name, playerId),
            //type = HelperClass.Encrypt(tournamenttype, playerId),
            //players = tournamentplayers,
            //bid = tournamentbid
        };

        // Get a reference to the new tournament
        var newTournamentRef = menu.databaseReference.Child(HelperClass.Encrypt("Tournments", playerId)).Child(TournamentId);

        // Set the data for the new tournament
        newTournamentRef.SetRawJsonValueAsync(JsonUtility.ToJson(tournamentData)).ContinueWithOnMainThread(task =>
        {
            if (task.IsCompleted)
            {
                Debug.Log("Tournament created successfully!");

                // Automatically join the tournament
                JoinTournament(TournamentId);
            }
            else
            {
                Debug.LogError("Failed to create tournament: " + task.Exception);
            }
        });
    }

    public void JoinTournament(string tournamentId)
    {
        string userId = playerId; // Replace with actual user ID
        string encryptedTournamentId = HelperClass.Encrypt(tournamentId, playerId);
        string encryptedUserId = HelperClass.Encrypt(userId, playerId);

        // Get a reference to the tournament's players
        DatabaseReference tournamentPlayersRef = menu.databaseReference.Child("Tournments").Child(encryptedTournamentId).Child("Players");

        // Add the user to the tournament's players list
        tournamentPlayersRef.Child(encryptedUserId).SetValueAsync(true).ContinueWithOnMainThread(task =>
        {
            if (task.IsCompleted)
            {
                Debug.Log("Joined tournament successfully!");

                // Fetch and update player names after joining
                FetchPlayerNames(tournamentId);
            }
            else
            {
                Debug.LogError("Failed to join tournament: " + task.Exception);
            }
        });
    }

    public void FetchPlayerNames(string tournamentId)
    {
        string encryptedTournamentId = HelperClass.Encrypt(tournamentId, playerId);

        // Get a reference to the tournament's players
        DatabaseReference tournamentPlayersRef = menu.databaseReference.Child("Tournments").Child(encryptedTournamentId).Child("Players");

        // Fetch the player names
        tournamentPlayersRef.GetValueAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsCompleted)
            {
                DataSnapshot snapshot = task.Result;
                List<string> playerNames = new List<string>();

                foreach (DataSnapshot playerSnapshot in snapshot.Children)
                {
                    string encryptedPlayerId = playerSnapshot.Key;
                    string decryptedPlayerId = HelperClass.Decrypt(encryptedPlayerId, playerId);

                    // Assuming you have a method to fetch player name by ID
                    string playerName = FetchPlayerNameById(decryptedPlayerId);
                    playerNames.Add(playerName);
                }

                UpdatePlayerNames(playerNames);
            }
            else
            {
                Debug.LogError("Failed to fetch player names: " + task.Exception);
            }
        });
    }

    public void UpdatePlayerNames(List<string> playerNames)
    {
        for (int i = 0; i < names16.Count; i++)
        {
            if (i < playerNames.Count)
            {
                names16[i].text = playerNames[i];
            }
            else
            {
                names16[i].text = ""; // Clear the text if there are no more player names
            }
        }
    }

    private string FetchPlayerNameById(string playerId)
    {
        // Replace this with actual logic to fetch player name by ID
        return "Player " + playerId;
    }

    public void FindCurrentTournamet(string key)
    {
        currenttour = key;
        GetAllDataForTour(currenttour);
    }

    void GetAllDataForTour(string key)
    {
        foreach (Text tex in names16)
        {
            tex.text = "";
        }

        foreach (Text tex in names8)
        {
            tex.text = "";
        }

        foreach (Text tex in names4)
        {
            tex.text = "";
        }

        foreach (Text tex in names2)
        {
            tex.text = "";
        }

        foreach (Text tex in ready16)
        {
            tex.text = "";
        }

        foreach (Text tex in ready8)
        {
            tex.text = "";
        }

        foreach (Text tex in ready4)
        {
            tex.text = "";
        }

        foreach (Text tex in ready2)
        {
            tex.text = "";
        }

        StartCoroutine(GetPlayers(key));
    }

    IEnumerator GetPlayers(string key)
    {
        var typetask = menu.databaseReference.Child(HelperClass.Encrypt("Tournments", playerId)).Child(key).Child(HelperClass.Encrypt("players", playerId)).GetValueAsync();
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
            tournamentplayers = int.Parse(typetask.Result.Value.ToString());
            // Do something with the retrieved coins, e.g., update UI
        }

        StartCoroutine(LoadNames(key, tournamentplayers));
    }

    IEnumerator LoadNames(string key, int players)
    {
        string[] names = new string[16];

        for (int i = 0; i < 16; i++)
        {
            names[i] = "";
        }

        List<string> nameslist = new List<string>();

        for (int i = 1; i <= players; i++)
        {
            string Playerkey = HelperClass.Encrypt("player" + i, playerId);
            var typetask = menu.databaseReference.Child(HelperClass.Encrypt("Tournments", playerId)).Child(key).Child(Playerkey).GetValueAsync();
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
                names[i - 1] = typetask.Result.Value.ToString();
                nameslist.Add(names[i - 1]);
            }
        }

        int count = 0;

        foreach (Text tex in names16)
        {
            tex.text = names[count];
            count++;
        }

        nameslist.RemoveAll(string.IsNullOrEmpty);

        menu.names = nameslist;
        menu.names.TrimExcess();

        menu.SetParticipants();
    }
}
