using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using DominoTemplate.Controllers;
using System.Security.Cryptography;
public class Domino_player : MonoBehaviour
{
    public GameObject gameplay;
    public GameObject Deck;
    public PhotonView pv;
    public PhotonView gameman_pv;

    public int currentPlayerIndex;


    public int viewid;

    public string numberofplayers;

    public bool trybool;

    public int i = 27;


    private void Awake()
    {

        gameplay = GameObject.Find("MenuController");
        numberofplayers = PhotonNetwork.CurrentRoom.CustomProperties["unoplayer"].ToString();



        if (numberofplayers == "2")
        {
            viewid = 2001;
        }

        if (numberofplayers == "3")
        {
            viewid = 3001;
        }
        if (numberofplayers == "4")
        {
            if(PlayerPrefs.GetInt("ai") == 0)
            {
                viewid = 4001;

            }
            if (PlayerPrefs.GetInt("ai") == 1)
            {
                viewid = 1001;

            }
        }

        if (trybool)
        {
            if (PlayerPrefs.GetInt("ai") == 0)
            {
                viewid = 4001;

            }
            if (PlayerPrefs.GetInt("ai") == 1)
            {
                viewid = 1001;

            }
        }
        gameplay.GetComponent<MenuController>().viewid = viewid;
        gameplay.GetComponent<MenuController>().numberofplayers = int.Parse(numberofplayers);

        gameman_pv = gameplay.GetComponent<PhotonView>();

        if (pv.IsMine)
        {
            gameplay.GetComponent<MenuController>().playercode = pv.ViewID;
        }
        if (pv.ViewID == viewid && pv.IsMine)
        {
            pv.RPC("startgame", RpcTarget.All);
            Deck = GameObject.Find("DominoDeckController");

        }


 

        if (pv.ViewID == viewid && pv.IsMine)
            {
            DeckController car = GameObject.Find("DominoDeckController").GetComponent<DeckController>();

            car.playercode = pv.ViewID;
            while (i >= 0 && car._dominoTiles.Count > 0)
            {
                int m = Random.Range(0, i);

                pv.RPC("shuf", RpcTarget.All, m, i);

                i--;


            }
        }
        

        if (pv.IsMine)
        {
            gameplay.GetComponent<MenuController>().playercode = pv.ViewID;

        }
    }



    [PunRPC]
    public void shuf(int m, int i)
    {
        DeckController car = GameObject.Find("DominoDeckController").GetComponent<DeckController>();
            car.StartCoroutine(car.SetupRandomHands(m, i));
 
    }

    [PunRPC]
    public void startgame()
    {
        gameplay.GetComponent<MenuController>().StartDomino();

    }
   
}
