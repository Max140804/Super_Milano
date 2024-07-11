using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DominoTemplate.Controllers;
using DominoTemplate.View;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Photon.Pun;
using DominoTemplate.Core;

namespace DominoTemplate.DragAndDrop
{
    [RequireComponent(typeof(Image))]
    public class DragHandler : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        public PhotonView pv;
        public GameControler gameScript;

        [SerializeField] private RectTransform _dragObject = null;
        [SerializeField] private DominoView _dominoView = null;
        [SerializeField] private Button _currentButton = null;

        public List<RectTransform> _slots = new List<RectTransform>();
        public Dictionary<RectTransform, float> _slotDictionary;

        public RectTransform _gameBoard;
        private Vector2 _startPosition;
        private float timeOfTravel = 0.5f;
        private float currentTime;
        private float normalizedValue;
        private bool changeParent;
        private float _yOffset;

        private void Start()
        {
            gameScript = GameObject.Find("GameController(Clone)").GetComponent<GameControler>();
            _yOffset = Screen.height / 3f;
        }

        private void OnEnable()
        {
            GameControler.SetSlotPoosition += Init;
        }

        private void OnDisable()
        {
            GameControler.SetSlotPoosition -= Init;
        }

        private void Init(List<RectTransform> slots, RectTransform board)
        {
            _slots = slots;
            _gameBoard = board;
        }

        public DominoView GetDominoView()
        {
            return _dominoView;
        }

        // -> IBeginDragHandler, IDragHandler, IEndDragHandler all this for that functions <- //
        public void OnBeginDrag(PointerEventData eventData)
        {
            if (!_currentButton.interactable)
                return;
            if (!_dominoView.GetDomino().Available)
                return;
            pv.RPC("before", RpcTarget.All);
            gameScript.pv.RPC("CreateSlot", RpcTarget.All);
            pv.RPC("after", RpcTarget.All);
            SetDraggedPosition(eventData);
        }

        [PunRPC]
        public void before()
        {
            gameScript.currentDrag = _dominoView;
        }
        [PunRPC]
        public void after()
        {
            _slotDictionary = new Dictionary<RectTransform, float>(_slots.Count);
            _startPosition = _dragObject.anchoredPosition;
        }
        [PunRPC]
        public void enddrag()
        {
            gameScript.currentDrag = null;

        }
        public void OnDrag(PointerEventData eventData)
        {
            if (!_currentButton.interactable)
                return;
            if (!_dominoView.GetDomino().Available)
                return;

            SetDraggedPosition(eventData);
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            if (!_currentButton.interactable)
                return;
            if (!_dominoView.GetDomino().Available)
                return;


            pv.RPC("enddrag", RpcTarget.All);
            RectTransform near = CheckTheNearestSlot();
            if (_dragObject.position.y > _yOffset / 1.5f && gameScript.GetInfoSlots() && near != null)
            {
                near.gameObject.GetComponent<EmptySlot>().pv.RPC("changename", RpcTarget.All, UnityEngine.Random.Range(0, 9432));

                pv.RPC("LerpMove", RpcTarget.All, near.name, 1);
                Debug.Log("nas" + near.name);

            }
            else
            {

                near.gameObject.GetComponent<EmptySlot>().pv.RPC("changename", RpcTarget.All, UnityEngine.Random.Range(0, 9432));

                pv.RPC("LerpMove", RpcTarget.All, near.name, 0);
                Debug.Log("nas" + near.name);

            }
        }

        private void SetDraggedPosition(PointerEventData data)
        {
            // Function for setting this pos to mouse pos
            Vector3 globalMousePos;

            if (RectTransformUtility.ScreenPointToWorldPointInRectangle(_dragObject, data.position,
                    data.pressEventCamera,
                    out globalMousePos))
            {
                _dragObject.position = globalMousePos;
            }
        }

        [PunRPC]
        public void LerpMove(string near, int i)
        {
            RectTransform nearestSlot = GameObject.Find(near).GetComponent<RectTransform>();
            Debug.Log(nearestSlot.name + "+" + i);
            currentTime = 0;
            normalizedValue = 0;
            changeParent = false;


            while (currentTime < timeOfTravel)
            {
                currentTime += Time.deltaTime;
                normalizedValue = currentTime / timeOfTravel;

                if (i == 1)
                {
                    _dominoView.ChangeBackState(false);

                    // This will drag to slot
                    if (!changeParent)
                    {
                        changeParent = true;
                        _dragObject.sizeDelta = nearestSlot.sizeDelta;
                        _dragObject.rotation = nearestSlot.rotation;
                        _dragObject.SetParent(_gameBoard, true);
                        _dragObject.localScale = Vector3.one;
                    }
                    var distanceToSlot = Vector2.Distance(_dragObject.position, nearestSlot.position);

                    _dragObject.position = Vector3.Lerp(_dragObject.position, nearestSlot.position, normalizedValue);

                    if (distanceToSlot <= 1f)
                    {
                    _dragObject.position = nearestSlot.position;

                        _dominoView.OnTable();
                        gameScript.ConfirmSlotOccupation(ref nearestSlot, _dominoView.GetDomino());
                        gameScript.RemoveAllSlots();
                        return; // Exiting the method
                    }
                }
                else
                {

                    // This will drag to start
                    var distanceToStart = Vector2.Distance(_dragObject.anchoredPosition, _startPosition);

                    _dragObject.anchoredPosition =
                        Vector3.Lerp(_dragObject.anchoredPosition, _startPosition, normalizedValue);

                    nearestSlot = null;
                    if (distanceToStart <= 1f)
                    {
                        _dragObject.anchoredPosition = _startPosition;

                        gameScript.ConfirmSlotOccupation(ref nearestSlot, _dominoView.GetDomino());
                        gameScript.RemoveAllSlots();
                        return; // Exiting the method
                    }
                }
            }
        }
        private RectTransform CheckTheNearestSlot()
        {
            _slotDictionary.Clear();

            RectTransform result = null;

            for (int i = 0; i < _slots.Count; i++)
            {
                _slotDictionary.Add(_slots[i], Vector2.Distance(_dragObject.position, _slots[i].position));
            }

            var ordered = _slotDictionary.OrderBy(x => x.Value);

            foreach (var value in ordered)
            {
                if (result == null)
                {
                    result = value.Key;
                    break;
                }
            }

            return result;
        }


        private RectTransform _nextParent;
        private bool _standing;
        private bool _onAI;

        public void SendToNextHand(RectTransform nextHand, bool stand, bool onAI)
        {
            _nextParent = nextHand;
            _standing = stand;
            _onAI = onAI;

            StartCoroutine(FlyToHand());
        }

        private IEnumerator FlyToHand()
        {
            float localTimeOfTravel = 1f;
            currentTime = 0;
            normalizedValue = 0;
            changeParent = false;

            while (currentTime < localTimeOfTravel)
            {
                currentTime += Time.deltaTime;
                normalizedValue = currentTime / localTimeOfTravel;


                if (!changeParent)
                {
                    changeParent = true;
                    _dragObject.rotation = _nextParent.rotation;
                    _dragObject.localScale = Vector3.one;
                }

                var distanceToSlot = Vector2.Distance(_dragObject.position, _nextParent.position);
                _dragObject.position = Vector3.Lerp(_dragObject.position, _nextParent.position, normalizedValue);

                if (distanceToSlot <= 1f)
                {
                    _dragObject.position = _nextParent.position;

                    if (_standing)
                        _dragObject.localRotation = Quaternion.Euler(0, 0, 0);
                    else
                        _dragObject.localRotation = Quaternion.Euler(0, 0, 90);

                    _dragObject.SetParent(_nextParent, false);
                    if (_onAI)
                        _dominoView.OnAIHands();

                    yield break;
                }

                yield return null;
            }
        }
    }
}