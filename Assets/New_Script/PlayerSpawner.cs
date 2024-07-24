using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class PlayerSpawner : MonoBehaviourPunCallbacks
{
    public GameObject playerPrefab; // The player prefab to be instantiated
    public Transform spawnPoint; // Single spawn point for players

    private Dictionary<int, GameObject> spawnedPlayers = new Dictionary<int, GameObject>();

    private void Awake()
    {
        // Spawn all players currently in the room
        SpawnAllPlayers();
    }

    public override void OnJoinedRoom()
    {
        // Spawn the local player when joining a room
        SpawnPlayer(PhotonNetwork.LocalPlayer);
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        // Spawn new player when they enter the room
        if (!spawnedPlayers.ContainsKey(newPlayer.ActorNumber))
        {
            SpawnPlayer(newPlayer);
        }
    }

    private void SpawnAllPlayers()
    {
        // Spawn all players who are already in the room
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
        if (playerPrefab != null && spawnPoint != null)
        {
            // Instantiate player prefab at the spawn point
            Vector3 spawnPosition = spawnPoint.position;
            Quaternion spawnRotation = Quaternion.identity;

            GameObject playerInstance = PhotonNetwork.Instantiate(playerPrefab.name, spawnPosition, spawnRotation);
            playerInstance.transform.parent = transform;
            playerInstance.GetComponent<DominoPlayerData>().SetPlayerName(player.NickName);

            // Track the spawned player
            spawnedPlayers[player.ActorNumber] = playerInstance;
        }
        else
        {
            Debug.LogError("Player prefab or spawn point is not assigned.");
        }
    }
}
