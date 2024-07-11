using Photon.Pun;
using UnityEngine;
using UnityEngine.UI;

namespace DominoTemplate.Core
{
    public class EmptySlot : MonoBehaviour
    {
        [SerializeField] private RectTransform _ownTransform = null;
        [SerializeField] private Image _ownImage = null;
        public PhotonView pv;

        [PunRPC]
        public void changename(int name)
        {
            string relname = name.ToString();
            gameObject.name = relname;
        }


            public RectTransform GetOwnRectTransform()
        {
            return _ownTransform;
        }

        public void BecomeInvisible()
        {
            _ownImage.color = Color.clear;
        }
    }
}