using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;

public class CardVisibilityManager : MonoBehaviourPunCallbacks
{
    public Image myCardImage;
    public Image opponentCardImage;

    private bool isPlayed = false;
    DominoGameManager cardManager;

    private void Awake()
    {
        cardManager = FindObjectOfType<DominoGameManager>();

        if (photonView.IsMine || cardManager.InBoneyard)
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

    
}
