// PressButton: a floor/wall button any actor can press with interact. It latches on until the
// next attempt and fires OnPressed (wired in the inspector, e.g. to SignalWire.Send).
// The HUD also listens to Pressed to start pulsing the clone button. As an ISignalSource it also
// feeds gates and cables (Chamber 3's door lock).
using System;
using GameJam.Core;
using UnityEngine;
using UnityEngine.Events;

namespace GameJam.Interactables
{
    /// <summary>A one-shot button: the first interact presses it, and it stays pressed until the room resets.</summary>
    public class PressButton : MonoBehaviour, IInteractable, IResettable, ISignalSource
    {
        [Header("Power (optional)")]
        [Tooltip("If set (any ISignalSource, e.g. a PowerGenerator), the button can only be pressed while it is on.")]
        [SerializeField] private MonoBehaviour _requiredPower;

        [Header("Visuals")]
        [SerializeField] private SpriteRenderer _renderer;
        [SerializeField] private Color _unpressedColor = new Color(0.9f, 0.15f, 0.15f);
        [SerializeField] private Color _pressedColor = new Color(0.35f, 0.08f, 0.08f);

        [Header("Events")]
        [Tooltip("Fired once when the button is pressed. Hook up what it powers here.")]
        [SerializeField] private UnityEvent _onPressed = new UnityEvent();

        private bool _isPressed;
        private ISignalSource _power;

        /// <summary>True once the button has been pressed this attempt (for gates and cables).</summary>
        public bool IsOn => _isPressed;

        /// <summary>Raised from code when the button is pressed, with the actor that pressed it.</summary>
        public event Action<IActor> Pressed;

        private void Awake()
        {
            _power = _requiredPower as ISignalSource;
            if (_requiredPower != null && _power == null)
            {
                Debug.LogError($"{name}: Required Power must implement ISignalSource.", this);
            }
        }

        private void OnEnable()
        {
            LevelReset.Register(this);
            ApplyVisual();
        }

        private void OnDisable()
        {
            LevelReset.Unregister(this);
        }

        /// <summary>Presses the button if it is not already down (and, if it needs power, the power is on).</summary>
        public void Interact(IActor actor)
        {
            if (_isPressed || (_power != null && !_power.IsOn))
            {
                return;
            }

            _isPressed = true;
            ApplyVisual();
            _onPressed.Invoke();
            Pressed?.Invoke(actor);
        }

        /// <summary>Releases the button for the next attempt.</summary>
        public void ResetState()
        {
            _isPressed = false;
            ApplyVisual();
        }

        private void ApplyVisual()
        {
            if (_renderer != null)
            {
                _renderer.color = _isPressed ? _pressedColor : _unpressedColor;
            }
        }
    }
}
