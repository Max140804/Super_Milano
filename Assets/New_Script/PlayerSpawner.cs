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
            GameObject instance = PhotonNetwork.Instantiate(playerPrefab.name, spawnPoint.position, Quaternion.identity);
            instance.transform.SetParent(spawnPoint);
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
        GameObject instance = PhotonNetwork.Instantiate(playerPrefab.name, spawnPoint.position, Quaternion.identity);
        instance.transform.SetParent(spawnPoint);
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        if (!PhotonNetwork.IsMasterClient)
        {
            GameObject instance = PhotonNetwork.Instantiate(playerPrefab.name, spawnPoint.position, Quaternion.identity);
            instance.transform.SetParent(spawnPoint);
        }
    }
}
