using Photon.Pun;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class DominoHand : MonoBehaviourPun
{
    private List<GameObject> hand = new List<GameObject>();
    public int totalScore;
    public TextMeshProUGUI scoreText;

    public void AddToHand(GameObject domino)
    {
        hand.Add(domino);
        domino.transform.SetParent(transform, false);
        domino.transform.localPosition = Vector3.zero;
        domino.transform.localRotation = Quaternion.identity;
        domino.transform.localScale = Vector3.one;
        ToggleAddButton(domino, false);
      
        PhotonView photonView = GetComponentInParent<PhotonView>();
        if (photonView != null)
        {
            photonView.TransferOwnership(PhotonNetwork.LocalPlayer);
        }
        else
        {
            
        }

        DominoHandMirror.Instance.AddToHand(PhotonNetwork.LocalPlayer.ActorNumber, domino);
    }

    public bool RemoveFromHand(GameObject domino)
    {
        if (domino == null)
        {
            Debug.LogError("Domino is null.");
            return false;
        }

        if (hand == null)
        {
            Debug.LogError("Hand is null.");
            return false;
        }

        if (hand.Contains(domino))
        {
            hand.Remove(domino);

            if (DominoHandMirror.Instance == null)
            {
                Debug.LogError("DominoHandMirror.Instance is null.");
                return false;
            }

            DominoHandMirror.Instance.RemoveFromHand(PhotonNetwork.LocalPlayer.ActorNumber, domino);
            //Debug.Log($"Domino removed from hand: {removed}");

            CheckForGameOver();
            return true;
        }
        else
        {
            Debug.LogError("Domino not found in hand.");
        }

        return false;
    }


    public bool Contains(GameObject domino)
    {
        return hand.Contains(domino);
    }

    public int GetHandCount()
    {
        return hand.Count;
    }

    public int GetTotalHandValue()
    {
        int totalValue = 0;
        foreach (GameObject domino in hand)
        {
            CardData cardData = domino.GetComponent<CardData>();
            if (cardData != null)
            {
                totalValue += cardData.topValue + cardData.bottomValue;
            }
        }
        return totalValue;
    }

    private void ToggleAddButton(GameObject domino, bool active)
    {
        GameObject addButton = domino.transform.Find("AddButton").gameObject;
        if (addButton != null)
        {
            addButton.SetActive(active);
        }
    }

    private void CheckForGameOver()
    {
        if (hand.Count == 0)
        {
            FindObjectOfType<DominoGameManager>().GameOver();
        }
    }

    [PunRPC]
    public void UpdateScoreText(int newScore)
    {
        totalScore = newScore;
        if (scoreText != null)
        {
            scoreText.text = "Score: " + totalScore.ToString();
        }
    }

    public void UpdateScore(int newScore)
    {
        photonView.RPC("UpdateScoreText", RpcTarget.AllBuffered, newScore);
    }

    public List<GameObject> GetDominoesInHand()
    {
        return new List<GameObject>(hand);
    }

    public void CollectAllCards(List<GameObject> allDominoes)
    {
        foreach (Transform child in transform)
        {
            allDominoes.Add(child.gameObject);
            child.SetParent(null);
        }
        ClearHand();
    }

    public void ClearHand()
    {
        hand.Clear();
    }
}
