using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using System.Collections;

public class PlayerSpawner : MonoBehaviourPunCallbacks
{
    public GameObject playerPrefab;
    public Transform spawnPointDown;
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
        foreach (Player player in PhotonNetwork.PlayerList)
        {
            Transform spawnPoint = player == PhotonNetwork.LocalPlayer ? spawnPointDown : spawnPointUp;
            GameObject instance = PhotonNetwork.Instantiate(playerPrefab.name, spawnPoint.position, Quaternion.identity);
            // Wait until the next frame
            yield return null;

            // Set the parent after instantiation
            instance.transform.SetParent(spawnPoint);
            instance.transform.localScale = playerPrefab.transform.localScale;
            PhotonView photonView = instance.GetComponent<PhotonView>();
            if (photonView != null)
            {
                photonView.RPC("SetPlayerName", RpcTarget.AllBuffered, PhotonNetwork.NickName);
            }

            Debug.Log("Player prefab instantiated and parented: " + instance.name);

        }
      
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
