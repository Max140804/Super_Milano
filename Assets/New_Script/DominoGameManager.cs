using UnityEngine;
using System.Collections.Generic;
using Photon.Pun;

public class DominoGameManager : MonoBehaviour
{
    public GameObject dominoPrefab;
    public DominoCard[] dominoCards;
    public Transform spawnGB;
    public int tilesPerPlayer = 7;

    private List<GameObject> allDominoes = new List<GameObject>();
    private List<DominoHand> players = new List<DominoHand>();
    private DominoBoneYard boneYard;

    void Start()
    {
        
        for (int i = 0; i < dominoCards.Length; i++)
        {
            GameObject domino = CreateDominoCard(dominoCards[i]);
            allDominoes.Add(domino);
        }

        players.AddRange(FindObjectsOfType<DominoHand>());
        boneYard = FindObjectOfType<DominoBoneYard>();

        DealDominoes();
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

        int currentDominoIndex = 0;

        for (int i = 0; i < tilesPerPlayer; i++)
        {
            foreach (DominoHand player in players)
            {
                if (currentDominoIndex < allDominoes.Count)
                {
                    GameObject domino = allDominoes[currentDominoIndex];
                    domino.transform.SetParent(player.transform, false);
                    domino.transform.localPosition = Vector3.zero;
                    domino.transform.localRotation = Quaternion.identity;
                    domino.transform.localScale = Vector3.one;
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
            boneYard.AddToBoneYard(domino);
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
            int randomIndex = Random.Range(i, list.Count);
            list[i] = list[randomIndex];
            list[randomIndex] = temp;
        }
    }

    public DominoBoneYard GetBoneYard()
    {
        return boneYard;
    }
}
