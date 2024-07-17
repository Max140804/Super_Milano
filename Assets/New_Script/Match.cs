using System;
using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;

public class Match : MonoBehaviour
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
