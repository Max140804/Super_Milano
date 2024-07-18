using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class NewTournament : MonoBehaviour
{

    public string Name { get; private set; }
    public int MaxPlayers { get; private set; }
    public List<Player> Players { get; private set; }
    public bool IsActive { get; private set; }

    public NewTournament(string name, int maxPlayers)
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
