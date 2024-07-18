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

        menu.databaseReference.Child(HelperClass.Encrypt("players", menu.playerId)).Child(HelperClass.Encrypt(menu.playerId, menu.playerId)).Child(HelperClass.Encrypt("Tournments", playerId)).GetValueAsync().ContinueWithOnMainThread(task =>
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

    IEnumerator tour(string key, Tournament tournamentComponent)
    {
        // Fetch tournament details from database
        // Example: Fetch name, type, players, bid using coroutines

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
    public void GetTournamentsData()
    {
        parent.GetComponent<RectTransform>().sizeDelta = new Vector2(parent.GetComponent<RectTransform>().sizeDelta.x, 0);
        foreach (Transform child in parent.transform)
        {
            Destroy(child.gameObject);
        }
        menu.databaseReference.Child(HelperClass.Encrypt("Tournments", playerId)).GetValueAsync().ContinueWithOnMainThread(task => {
            if (task.IsFaulted)
            {
                // Handle the error
                Debug.LogError("Error getting data: " + task.Exception);
            }
            else if (task.IsCompleted)
            {
                DataSnapshot snapshot = task.Result;

                foreach (DataSnapshot childSnapshot in snapshot.Children)
                {
                    Debug.Log("xxx");

                    parent.GetComponent<RectTransform>().sizeDelta = new Vector2(parent.GetComponent<RectTransform>().sizeDelta.x, parent.GetComponent<RectTransform>().sizeDelta.y + 350);
                    // Assuming each child has some data you want to use
                    string tournamentName = HelperClass.Decrypt(childSnapshot.Key, playerId);
                    int tournamentplayers = int.Parse(childSnapshot.Child(HelperClass.Encrypt("players", playerId)).Value.ToString());
                    float tournamentbid = float.Parse(childSnapshot.Child(HelperClass.Encrypt("bid", playerId)).Value.ToString());
                    string tournamenttype = childSnapshot.Child(HelperClass.Encrypt("type", playerId)).Value.ToString();
                    Debug.Log("ddddddddddddddd");

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

    public void Create()
    {
        if (tournamenttype == "Domino")
        {
            tournamentplayers = 16;
        }
        StartCoroutine(createtournment());

        GetTournamentsData();
    }

    public IEnumerator createtournment()
    {
        var check = menu.databaseReference.Child(HelperClass.Encrypt("Tournments", playerId)).Child(HelperClass.Encrypt(tournamentname.text, playerId)).GetValueAsync();
        yield return new WaitUntil(() => check.IsCompleted);

        if (check.Exception != null)
        {
            Debug.LogError($"Failed to get player coins: {check.Exception}");
        }
        else if (check.Result.Value == null)
        {
            StartCoroutine(Createtour(tournamentname.text, tournamenttype, tournamentplayers, tournamentbid));
        }
        else
        {
            menu.errorpanel.gameObject.SetActive(true);
            menu.errorpanel_text.text = "Name is used, Please change tournament name";
        }
    }

    public IEnumerator Createtour(string name, string type, int players, float bid)
    {
        var task = menu.databaseReference.Child(HelperClass.Encrypt("Tournments", playerId)).Child(HelperClass.Encrypt(name, playerId)).Child(HelperClass.Encrypt("type", playerId)).SetValueAsync(type);
        yield return new WaitUntil(() => task.IsCompleted);

        if (task.Exception != null)
        {
            Debug.LogError($"Failed to set player coins: {task.Exception}");
        }
        else
        {
            // Success
            Debug.Log("Successfully set player coins.");
        }

        var task2 = menu.databaseReference.Child(HelperClass.Encrypt("Tournments", playerId)).Child(HelperClass.Encrypt(name, playerId)).Child(HelperClass.Encrypt("players", playerId)).SetValueAsync(players);
        yield return new WaitUntil(() => task2.IsCompleted);

        if (task2.Exception != null)
        {
            Debug.LogError($"Failed to set player coins: {task2.Exception}");
        }
        else
        {
            // Success
            Debug.Log("Successfully set player coins.");
        }

        var task3 = menu.databaseReference.Child(HelperClass.Encrypt("Tournments", playerId)).Child(HelperClass.Encrypt(name, playerId)).Child(HelperClass.Encrypt("bid", playerId)).SetValueAsync(bid);
        yield return new WaitUntil(() => task3.IsCompleted);

        if (task3.Exception != null)
        {
            Debug.LogError($"Failed to set player coins: {task3.Exception}");
        }
        else
        {
            // Success
            Debug.Log("Successfully set player coins.");
        }

        var task4 = menu.databaseReference.Child(HelperClass.Encrypt("players", playerId)).Child(HelperClass.Encrypt(menu.playerId, playerId)).Child(HelperClass.Encrypt("Tournments", playerId)).Child(HelperClass.Encrypt(name, playerId)).SetValueAsync("true");
        yield return new WaitUntil(() => task4.IsCompleted);

        if (task4.Exception != null)
        {
            Debug.LogError($"Failed to set player coins: {task4.Exception}");
        }
        else
        {
            // Success
            Debug.Log("Successfully set player coins.");
        }
    }

    public void Join(string tournamentname)
    {
        StartCoroutine(jointour(tournamentname));
    }

    public IEnumerator jointour(string name)
    {
        var check = menu.databaseReference.Child(HelperClass.Encrypt("Tournments", playerId)).Child(HelperClass.Encrypt(name, playerId)).Child(HelperClass.Encrypt("currentplayers", playerId)).GetValueAsync();
        yield return new WaitUntil(() => check.IsCompleted);

        if (check.Exception != null)
        {
            Debug.LogError($"Failed to get player coins: {check.Exception}");
        }
        else if (check.Result.Value == null)
        {
            menu.errorpanel.gameObject.SetActive(true);
            menu.errorpanel_text.text = "The tournament is Full";
        }
        else
        {
            int currentplayers = int.Parse(check.Result.Value.ToString());

            var playerstask = menu.databaseReference.Child(HelperClass.Encrypt("Tournments", playerId)).Child(HelperClass.Encrypt(name, playerId)).Child(HelperClass.Encrypt("players", playerId)).GetValueAsync();
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
                int players = int.Parse(playerstask.Result.Value.ToString());

                if (currentplayers < players)
                {
                    StartCoroutine(Jointour(name, currentplayers + 1));
                }
                else
                {
                    menu.errorpanel.gameObject.SetActive(true);
                    menu.errorpanel_text.text = "The tournament is Full";
                }
            }
        }
    }

    public IEnumerator Jointour(string name, int currentplayers)
    {
        var task4 = menu.databaseReference.Child(HelperClass.Encrypt("players", playerId)).Child(HelperClass.Encrypt(menu.playerId, playerId)).Child(HelperClass.Encrypt("Tournments", playerId)).Child(HelperClass.Encrypt(name, playerId)).SetValueAsync("true");
        yield return new WaitUntil(() => task4.IsCompleted);

        if (task4.Exception != null)
        {
            Debug.LogError($"Failed to set player coins: {task4.Exception}");
        }
        else
        {
            // Success
            Debug.Log("Successfully set player coins.");
        }

        var task3 = menu.databaseReference.Child(HelperClass.Encrypt("Tournments", playerId)).Child(HelperClass.Encrypt(name, playerId)).Child(HelperClass.Encrypt("currentplayers", playerId)).SetValueAsync(currentplayers);
        yield return new WaitUntil(() => task3.IsCompleted);

        if (task3.Exception != null)
        {
            Debug.LogError($"Failed to set player coins: {task3.Exception}");
        }
        else
        {
            // Success
            Debug.Log("Successfully set player coins.");
        }
    }

    public void Start_Tournemnt(string tournamentname)
    {
        StartCoroutine(Start_Tour(tournamentname));
    }

    public IEnumerator Start_Tour(string name)
    {
        List<string> playersList = new List<string>();
        string taskKey = HelperClass.Encrypt("Tournments", playerId) + "/" + HelperClass.Encrypt(name, playerId) + "/" + HelperClass.Encrypt("players", playerId);

        var playerstask = menu.databaseReference.Child(taskKey).GetValueAsync();
        yield return new WaitUntil(() => playerstask.IsCompleted);

        if (playerstask.Exception != null)
        {
            Debug.LogError($"Failed to get players: {playerstask.Exception}");
        }
        else
        {
            foreach (DataSnapshot playerSnapshot in playerstask.Result.Children)
            {
                playersList.Add(playerSnapshot.Key);
            }

            if (playersList.Count % 2 == 0)
            {
                // Call method to start matches
                StartMatches(playersList);
            }
            else
            {
                menu.errorpanel.gameObject.SetActive(true);
                menu.errorpanel_text.text = "The number of players is not even";
            }
        }
    }

    public void StartMatches(List<string> players)
    {
        // Pair players and start matches
        for (int i = 0; i < players.Count; i += 2)
        {
            string player1 = players[i];
            string player2 = players[i + 1];

            // Code to start the match between player1 and player2

            // Broadcast the match start event using Photon
            object[] content = new object[] { player1, player2 };
            RaiseEventOptions raiseEventOptions = new RaiseEventOptions { Receivers = ReceiverGroup.All };
            SendOptions sendOptions = new SendOptions { Reliability = true };
            PhotonNetwork.RaiseEvent(TournamentMatchEndEventCode, content, raiseEventOptions, sendOptions);
        }
    }

    public void UpdatePlayerNames(List<string> playerNames, int stage)
    {
        switch (stage)
        {
            case 16:
                for (int i = 0; i < playerNames.Count && i < names16.Count; i++)
                {
                    names16[i].text = playerNames[i];
                }
                break;
            case 8:
                for (int i = 0; i < playerNames.Count && i < names8.Count; i++)
                {
                    names8[i].text = playerNames[i];
                }
                break;
                // Add cases for other stages (4, 2, etc.) as needed
        }
    }



}
