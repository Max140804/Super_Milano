using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
public class uno_player : MonoBehaviour
{
    public List<Card> cards;
    public GameObject manager;
    public GameObject gameplay;
    public PhotonView pv;
    public PhotonView gameman_pv;

    public int currentPlayerIndex;


    public int viewid;

    public string numberofplayers;

    public int trybool;

    public float bid;
    public int fgdg;
    // Start is called before the first frame update

    private void Awake()
    {
        gameplay = GameObject.Find("GamePlay");
        numberofplayers = PhotonNetwork.CurrentRoom.CustomProperties["unoplayer"].ToString();
        bid = float.Parse(PhotonNetwork.CurrentRoom.CustomProperties["Bid"].ToString());

        trybool = PlayerPrefs.GetInt("ai");

        gameplay.GetComponent<GamePlayManager>().bid = bid;
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
            viewid = 4001;
        }

        if(trybool == 1)
        {
            viewid = 1001;

        }
        gameplay.GetComponent<GamePlayManager>().viewid = viewid;
        gameplay.GetComponent<GamePlayManager>().numberofplayers = int.Parse(numberofplayers);

        cards = new List<Card>(gameplay.GetComponent<GamePlayManager>().cards);
        gameman_pv = gameplay.GetComponent<PhotonView>();
        currentPlayerIndex = Random.Range(0, gameplay.GetComponent<GamePlayManager>().numberofplayers);
        if (pv.ViewID == viewid)
        {
            GameObject.Find("Game_Not_Started").SetActive(false);

            gameplay.GetComponent<GamePlayManager>().enabled = true;
        }
        if (pv.ViewID == viewid && pv.IsMine)
        {
            for (int i = 0; i < cards.Count; i++)
            {
                Card temp = cards[i];
                int randomIndex = Random.Range(i, cards.Count);

                gameman_pv.RPC("shuff", RpcTarget.All, i, randomIndex, currentPlayerIndex);

                cards[i] = cards[randomIndex];
                cards[randomIndex] = temp;
            }
        }


        if(pv.IsMine)
        {
            gameplay.GetComponent<GamePlayManager>().playercode = pv.ViewID;

        }
    }
    void Start()
    {
        manager = GameObject.Find("Uno_Manager");
        manager.GetComponent<Uno_Manager>().playercode = pv.ViewID;
    }

    // Update is called once per frame
    void Update()
    {
        fgdg = PhotonNetwork.CurrentRoom.Players.Count;
    }
}
