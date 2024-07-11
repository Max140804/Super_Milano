using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using System.IO;
using UnityEngine.UI;
using ExitGames.Client.Photon;
using Lean.Localization;

public class Uno_Manager : MonoBehaviourPunCallbacks
{
    public int playercode;
    public int turn;

    public LeanToken roomnum;

    public bool isntantiated;
    public GameObject musicbackground;
    public GameObject soundbackground;
    void Awake()
    {
        if (PlayerPrefs.GetInt("music") == 1)
        {
            musicbackground.SetActive(true);

        }
        else
        {
            musicbackground.SetActive(false);

        }
        if (PlayerPrefs.GetInt("sound") == 1)
        {
            soundbackground.SetActive(true);
        }
        else
        {
            soundbackground.SetActive(false);

        }
    }

    void Update()
    {
        if (PhotonNetwork.IsConnectedAndReady && isntantiated == false)
        {
            // Instantiate the player prefab for the local player
            GameObject player = PhotonNetwork.Instantiate(Path.Combine("PhotonPrefabs", "UnoPlayer"), Vector3.zero, Quaternion.identity);
            isntantiated = true;
        }
        roomnum.Value = PhotonNetwork.CurrentRoom.Name;

    }
}
