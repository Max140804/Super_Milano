using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;

public class CardVisibilityManager : MonoBehaviourPunCallbacks, IPunObservable
{
    public Image myCardImage;
    public Image opponentCardImage;

    private Sprite cardVisual;
    private bool isPlayed; // Track if the card has been played

    private void Start()
    {
        if (photonView.IsMine)
        {
            myCardImage.gameObject.SetActive(true);
            opponentCardImage.gameObject.SetActive(false);
        }
        else
        {
            myCardImage.gameObject.SetActive(false);
            opponentCardImage.gameObject.SetActive(true);
        }
    }

    public void PlayCard()
    {
        if (photonView.IsMine && !isPlayed)
        {
            isPlayed = true;
            photonView.RPC("RPC_UpdateCardVisibility", RpcTarget.AllBuffered, isPlayed);
        }
    }

    [PunRPC]
    void RPC_UpdateCardVisibility(bool playedState)
    {
        isPlayed = playedState;
        UpdateCardVisibility();
    }

    void UpdateCardVisibility()
    {
        myCardImage.gameObject.SetActive(!isPlayed);
        opponentCardImage.gameObject.SetActive(isPlayed);
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            stream.SendNext(isPlayed);
        }
        else
        {
            isPlayed = (bool)stream.ReceiveNext();
            UpdateCardVisibility();
        }
    }
}
