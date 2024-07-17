using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;
using System;

public class TournamentSystem : MonoBehaviourPunCallbacks
{
    public GameObject tournamentPrefab;
    public GameObject content;
    public Text statusText;
    public Text matchScheduleText;

    public List<Text> playerNames;
    public List<Text> readyStatus;

    private List<Player> players = new List<Player>();
    private List<Match> matches = new List<Match>();
    private int matchCounter = 1;
    private bool tournamentStarted = false;

    private List<Tournament> availableTournaments = new List<Tournament>();
    private List<Tournament> activeTournaments = new List<Tournament>();

    // Example method to add players to the tournament
    public void AddPlayer(string playerName)
    {
        if (players.Count < 16) // Adjust maximum players as needed
        {
            GameObject playerObj = new GameObject(playerName);
            //Player newPlayer = playerObj.AddComponent<Player>();
            //players.Add(newPlayer);
            statusText.text = $"{playerName} joined the tournament!";

            // Automatically start the tournament if enough players
            if (players.Count >= 16)
            {
                StartTournament();
            }
        }
        else
        {
            statusText.text = "Maximum player limit reached.";
        }
    }

    private void StartTournament()
    {
        if (!tournamentStarted)
        {
            tournamentStarted = true;
            CreateMatches();
        }
    }

    // Method to create matches for the tournament
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
            matchScheduleText.text += $"Match {matchCounter}: {match.Player1.NickName} vs {match.Player2.NickName} at {match.MatchTime}\n";
            matchCounter++;

            // Create a Photon room for each match
            RoomOptions roomOptions = new RoomOptions();
            roomOptions.IsVisible = true;
            roomOptions.IsOpen = true;
            roomOptions.MaxPlayers = 2; // Each match room can accommodate 2 players
            PhotonNetwork.CreateRoom($"Match_{matchCounter}", roomOptions);
        }
    }

    // Photon Callbacks
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

    }

    // Function to display all available tournaments
    public void DisplayAvailableTournaments()
    {
        // Clear previous tournament display
        foreach (Transform child in content.transform)
        {
            Destroy(child.gameObject);
        }

        // Display each tournament
        foreach (Tournament tournament in availableTournaments)
        {
            GameObject tournamentObj = Instantiate(tournamentPrefab, content.transform);
            TournamentDisplay tournamentDisplay = tournamentObj.GetComponent<TournamentDisplay>();

            if (tournamentDisplay != null)
            {
                tournamentDisplay.SetTournamentInfo(tournament);
                Button joinButton = tournamentDisplay.GetJoinButton();
                joinButton.onClick.AddListener(() => JoinTournament(tournament));
            }
        }
    }

    // Function to join a tournament
    public void JoinTournament(Tournament tournament)
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

    private void AddPlayerToTournament(Tournament tournament, string playerName)
    {
        GameObject playerObj = new GameObject(playerName);
        //Player newPlayer = playerObj.AddComponent<Player>();
        //tournament.Players.Add(newPlayer);

        if (tournament.Players.Count >= tournament.MaxPlayers)
        {
            StartTournament(tournament);
        }
    }

    private void StartTournament(Tournament tournament)
    {
        tournament.StartTournament();
        activeTournaments.Add(tournament);
        availableTournaments.Remove(tournament);
        DisplayActiveTournaments();
    }

    // Function to display active tournaments
    public void DisplayActiveTournaments()
    {
        // Clear previous tournament display
        foreach (Transform child in content.transform)
        {
            Destroy(child.gameObject);
        }

        // Display each active tournament
        foreach (Tournament tournament in activeTournaments)
        {
            GameObject tournamentObj = Instantiate(tournamentPrefab, content.transform);
            TournamentDisplay tournamentDisplay = tournamentObj.GetComponent<TournamentDisplay>();

            if (tournamentDisplay != null)
            {
                tournamentDisplay.SetTournamentInfo(tournament);
            }
        }
    }

    public string GetCurrentPlayerName()
    {
        // Placeholder for getting the current player's name
        return "Player_" + UnityEngine.Random.Range(1, 1000).ToString();
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

    public class Tournament
    {
        public string Name { get; private set; }
        public int MaxPlayers { get; private set; }
        public List<Player> Players { get; private set; }
        public bool IsActive { get; private set; }

        public Tournament(string name, int maxPlayers)
        {
            Name = name;
            MaxPlayers = maxPlayers;
            Players = new List<Player>();
            IsActive = false;
        }

        public void StartTournament()
        {
            IsActive = true;
        }
    }
}

public class TournamentDisplay : MonoBehaviour
{
    public Text tournamentNameText;
    public Button joinButton;

    public void SetTournamentInfo(TournamentSystem.Tournament tournament)
    {
        tournamentNameText.text = tournament.Name;
    }

    public Button GetJoinButton()
    {
        return joinButton;
    }
}
