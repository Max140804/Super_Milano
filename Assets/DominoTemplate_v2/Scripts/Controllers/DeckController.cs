using System.Collections;
using System.Collections.Generic;
using DominoTemplate.Core;
using DominoTemplate.DragAndDrop;
using DominoTemplate.View;
using Photon.Pun;
using Unity.VisualScripting;
using UnityEngine;

namespace DominoTemplate.Controllers
{
    public class DeckController : MonoBehaviour
    {
        public GameObject gameplay;

        [SerializeField] private RectTransform _deckHolder = null;
        [SerializeField] private RectTransform _player = null;
        [SerializeField] private RectTransform _rightAI = null;
        [SerializeField] private RectTransform _leftAI = null;
        [SerializeField] private RectTransform _topAI = null;

        [SerializeField] private DragHandler _dominoPrefab = null;
        [SerializeField] private Sprite[] _spriteArray = null;

        public int dominoCount = 28;
        private GameControler _gameScript;
        private HandController _handScript;
        private GameTurnController _turnScript;

        public List<DragHandler> _dominoTiles;

        public List<DragHandler> _playerTiles;
        public List<DragHandler> _rightAITiles;
        public List<DragHandler> _leftAITiles;
        public List<DragHandler> _topAITiles;

        public int playercode;
        public List<RectTransform> _players_1;
        public List<RectTransform> _players_2;
        public List<RectTransform> _players_3;
        public List<RectTransform> _players_4;




        public string number;
        public void SetAllRefs(GameControler newGame, HandController newHand, GameTurnController newTurn)
        {
            _gameScript = newGame;
            _handScript = newHand;
            _turnScript = newTurn;
        }

        private void Start()
        {
            number = PhotonNetwork.CurrentRoom.CustomProperties["unoplayer"].ToString();

            gameplay = GameObject.Find("MenuController");
            playercode = gameplay.GetComponent<MenuController>().playercode;
            if (playercode == 1001)
            {
                _player = _players_1[0];
                _leftAI = _players_1[1];
                _topAI = _players_1[2];
                _rightAI = _players_1[3];


            }
            if (playercode == 2001)
            {
                _player = _players_2[0];
                _leftAI = _players_2[1];
                _topAI = _players_2[2];
                _rightAI = _players_2[3];


            }
            if (playercode == 3001)
            {
                _player = _players_3[0];
                _leftAI = _players_3[1];
                _topAI = _players_3[2];
                _rightAI = _players_3[3];


            }
            if (playercode == 4001)
            {
                _player = _players_4[0];
                _leftAI = _players_4[1];
                _topAI = _players_4[2];
                _rightAI = _players_4[3];

            }
        }
        public void RestartDominos()
        {
            KillAllDominos();
            SetupNewGame();
        }


        public List<DragHandler> GetList(int listNum)
        {
            List<DragHandler> result = new List<DragHandler>();

            if (listNum == 0)
                result = _dominoTiles;
            else if (listNum == 1)
                result = _playerTiles;
            else if (listNum == 2)
                result = _leftAITiles;
            else if (listNum == 3)
                result = _topAITiles;
            else if (listNum == 4)
                result = _rightAITiles;
            else
                Debug.Log("Picked wrong list");

            return result;
        }


        public void ControlAllHands()
        {
            int i = 1;


            while (i < 5)
            {
                List<DragHandler> tempList = GetList(i);
                ControlPlaced(tempList);

                i++;
            }
        }

        private void ControlPlaced(List<DragHandler> tempTileHolder)
        {
            int i = 0;

            while (i < tempTileHolder.Count)
            {
                if (tempTileHolder[i].GetDominoView().IsOnTable())
                {
                    tempTileHolder.RemoveAt(i);
                }

                i++;
            }
        }


        public IEnumerator SetupRandomHands(int raa, int i)
        {
            
            if(number == "4")
            {
                _playerTiles = new List<DragHandler>();
                _rightAITiles = new List<DragHandler>();
                _leftAITiles = new List<DragHandler>();
                _topAITiles = new List<DragHandler>();

                yield return new WaitForSeconds(1f);

                int rand = raa;
                if (playercode == 1001)
                {
                    if (i < 7)
                        TileSwapHelper(ref _player, ref _playerTiles, rand, true, false);
                    else if (i < 14)
                        TileSwapHelper(ref _leftAI, ref _leftAITiles, rand, false, true);
                    else if (i < 21)
                        TileSwapHelper(ref _topAI, ref _topAITiles, rand, true, true);
                    else
                        TileSwapHelper(ref _rightAI, ref _rightAITiles, rand, false, true);

                }
                if (playercode == 2001)
                {
                    if (i < 7)
                        TileSwapHelper(ref _player, ref _rightAITiles, rand, false, true);
                    else if (i < 14)
                        TileSwapHelper(ref _leftAI, ref _playerTiles, rand, true, false);
                    else if (i < 21)
                        TileSwapHelper(ref _topAI, ref _leftAITiles, rand, false, true);
                    else
                        TileSwapHelper(ref _rightAI, ref _topAITiles, rand, true, true);

                }
                if (playercode == 3001)
                {
                    if (i < 7)
                        TileSwapHelper(ref _player, ref _topAITiles, rand, true, true);
                    else if (i < 14)
                        TileSwapHelper(ref _leftAI, ref _rightAITiles, rand, false, true);
                    else if (i < 21)
                        TileSwapHelper(ref _topAI, ref _playerTiles, rand, true, false);
                    else
                        TileSwapHelper(ref _rightAI, ref _leftAITiles, rand, false, true);

                }
                if (playercode == 4001)
                {
                    if (i < 7)
                        TileSwapHelper(ref _player, ref _leftAITiles, rand, false, true);
                    else if (i < 14)
                        TileSwapHelper(ref _leftAI, ref _topAITiles, rand, true, true);
                    else if (i < 21)
                        TileSwapHelper(ref _topAI, ref _rightAITiles, rand, false, true);
                    else
                        TileSwapHelper(ref _rightAI, ref _playerTiles, rand, true, false);

                }



                yield return new WaitForSeconds(0.2f);


                _turnScript.EndTurn("Game Started", 1);

                Debug.Log("===> All list count -> " + _dominoTiles.Count + " Player count -> " + _playerTiles.Count);
                Debug.Log("===> _rightAITiles count -> " + _rightAITiles.Count + " _leftAITiles count -> " +
                          _leftAITiles.Count + " _topAITiles count -> " + _topAITiles.Count);
                yield return null;
            }
            if (number == "2")
            {
                _playerTiles = new List<DragHandler>();
                _rightAITiles = new List<DragHandler>();
                _leftAITiles = new List<DragHandler>();
                _topAITiles = new List<DragHandler>();

                yield return new WaitForSeconds(1f);

                int rand = raa;
                if (playercode == 1001)
                {
                    if (i < 8)
                        TileSwapHelper(ref _player, ref _playerTiles, rand, true, false);
                    else if (i < 16)
                        TileSwapHelper(ref _topAI, ref _topAITiles, rand, true, true);
                    else
                        TileSwapHelper(ref _rightAI, ref _rightAITiles, rand, false, true);

                }
                if (playercode == 2001)
                { 
                    if (i < 7)
                        TileSwapHelper(ref _player, ref _topAITiles, rand, true, true);
                    else if (i < 16)
                        TileSwapHelper(ref _topAI, ref _playerTiles, rand, true, false);
                    else
                        TileSwapHelper(ref _rightAI, ref _leftAITiles, rand, false, true);

                }




                yield return new WaitForSeconds(0.2f);


                _turnScript.EndTurn("Game Started", 1);

                Debug.Log("===> All list count -> " + _dominoTiles.Count + " Player count -> " + _playerTiles.Count);
                Debug.Log("===> _rightAITiles count -> " + _rightAITiles.Count + " _leftAITiles count -> " +
                          _leftAITiles.Count + " _topAITiles count -> " + _topAITiles.Count);
                yield return null;
            }

        }


        private void TileSwapHelper(ref RectTransform nextParent, ref List<DragHandler> newList, int tileIndex,
            bool standing, bool handedAI)
        {
            _dominoTiles[tileIndex].GetDominoView().ChangeBackState(handedAI);
            _dominoTiles[tileIndex].SendToNextHand(nextParent, standing, handedAI);
            newList.Add(_dominoTiles[tileIndex]);
            _dominoTiles.RemoveAt(tileIndex);
        }


        private void SetupNewGame()
        {
            int i;

            _dominoTiles = new List<DragHandler>();
            i = 0;
            while (i < dominoCount) // Create all dominoes
            {
                DragHandler tempDomino = Instantiate(_dominoPrefab, _deckHolder);
                tempDomino.pv.ViewID = i + 5;
                DominoView tempDominoView;

                _dominoTiles.Add(tempDomino);
                tempDominoView = _dominoTiles[i].GetDominoView();
                _dominoTiles[i].gameScript = _gameScript;
                if (i < _spriteArray.Length)
                {
                    tempDominoView.LockTile();
                    if (_spriteArray[i] != null)
                        tempDominoView.SetDomino(SetupDominoInfo(i), _spriteArray[i]);
                    else
                        Debug.Log("Needed Sprite is not exits!!!!");
                }
                else
                    Debug.Log("Something wrong with sprites!!!");

                i++;
            }

            _handScript.SetLockPlayerButtons(false);
        }

        private Domino SetupDominoInfo(int dominoNum)
        {
            Domino resultInfo = new Domino();

            resultInfo.id = dominoNum;
            resultInfo.Available = true;

            if (0 <= dominoNum && dominoNum < 7)
            {
                resultInfo.TopIndex = 0;
                resultInfo.BottomIndex = dominoNum;
            }
            else if (7 <= dominoNum && dominoNum < 13)
            {
                resultInfo.TopIndex = 1;
                resultInfo.BottomIndex = dominoNum - 6;
            }
            else if (13 <= dominoNum && dominoNum < 18)
            {
                resultInfo.TopIndex = 2;
                resultInfo.BottomIndex = dominoNum - 11;
            }
            else if (18 <= dominoNum && dominoNum < 22)
            {
                resultInfo.TopIndex = 3;
                resultInfo.BottomIndex = dominoNum - 15;
            }
            else if (22 <= dominoNum && dominoNum < 25)
            {
                resultInfo.TopIndex = 4;
                resultInfo.BottomIndex = dominoNum - 18;
            }
            else if (25 <= dominoNum && dominoNum < 27)
            {
                resultInfo.TopIndex = 5;
                resultInfo.BottomIndex = dominoNum - 20;
            }
            else
            {
                resultInfo.TopIndex = 6;
                resultInfo.BottomIndex = 6;
            }


            return resultInfo;
        }


        public bool CheckForGameOver(ref string gameOverCase, ref int playerwin)
        {
            if (_playerTiles.Count == 0)
            {
                gameOverCase = "Player win!";
                playerwin = 1;
            }
            if (_leftAITiles.Count == 0)
            {
                gameOverCase = "Left AI win!";
                playerwin = 2;

            }
            if (_topAITiles.Count == 0)
            { 
                gameOverCase = "Top AI win!";
                playerwin = 3;

            }
            if (_rightAITiles.Count == 0)
            {
                gameOverCase = "Right AI win!";
                playerwin = 4;

            }
            if (gameOverCase != null)
            {
                return true;
            }
            return false;
        }


        private void ClearList(List<DragHandler> tempList)
        {
            int i;

            i = 0;
            while (i < tempList.Count)
            {
                DragHandler tempTile = tempList[i];
                if (tempTile.gameObject != null)
                    Destroy(tempTile.gameObject);
                i++;
                if (i >= tempList.Count)
                    tempList.RemoveRange(0, tempList.Count);
            }
        }

        private void KillAllDominos()
        {
            int i;

            i = 0;
            while (i < 5)
            {
                List<DragHandler> tempList = GetList(i);

                if (tempList != null)
                    ClearList(tempList);
                i++;
            }
        }
    }
}