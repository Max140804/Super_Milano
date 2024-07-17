using System.Collections.Generic;
using UnityEngine;

public class DominoBoneYard : MonoBehaviour
{
    private List<GameObject> boneyard = new List<GameObject>();
    private DominoGameManager gameManager;

    public Animator anim;

    private void Awake()
    {
        gameManager = FindObjectOfType<DominoGameManager>();
        
    }

    public void AddToBoneYard(GameObject domino)
    {
        boneyard.Add(domino);
        domino.transform.SetParent(transform, false);
        domino.transform.localPosition = Vector3.zero;
        domino.transform.localRotation = Quaternion.identity;
        domino.transform.localScale = Vector3.one;

        ToggleAddButton(domino, true);
    }

    public bool RemoveFromBoneYard(GameObject domino)
    {
        if (boneyard.Contains(domino))
        {
            boneyard.Remove(domino);

            ToggleAddButton(domino, false);

            return true;
        }
        return false;
    }

    public bool Contains(GameObject domino)
    {
        return boneyard.Contains(domino);
    }

    public int GetBoneYardCount()
    {
        return boneyard.Count;
    }

    public GameObject GetDominoFromBoneYard(int index)
    {
        if (index >= 0 && index < boneyard.Count)
        {
            return boneyard[index];
        }
        return null;
    }

    public void AddToHand(DominoHand hand)
    {
        DominoBoneYard boneYard = gameManager.GetBoneYard();
        if (boneYard != null && boneYard.Contains(gameObject))
        {
            boneYard.RemoveFromBoneYard(gameObject);
            hand.AddToHand(gameObject);

            ToggleAddButton(gameObject, false);
        }
        else
        {
            Debug.LogWarning("This card is not in the boneyard.");
        }
    }


    public int GetBoneyardCount()
    {
        return boneyard.Count;
    }

    public GameObject GetDominoFromBoneYard()
    {
        if (boneyard.Count > 0)
        {
            GameObject domino = boneyard[0];
            boneyard.RemoveAt(0);
            return domino;
        }
        return null;
    }

    public bool ContainsDominoes()
    {
        return boneyard.Count > 0;
    }

    private void ToggleAddButton(GameObject domino, bool active)
    {
        GameObject addButton = domino.transform.Find("AddButton").gameObject;
        if (addButton != null)
        {
            addButton.SetActive(active);
        }
    }

    public void OpenBoneYard()
    {
        anim.Play("Open");
    }

    public void CloseBoneYard()
    {
        anim.Play("Close");
    }

    public void CollectAllCards(List<GameObject> allDominoes)
    {
        // Add logic to collect all cards back into the allDominoes list
        foreach (Transform child in transform)
        {
            allDominoes.Add(child.gameObject);
            child.SetParent(null);
        }
        ClearBoneYard();
    }

    public void ClearBoneYard()
    {
        // Clear the boneyard list
        boneyard.Clear();
    }
}
