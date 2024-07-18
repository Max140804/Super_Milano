using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class PlayerSpawner : MonoBehaviourPunCallbacks
{
    public GameObject playerPrefab;
    private Dictionary<int, GameObject> spawnedPlayers = new Dictionary<int, GameObject>();

    private void Awake()
    {
        SpawnAllPlayers();
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        SpawnPlayer(newPlayer);
    }

    private void SpawnAllPlayers()
    {
        foreach (Player player in PhotonNetwork.PlayerList)
        {
            if (!spawnedPlayers.ContainsKey(player.ActorNumber))
            {
                SpawnPlayer(player);
            }
        }
    }

    private void SpawnPlayer(Player player)
    {
        
        if (playerPrefab != null)
        {
            Vector3 spawnPosition = transform.position;
            Quaternion spawnRotation = Quaternion.identity;

            GameObject playerInstance = PhotonNetwork.Instantiate(playerPrefab.name, spawnPosition, spawnRotation);
            playerInstance.transform.parent = transform;

            spawnedPlayers[player.ActorNumber] = playerInstance;
        }
        else
        {
            Debug.LogError("Player prefab is not assigned.");
        }
    }
}
