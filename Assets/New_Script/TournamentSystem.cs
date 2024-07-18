using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Photon.Pun;
using Photon.Realtime;
using System;
using System.Collections;

public class TournamentSystem : MonoBehaviourPunCallbacks
{
    public TournamentDisplay tournamentPrefab;
    public Transform contentActive;
    public Transform contentAvailable;
    public TextMeshProUGUI statusText;
    public TextMeshProUGUI matchScheduleText;
    public InputField tournamentNameInputField;
    public int maxPlayersInputField = 16;
    public GameObject createPanel;
    public GameObject tournamentRoomPanel;
    public GameObject tournamentViewPanel;
    public TextMeshProUGUI roomName;
    public List<Text> playerNames;
    //public List<Text> readyStatus;

    MainMenu menu;

    List<TournamentDisplay> tournamentList = new List<TournamentDisplay>();
    private List<Player> players = new List<Player>();
    private List<Match> matches = new List<Match>();
    private int matchCounter = 1;
    private bool tournamentStarted = false;
    private List<NewTournamentCreation> availableTournaments = new List<NewTournamentCreation>();
    private List<NewTournamentCreation> activeTournaments = new List<NewTournamentCreation>();

    private Coroutine countdownCoroutine;

    private void Awake()
    {
        menu = FindObjectOfType<MainMenu>();
    }

    private void StartCountdown(NewTournamentCreation tournament)
    {
        if (countdownCoroutine != null)
        {
            StopCoroutine(countdownCoroutine);
        }
        countdownCoroutine = StartCoroutine(CountdownCoroutine(tournament));
    }

    IEnumerator CountdownCoroutine(NewTournamentCreation tournament)
    {
        while (true)
        {
            TimeSpan timeRemaining = tournament.GetTimeRemaining();
            if (timeRemaining <= TimeSpan.Zero || players.Count == 16)
            {
                tournament.StartTournament();
                
                break;
            }
            
        }
        yield return new WaitForSeconds(1);
    }


   /* public void AddPlayer(string playerName)
    {
        if (players.Count < 16)
        {
            GameObject playerObj = new GameObject(playerName);
            statusText.text = $"{playerName} joined the tournament!";
            if (players.Count >= 16)
            {
                StartTournament();
            }
        }
        else
        {
            statusText.text = "Maximum player limit reached.";
        }
    }*/


    private void CreateMatches()
    {
        int numPlayers = players.Count;
        int numMatches = numPlayers / 2;

        for (int i = 0; i < numMatches; i++)
        {
            Player player1 = players[i * 2];
            Player player2 = players[i * 2 + 1];
            DateTime matchTime = DateTime.Now.AddHours(i + 1);

            Match match = new Match(player1, player2, matchTime);
            matches.Add(match);

            matchCounter++;

            // Create a Photon room for each match
            RoomOptions roomOptions = new RoomOptions();
            roomOptions.IsVisible = true;
            roomOptions.IsOpen = true;
            roomOptions.MaxPlayers = 2; // Each match room can accommodate 2 players
            //PhotonNetwork.CreateRoom($"Match_{matchCounter}", roomOptions);

            // Find the button corresponding to this match and update its text
            /*Button matchButton = contentActive.transform.GetChild(i).GetComponent<Button>();
            if (matchButton != null)
            {
                Text buttonText = matchButton.GetComponentInChildren<Text>();
                if (buttonText != null)
                {
                    buttonText.text = $"Match {matchCounter}: {match.Player1.NickName} vs {match.Player2.NickName} at {match.MatchTime}";
                }
            }*/
        }
    }

    public void CreateNewTournament()
    {
        string tournamentName = tournamentNameInputField.text;
        int maxPlayers = 16;
        if (string.IsNullOrEmpty(tournamentName))
        {
            statusText.text = "Invalid tournament name or player count.";
            return;
        }

        DateTime startTime = DateTime.Now.AddMinutes(30);
        TimeSpan duration = TimeSpan.FromHours(4);
        menu.players = 16;
        NewTournamentCreation newTournament = new NewTournamentCreation(tournamentName, maxPlayers, startTime, duration);
        availableTournaments.Add(newTournament);
        statusText.text = $"Tournament '{tournamentName}' created with max {maxPlayers} players.";
        tournamentViewPanel.SetActive(true);
        DisplayAvailableTournaments();
        //StartCountdown(newTournament);
    }

    public override void OnCreateRoomFailed(short returnCode, string message)
    {
        Debug.LogError($"Room creation failed: {message}");
    }

    public override void OnCreatedRoom()
    {
        Debug.Log("Match room created successfully!");
    }

    public override void OnJoinedRoom()
    {
        Debug.Log("Joined match room!");
        createPanel.SetActive(false);
        //tournamentRoomPanel.SetActive(false);
        
        roomName.text = "Room Name: " + PhotonNetwork.CurrentRoom.Name;
    }

    // Function to display all available tournaments
    public void DisplayAvailableTournaments()
    {
        // Clear previous tournament display
        foreach (TournamentDisplay child in tournamentList)
        {
            Destroy(child.gameObject);
        }
        tournamentList.Clear();

        // Display each tournament
        foreach (NewTournamentCreation tournament in availableTournaments)
        {
            GameObject tournamentObj = Instantiate(tournamentPrefab.gameObject, contentAvailable);
            TournamentDisplay tournamentDisplay = tournamentObj.GetComponent<TournamentDisplay>();

            if (tournamentDisplay != null)
            {
                tournamentDisplay.SetTournamentInfo(tournament);
                tournamentList.Add(tournamentDisplay);
            }
        }
    }

    // Function to join a tournament
    public void JoinTournament(NewTournamentCreation tournament)
    {
        if (tournament.Players.Count < tournament.MaxPlayers)
        {
            // Assuming you have a method to get the current player's nickname
            string playerName = GetCurrentPlayerName();
            AddPlayerToTournament(tournament, playerName);
            statusText.text = $"Joined tournament: {tournament.Name}";
        }
        else
        {
            statusText.text = "Tournament is full.";
        }
    }

    private void AddPlayerToTournament(NewTournamentCreation tournament, string playerName)
    {
        GameObject playerObj = new GameObject(playerName);
       // Player newPlayer = playerObj.AddComponent<Photon.Realtime.Player>();
        //tournament.Players.Add(newPlayer);

        if (tournament.Players.Count >= tournament.MaxPlayers)
        {
            StartTournament(tournament);
        }
    }

    private void StartTournament(NewTournamentCreation tournament)
    {
        tournament.StartTournament();
        activeTournaments.Add(tournament);
        availableTournaments.Remove(tournament);
        DisplayActiveTournaments();

        if (!tournamentStarted)
        {
            tournamentStarted = true;
            CreateMatches();
        }
    }

    // Function to display active tournaments
    public void DisplayActiveTournaments()
    {
        foreach (TournamentDisplay child in tournamentList)
        {
            Destroy(child.gameObject);
        }
        tournamentList.Clear();

        // Display each tournament
        foreach (NewTournamentCreation tournament in availableTournaments)
        {
            GameObject tournamentObj = Instantiate(tournamentPrefab.gameObject, contentActive);
            TournamentDisplay tournamentDisplay = tournamentObj.GetComponent<TournamentDisplay>();

            if (tournamentDisplay != null)
            {
                tournamentDisplay.SetTournamentInfo(tournament);
                tournamentList.Add(tournamentDisplay);
            }
        }
    }

    public string GetCurrentPlayerName()
    {
        // Placeholder for getting the current player's name
        return PhotonNetwork.LocalPlayer.NickName;
    }

    // Example class to represent a match
    private class Match
    {
        public Player Player1 { get; private set; }
        public Player Player2 { get; private set; }
        public DateTime MatchTime { get; private set; }

        public Match(Player player1, Player player2, DateTime matchTime)
        {
            Player1 = player1;
            Player2 = player2;
            MatchTime = matchTime;
        }
    }

    public class NewTournamentCreation
    {
        public string Name { get; private set; }
        public int MaxPlayers { get; private set; }
        public List<Player> Players { get; private set; }
        public bool IsActive { get; private set; }
        [HideInInspector] public int timeRemainingTillStart;
        [HideInInspector] public string Status;

        private DateTime startTime;
        private TimeSpan duration;

        public NewTournamentCreation(string name, int maxPlayers, DateTime startTime, TimeSpan duration)
        {
            Name = name;
            MaxPlayers = maxPlayers;
            Players = new List<Player>();
            IsActive = false;
            this.startTime = startTime;
            this.duration = duration;
        }

        public void StartTournament()
        {
            IsActive = true;
            Status = "ACTIVE";
        }

        public TimeSpan GetTimeRemaining()
        {
            return startTime - DateTime.Now;
        }
    }

}
