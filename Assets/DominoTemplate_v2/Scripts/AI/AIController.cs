using System.Collections;
using System.Collections.Generic;
using DominoTemplate.Controllers;
using DominoTemplate.Core;
using DominoTemplate.DragAndDrop;
using DominoTemplate.View;
using Photon.Pun;
using UnityEngine;

namespace DominoTemplate.AI
{
    public class AIController : MonoBehaviour
    {
        
        [SerializeField] private AICheater _AICheaterScript = null;

        private GameControler _gameScript;
        private DeckController _deckScript;
        private SlotHelper _slotPosScript;
        private GameTurnController _turnScript;
        public PhotonView pv;
        public bool AI;
        private int _difficulty;


        void Start()
        {
            if (PlayerPrefs.GetInt("ai") == 0)
            {
                AI = false;
            }
            if (PlayerPrefs.GetInt("ai") == 1)
            {
                AI = true;

            }
        }
        public void SetAllRefs(GameControler newGame, DeckController newDeck, SlotHelper newSlot,
            GameTurnController newTurn)
        {
            _gameScript = newGame;
            _deckScript = newDeck;
            _slotPosScript = newSlot;
            _turnScript = newTurn;
        }

        public void SetDifficulty(int newDifficult)
        {
            _difficulty = newDifficult;
            _AICheaterScript.SetData(_deckScript, _slotPosScript,
                this, _difficulty);
        }

        public List<DragHandler> GetMatchingTiles(List<DragHandler> tempTiles)
        {
            int i;
            List<DragHandler> matchingTiles = new List<DragHandler>();
            int rightNum = -1;
            int leftNum = -1;

            _slotPosScript.TellBranchNums(ref rightNum, ref leftNum);
            i = 0;
            while (i < tempTiles.Count)
            {
                DominoView dominoView = tempTiles[i].GetDominoView();
                Domino dominoInfo = dominoView.GetDomino();

                if (dominoInfo.TopIndex == rightNum || dominoInfo.BottomIndex == rightNum
                                                    || dominoInfo.TopIndex == leftNum ||
                                                    dominoInfo.BottomIndex == leftNum
                                                    || rightNum == -1 || leftNum == -1)
                {
                    matchingTiles.Add(tempTiles[i]);
                }

                i++;
            }

            return matchingTiles;
        }

        private DragHandler PickRandomTile(List<DragHandler> tempTiles)
        {
            List<DragHandler> matchingTiles;
            int rand;

            matchingTiles = GetMatchingTiles(tempTiles);
            // Pass if no matching tiles
            if (matchingTiles.Count == 0)
                return null;
            rand = UnityEngine.Random.Range(0, matchingTiles.Count);

            return matchingTiles[rand];
        }


        private bool CheckStart()
        {
            int rightNum = -1;
            int leftNum = -1;

            _slotPosScript.TellBranchNums(ref rightNum, ref leftNum);
            if (rightNum == -1 || leftNum == -1)
                return true;
            return false;
        }

        private IEnumerator DropTile(DragHandler toDrop)
        {
            DominoView tempDominoView = toDrop.GetDominoView();

            yield return new WaitForSeconds(UnityEngine.Random.Range(0.5f, 3f));

            _gameScript.currentDrag = tempDominoView;
            _gameScript.pv.RPC("CreateSlot", RpcTarget.All);
            _gameScript.currentDrag = null;

            RectTransform rightSlot = null;
            RectTransform leftSlot = null;


            _gameScript.GetSlots(ref rightSlot, ref leftSlot);

            if (rightSlot != null && leftSlot != null)
            {
                int rand = UnityEngine.Random.Range(0, 2);

                if (CheckStart())
                    rand = 0;

                if (rand == 0)
                    toDrop.pv.RPC("LerpMove", RpcTarget.All, rightSlot.name, 1);
                else
                    toDrop.pv.RPC("LerpMove", RpcTarget.All, leftSlot.name, 1);
            }
            else if (rightSlot != null)
                toDrop.pv.RPC("LerpMove", RpcTarget.All, rightSlot.name, 1);
            else if (leftSlot != null)
                toDrop.pv.RPC("LerpMove", RpcTarget.All, leftSlot.name, 1);
            else
            {
                Debug.Log("Something wrong with slots");
                toDrop.pv.RPC("LerpMove", RpcTarget.All, rightSlot.name, 1);
            }

            yield return null;
        }



        public void PassTurn()
        {
            pv.RPC("pass", RpcTarget.All);

        }

        [PunRPC]
        public void pass()
        {
            Debug.Log("===========PASS==========");
            _turnScript.PassBehaviour();
            _turnScript.EndTurn("Pass", 1);
            _gameScript.RemoveAllSlots();
        }

        public void MakeTurn(int aiNum, bool nextPlayer)
        {
            if (AI)
            {
                List<DragHandler> AITiles;
                DragHandler selectedTile = null;

                if (aiNum < 2 || aiNum > 4)
                {
                    Debug.Log("Wrong AI Turn");
                    return;
                }


                AITiles = _deckScript.GetList(aiNum);
                if (_difficulty == 1)
                    selectedTile = PickRandomTile(AITiles);
                else
                {
                    List<DragHandler> nextTiles;
                    if (aiNum == 4)
                        nextTiles = _deckScript.GetList(1);
                    else
                        nextTiles = _deckScript.GetList(aiNum + 1);
                    selectedTile = _AICheaterScript.PickHardTile(AITiles, nextTiles, nextPlayer);
                }


                if (selectedTile == null)
                    PassTurn();
                else
                    StartCoroutine(DropTile(selectedTile));
            }
        }
    }
}