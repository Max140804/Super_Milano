using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using System.Collections.Generic;

public class PlayerSpawner : MonoBehaviourPunCallbacks
{
    public GameObject playerPrefab;
    public Transform spawnPointDown;
    public Transform spawnPointUp;

    private Dictionary<int, GameObject> spawnedPlayers = new Dictionary<int, GameObject>();

    private void Start()
    {
        // Spawn existing players at the start
        SpawnAllPlayers();
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        // Spawn the newly joined player
        SpawnPlayer(newPlayer);
    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        // Remove the player who left
        if (spawnedPlayers.ContainsKey(otherPlayer.ActorNumber))
        {
            Destroy(spawnedPlayers[otherPlayer.ActorNumber]);
            spawnedPlayers.Remove(otherPlayer.ActorNumber);
        }
    }

    private void SpawnAllPlayers()
    {
        foreach (Player player in PhotonNetwork.PlayerList)
        {
            SpawnPlayer(player);
        }
    }

    private void SpawnPlayer(Player player)
    {
        if (spawnedPlayers.ContainsKey(player.ActorNumber))
        {
            // Player is already spawned
            return;
        }

        Transform spawnPoint = player == PhotonNetwork.LocalPlayer ? spawnPointDown : spawnPointUp;

        GameObject playerToSpawn = PhotonNetwork.Instantiate(playerPrefab.name, spawnPoint.position, spawnPoint.rotation);
        playerToSpawn.transform.SetParent(spawnPoint, false);
        playerToSpawn.transform.localPosition = Vector3.zero;
        playerToSpawn.transform.localScale = playerPrefab.transform.localScale;

        spawnedPlayers[player.ActorNumber] = playerToSpawn;
    }
}
