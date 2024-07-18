using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TournamentDisplay : MonoBehaviour
{
    public TextMeshProUGUI tournamentNameText;
    public TextMeshProUGUI isActiveText;
    public TextMeshProUGUI timeTillStartText;
    TournamentSystem system;
    TournamentSystem.NewTournamentCreation tour;

    private void Awake()
    {
        system = FindObjectOfType<TournamentSystem>();
    }

    public void SetTournamentInfo(TournamentSystem.NewTournamentCreation tournament)
    {
        tournamentNameText.text = tournament.Name;
        TimeSpan timeRemaining = tournament.GetTimeRemaining();
        isActiveText.text = "ACTIVE";
        timeTillStartText.text = $"Starts In: {timeRemaining.Hours:D2}:{timeRemaining.Minutes:D2}:{timeRemaining.Seconds:D2}";
        tour = tournament;
    }

    public void OnClick()
    {
        system.JoinTournament(tour);
    }
}


