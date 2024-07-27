using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using ExitGames.Client.Photon;

public class PlayerSpawner : MonoBehaviourPunCallbacks
{
    public GameObject playerPrefab;
    public Transform spawnPointDown;
    public Transform spawnPointUp;

    private void Start()
    {
        if (PhotonNetwork.IsConnected && PhotonNetwork.InRoom)
        {
            if (PhotonNetwork.LocalPlayer.CustomProperties.TryGetValue("PlayerSpawned", out object isPlayerSpawned) && (bool)isPlayerSpawned)
            {
                // Player is already spawned
                return;
            }

            SpawnPlayer();
        }
    }

    private void SpawnPlayer()
    {
        StartCoroutine(SpawnPlayerCoroutine());
    }

    private IEnumerator SpawnPlayerCoroutine()
    {
        // Get the local player's spawn point
        Transform spawnPoint = PhotonNetwork.LocalPlayer.IsMasterClient ? spawnPointDown : spawnPointUp;

        GameObject instance = PhotonNetwork.Instantiate(playerPrefab.name, spawnPoint.position, Quaternion.identity);

        yield return null;

        instance.transform.SetParent(spawnPoint);
        instance.transform.localScale = playerPrefab.transform.localScale;
        PhotonView photonView = instance.GetComponent<PhotonView>();
        if (photonView != null)
        {
            photonView.RPC("SetPlayerName", RpcTarget.AllBuffered, PhotonNetwork.NickName);
        }

        ExitGames.Client.Photon.Hashtable playerProperties = new ExitGames.Client.Photon.Hashtable { { "PlayerSpawned", true } };
        PhotonNetwork.LocalPlayer.SetCustomProperties(playerProperties);

        Debug.Log("Player prefab instantiated and parented: " + instance.name);
    }

    public override void OnJoinedRoom()
    {
        if (PhotonNetwork.LocalPlayer.CustomProperties.TryGetValue("PlayerSpawned", out object isPlayerSpawned) && (bool)isPlayerSpawned)
        {
            return;
        }

        SpawnPlayer();
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        if (!PhotonNetwork.IsMasterClient)
        {
            return;
        }

        if (newPlayer.CustomProperties.TryGetValue("PlayerSpawned", out object isPlayerSpawned) && (bool)isPlayerSpawned)
        {
            return;
        }

        photonView.RPC("RequestPlayerSpawn", newPlayer);
    }

    [PunRPC]
    private void RequestPlayerSpawn()
    {
        SpawnPlayer();
    }
}
