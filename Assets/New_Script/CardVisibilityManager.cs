using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;

public class CardVisibilityManager : MonoBehaviourPunCallbacks
{
    //public GameObject myCardImage;
    public GameObject opponentCardImage;

    PhotonView view;
        

    private void Start()
    {
        view = GetComponent<PhotonView>();

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
