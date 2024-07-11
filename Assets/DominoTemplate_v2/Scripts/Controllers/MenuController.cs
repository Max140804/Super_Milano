using UnityEngine;

namespace DominoTemplate.Controllers
{
    public class MenuController : MonoBehaviour
    {
        [SerializeField] private RectTransform _canvas = null;
        [SerializeField] private GameControler _allGameStuffPrefab = null;
        [SerializeField] private RectTransform _entireMainMenuStuff = null;
        [SerializeField] private RectTransform _mainMenu = null;
        [SerializeField] private RectTransform _chooseDifficulty = null;
        [SerializeField] private RectTransform _inGameMenuStuff = null;
        [SerializeField] private RectTransform _inGameMenu = null;

        private RectTransform _currentGameHolder;
        private GameControler _currentGameScript;

        private int _difficulty;
        private bool _isOpenMenu;

        public int viewid;
        public int numberofplayers;
        public int playercode;

        private void Start()
        {
        }

        private void CloseAllMainMenus()
        {
            _mainMenu.gameObject.SetActive(false);
            _chooseDifficulty.gameObject.SetActive(false);
        }

        public void OpenMainMenu()
        {
            CloseAllMainMenus();
            _mainMenu.gameObject.SetActive(true);
        }

        public void OpenDifficultyMenu()
        {
            CloseAllMainMenus();
            _chooseDifficulty.gameObject.SetActive(true);
        }

        public void InGameMenuActivate()
        {
            _isOpenMenu = !_isOpenMenu;
            _inGameMenu.gameObject.SetActive(_isOpenMenu);
        }

        public void SetDifficulty(int selectedDifficulty)
        {
            _difficulty = selectedDifficulty;
            StartDomino();
        }

        private void EndGame()
        {
            CloseAllMainMenus();

            if (_currentGameHolder != null)
                Destroy(_currentGameHolder.gameObject);
        }

        public void RestartDomino()
        {
            EndGame();
            // This will restart inGameMenu;
            _isOpenMenu = false;
            _inGameMenu.gameObject.SetActive(_isOpenMenu);

            StartDomino();
        }

        public void ToLobby()
        {
            EndGame();
            // This will close inGameMenu;
            _isOpenMenu = false;
            _inGameMenu.gameObject.SetActive(_isOpenMenu);
            _inGameMenuStuff.gameObject.SetActive(false);
            _entireMainMenuStuff.gameObject.SetActive(true);
            OpenMainMenu();
        }


        public void StartDomino()
        {
            // this turns off main menu stuff
            _entireMainMenuStuff.gameObject.SetActive(false);
            _inGameMenuStuff.gameObject.SetActive(true);

            _currentGameScript = Instantiate(_allGameStuffPrefab);
            _currentGameHolder = _currentGameScript.GetRect();

            _currentGameHolder.SetParent(_canvas, false);
            _currentGameHolder.localScale = Vector3.one;
            _currentGameHolder.offsetMin = new Vector2(0, 0);
            _currentGameHolder.offsetMin = new Vector2(0, 0);

            _currentGameScript.RestartGame(_difficulty);
        }
    }
}