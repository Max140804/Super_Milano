using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using System.Collections;

public class PlayerSpawner : MonoBehaviourPunCallbacks
{
    public GameObject playerPrefab;
    public Transform spawnPoint;

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
        SpawnPlayer();
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        if (!PhotonNetwork.IsMasterClient)
        {
            SpawnPlayer();
        }
    }
}
