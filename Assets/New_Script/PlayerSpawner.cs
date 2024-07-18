using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class PlayerSpawner : MonoBehaviour
{
    public GameObject playerPrefab;
    //public Transform spawnPoint;

    private void Awake()
    {
        if (PhotonNetwork.IsConnectedAndReady)
        {
            PhotonNetwork.Instantiate(playerPrefab.name, transform.position, Quaternion.identity);
        }
        else
        {
            StartCoroutine(WaitForConnection());
        }
    }

    private IEnumerator WaitForConnection()
    {
        while (!PhotonNetwork.IsConnectedAndReady)
        {
            yield return null;
        }

        PhotonNetwork.Instantiate(playerPrefab.name, transform.position, Quaternion.identity);
    }
}
