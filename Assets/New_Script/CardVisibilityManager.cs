using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;

public class CardVisibilityManager : MonoBehaviourPunCallbacks, IPunObservable
{
    public Image myCardImage;
    public Image opponentCardImage;

    private Sprite cardVisual; 

    private void Start()
    {
        if (photonView.IsMine)
        {
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
            myCardImage.gameObject.SetActive(false);
            opponentCardImage.gameObject.SetActive(true);
        }
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        
    }
}
