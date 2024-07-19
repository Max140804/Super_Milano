using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;

public class CardVisibilityManager : MonoBehaviourPunCallbacks, IPunObservable
{
    public Image myCardImage;
    public Image opponentCardImage;

    private Sprite cardVisual;
    private bool isPlayed = false;

    private void Awake()
    {
        // Initialize visibility based on ownership
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
        // Ensure visibility updates based on the current state
        if (photonView.IsMine)
        {
            myCardImage.gameObject.SetActive(!isPlayed);
            opponentCardImage.gameObject.SetActive(false);
        }
        else
        {
            myCardImage.gameObject.SetActive(false);
            opponentCardImage.gameObject.SetActive(!isPlayed);
        }
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
