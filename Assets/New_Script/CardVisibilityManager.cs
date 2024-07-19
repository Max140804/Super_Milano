using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;

public class CardVisibilityManager : MonoBehaviourPunCallbacks
{
    public Image myCardImage;
    public Image opponentCardImage;

    PhotonView view;
        

    private void Awake()
    {
        view = GetComponent<PhotonView>();

        if (!view.IsMine)
            return;

        myCardImage.gameObject.SetActive(true);
        opponentCardImage.gameObject.SetActive(false);
    }

    
}
