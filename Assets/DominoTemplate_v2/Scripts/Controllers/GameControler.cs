using System;
using System.Collections.Generic;
using System.Collections;
using DominoTemplate.AI;
using DominoTemplate.Core;
using DominoTemplate.DragAndDrop;
using DominoTemplate.View;
using UnityEngine;
using Photon.Pun;

namespace DominoTemplate.Controllers
{
    public class GameControler : MonoBehaviour
    {
        public PhotonView pv;
        public int playercode;
        public static Action<List<RectTransform>, RectTransform> SetSlotPoosition;

        [HideInInspector] public DominoView currentDrag;

        [SerializeField] private RectTransform _gameBoard = null;
        [SerializeField] private EmptySlot _slotPrefab = null;
        [SerializeField] private SlotHelper _slotPosScript = null;
        [SerializeField] private HandController _handScript = null;
        [SerializeField] private AIController _AIScript = null;
        [SerializeField] private DeckController _deckScript = null;
        [SerializeField] private RectTransform _ownRect = null;
        [SerializeField] private GameTurnController _turnScript = null;

        private List<RectTransform> _slotList = new List<RectTransform>();

        private RectTransform slot;
        private RectTransform slot2;

        private bool _slotsCreated;

        public int ran = 9000;
        private void Awake()
        {
            pv.ViewID = 500;
        }
        private void OnEnable()
        {

        }

        private void OnDisable()
        {

        }

        private void Update()
        {
            _turnScript.GameProcess();
        }

        public RectTransform GetRect()
        {
            return _ownRect;
        }

        public void RestartGame(int difficulty)
        {
            SetupComponentReferences();
            _turnScript.InitGameVariables();

            _AIScript.SetDifficulty(difficulty);
            _deckScript.RestartDominos();
            _slotPosScript.RestartSlotHelper();
        }

        private void SetupComponentReferences()
        {
            _AIScript.SetAllRefs(this, _deckScript, _slotPosScript, _turnScript);
            _deckScript.SetAllRefs(this, _handScript, _turnScript);
            _handScript.SetAllRefs(_deckScript, _slotPosScript);
            _turnScript.SetAllRefs(_handScript, _deckScript, _AIScript);
        }

        // Creates Slot for Domino Tile
        [PunRPC]
        public void CreateSlot()
        {
            if (currentDrag == null)
            {
                Debug.Log("Something not right");
                return;
            }

            slot = null;
            slot2 = null;

            _slotsCreated = _slotPosScript.SetGamePositions(ref slot, ref slot2,
                currentDrag, _slotPrefab, _turnScript.GetPlayerTurn());

            if (slot != null)
            {
                slot.SetParent(_gameBoard, false);
                if (slot.GetComponent<PhotonView>().ViewID == 0)
                {
                    slot.GetComponent<PhotonView>().ViewID = ran + 1;
                    ran++;
                }
                _slotList.Add(slot);
            }

            if (slot2 != null)
            {
                slot2.SetParent(_gameBoard, false);
                if(slot2.GetComponent<PhotonView>().ViewID == 0)
                {
                    slot2.GetComponent<PhotonView>().ViewID = ran + 1;
                    ran++;
                }


                _slotList.Add(slot2);
            }

            SetSlotPoosition?.Invoke(_slotList, _gameBoard);
        }


        public void ConfirmSlotOccupation(ref RectTransform slotOccupied, Domino dominoInfo)
        {
            if (slotOccupied == null)
                return;

            if (slot != null)
            {
                if (slotOccupied.position == slot.position)
                {
                    // Right
                    _slotPosScript.ConfirmOccupation(slotOccupied, 0, dominoInfo);
                }
            }

            if (slot2 != null)
            {
                if (slotOccupied.position == slot2.position)
                {
                    // Left
                    _slotPosScript.ConfirmOccupation(slotOccupied, 1, dominoInfo);
                }
            }

            if (slotOccupied != null)
            {
                _turnScript.SetLastActivePlayer();
                _handScript.LockAllTiles();
                _turnScript.EndTurn(null, 0);
            }
        }


        public void RemoveAllSlots()
        {
            if (_slotList.Count > 0)
            {
                for(int i = 0; i < _slotList.Count; i++)
                {
                    Destroy(_slotList[i].gameObject);
                }
                _slotList.Clear();
            }
        }

        public bool GetInfoSlots()
        {
            return _slotsCreated;
        }

        public void GetSlots(ref RectTransform rightSlot, ref RectTransform leftSlot)
        {
            rightSlot = slot;
            leftSlot = slot2;
        }
    }
}