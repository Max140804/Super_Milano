using System.Collections.Generic;
using DominoTemplate.Core;
using DominoTemplate.DragAndDrop;
using DominoTemplate.View;
using UnityEngine;
using UnityEngine.UI;

namespace DominoTemplate.Controllers
{
    public class HandController : MonoBehaviour
    {
        private DeckController _deckScript;
        private SlotHelper _slotPosScript;

        [SerializeField] private Button playerPassButton = null;

        public void SetAllRefs(DeckController newDeck, SlotHelper newSlot)
        {
            _deckScript = newDeck;
            _slotPosScript = newSlot;
        }

        private void UnlockTile(List<DragHandler> tempList, bool isPlayer)
        {
            int i;

            i = 0;
            while (i < tempList.Count)
            {
                tempList[i].GetDominoView().UnLockTile(isPlayer);
                i++;
            }
        }

        public void LockTilesHere(List<DragHandler> tempList)
        {
            int i;

            i = 0;
            while (i < tempList.Count)
            {
                tempList[i].GetDominoView().LockTile();
                i++;
            }
        }

        public void LockAllTiles()
        {
            int i;

            i = 1;
            while (i < 5)
            {
                LockTilesHere(_deckScript.GetList(i));
                i++;
            }
        }

        public void SetLockPlayerButtons(bool theLock)
        {
            playerPassButton.interactable = theLock;
        }


        public void SetTurnFor(bool player, bool leftAI, bool topAI, bool rightAI)
        {
            LockAllTiles();
            SetLockPlayerButtons(false);
            if (player)
            {
                if (PlayerAvalibleTiles() == 0)
                    playerPassButton.interactable = true;
                else
                    playerPassButton.interactable = false;
            }
            else if (leftAI)
                UnlockTile(_deckScript.GetList(2), false);
            else if (topAI)
                UnlockTile(_deckScript.GetList(3), false);
            else if (rightAI)
                UnlockTile(_deckScript.GetList(4), false);
        }


        private int PlayerAvalibleTiles()
        {
            int rightNum = -1;
            int leftNum = -1;
            int i = 0;
            List<DragHandler> playerTileList = _deckScript.GetList(1);
            int unlocked = 0;

            _slotPosScript.TellBranchNums(ref rightNum, ref leftNum);
            while (i < playerTileList.Count)
            {
                DominoView dominoView = playerTileList[i].GetDominoView();
                Domino dominoInfo = dominoView.GetDomino();

                if (dominoInfo.TopIndex == rightNum || dominoInfo.BottomIndex == rightNum
                                                    || dominoInfo.TopIndex == leftNum ||
                                                    dominoInfo.BottomIndex == leftNum)
                {
                    dominoView.UnLockTile(true);
                    unlocked++;
                }
                else if (rightNum == -1 || leftNum == -1)
                {
                    dominoView.UnLockTile(true);
                    unlocked++;
                }

                i++;
            }

            return unlocked;
        }
    }
}