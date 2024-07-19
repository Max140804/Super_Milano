using UnityEngine;
using TMPro;
using Photon.Pun;
using Photon.Realtime;

public class TurnManager : MonoBehaviourPunCallbacks
{
    public static TurnManager Instance;
    public TextMeshProUGUI turnText;

    private int currentPlayerIndex;
    private int totalPlayers;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        totalPlayers = PhotonNetwork.PlayerList.Length;

        if (PhotonNetwork.IsMasterClient)
        {
            currentPlayerIndex = 0;
            photonView.RPC("RPC_SetCurrentPlayerIndex", RpcTarget.All, currentPlayerIndex);
        }
        else
        {
            photonView.RPC("RPC_RequestCurrentPlayerIndex", RpcTarget.MasterClient);
        }

        UpdateTurnText();
    }

    public bool IsMyTurn()
    {
        return PhotonNetwork.LocalPlayer.ActorNumber == PhotonNetwork.PlayerList[currentPlayerIndex].ActorNumber;
    }

    public void EndTurn()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            currentPlayerIndex = (currentPlayerIndex + 1) % totalPlayers;
            Debug.Log("Turn ended. Next player index: " + currentPlayerIndex);
            photonView.RPC("RPC_SetCurrentPlayerIndex", RpcTarget.All, currentPlayerIndex);
        }
    }

    [PunRPC]
    private void RPC_SetCurrentPlayerIndex(int index)
    {
        currentPlayerIndex = index;
        UpdateTurnText();
    }

    [PunRPC]
    private void RPC_RequestCurrentPlayerIndex()
    {
        photonView.RPC("RPC_SetCurrentPlayerIndex", RpcTarget.All, currentPlayerIndex);
    }

    private void UpdateTurnText()
    {
        if (PhotonNetwork.PlayerList.Length > 0)
        {
            string playerName = PhotonNetwork.PlayerList[currentPlayerIndex].NickName;
            turnText.text = $"It's {playerName}'s turn";
            Debug.Log($"It's {playerName}'s turn");
        }
        else
        {
            turnText.text = "Waiting for players...";
            Debug.Log("Waiting for players...");
        }
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        totalPlayers = PhotonNetwork.PlayerList.Length;
        UpdateTurnText();
        Debug.Log(newPlayer.NickName + " entered the room. Total players: " + totalPlayers);
    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        totalPlayers = PhotonNetwork.PlayerList.Length;

        if (PhotonNetwork.IsMasterClient)
        {
            currentPlayerIndex = currentPlayerIndex % totalPlayers;
            photonView.RPC("RPC_SetCurrentPlayerIndex", RpcTarget.All, currentPlayerIndex);
        }

        UpdateTurnText();
        Debug.Log(otherPlayer.NickName + " left the room. Total players: " + totalPlayers);
    }
}
