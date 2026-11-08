// PauseMenu: the Esc menu in every chamber (PauseMenu prefab). Esc pauses through
// GameStateController, which freezes time; this script then shows a dark overlay with Resume,
// Settings (key rebinding, see SettingsPanel), Restart Game (back to the first chamber) and Quit
// Game. Esc goes back from Settings, resumes from the main page, and is ignored while a key is
// being rebound (there it cancels the rebind).
using GameJam.Core;
using GameJam.Player;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace GameJam.UI
{
    /// <summary>Routes the pause key and shows the pause overlay while the game is paused.</summary>
    public class PauseMenu : MonoBehaviour
    {
        [Header("Game references")]
        [SerializeField] private GameStateController _game;
        [SerializeField] private PlayerInputReader _input;
        [SerializeField] private KeyRebinder _rebinder;

        [Header("UI references")]
        [Tooltip("Dark full-screen backdrop holding both pages. Shown only while paused; it also blocks clicks on the HUD.")]
        [SerializeField] private GameObject _overlay;
        [Tooltip("Page with Resume, Settings, Restart Game and Quit Game.")]
        [SerializeField] private GameObject _mainPage;
        [SerializeField] private SettingsPanel _settings;
        [SerializeField] private Button _resumeButton;
        [SerializeField] private Button _settingsButton;
        [SerializeField] private Button _restartButton;
        [SerializeField] private Button _quitButton;

        private void Awake()
        {
            _settings.Init(_input, _rebinder);
        }

        private void OnEnable()
        {
            _input.PauseRequested += HandlePauseKey;
            _game.StateChanged += HandleStateChanged;
            _settings.BackRequested += CloseSettings;
            _resumeButton.onClick.AddListener(_game.Resume);
            _settingsButton.onClick.AddListener(OpenSettings);
            _restartButton.onClick.AddListener(SceneFlow.NewGame);
            _quitButton.onClick.AddListener(SceneFlow.QuitGame);

            HandleStateChanged(_game.State);
        }

        private void OnDisable()
        {
            _input.PauseRequested -= HandlePauseKey;
            _game.StateChanged -= HandleStateChanged;
            _settings.BackRequested -= CloseSettings;
            _resumeButton.onClick.RemoveListener(_game.Resume);
            _settingsButton.onClick.RemoveListener(OpenSettings);
            _restartButton.onClick.RemoveListener(SceneFlow.NewGame);
            _quitButton.onClick.RemoveListener(SceneFlow.QuitGame);
        }

        /// <summary>Esc: ignored while rebinding or changing scene, back from Settings, otherwise pause or resume.</summary>
        private void HandlePauseKey()
        {
            if (_rebinder.IsRebinding || SceneFlow.IsChangingScene)
            {
                return;
            }

            if (_settings.IsOpen)
            {
                CloseSettings();
            }
            else if (_game.State == GameState.Paused)
            {
                _game.Resume();
            }
            else
            {
                _game.Pause();
            }
        }

        /// <summary>Shows the overlay on its main page while paused, and hides everything otherwise.</summary>
        private void HandleStateChanged(GameState state)
        {
            bool paused = state == GameState.Paused;
            _overlay.SetActive(paused);
            _settings.Close();
            _mainPage.SetActive(true);

            // Forget the last clicked button, so Space or Enter in play does not press it again.
            if (!paused && EventSystem.current != null)
            {
                EventSystem.current.SetSelectedGameObject(null);
            }
        }

        private void OpenSettings()
        {
            _mainPage.SetActive(false);
            _settings.Open();
        }

        private void CloseSettings()
        {
            _settings.Close();
            _mainPage.SetActive(true);
        }
    }
}
