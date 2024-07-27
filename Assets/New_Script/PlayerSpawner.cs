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
        SpawnAllPlayers();
    }

    private void SpawnAllPlayers()
    {
        foreach (Player player in PhotonNetwork.PlayerList)
        {
            Transform spawnPoint = player == PhotonNetwork.LocalPlayer ? spawnPointDown : spawnPointUp;

            if (spawnedPlayers.ContainsKey(player.ActorNumber))
            {
                continue;
            }

            GameObject playerToSpawn = PhotonNetwork.Instantiate(playerPrefab.name, spawnPoint.position, spawnPoint.rotation);
            playerToSpawn.transform.SetParent(spawnPoint, false);
            playerToSpawn.transform.localPosition = Vector3.zero;
            playerToSpawn.transform.localScale = playerPrefab.transform.localScale;

            spawnedPlayers[player.ActorNumber] = playerToSpawn;
        }
    }
}
