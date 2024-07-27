using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;

public class PlayerSpawner : MonoBehaviourPunCallbacks
{
    public GameObject playerPrefab;
    public Transform spawnPointDown;
    public Transform spawnPointUp;

    private void Start()
    {
        Transform spawnPoint = null;
        if (PhotonNetwork.IsMasterClient)
        {
            spawnPoint = spawnPointDown;
        }
        else
        {
            spawnPoint = spawnPointUp;
        }

        GameObject playerToSpawn = PhotonNetwork.Instantiate(playerPrefab.name, spawnPoint.position, spawnPoint.rotation);
        playerToSpawn.transform.localScale = playerToSpawn.transform.localScale;
        playerToSpawn.transform.parent = spawnPoint;

    }
}
