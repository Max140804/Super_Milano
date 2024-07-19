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
using TMPro;
using ExitGames.Client.Photon;

public class Events_Manager : MonoBehaviourPunCallbacks, IOnEventCallback
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

    private const byte TournamentMatchEndEventCode = 1;
    int playersInRoom = 0;
    bool playerAlreadyInTour;
    float timeBtwUpdt = 1.5f;
    float nextUpdtTime;

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

    private void Update()
    {
        if (PhotonNetwork.InRoom)
        {
            menu.errorpanel.gameObject.SetActive(true);
            menu.errorpanel_text.text = "You are already in a room, please leave room before creating another.";
        }
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

        tournamentComponent.SetData(name, type, players, bid);
    }
    public void Create()
    {
        if (tournamenttype == "Domino")
        {
            tournamentplayers = 16;
            menu.players = 16;
        }
        StartCoroutine(createtournment());
        menu.isTournament = true;
        Instantiate(tournamentPrefab, content.transform);
        PhotonNetwork.CreateRoom(tournamentname.text, new RoomOptions() { MaxPlayers = tournamentplayers, EmptyRoomTtl = 300000, PlayerTtl = 30000 }, TypedLobby.Default);
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
            Debug.LogError($"Failed to set tournament type: {task.Exception}");
            yield break; // Stop further execution if error occurs
        }

        var task2 = menu.databaseReference.Child(HelperClass.Encrypt("Tournments", playerId)).Child(HelperClass.Encrypt(name, playerId)).Child(HelperClass.Encrypt("players", playerId)).SetValueAsync(players);
        yield return new WaitUntil(() => task2.IsCompleted);

        if (task2.Exception != null)
        {
            Debug.LogError($"Failed to set tournament players: {task2.Exception}");
            yield break; // Stop further execution if error occurs
        }

        var task3 = menu.databaseReference.Child(HelperClass.Encrypt("Tournments", playerId)).Child(HelperClass.Encrypt(name, playerId)).Child(HelperClass.Encrypt("bid", playerId)).SetValueAsync(bid);
        yield return new WaitUntil(() => task3.IsCompleted);

        if (task3.Exception != null)
        {
            Debug.LogError($"Failed to set tournament bid: {task3.Exception}");
            yield break; // Stop further execution if error occurs
        }

        var task4 = menu.databaseReference.Child(HelperClass.Encrypt("players", playerId))
            .Child(HelperClass.Encrypt(menu.playerId, playerId))
            .Child(HelperClass.Encrypt("Tournments", playerId))
            .Child(HelperClass.Encrypt(name, playerId))
            .SetValueAsync("true");
        yield return new WaitUntil(() => task4.IsCompleted);

        if (task4.Exception != null)
        {
            Debug.LogError($"Failed to add creator to tournament: {task4.Exception}");
            yield break; // Stop further execution if error occurs
        }

        playerAlreadyInTour = true;
        names16UI[0].text = menu.usernamee;

        GetCurrentTournamentsData();
       
        tourr.SetActive(true);
    }

    public void Join(string tournamentname)
    {
        //PhotonNetwork.JoinRoom(tournamentname);

        if (playerAlreadyInTour)
        {
            tourr.SetActive(true);
            return;
        }

        if (playersInRoom < 16)
        {
            names16UI[playersInRoom].text = menu.usernamee;
            playersInRoom++;
            StartCoroutine(JoinTournament(tournamentname));
        }
        else
        {
            Debug.LogWarning("Tournament is full!");
        }
    }
    public IEnumerator JoinTournament(string name)
    {
        var checkTask = menu.databaseReference.Child(HelperClass.Encrypt("players", playerId))
                                             .Child(HelperClass.Encrypt(menu.playerId, playerId))
                                             .Child(HelperClass.Encrypt("Tournments", playerId))
                                             .Child(HelperClass.Encrypt(name, playerId))
                                             .GetValueAsync();
        yield return new WaitUntil(() => checkTask.IsCompleted);

        if (checkTask.Exception != null)
        {
            Debug.LogError($"Failed to get join status: {checkTask.Exception}");
        }
        else if (checkTask.Result.Value != null)
        {
            Debug.Log("Player has already joined this tournament.");
            playerAlreadyInTour = true;
            tourr.SetActive(true);
        }
        else
        {
            playerAlreadyInTour = false;
            StartCoroutine(IncrementCurrentPlayers(name));
            //UpdatePlayerNames(menu.usernamee, 16);
           // Debug.Log(menu.usernamee + " Joined");
            yield return StartCoroutine(FetchAndSetPlayerNames(name));
        }
    }
    public IEnumerator FetchAndSetPlayerNames(string name)
    {
        string path = $"Tournments/{playerId}/{HelperClass.Encrypt(name, playerId)}/players";

        var playersTask = menu.databaseReference.Child(path).GetValueAsync();
        yield return new WaitUntil(() => playersTask.IsCompleted);

        if (playersTask.Exception != null)
        {
            Debug.LogError($"Failed to fetch player names: {playersTask.Exception}");
        }
        else if (playersTask.Result.Value != null)
        {
           // int currentStage = 16;

            foreach (DataSnapshot playerSnapshot in playersTask.Result.Children)
            {
                string playerName = playerSnapshot.Key;
                //UpdatePlayerNames(playerName, currentStage);
                Debug.Log(name + " Joined");
            }
        }
        else
        {
            Debug.Log("No players found for this tournament.");
        }
    }
    public IEnumerator FetchPlayerNames(string tournamentName, List<string> playerNames)
    {
        string path = $"Tournments/{playerId}/{HelperClass.Encrypt(tournamentName, playerId)}";

        var playersTask = menu.databaseReference.Child(path).GetValueAsync();
        yield return new WaitUntil(() => playersTask.IsCompleted);

        if (playersTask.Exception != null)
        {
            Debug.LogError($"Failed to fetch player names: {playersTask.Exception}");
        }
        else if (playersTask.Result.Value != null)
        {
            foreach (DataSnapshot playerSnapshot in playersTask.Result.Children)
            {
                string playerName = playerSnapshot.Key;
                playerNames.Add(playerName);
                Debug.Log(playerName);
            }
        }
        else
        {
            Debug.Log("No players found for this tournament.");
        }
    }
    public IEnumerator IncrementCurrentPlayers(string name)
    {
        var currentPlayersTask = menu.databaseReference.Child(HelperClass.Encrypt("Tournments", playerId))
                                                     .Child(HelperClass.Encrypt(name, playerId))
                                                     .Child(HelperClass.Encrypt("currentplayers", playerId))
                                                     .GetValueAsync();
        yield return new WaitUntil(() => currentPlayersTask.IsCompleted);

        if (currentPlayersTask.Exception != null)
        {
            Debug.LogError($"Failed to get current players count: {currentPlayersTask.Exception}");
        }
        else if (currentPlayersTask.Result.Value == null)
        {
            Debug.LogError("Current players count not found in database.");
        }
        else
        {
            int currentPlayers = int.Parse(currentPlayersTask.Result.Value.ToString());

            var totalPlayersTask = menu.databaseReference.Child(HelperClass.Encrypt("Tournments", playerId))
                                                       .Child(HelperClass.Encrypt(name, playerId))
                                                       .Child(HelperClass.Encrypt("players", playerId))
                                                       .GetValueAsync();
            yield return new WaitUntil(() => totalPlayersTask.IsCompleted);

            if (totalPlayersTask.Exception != null)
            {
                Debug.LogError($"Failed to get total players count: {totalPlayersTask.Exception}");
            }
            else if (totalPlayersTask.Result.Value == null)
            {
                Debug.LogError("Total players count not found in database.");
            }
            else
            {
                int totalPlayers = int.Parse(totalPlayersTask.Result.Value.ToString());

                if (currentPlayers < totalPlayers)
                {
                    var incrementTask = menu.databaseReference.Child(HelperClass.Encrypt("Tournments", playerId))
                                                             .Child(HelperClass.Encrypt(name, playerId))
                                                             .Child(HelperClass.Encrypt("currentplayers", playerId))
                                                             .SetValueAsync(currentPlayers + 1);
                    yield return new WaitUntil(() => incrementTask.IsCompleted);

                    if (incrementTask.Exception != null)
                    {
                        Debug.LogError($"Failed to update current players count: {incrementTask.Exception}");
                    }
                    else
                    {
                        var joinStatusTask = menu.databaseReference.Child(HelperClass.Encrypt("players", playerId))
                                                                 .Child(HelperClass.Encrypt(menu.playerId, playerId))
                                                                 .Child(HelperClass.Encrypt("Tournments", playerId))
                                                                 .Child(HelperClass.Encrypt(name, playerId))
                                                                 .SetValueAsync("true");
                        yield return new WaitUntil(() => joinStatusTask.IsCompleted);

                        if (joinStatusTask.Exception != null)
                        {
                            Debug.LogError($"Failed to update join status: {joinStatusTask.Exception}");
                        }
                        else
                        {
                            Debug.Log("Successfully joined tournament.");
                            Debug.Log(name);
                            tourr.SetActive(true);
                            //UpdatePlayerNames(menu.usernamee, 16);
                        }
                    }
                }
                else
                {
                    menu.errorpanel.gameObject.SetActive(true);
                    menu.errorpanel_text.text = "The tournament is full.";
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

    public override void OnJoinedRoom()
    {
        tourr.SetActive(true);
        tourrName.text = PhotonNetwork.CurrentRoom.Name;
    }

    public override void OnRoomListUpdate(List<RoomInfo> roomList)
    {
        UpdateRoomList(roomList);
        /*if (Time.time >= nextUpdtTime)
        {
            UpdateRoomList(roomList);
            nextUpdtTime = Time.time + timeBtwUpdt;
        }*/

        
    }

    void UpdateRoomList(List<RoomInfo> list)
    {
        foreach (Tournament item in tourPrefList)
        {
            Destroy(item.gameObject);
        }
        tourPrefList.Clear();

        foreach (RoomInfo room in list)
        {
            Tournament newTour = Instantiate(tournamentPrefab, content.transform);

            menu.databaseReference.Child("Tournaments").GetValueAsync().ContinueWithOnMainThread(task =>
            {
                if (task.IsFaulted)
                {
                    Debug.LogError("Error getting data: " + task.Exception);
                }
                else if (task.IsCompleted)
                {
                    DataSnapshot snapshot = task.Result;
                        
                    string tournamentName = snapshot.Key;
                    int tournamentplayers = int.Parse(snapshot.Child("players").Value.ToString());
                    float tournamentbid = float.Parse(snapshot.Child("bid").Value.ToString());
                    string tournamenttype = snapshot.Child("type").Value.ToString();

                    newTour.SetData(tournamentName, tournamenttype, tournamentplayers, tournamentbid); 
                    tourPrefList.Add(newTour);
                }
            });

           
        }
    }
}