using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

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
        GameObject instance = PhotonNetwork.Instantiate(playerPrefab.name, spawnPoint.position, Quaternion.identity);
        instance.transform.parent = spawnPoint;

        PhotonView photonView = instance.GetComponent<PhotonView>();
        if (photonView != null)
        {
            photonView.RPC("SetPlayerName", RpcTarget.AllBuffered, PhotonNetwork.NickName);
        }

        Debug.Log("Player prefab instantiated: " + instance.name);
    }

    public override void OnJoinedRoom()
    {
        SpawnPlayer();
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        if (!PhotonNetwork.IsMasterClient)
        {
            GameObject instance = PhotonNetwork.Instantiate(playerPrefab.name, spawnPoint.position, Quaternion.identity);
            instance.transform.parent = spawnPoint;

            PhotonView photonView = instance.GetComponent<PhotonView>();
            if (photonView != null)
            {
                photonView.RPC("SetPlayerName", RpcTarget.AllBuffered, PhotonNetwork.NickName);
            }
        }
    }
}
