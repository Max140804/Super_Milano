using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using System.Collections;

public class PlayerSpawner : MonoBehaviourPunCallbacks
{
    public GameObject playerPrefab;
    public Transform SpawnpointDown;
    public Transform spawnPointUp;

    private void Start()
    {
        if (PhotonNetwork.IsConnected)
        {
            SpawnPlayer();
        }
    }

    private void SpawnPlayer()
    {
        StartCoroutine(SpawnPlayerCoroutine());
    }

    private IEnumerator SpawnPlayerCoroutine()
    {
        Transform spawnPoint = PhotonNetwork.LocalPlayer.IsLocal ? SpawnpointDown : spawnPointUp;

        GameObject instance = PhotonNetwork.Instantiate(playerPrefab.name, spawnPoint.position, Quaternion.identity);

        yield return null;

        instance.transform.SetParent(spawnPoint);

        PhotonView photonView = instance.GetComponent<PhotonView>();
        if (photonView != null)
        {
            photonView.RPC("SetPlayerName", RpcTarget.AllBuffered, PhotonNetwork.NickName);
        }

        Debug.Log("Player prefab instantiated and parented: " + instance.name);
    }

    public override void OnJoinedRoom()
    {
        GameObject instance = PhotonNetwork.Instantiate(playerPrefab.name, SpawnpointDown.position, Quaternion.identity);
        instance.transform.SetParent(SpawnpointDown);
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        if (PhotonNetwork.LocalPlayer == newPlayer)
        { 
            GameObject instance = PhotonNetwork.Instantiate(playerPrefab.name, SpawnpointDown.position, Quaternion.identity);
            instance.transform.SetParent(SpawnpointDown);
        }
        else
        {
            GameObject instance = PhotonNetwork.Instantiate(playerPrefab.name, spawnPointUp.position, Quaternion.identity);
            instance.transform.SetParent(spawnPointUp);
        }
    }
}
