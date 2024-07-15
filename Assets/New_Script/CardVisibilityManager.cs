using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;

public class CardVisibilityManager : MonoBehaviourPunCallbacks, IPunObservable
{
    public Image myCardImage; // UI Image for the local player's card visual
    public Image opponentCardImage; // UI Image for opponents' card visual

    private Sprite cardVisual; // Reference to the card visual data

    private void Start()
    {
        if (photonView.IsMine)
        {
            // If this is the local player's card, set the card visual from cardData
            CardData cardData = GetComponent<CardData>();
            if (cardData != null)
            {
                cardVisual = cardData.cardData.cardVisual;
                myCardImage.sprite = cardVisual;
            }

            myCardImage.gameObject.SetActive(true);
            opponentCardImage.gameObject.SetActive(false);
        }
        else
        {
            // If this is an opponent's card, show a specified opponent card image
            myCardImage.gameObject.SetActive(false);
            opponentCardImage.gameObject.SetActive(true);
            // Set opponent card image or hide completely as needed
        }
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        // Implement serialization if needed to sync specific data
    }
}
