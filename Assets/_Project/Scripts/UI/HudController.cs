// HudController: the run buttons in the bottom right and the level complete panel.
// The R (save) button is framed in the colour of the clone the player controls, its caption says
// which clone the run is saved into (and which one is next if a switch is picked), and it pulses
// after the player presses a hinted button until they make their first clone. The T (retry)
// button restarts without saving. The F (record) button marks the run so dying saves it as a
// clone; it turns red with a blinking dot while marked. All three buttons show their current key,
// which follows rebinds made in the pause menu. The clone icons are handled by CloneColumn.
using GameJam.Afterimage;
using GameJam.Core;
using GameJam.Interactables;
using GameJam.Player;
using UnityEngine;
using UnityEngine.UI;

namespace GameJam.UI
{
    /// <summary>Save, retry and record buttons with their captions, the save button's pulse, and the level complete panel.</summary>
    public class HudController : MonoBehaviour
    {
        [Header("Game references")]
        [SerializeField] private GameStateController _game;
        [SerializeField] private AfterimageManager _afterimages;
        [SerializeField] private PlayerInputReader _input;
        [SerializeField] private KeyRebinder _rebinder;
        [Tooltip("Pressing any of these makes the clone button pulse until the first clone is made.")]
        [SerializeField] private PressButton[] _pulseTriggers;

        [Header("UI references")]
        [SerializeField] private Button _cloneButton;
        [Tooltip("Key letter drawn on the clone button.")]
        [SerializeField] private Text _cloneKeyLabel;
        [Tooltip("Frame image of the clone button, tinted with the controlled clone's colour.")]
        [SerializeField] private Image _cloneButtonFrame;
        [Tooltip("Small text under R: which clone the run is saved into, and the next one if a switch is picked.")]
        [SerializeField] private Text _cloneCaption;
        [SerializeField] private Button _retryButton;
        [Tooltip("Key letter drawn on the retry button.")]
        [SerializeField] private Text _retryKeyLabel;
        [Tooltip("Small text under T: retry, and the next clone if a switch is picked.")]
        [SerializeField] private Text _retryCaption;
        [SerializeField] private Button _recordButton;
        [Tooltip("Key letter drawn on the record button.")]
        [SerializeField] private Text _recordKeyLabel;
        [Tooltip("Frame image of the record button, tinted red while the run is marked.")]
        [SerializeField] private Image _recordFrame;
        [Tooltip("Record dot on the record button; blinks while the run is marked.")]
        [SerializeField] private Image _recordGlyph;
        [Tooltip("Small text under F: whether dying will save this run.")]
        [SerializeField] private Text _recordCaption;
        [SerializeField] private GameObject _completePanel;

        [Header("Pulse")]
        [SerializeField] private float _pulseSpeed = 6f;
        [Tooltip("How much bigger the clone button grows at the peak of a pulse (0.2 = 20%).")]
        [SerializeField] private float _pulseAmount = 0.2f;

        [Header("Record button")]
        [SerializeField] private Color _recordIdleFrame = Color.white;
        [SerializeField] private Color _recordMarkedFrame = new Color(1f, 0.25f, 0.2f);
        [SerializeField] private Color _recordIdleGlyph = new Color(0.45f, 0.15f, 0.13f);
        [SerializeField] private Color _recordMarkedGlyph = new Color(1f, 0.2f, 0.15f);
        [Tooltip("Blinks per second of the record dot while the run is marked.")]
        [SerializeField] private float _recordBlinkRate = 2f;

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
            _recordButton.onClick.AddListener(_game.ToggleRunMark);
            _afterimages.Changed += HandleClonesChanged;
            _game.StateChanged += HandleStateChanged;
            _game.RunMarkedChanged += HandleRunMarkedChanged;
            _rebinder.Changed += RefreshKeyLabels;
            for (int i = 0; i < _pulseTriggers.Length; i++)
            {
                _pulseTriggers[i].Pressed += HandleTriggerPressed;
            }

            HandleClonesChanged();
            HandleRunMarkedChanged(_game.RunMarked);
            RefreshKeyLabels();
            _completePanel.SetActive(false);
        }

        private void OnDisable()
        {
            _cloneButton.onClick.RemoveListener(_game.RequestClone);
            _retryButton.onClick.RemoveListener(_game.RequestRetry);
            _recordButton.onClick.RemoveListener(_game.ToggleRunMark);
            _afterimages.Changed -= HandleClonesChanged;
            _game.StateChanged -= HandleStateChanged;
            _game.RunMarkedChanged -= HandleRunMarkedChanged;
            _rebinder.Changed -= RefreshKeyLabels;
            for (int i = 0; i < _pulseTriggers.Length; i++)
            {
                _pulseTriggers[i].Pressed -= HandleTriggerPressed;
            }
        }

        private void Update()
        {
            // Both effects are purely visual, so unscaled real time is fine here.
            // Only touch the scale while pulsing, so CloneHudReveal can animate the button in.
            if (_isPulsing)
            {
                float scale = 1f + _pulseAmount * (0.5f + 0.5f * Mathf.Sin(Time.unscaledTime * _pulseSpeed));
                _cloneButtonRect.localScale = new Vector3(scale, scale, 1f);
            }

            // The record dot blinks while the run is marked, like a recording light.
            if (_game.RunMarked)
            {
                bool lit = Mathf.Repeat(Time.unscaledTime * _recordBlinkRate, 1f) < 0.6f;
                _recordGlyph.color = lit ? _recordMarkedGlyph : _recordIdleGlyph;
            }
        }

        /// <summary>Shows on the record button whether dying will save this run.</summary>
        private void HandleRunMarkedChanged(bool marked)
        {
            _recordFrame.color = marked ? _recordMarkedFrame : _recordIdleFrame;
            _recordGlyph.color = marked ? _recordMarkedGlyph : _recordIdleGlyph;
            _recordCaption.text = marked ? "SAVES ON DEATH" : "RECORD";
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
            if (_afterimages.CloneCount > 0 && _isPulsing)
            {
                _isPulsing = false;
                _cloneButtonRect.localScale = Vector3.one;
            }

            int controlled = _afterimages.ControlledSlot;
            int pending = _afterimages.PendingSlot;
            bool hasPending = pending != AfterimageManager.NoSlot;

            _cloneButtonFrame.color = _afterimages.GetSlotColour(controlled);
            // At the chamber's clone limit R can only re-record, so the caption says so.
            bool atLimit = _afterimages.CloneCount >= _afterimages.MaxClones;
            if (hasPending)
            {
                _cloneCaption.text = $"SAVE {controlled + 1} → PLAY {pending + 1}";
            }
            else
            {
                _cloneCaption.text = atLimit ? $"SAVE CLONE {controlled + 1} · MAX {_afterimages.MaxClones}" : $"SAVE CLONE {controlled + 1}";
            }
            _retryCaption.text = hasPending ? $"RETRY → {pending + 1}" : "RETRY";
        }

        /// <summary>Draws the current save, retry and record keys on their buttons.</summary>
        private void RefreshKeyLabels()
        {
            _cloneKeyLabel.text = _input.CloneTarget.KeyName.ToUpperInvariant();
            _retryKeyLabel.text = _input.RetryTarget.KeyName.ToUpperInvariant();
            _recordKeyLabel.text = _input.MarkRunTarget.KeyName.ToUpperInvariant();
        }

        private void HandleStateChanged(GameState state)
        {
            _completePanel.SetActive(state == GameState.LevelComplete);
        }
    }
}
