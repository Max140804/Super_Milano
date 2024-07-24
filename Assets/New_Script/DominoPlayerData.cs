using UnityEngine;
using TMPro;
using Photon.Pun;

public class DominoPlayerData : MonoBehaviourPunCallbacks
{
    public TextMeshProUGUI playerNameText;
    [HideInInspector]public string playerName;

    void Start()
    {
        if (photonView.IsMine)
        {
            playerName = PhotonNetwork.LocalPlayer.NickName;
            Debug.Log("Local player name: " + playerName);
            photonView.RPC("RPC_SetPlayerName", RpcTarget.AllBuffered, playerName);
        }
    }

    [PunRPC]
    public void RPC_SetPlayerName(string newPlayerName)
    {
        playerName = newPlayerName;
        playerNameText.text = playerName;
        Debug.Log("Player name set to: " + playerName);
    }

    public void SetPlayerName(string newPlayerName)
    {
        playerNameText.text = newPlayerName;
    }
}
