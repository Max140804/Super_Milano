using UnityEngine;
using System.Collections.Generic;

public class DominoGameManager : MonoBehaviour
{
    public GameObject dominoPrefab; // Reference to the prefab
    public DominoCard[] dominoCards; // Array of all possible domino cards
    public Transform spawnGB; // Parent GameObject for spawning domino cards
    public int tilesPerPlayer = 7; // Number of tiles per player

    private List<GameObject> allDominoes = new List<GameObject>();
    private List<DominoHand> players = new List<DominoHand>();
    private DominoBoneYard boneYard;

    void Start()
    {
        // Create and assign data to domino cards
        for (int i = 0; i < dominoCards.Length; i++)
        {
            GameObject domino = CreateDominoCard(dominoCards[i]);
            allDominoes.Add(domino);
        }

        // Find players and the boneyard
        players.AddRange(FindObjectsOfType<DominoHand>());
        boneYard = FindObjectOfType<DominoBoneYard>();

        // Shuffle and deal the dominoes
        DealDominoes();
    }

    GameObject CreateDominoCard(DominoCard cardData)
    {
        // Instantiate the prefab
        GameObject dominoInstance = Instantiate(dominoPrefab, spawnGB);

        // Get the CardData component and assign the card data
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
        // Shuffle the dominoes
        Shuffle(allDominoes);

        // Deal the tiles
        int currentDominoIndex = 0;

        for (int i = 0; i < tilesPerPlayer; i++)
        {
            foreach (DominoHand player in players)
            {
                if (currentDominoIndex < allDominoes.Count)
                {
                    GameObject domino = allDominoes[currentDominoIndex];
                    domino.transform.SetParent(player.transform, false); // Set as child of the DominoHand GameObject
                    domino.transform.localPosition = Vector3.zero; // Reset local position
                    domino.transform.localRotation = Quaternion.identity; // Reset local rotation
                    domino.transform.localScale = Vector3.one; // Reset local scale
                    player.AddToHand(domino);
                    currentDominoIndex++;
                }
            }
        }

        // Add remaining tiles to the boneYard
        for (int i = currentDominoIndex; i < allDominoes.Count; i++)
        {
            GameObject domino = allDominoes[i];
            domino.transform.SetParent(boneYard.transform, false); // Set as child of the DominoBoneYard GameObject
            domino.transform.localPosition = Vector3.zero; // Reset local position
            domino.transform.localRotation = Quaternion.identity; // Reset local rotation
            domino.transform.localScale = Vector3.one; // Reset local scale
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
