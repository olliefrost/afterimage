// HudController: the minimal uGUI HUD. Shows the clone count, owns the on-screen clone button
// (which pulses after the player presses a hinted button, until they make their first clone)
// and shows the level complete panel.
using GameJam.Afterimage;
using GameJam.Core;
using GameJam.Interactables;
using UnityEngine;
using UnityEngine.UI;

namespace GameJam.UI
{
    /// <summary>Clone counter, pulsing clone button and level complete panel.</summary>
    public class HudController : MonoBehaviour
    {
        [Header("Game references")]
        [SerializeField] private GameStateController _game;
        [SerializeField] private AfterimageManager _afterimages;
        [Tooltip("Pressing any of these makes the clone button pulse until the first clone is made.")]
        [SerializeField] private PressButton[] _pulseTriggers;

        [Header("UI references")]
        [SerializeField] private Text _cloneCountText;
        [SerializeField] private Button _cloneButton;
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
            _afterimages.CloneCountChanged += HandleCloneCountChanged;
            _game.StateChanged += HandleStateChanged;
            for (int i = 0; i < _pulseTriggers.Length; i++)
            {
                _pulseTriggers[i].Pressed += HandleTriggerPressed;
            }

            HandleCloneCountChanged(_afterimages.CloneCount);
            _completePanel.SetActive(false);
        }

        private void OnDisable()
        {
            _cloneButton.onClick.RemoveListener(_game.RequestClone);
            _afterimages.CloneCountChanged -= HandleCloneCountChanged;
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

        private void HandleCloneCountChanged(int count)
        {
            _cloneCountText.text = $"Clones: {count}";
            if (count > 0)
            {
                _isPulsing = false;
            }
        }

        private void HandleStateChanged(GameState state)
        {
            _completePanel.SetActive(state == GameState.LevelComplete);
        }
    }
}
