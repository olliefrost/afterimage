// HudController: the restart buttons in the bottom right and the level complete panel.
// The R (save) button is framed in the colour of the clone the player controls, its caption says
// which clone the run is saved into (and which one is next if a switch is picked), and it pulses
// after the player presses a hinted button until they make their first clone. The T (retry)
// button restarts without saving. The clone icons are handled by CloneColumn.
using GameJam.Afterimage;
using GameJam.Core;
using GameJam.Interactables;
using UnityEngine;
using UnityEngine.UI;

namespace GameJam.UI
{
    /// <summary>Save and retry buttons with their captions, the save button's pulse, and the level complete panel.</summary>
    public class HudController : MonoBehaviour
    {
        [Header("Game references")]
        [SerializeField] private GameStateController _game;
        [SerializeField] private AfterimageManager _afterimages;
        [Tooltip("Pressing any of these makes the clone button pulse until the first clone is made.")]
        [SerializeField] private PressButton[] _pulseTriggers;

        [Header("UI references")]
        [SerializeField] private Button _cloneButton;
        [Tooltip("Frame image of the clone button, tinted with the controlled clone's colour.")]
        [SerializeField] private Image _cloneButtonFrame;
        [Tooltip("Small text under R: which clone the run is saved into, and the next one if a switch is picked.")]
        [SerializeField] private Text _cloneCaption;
        [SerializeField] private Button _retryButton;
        [Tooltip("Small text under T: retry, and the next clone if a switch is picked.")]
        [SerializeField] private Text _retryCaption;
        [SerializeField] private GameObject _completePanel;

        [Header("Pulse")]
        [SerializeField] private float _pulseSpeed = 6f;
        [Tooltip("How much bigger the clone button grows at the peak of a pulse (0.2 = 20%).")]
        [SerializeField] private float _pulseAmount = 0.2f;

        private RectTransform _cloneButtonRect;
        private bool _isPulsing;

        private void Awake()
        {
            _cloneButtonRect = _cloneButton.GetComponent<RectTransform>();
        }

        private void OnEnable()
        {
            _cloneButton.onClick.AddListener(_game.RequestClone);
            _retryButton.onClick.AddListener(_game.RequestRetry);
            _afterimages.Changed += HandleClonesChanged;
            _game.StateChanged += HandleStateChanged;
            for (int i = 0; i < _pulseTriggers.Length; i++)
            {
                _pulseTriggers[i].Pressed += HandleTriggerPressed;
            }

            HandleClonesChanged();
            _completePanel.SetActive(false);
        }

        private void OnDisable()
        {
            _cloneButton.onClick.RemoveListener(_game.RequestClone);
            _retryButton.onClick.RemoveListener(_game.RequestRetry);
            _afterimages.Changed -= HandleClonesChanged;
            _game.StateChanged -= HandleStateChanged;
            for (int i = 0; i < _pulseTriggers.Length; i++)
            {
                _pulseTriggers[i].Pressed -= HandleTriggerPressed;
            }
        }

        private void Update()
        {
            // Pulsing is purely visual, so unscaled real time is fine here.
            float scale = _isPulsing ? 1f + _pulseAmount * (0.5f + 0.5f * Mathf.Sin(Time.unscaledTime * _pulseSpeed)) : 1f;
            _cloneButtonRect.localScale = new Vector3(scale, scale, 1f);
        }

        /// <summary>Starts the pulse when the live player presses a hinted button before ever cloning.</summary>
        private void HandleTriggerPressed(IActor actor)
        {
            if (!actor.IsClone && _afterimages.CloneCount == 0)
            {
                _isPulsing = true;
            }
        }

        /// <summary>Stops the pulse once a clone exists, and shows which clone R saves into and what comes next.</summary>
        private void HandleClonesChanged()
        {
            if (_afterimages.CloneCount > 0)
            {
                _isPulsing = false;
            }

            int controlled = _afterimages.ControlledSlot;
            int pending = _afterimages.PendingSlot;
            bool hasPending = pending != AfterimageManager.NoSlot;

            _cloneButtonFrame.color = _afterimages.GetSlotColour(controlled);
            _cloneCaption.text = hasPending ? $"SAVE {controlled + 1} → PLAY {pending + 1}" : $"SAVE CLONE {controlled + 1}";
            _retryCaption.text = hasPending ? $"RETRY → {pending + 1}" : "RETRY";
        }

        private void HandleStateChanged(GameState state)
        {
            _completePanel.SetActive(state == GameState.LevelComplete);
        }
    }
}
