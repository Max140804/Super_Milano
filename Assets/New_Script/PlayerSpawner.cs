using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class PlayerSpawner : MonoBehaviourPunCallbacks
{
    public GameObject playerPrefab;
    public Transform spawnPoint;

    private Dictionary<int, GameObject> spawnedPlayers = new Dictionary<int, GameObject>();

    private void Awake()
    {
        GameObject instance = PhotonNetwork.Instantiate(playerPrefab.name, spawnPoint.position, Quaternion.identity);
        instance.transform.parent = spawnPoint;
    }

    /*private void Awake()
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

            // Set player name on the player instance
            playerInstance.GetComponent<DominoPlayerData>().SetPlayerName(player.NickName);

            // Set ownership
            PhotonView photonView = playerInstance.GetComponent<PhotonView>();
            photonView.TransferOwnership(player);

            // Track the spawned player
            spawnedPlayers[player.ActorNumber] = playerInstance;
        }
        else
        {
            Debug.LogError("Player prefab or spawn point is not assigned.");
        }
    }*/
}
