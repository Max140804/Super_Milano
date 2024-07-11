using DominoTemplate.Core;
using DominoTemplate.View;
using UnityEngine;

namespace DominoTemplate.DragAndDrop
{
    public class SlotHelper : MonoBehaviour
    {
        public float tileOffset = 60;
        public float lyingOffset = 30;
        public float tileDist_hor;

        // Tiles;
        public float _rightTiles_Hor;
        public float _rightTiles_Ver;
        public int _rightPhase;
        public int _rightNum;

        public float _leftTiles_Hor;
        public float _leftTiles_Ver;
        public int _leftPhase;
        public int _leftNum;

        public bool started;

        public void RestartSlotHelper()
        {
            _rightTiles_Hor = 0;
            _rightTiles_Ver = 0;
            _rightPhase = 0;

            _leftTiles_Hor = 0;
            _leftTiles_Ver = 0;
            _leftPhase = 0;

            _rightNum = -1;
            _leftNum = -1;

            started = false;
        }

        public void TellBranchNums(ref int rightBranch, ref int leftBranch)
        {
            rightBranch = _rightNum;
            leftBranch = _leftNum;
        }

        private void ConfirmMoving(ref float tile_Hor, ref float tile_Ver, ref int phase, RectTransform slot,
            int scenario)
        {
            float vertShift;
            int sideLimit = 13;
            int shift1;
            int shift2;

            if (scenario == 0)
            {
                // right branch
                shift1 = -1;
                shift2 = -2;
                vertShift = -1.5f;
            }
            else
            {
                // left branch
                shift1 = 1;
                shift2 = 2;
                vertShift = 1.5f;
            }
            Debug.Log("til" + tile_Hor);

            Debug.Log("pase" + phase);
            Debug.Log("rot" + slot.localRotation.z);
            // this is first turn
            if (phase == 0)
            {
                // If Standing
                if (slot.localRotation.z == 0)
                    tile_Hor -= shift1;
                else
                    tile_Hor -= shift2;

                if (scenario == 0 && tile_Hor > sideLimit && tile_Ver == 0)
                    phase++;
                else if (scenario == 1 && tile_Hor < -sideLimit && tile_Ver == 0)
                    phase++;
            }
            else if (phase == 1)
            {
                tile_Ver += vertShift;
                tile_Hor -= shift1;
                phase++;
            }
            else if (phase == 2)
            {
                tile_Ver += vertShift;
                tile_Hor += shift1;
                phase++;
            }
            else if (phase == 3)
            {
                tile_Hor += shift2;
                phase++;
            }
            else
            {
                // Phase 4
                // If Standing 
                if (slot.localRotation.z == 90)
                    tile_Hor += shift1;
                else
                    tile_Hor += shift2;
            }
            Debug.Log("til" + tile_Hor);
            Debug.Log("rot" + slot.rotation.z);

            Debug.Log("pase" + phase);
        }


        public void ConfirmOccupation(RectTransform slot, int scenario, Domino dominoInfo)
        {
            if (dominoInfo.TopIndex < 0 || dominoInfo.BottomIndex < 0)
                Debug.Log("Weird Domino info!");
            if (started)
            {
                started = false;
                _rightTiles_Hor -= 0.5f;
                _leftTiles_Hor += 0.5f;
            }

            if (_rightNum == -1 && _leftNum == -1)
            {
                _rightNum = dominoInfo.TopIndex;
                _leftNum = dominoInfo.BottomIndex;
            }

            // Right side
            if (scenario == 0)
            {
                ConfirmMoving(ref _rightTiles_Hor, ref _rightTiles_Ver, ref _rightPhase, slot, scenario);
                if (_rightNum == dominoInfo.TopIndex)
                    _rightNum = dominoInfo.BottomIndex;
                else
                    _rightNum = dominoInfo.TopIndex;
            }

            // Left side
            if (scenario == 1)
            {
                ConfirmMoving(ref _leftTiles_Hor, ref _leftTiles_Ver, ref _leftPhase, slot, scenario);
                if (_leftNum == dominoInfo.TopIndex)
                    _leftNum = dominoInfo.BottomIndex;
                else
                    _leftNum = dominoInfo.TopIndex;
            }


            Debug.Log("Right branch shift Hor " + _rightTiles_Hor + " Ver " + _rightTiles_Ver + " Phase " +
                      _rightPhase);
            Debug.Log("Left branch shift Hor " + _leftTiles_Hor + " Ver " + _leftTiles_Ver + " Phase " + _leftPhase);
            Debug.Log("Right num -> " + _rightNum + " | " + _leftNum + " <- Left num");
        }


        private void SetSlotPosition(ref RectTransform slot, bool standing, int scenario, Domino dominoInfo)
        {
            int phase;
            int branchNum;
            float tile_Ver;
            float tile_Hor;
            float horOffset;
            float angle;
            float resAngle = 0;
            float horShift;

            if (scenario == 0)
            {
                // Right
                tile_Hor = _rightTiles_Hor;
                tile_Ver = _rightTiles_Ver;
                phase = _rightPhase;
                branchNum = _rightNum;
                horOffset = lyingOffset;
                angle = 90;
                horShift = -tileDist_hor;
            }
            else
            {
                // Left
                tile_Hor = _leftTiles_Hor;
                tile_Ver = _leftTiles_Ver;
                phase = _leftPhase;
                branchNum = _leftNum;
                horOffset = -lyingOffset;
                angle = -90;
                horShift = tileDist_hor;
            }

            // This must correctly place domino tile
            if (standing)
            {
                if (phase < 2)
                    slot.anchoredPosition = new Vector2((tileOffset * tile_Hor), tileOffset * tile_Ver);
                else if (phase == 2)
                    slot.anchoredPosition = new Vector2((tileOffset * tile_Hor) + horShift, tileOffset * tile_Ver);
                else
                    slot.anchoredPosition = new Vector2((tileOffset * tile_Hor + horOffset * 2), tileOffset * tile_Ver);
            }
            else
            {
                resAngle += angle;
                slot.anchoredPosition = new Vector2((tileOffset * tile_Hor + horOffset), tileOffset * tile_Ver);
            }

//		slot.anchoredPosition =  new Vector2 (slot.anchoredPosition.x * sizeOfTable, slot.anchoredPosition.y * sizeOfTable);
            // This must correctly rotate domino tile
            if (branchNum != -1)
            {
                if (dominoInfo.TopIndex != branchNum && phase < 2)
                    resAngle += 180;

                if (scenario == 0)
                {
                    if (dominoInfo.TopIndex != branchNum && phase == 2)
                        resAngle += 180;
                    if (dominoInfo.BottomIndex != branchNum && phase > 2)
                        resAngle += 180;
                }
                else
                {
                    if (dominoInfo.BottomIndex != branchNum && phase >= 2)
                        resAngle += 180;
                }
            }

            slot.localRotation = Quaternion.Euler(0, 0, resAngle);
            slot.localScale = Vector3.one;
        }

        public bool SetGamePositions(ref RectTransform slot1, ref RectTransform slot2,
            DominoView dominoTile, EmptySlot slotPrefab, bool playerTurn)
        {
            bool standing = false;
            Domino tileInfo = dominoTile.GetDomino();

            EmptySlot slotScript1;
            EmptySlot slotScript2;
            // Check if slot could exist
            // The result is this
            if (tileInfo.TopIndex == _rightNum
                || tileInfo.BottomIndex == _rightNum || _rightNum == -1)
            {
                slotScript1 = Instantiate(slotPrefab);
                slot1 = slotScript1.GetOwnRectTransform();
                if (!playerTurn)
                    slotScript1.BecomeInvisible();
            }

            if (tileInfo.TopIndex == _leftNum
                || tileInfo.BottomIndex == _leftNum || _leftNum == -1)
            {
                slotScript2 = Instantiate(slotPrefab);
                slot2 = slotScript2.GetOwnRectTransform();
                if (!playerTurn)
                    slotScript2.BecomeInvisible();
            }


            if (slot1 == null && slot2 == null)
                return false;

            // Right scenario
            if (slot1 != null)
            {
                standing = CheckStanding(tileInfo, _rightPhase);
                SetSlotPosition(ref slot1, standing, 0, tileInfo);
            }

            // Left scenario
            if (slot2 != null)
            {
                standing = CheckStanding(tileInfo, _leftPhase);
                SetSlotPosition(ref slot2, standing, 1, tileInfo);
            }

            // Case if Center (only happens on start)
            if (_rightTiles_Hor == 0 && _rightTiles_Ver == 0 && _leftTiles_Hor == 0 && _leftTiles_Ver == 0)
            {
                if (slot1 != null)
                    slot1.anchoredPosition = new Vector2(0, 0);
                if (slot2 != null)
                    slot2.anchoredPosition = new Vector2(0, 0);
                if (standing)
                    started = false;
                else
                    started = true;
            }

            return true;
        }


        private bool CheckStanding(Domino tileInfo, int phase)
        {
            bool standing;

            standing = false;

            if (tileInfo.id == 0 || tileInfo.id == 7 || tileInfo.id == 13 || tileInfo.id == 18
                || tileInfo.id == 22 || tileInfo.id == 25 || tileInfo.id == 27)
                standing = true;
            if (phase == 1) // First Corner
                standing = false;
            else if (phase == 2) // First up/down turn
                standing = true;
            else if (phase == 3) // End of first corner	
                standing = false;

            return standing;
        }
    }
}