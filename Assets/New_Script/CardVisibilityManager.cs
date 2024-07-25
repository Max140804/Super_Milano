using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;

public class CardVisibilityManager : MonoBehaviourPunCallbacks
{
    //public GameObject myCardImage;
    public GameObject opponentCardImage;

    PhotonView view;
        

    private void Update()
    {
        view = GetComponentInParent<PhotonView>();

        if (view.IsMine)
        {
            opponentCardImage.SetActive(false);
        }
        else
        {
            opponentCardImage.SetActive(true);
        }
    }


}
