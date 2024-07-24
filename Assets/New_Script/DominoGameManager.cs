using UnityEngine;
using System.Collections.Generic;
using TMPro;
using Photon.Pun;
using Photon.Realtime;
using Firebase;
using Firebase.Database;
using System;
using System.Collections;
using ExitGames.Client.Photon;

public enum GameModes
{
    OneVsOne,
    AllFives
}

public class DominoGameManager : MonoBehaviourPunCallbacks
{
    public GameObject dominoPrefab;
    public DominoCard[] dominoCards;
    public Transform spawnGB;
    public int finalScore = 50;

    private List<GameObject> allDominoes = new List<GameObject>();
    private List<DominoHand> players = new List<DominoHand>();
    private DominoBoneYard boneYard;

    private DatabaseReference databaseReference;

    public GameModes gameMode = GameModes.OneVsOne;
    public bool isTournamentGame = false;
    TurnManager turn;
    public bool InBoneyard;

    private const byte TournamentMatchEndEventCode = 1;

    private void Awake()
    {
        turn = FindObjectOfType<TurnManager>();
        PhotonNetwork.AutomaticallySyncScene = true;
    }

    void Start()
    {
        InitializeDominoes();
        players.AddRange(FindObjectsOfType<DominoHand>());
        boneYard = FindObjectOfType<DominoBoneYard>();

        DealDominoes();

        FirebaseApp.CheckAndFixDependenciesAsync().ContinueWith(task =>
        {
            FirebaseApp app = FirebaseApp.DefaultInstance;
            databaseReference = FirebaseDatabase.DefaultInstance.RootReference;
        });
    }

    void InitializeDominoes()
    {
        foreach (var card in dominoCards)
        {
            GameObject domino = CreateDominoCard(card);
            allDominoes.Add(domino);
        }
    }

    GameObject CreateDominoCard(DominoCard cardData)
    {
        GameObject dominoInstance = Instantiate(dominoPrefab, spawnGB);

        CardData cardDataComponent = dominoInstance.GetComponent<CardData>();
        if (cardDataComponent != null)
        {
            cardDataComponent.AssignCardData(cardData);
        }
        else
        {
            Debug.LogError("CardData component not found on the prefab.");
        }

        return dominoInstance;
    }

    void DealDominoes()
    {
        Shuffle(allDominoes);

        int tilesPerPlayer = GetTilesPerPlayer();
        int currentDominoIndex = 0;

        foreach (DominoHand player in players)
        {
            for (int i = 0; i < tilesPerPlayer; i++)
            {
                if (currentDominoIndex < allDominoes.Count)
                {
                    GameObject domino = allDominoes[currentDominoIndex];
                    domino.transform.SetParent(player.transform, false);
                    domino.transform.localPosition = Vector3.zero;
                    domino.transform.localRotation = Quaternion.identity;
                    domino.transform.localScale = Vector3.one;

                    // Set the ownership of the domino to the player
                    PhotonView photonView = domino.GetComponent<PhotonView>();
                    if (photonView != null)
                    {
                        photonView.TransferOwnership(player.GetComponent<PhotonView>().Owner);
                    }
                    else
                    {
                        Debug.Log("PhotonView component not found on the domino prefab.");
                    }

                    player.AddToHand(domino);
                    currentDominoIndex++;
                }
            }
        }

        // Add remaining tiles to the boneYard
        for (int i = currentDominoIndex; i < allDominoes.Count; i++)
        {
            GameObject domino = allDominoes[i];
            domino.transform.SetParent(boneYard.transform, false);
            domino.transform.localPosition = Vector3.zero;
            domino.transform.localRotation = Quaternion.identity;
            domino.transform.localScale = Vector3.one;

            if (domino.GetComponent<PhotonView>() != null)
            {
                // Optionally, you can also set ownership for the boneYard if needed
                // but typically the bone yard doesn't need this.
            }

            boneYard.AddToBoneYard(domino);
        }
    }


    int GetTilesPerPlayer()
    {
        switch (gameMode)
        {
            case GameModes.OneVsOne:
                return 7;
            case GameModes.AllFives:
                return 5;
            default:
                return 7;
        }
    }

    public void RemoveFromHand(GameObject domino)
    {
        foreach (DominoHand player in players)
        {
            if (player.RemoveFromHand(domino))
            {
                return;
            }
        }

        if (boneYard.RemoveFromBoneYard(domino))
        {
            return;
        }
    }

    void Shuffle(List<GameObject> list)
    {
        for (int i = 0; i < list.Count; i++)
        {
            GameObject temp = list[i];
            int randomIndex = UnityEngine.Random.Range(i, list.Count);
            list[i] = list[randomIndex];
            list[randomIndex] = temp;
        }
    }

    public DominoBoneYard GetBoneYard()
    {
        return boneYard;
    }

    public void GameOver()
    {
        DominoHand winningPlayer = null;
        int totalRemainingValues = 0;

        foreach (DominoHand player in players)
        {
            if (player.GetHandCount() == 0)
            {
                winningPlayer = player;
            }
            else
            {
                totalRemainingValues += player.GetTotalHandValue();
            }
        }

        if (winningPlayer != null)
        {
            foreach (DominoHand player in players)
            {
                int scoreToAdd = 0;

                if (player == winningPlayer)
                {
                    scoreToAdd = totalRemainingValues;

                    if (gameMode == GameModes.AllFives)
                    {
                        scoreToAdd = ((scoreToAdd + 4) / 5) * 5;
                    }

                    player.UpdateScore(player.totalScore + scoreToAdd);
                    StartCoroutine(UpdateGameResult(new GameResultData { PlayerId = player.gameObject.name, Result = scoreToAdd }));
                }
                else
                {
                    player.UpdateScore(player.totalScore);
                    StartCoroutine(UpdateGameResult(new GameResultData { PlayerId = player.gameObject.name, Result = 0 }));
                }
            }

            Debug.Log($"Game Over! The winning player is {winningPlayer.gameObject.name} with a total remaining value of {totalRemainingValues} for other players.");

            if (isTournamentGame)
            {
                if (CheckForFinalScore())
                {
                    EndTournamentMatch();
                }
                else
                {
                    StartCoroutine(ResetAndDealNewRound());
                }
            }
            else
            {
                if (CheckForFinalScore())
                {
                    EndMatch();
                    Debug.Log($"Game Over! The winning player is {winningPlayer.gameObject.name} with a total remaining value of {totalRemainingValues} for other players.");
                }
                else
                {
                    StartCoroutine(ResetAndDealNewRound());
                }
            }
        }
        else
        {
            Debug.LogError("Game Over called but no player has an empty hand.");
        }
    }

    bool CheckForFinalScore()
    {
        foreach (DominoHand player in players)
        {
            if (player.totalScore >= finalScore)
            {
                return true;
            }
        }
        return false;
    }

    void EndMatch()
    {
        Debug.Log("Match has ended as a player has reached the final score.");
        PhotonNetwork.LeaveRoom();
        PhotonNetwork.LoadLevel(0);
    }

    void EndTournamentMatch()
    {
        Debug.Log("Tournament match has ended as a player has reached the final score.");
        List<string> winners = new List<string>();

        foreach (DominoHand player in players)
        {
            if (player.totalScore >= finalScore)
            {
                winners.Add(player.gameObject.name);
            }
        }

        object[] content = new object[] { winners.ToArray() };
        PhotonNetwork.RaiseEvent(TournamentMatchEndEventCode, content, new RaiseEventOptions { Receivers = ReceiverGroup.All }, SendOptions.SendReliable);
    }

    IEnumerator ResetAndDealNewRound()
    {
        yield return new WaitForSeconds(2);


        foreach (var domino in allDominoes)
        {
            domino.transform.SetParent(spawnGB, false);
            domino.transform.localPosition = Vector3.zero;
            domino.transform.localRotation = Quaternion.identity;
            domino.transform.localScale = Vector3.one;
        }
        foreach (DominoHand player in players)
        {
            player.ClearHand();
        }
        boneYard.ClearBoneYard();

        DealDominoes();
    }

    IEnumerator UpdateGameResult(GameResultData data)
    {
        var task = databaseReference.Child("players").Child(data.PlayerId).Child("gameResult").SetValueAsync(data.Result);
        yield return new WaitUntil(() => task.IsCompleted);
    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        Debug.Log($"{otherPlayer.NickName} left the room.");
    }
}

public class GameResultData
{
    public string PlayerId { get; set; }
    public int Result { get; set; }
}
