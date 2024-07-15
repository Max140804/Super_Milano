using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using System.IO;
using UnityEngine.UI;
using Lean.Localization;

public class Domino_Manager : MonoBehaviour
{
    public LeanToken roomnum;
    public bool isntantiated; 
    public GameObject musicbackground;
    public GameObject soundbackground;
    void Awake()
    {
        if (PlayerPrefs.GetInt("music") == 1)
        {
            musicbackground.SetActive(true);

        }
        else
        {
            musicbackground.SetActive(false);

        }
        if (PlayerPrefs.GetInt("sound") == 1)
        {
            soundbackground.SetActive(true);
        }
        else
        {
            soundbackground.SetActive(false);

        }
    }

    void Update()
    {
        if (PhotonNetwork.IsConnectedAndReady && isntantiated == false)
        {
            GameObject player = PhotonNetwork.Instantiate(Path.Combine("PhotonPrefabs", "DominoPlayer"), gameObject.transform.position, gameObject.transform.rotation);
            isntantiated = true;

        }
        roomnum.Value = PhotonNetwork.CurrentRoom.Name;

    }
}
