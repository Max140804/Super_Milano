using System;
using System.Collections;
using System.Security.Cryptography;
using DominoTemplate.AI;
using Firebase.Database;
using Firebase;
using UnityEngine;
using UnityEngine.UI;
using Firebase.Extensions;
namespace DominoTemplate.Controllers
{
    public class GameTurnController : MonoBehaviour
    {
        [SerializeField] private Text endTurnMessage = null;
        [SerializeField] private Text _playerScoreText = null;
        [SerializeField] private Text _leftAIScoreText = null;
        [SerializeField] private Text _topAIScoreText = null;
        [SerializeField] private Text _rightAIScoreText = null;

        private HandController _handScript = null;
        private DeckController _deckScript = null;
        private AIController _AIScript = null;

        public int turn;

        public bool _playerTurn;
        public bool _leftAITurn;
        public bool _topAITurn;
        public bool _rightAITurn;

        public int _lastActivePlayer;

        private bool _checkTiles;
        private bool _gameOver;

        private int _playerScore;
        private int _leftAIScore;
        private int _topAIScore;
        private int _rightAIScore;


        private FirebaseApp app;

        private DatabaseReference databaseReference;

        public float coins;
        public string PlayerId;
        public float bid;
        public int playerwin;

        public float gameplayed;
        public float gamewin;
        public float gamelose;

        public bool GetPlayerTurn()
        {
            return _playerTurn;
        }
        private void Awake()
        {
            PlayerId = PlayerPrefs.GetString("playerid");
            bid = PlayerPrefs.GetFloat("bid");
            gameplayed = PlayerPrefs.GetInt("played");
            gamewin = PlayerPrefs.GetInt("win");
            gamelose = PlayerPrefs.GetInt("lose");

            FirebaseApp.CheckAndFixDependenciesAsync().ContinueWithOnMainThread(task =>
            {

                if (task.Result == DependencyStatus.Available)
                {
                    InitializeFirebase();

                }
            });
        }
        private void InitializeFirebase()
        {

            app = FirebaseApp.DefaultInstance;

            // Ensure that the DatabaseURL is set
            if (string.IsNullOrEmpty(app.Options.DatabaseUrl?.ToString()))
            {
                Uri databaseUrl = new Uri("https://super-milano-default-rtdb.firebaseio.com/");
                app.Options.DatabaseUrl = databaseUrl;
            }

            // Firebase is ready to use.

            // Initialize the database reference after Firebase is initialized
            databaseReference = FirebaseDatabase.GetInstance(app).RootReference;
            Debug.Log("initialized");
            StartCoroutine(GetPlayerCoins(PlayerId));

        }


        private IEnumerator UpdateCoinsCoroutine(string playerId, float coins)
        {
            var task = databaseReference.Child(HelperClass.Encrypt("players", playerId)).Child(HelperClass.Encrypt(playerId, playerId)).Child(HelperClass.Encrypt("coins", playerId)).SetValueAsync(HelperClass.Encrypt(coins.ToString(), playerId));
            yield return new WaitUntil(() => task.IsCompleted);

            if (task.Exception != null)
            {
                Debug.LogError($"Failed to update coins: {task.Exception}");
            }
            else
            {
                Debug.Log("Coins updated successfully");
            }
        }

        public IEnumerator GetPlayerCoins(string playerId)
        {
            var task = databaseReference.Child(HelperClass.Encrypt("players", playerId)).Child(HelperClass.Encrypt(playerId, playerId)).Child(HelperClass.Encrypt("coins", playerId)).GetValueAsync();
            yield return new WaitUntil(() => task.IsCompleted);

            if (task.Exception != null)
            {
                Debug.LogError($"Failed to get player coins: {task.Exception}");
            }
            else if (task.Result.Value == null)
            {
                Debug.Log("Player not found or coins not set.");
            }
            else
            {
                coins = float.Parse(HelperClass.Decrypt(task.Result.Value.ToString(), playerId));
                Debug.Log($"Player has {coins} coins.");
                // Do something with the retrieved coins, e.g., update UI
            }
        }
        public void UpdateStatics(string playerId, float played, float win, float lose)
        {
            StartCoroutine(Updateplayed(playerId, played));
            StartCoroutine(Updatewin(playerId, win));
            StartCoroutine(Updatelose(playerId, lose));

        }
        private IEnumerator Updateplayed(string playerId, float played)
        {
            var task = databaseReference.Child(HelperClass.Encrypt("players", playerId)).Child(HelperClass.Encrypt(playerId.ToString(), playerId)).Child(HelperClass.Encrypt("played", playerId)).SetValueAsync(HelperClass.Encrypt(played.ToString(), playerId));
            yield return new WaitUntil(() => task.IsCompleted);
        }
        private IEnumerator Updatewin(string playerId, float win)
        {
            var task = databaseReference.Child(HelperClass.Encrypt("players", playerId)).Child(HelperClass.Encrypt(playerId.ToString(), playerId)).Child(HelperClass.Encrypt("win", playerId)).SetValueAsync(HelperClass.Encrypt(win.ToString(), playerId));
            yield return new WaitUntil(() => task.IsCompleted);

        }
        private IEnumerator Updatelose(string playerId, float lose)
        {
            var task = databaseReference.Child(HelperClass.Encrypt("players", playerId)).Child(HelperClass.Encrypt(playerId.ToString(), playerId)).Child(HelperClass.Encrypt("lose", playerId)).SetValueAsync(HelperClass.Encrypt(lose.ToString(), playerId));
            yield return new WaitUntil(() => task.IsCompleted);
        }

        public void SetAllRefs(HandController newHand, DeckController newDeck, AIController newAI)
        {
            _handScript = newHand;
            _deckScript = newDeck;
            _AIScript = newAI;
        }

        public void InitGameVariables()
        {
            turn = 0;

            _playerScore = 0;
            _leftAIScore = 0;
            _topAIScore = 0;
            _rightAIScore = 0;

            Debug.Log(turn);

            _lastActivePlayer = -1;

            _checkTiles = false;
            endTurnMessage.text = " ";
            _gameOver = false;
        }


        public void SetLastActivePlayer()
        {
            _lastActivePlayer = turn + 1;
        }

        public void PassBehaviour()
        {
            int scoreForPass = 1;
            int last = 0;

            if (_deckScript.playercode == 1001)
            {
                last = _lastActivePlayer;

            }
            if (_deckScript.playercode == 2001)
            {
                last = _lastActivePlayer + 3;

            }
            if (_deckScript.playercode == 3001)
            {
                last = _lastActivePlayer + 2;

            }
            if (_deckScript.playercode == 4001)
            {
                last = _lastActivePlayer + 1;

            }



            if (last > 4)
            {
                last = last - 4;
            }


            if (_playerTurn)
            {
                _handScript.SetLockPlayerButtons(false);
                _handScript.LockTilesHere(_deckScript.GetList(1));
            }

            switch (last)
            {
                case 1:
                    if (!_playerTurn)
                        _playerScore += scoreForPass;
                    else
                        _gameOver = true;
                    break;

                case 2:
                    if (!_leftAITurn)
                        _leftAIScore += scoreForPass;
                    else
                        _gameOver = true;
                    break;

                case 3:
                    if (!_topAITurn)
                        _topAIScore += scoreForPass;
                    else
                        _gameOver = true;
                    break;

                case 4:
                    if (!_rightAITurn)
                        _rightAIScore += scoreForPass;
                    else
                        _gameOver = true;
                    break;

                default:
                    Debug.Log("Something went wrong with Pass Scoring");
                    break;
            }

            _playerScoreText.text = "Score: " + _playerScore;
            _leftAIScoreText.text = "Score: " + _leftAIScore;
            _topAIScoreText.text = "Score: " + _topAIScore;
            _rightAIScoreText.text = "Score: " + _rightAIScore;
        }


        private IEnumerator EndTurnLogic(string message, float time)
        {
            if (message == null)
                endTurnMessage.text = " ";
            else
                endTurnMessage.text = message;

            yield return new WaitForSeconds(time);

            endTurnMessage.text = " ";
            _checkTiles = true;
        }

        public void EndTurn(string message, float time)
        {
            Debug.Log("Turns Player-> " + _playerTurn + " LeftAI-> " + _leftAITurn
                      + " TopAI-> " + _topAITurn + " RightAI-> " + _rightAITurn);

            if(turn < 4)
            {
                turn++;
            }
            if (turn == 4)
            {
                turn = 0;
            }

            StartCoroutine(EndTurnLogic(message, time));
        }


        public void Update()
        {
            int turn_for_each = turn;

            if (_deckScript.playercode == 1001)
            {
                turn_for_each = turn;

            }
            if (_deckScript.playercode == 2001)
            {
                turn_for_each = turn + 3;

            }
            if (_deckScript.playercode == 3001)
            {
                turn_for_each = turn + 2;

            }
            if (_deckScript.playercode == 4001)
            {
                turn_for_each = turn + 1;

            }

            if(turn_for_each > 3)
            {
                turn_for_each = turn_for_each - 4;
         }


            if (turn_for_each == 0)
            {
                _playerTurn = true;
                _leftAITurn = false;
                _topAITurn = false;
                _rightAITurn = false;
            }
            if (turn_for_each == 1)
            {
                _playerTurn = false;
                _leftAITurn = true;
                _topAITurn = false;
                _rightAITurn = false;
            }
            if (turn_for_each == 2)
            {
                _playerTurn = false;
                _leftAITurn = false;
                _topAITurn = true;
                _rightAITurn = false;
            }
            if (turn_for_each == 3)
            {
                _playerTurn = false;
                _leftAITurn = false;
                _topAITurn = false;
                _rightAITurn = true;
            }
            if (turn_for_each == 4)
            {
                _playerTurn = false;
                _leftAITurn = false;
                _topAITurn = false;
                _rightAITurn = false;
            }
        }

        public void GameProcess()
        {
            if (_checkTiles)
            {
                string endGameCase = null;
                _checkTiles = false;

                _deckScript.ControlAllHands();
                if (_gameOver == false) //Protection, cuz could be scenarios when game is already over
                {
                    _gameOver = _deckScript.CheckForGameOver(ref endGameCase, ref playerwin);
                    if (_gameOver)
                    {
                        if(playerwin == 1 && PlayerPrefs.GetInt("ai") == 0)
                        {
                            UpdateStatics(PlayerId, gameplayed, gamewin + 1, gamelose - 1);

                            float finalbid = (bid * 4) * 0.9f;
                            StartCoroutine(UpdateCoinsCoroutine(PlayerId, coins + finalbid));
                        }
                        endTurnMessage.text = endGameCase;
                        _handScript.SetLockPlayerButtons(false);
                    }
                    else
                    {
                        _handScript.SetTurnFor(_playerTurn, _leftAITurn, _topAITurn, _rightAITurn);

                        // AI Logic
                        if (_leftAITurn)
                        {
                            _AIScript.MakeTurn(2, false);
                        }
                        else if (_topAITurn)
                        {
                            _AIScript.MakeTurn(3, false);
                        }
                        else if (_rightAITurn)
                        {
                            _AIScript.MakeTurn(4, true);
                        }
                    }
                }
                else
                {
                    endTurnMessage.text = "\"Fish\" Game Over";
                    _handScript.SetLockPlayerButtons(false);
                }
            }

            if (_gameOver)
            {

            }
        }
    }
}