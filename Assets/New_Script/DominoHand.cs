using System.Collections.Generic;
using UnityEngine;

public class DominoHand : MonoBehaviour
{
    private List<GameObject> hand = new List<GameObject>();

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
            return true;
        }
        return false;
    }

    public bool Contains(GameObject domino)
    {
        return hand.Contains(domino);
    }

    private void ToggleAddButton(GameObject domino, bool active)
    {
        GameObject addButton = domino.transform.Find("AddButton").gameObject;
        if (addButton != null)
        {
            addButton.SetActive(active);
        }
    }
}
