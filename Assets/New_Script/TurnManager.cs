using UnityEngine;
using TMPro;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.SceneManagement; // For scene management

public class TurnManager : MonoBehaviourPunCallbacks
{
    public static TurnManager Instance;
    public TextMeshProUGUI turnText;

    private int currentPlayerIndex;
    private int totalPlayers;
    private bool isOfflineMode;

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
        isOfflineMode = SceneManager.GetActiveScene().buildIndex == 5;
        if (isOfflineMode)
        {
            // Initialize offline mode
            StartOfflineTurnManagement();
        }
        else
        {
            // Initialize Photon Network mode
            StartPhotonTurnManagement();
        }
    }

    private void StartOfflineTurnManagement()
    {
        // For offline mode, assume 2 players: local player and AI
        currentPlayerIndex = 0; // Start with the local player
        totalPlayers = 2; // Local player + AI
        UpdateTurnText();
    }

    private void StartPhotonTurnManagement()
    {
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
        if (isOfflineMode)
        {
            // Offline mode: return true if it's the local player's turn
            return currentPlayerIndex == 0; // Assuming index 0 is the local player
        }
        else
        {
            // Photon Network mode
            return PhotonNetwork.LocalPlayer.ActorNumber == PhotonNetwork.PlayerList[currentPlayerIndex].ActorNumber;
        }
    }

    public void EndTurn()
    {
        if (isOfflineMode)
        {
            // Offline mode: switch between local player and AI
            currentPlayerIndex = (currentPlayerIndex + 1) % totalPlayers;
            UpdateTurnText();
        }
        else
        {
            // Photon Network mode
            if (PhotonNetwork.IsMasterClient)
            {
                currentPlayerIndex = (currentPlayerIndex + 1) % totalPlayers;
                Debug.Log("Turn ended. Next player index: " + currentPlayerIndex);
                photonView.RPC("RPC_SetCurrentPlayerIndex", RpcTarget.All, currentPlayerIndex);
            }
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
        if (isOfflineMode)
        {
            // Offline mode: update turn text for local player vs AI
            string playerName = currentPlayerIndex == 0 ? "Player" : "AI";
            turnText.text = $"It's {playerName}'s turn";
            Debug.Log($"It's {playerName}'s turn");
        }
        else
        {
            // Photon Network mode
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
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        if (!isOfflineMode)
        {
            totalPlayers = PhotonNetwork.PlayerList.Length;
            UpdateTurnText();
            Debug.Log(newPlayer.NickName + " entered the room. Total players: " + totalPlayers);
        }
    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        if (!isOfflineMode)
        {
            totalPlayers = PhotonNetwork.PlayerList.Length;

            if (PhotonNetwork.IsMasterClient)
            {
                if (currentPlayerIndex >= totalPlayers)
                {
                    currentPlayerIndex = 0;
                }
                photonView.RPC("RPC_SetCurrentPlayerIndex", RpcTarget.All, currentPlayerIndex);
            }

            UpdateTurnText();
            Debug.Log(otherPlayer.NickName + " left the room. Total players: " + totalPlayers);
        }
    }
}
