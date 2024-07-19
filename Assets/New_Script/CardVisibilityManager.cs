using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;

public class CardVisibilityManager : MonoBehaviourPunCallbacks, IPunObservable
{
    public Image myCardImage;
    public Image opponentCardImage;

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
        Debug.Log("Awake called, myCardImage active: " + myCardImage.gameObject.activeSelf + ", opponentCardImage active: " + opponentCardImage.gameObject.activeSelf);
    }

    public void PlayCard()
    {
        if (photonView.IsMine && !isPlayed)
        {
            isPlayed = true;
            photonView.RPC("RPC_UpdateCardVisibility", RpcTarget.AllBuffered, isPlayed);
            Debug.Log("PlayCard executed, calling RPC_UpdateCardVisibility with isPlayed: " + isPlayed);
        }
    }

    [PunRPC]
    void RPC_UpdateCardVisibility(bool playedState)
    {
        isPlayed = playedState;
        Debug.Log("RPC_UpdateCardVisibility received, updating isPlayed to: " + isPlayed);
        UpdateCardVisibility();
    }

    void UpdateCardVisibility()
    {
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
        Debug.Log("UpdateCardVisibility called, myCardImage active: " + myCardImage.gameObject.activeSelf + ", opponentCardImage active: " + opponentCardImage.gameObject.activeSelf);
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            stream.SendNext(isPlayed);
            Debug.Log("Sending isPlayed: " + isPlayed);
        }
        else
        {
            isPlayed = (bool)stream.ReceiveNext();
            Debug.Log("Received isPlayed: " + isPlayed);
            UpdateCardVisibility();
        }
    }
}
