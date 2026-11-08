// ChamberTitleCard: the "CHAMBER 1 / FIRST CLONE" card that fades in near the top of the screen
// when play first starts in a chamber (TitleCard on the HUD prefab; each scene sets its own text).
// It waits for GameStateController to reach Playing the first time, so in Tutorial1 it follows the
// intro, and after a scene wipe it shows as the chamber is revealed. Retries never show it again.
// Visual only: it takes no clicks and the player can move while it shows.
using GameJam.Core;
using UnityEngine;
using UnityEngine.UI;

namespace GameJam.UI
{
    /// <summary>Shows the chamber's number and name once, when its first attempt begins.</summary>
    public class ChamberTitleCard : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private GameStateController _game;
        [Tooltip("Fades the whole card in and out.")]
        [SerializeField] private CanvasGroup _group;
        [SerializeField] private Text _labelText;
        [SerializeField] private Text _nameText;

        [Header("Text (set per chamber)")]
        [Tooltip("Small line above the name, for example \"Chamber 1\". Shown in capitals.")]
        [SerializeField] private string _label = "";
        [Tooltip("The chamber's name from its Ideas file, for example \"First Clone\". Empty hides the card.")]
        [SerializeField] private string _chamberName = "";

        [Header("Timing")]
        [Tooltip("Seconds after play starts (once the scene wipe has cleared) before the card appears.")]
        [SerializeField] private float _delay = 0.3f;
        [SerializeField] private float _fadeIn = 0.5f;
        [SerializeField] private float _hold = 2f;
        [SerializeField] private float _fadeOut = 0.8f;

        // Seconds since the card started, or below zero while it is not showing.
        private float _elapsed = -1f;
        private bool _shown;

        private void Awake()
        {
            _labelText.text = _label.ToUpperInvariant();
            _nameText.text = _chamberName.ToUpperInvariant();
            _group.alpha = 0f;
            _group.blocksRaycasts = false;
            _group.interactable = false;
        }

        private void OnEnable()
        {
            _game.StateChanged += HandleStateChanged;
        }

        private void OnDisable()
        {
            _game.StateChanged -= HandleStateChanged;
        }

        private void Update()
        {
            if (_elapsed < 0f)
            {
                return;
            }

            // Game time, so the card freezes with the pause menu instead of fading out behind it.
            _elapsed += Time.deltaTime;
            float t = _elapsed - _delay;
            float alpha;
            if (t < _fadeIn)
            {
                alpha = Mathf.Clamp01(t / _fadeIn);
            }
            else if (t < _fadeIn + _hold)
            {
                alpha = 1f;
            }
            else
            {
                alpha = 1f - Mathf.Clamp01((t - _fadeIn - _hold) / _fadeOut);
            }

            _group.alpha = alpha;
            if (t >= _fadeIn + _hold + _fadeOut)
            {
                _elapsed = -1f;
            }
        }

        /// <summary>Starts the card the first time the chamber's play begins.</summary>
        private void HandleStateChanged(GameState state)
        {
            if (_shown || state != GameState.Playing || string.IsNullOrEmpty(_chamberName))
            {
                return;
            }

            _shown = true;
            _elapsed = 0f;
        }
    }
}
