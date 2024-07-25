using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;

public class PlayerSpawner : MonoBehaviourPunCallbacks
{
    public GameObject playerPrefab;
    public Transform spawnPointDown;
    public Transform spawnPointUp;

    private Dictionary<int, GameObject> playerInstances = new Dictionary<int, GameObject>();

    private void Start()
    {
        if (PhotonNetwork.IsConnected)
        {
            StartCoroutine(SpawnExistingPlayers());
        }
    }

    private IEnumerator SpawnExistingPlayers()
    {
        yield return new WaitForSeconds(1f);

        foreach (Player player in PhotonNetwork.PlayerList)
        {
            SpawnPlayerFor(player);
        }
    }

    private void SpawnPlayerFor(Player player)
    {
        Transform spawnPoint = player.IsLocal ? spawnPointDown : spawnPointUp;

        if (player.TagObject == null)
        {
            GameObject instance = PhotonNetwork.Instantiate(playerPrefab.name, spawnPoint.position, Quaternion.identity);

            Vector3 originalScale = instance.transform.localScale;
            instance.transform.SetParent(spawnPoint);
            instance.transform.localPosition = Vector3.zero;
            instance.transform.localScale = originalScale;

            player.TagObject = instance;
            playerInstances[player.ActorNumber] = instance;

            PhotonView photonView = instance.GetComponent<PhotonView>();
            if (photonView != null && player.IsLocal)
            {
                photonView.RPC("SetPlayerName", RpcTarget.AllBuffered, PhotonNetwork.NickName);
            }

            Debug.Log("Player prefab instantiated and parented: " + instance.name);
        }
        else
        {
            GameObject instance = (GameObject)player.TagObject;
            instance.transform.SetParent(spawnPoint);
            instance.transform.localPosition = Vector3.zero;

            Debug.Log("Player prefab already exists, re-parented: " + instance.name);
        }
    }

    public override void OnJoinedRoom()
    {
        if (!playerInstances.ContainsKey(PhotonNetwork.LocalPlayer.ActorNumber))
        {
            SpawnPlayerFor(PhotonNetwork.LocalPlayer);
        }
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        if (!playerInstances.ContainsKey(newPlayer.ActorNumber))
        {
            SpawnPlayerFor(newPlayer);
        }
    }
}
