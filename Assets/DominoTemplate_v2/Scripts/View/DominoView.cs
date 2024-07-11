using DominoTemplate.Core;
using UnityEngine;
using UnityEngine.UI;

namespace DominoTemplate.View
{
    public class DominoView : MonoBehaviour
    {
        [SerializeField] private Image _dominoImage = null;
        [SerializeField] private Button _currentButton = null;
        [SerializeField] private Domino _currentDomino = null;
        [SerializeField] private RectTransform _dominoTransform = null;
        [SerializeField] private GameObject _backObject;

        private bool _playerMove = false;
        private bool _onTable;
        public bool AI;

        public bool IsOnTable()
        {
            return _onTable;
        }

        public void OnTable()
        {
            _currentButton.interactable = true;
            _currentDomino.Available = false;
            _dominoTransform.localScale = Vector3.one;
            _onTable = true;
        }

        public void ChangeBackState(bool state)
        {
            _backObject.SetActive(state);
        }

        public void OnAIHands()
        {
            _dominoTransform.localScale = new Vector3(0.5f, 0.5f, 0);
            if (AI)
            {
                _currentDomino.Available = false;
            }
            if (!AI)
            {
                _currentDomino.Available = true;

            }
        }

        public void UnLockTile(bool isPlayer)
        {
            _currentButton.interactable = true;
            if (AI)
            {
                _currentDomino.Available = isPlayer;
            }
            if (!AI)
            {
                _currentDomino.Available = true;

            }
        }

        public void LockTile()
        {
            if (!_onTable)
            {
                _currentButton.interactable = false;
                _currentDomino.Available = false;
            }
        }


        public Domino GetDomino()
        {
            return _currentDomino;
        }

        public void SetDomino(Domino domino, Sprite dominoSprite)
        {
            _currentDomino = domino;
            _dominoImage.sprite = dominoSprite;
        }

        private void UpdateState()
        {
            _currentButton.interactable = _playerMove && _currentDomino.Available;
        }

        public void StartPosition()
        {
            _dominoTransform.anchoredPosition = Vector2.zero;
            _dominoTransform.localScale = Vector3.one;
            _dominoTransform.sizeDelta = new Vector2(60f, 120f);
        }

        public void UpdateRotation()
        {
            _dominoTransform.localRotation =
                _currentDomino.PortraitOrientation ? Quaternion.Euler(0, 0, 0) : Quaternion.Euler(0, 0, 90f);
        }
    }
}