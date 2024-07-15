using UnityEngine;
using TMPro;
using Photon.Pun;

public class TurnManager : MonoBehaviourPun
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
        return PhotonNetwork.LocalPlayer.ActorNumber == (currentPlayerIndex + 1);
    }

    public void EndTurn()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            currentPlayerIndex = (currentPlayerIndex + 1) % totalPlayers;
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
        }
        else
        {
            turnText.text = "Waiting for players...";
        }
    }
}
