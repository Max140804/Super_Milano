using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class DominoPlayerManager : MonoBehaviourPunCallbacks
{
    public GameObject playerPrefab;

    public override void OnJoinedRoom()
    {
        base.OnJoinedRoom();
        SpawnPlayer();
    }

    void SpawnPlayer()
    {
        Vector3 spawnPosition = new Vector3(0, 0, 0);
        GameObject playerObject = PhotonNetwork.Instantiate(playerPrefab.name, spawnPosition, Quaternion.identity);
    }
}
