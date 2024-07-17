using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class DominoHand : MonoBehaviour
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
    }

    public bool RemoveFromHand(GameObject domino)
    {
        if (hand.Contains(domino))
        {
            hand.Remove(domino);
            CheckForGameOver();
            return true;
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

    public void UpdateScoreText()
    {
        if (scoreText != null)
        {
            scoreText.text = totalScore.ToString();
        }
    }

    public void CollectAllCards(List<GameObject> allDominoes)
    {
        // Add logic to collect all cards back into the allDominoes list
        foreach (Transform child in transform)
        {
            allDominoes.Add(child.gameObject);
            child.SetParent(null);
        }
        ClearHand();
    }

    public void ClearHand()
    {
        // Clear the hand list
        hand.Clear();
    }
}
