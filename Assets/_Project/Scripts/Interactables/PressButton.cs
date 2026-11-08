// PressButton: a floor/wall button any actor can press with interact. It latches on until the
// next attempt and fires OnPressed (wired in the inspector, e.g. to SignalWire.Send).
// The HUD also listens to Pressed to start pulsing the clone button.
using System;
using GameJam.Core;
using UnityEngine;
using UnityEngine.Events;

namespace GameJam.Interactables
{
    /// <summary>A one-shot button: the first interact presses it, and it stays pressed until the room resets.</summary>
    public class PressButton : MonoBehaviour, IInteractable, IResettable
    {
        [Header("Visuals")]
        [SerializeField] private SpriteRenderer _renderer;
        [SerializeField] private Color _unpressedColor = new Color(0.9f, 0.15f, 0.15f);
        [SerializeField] private Color _pressedColor = new Color(0.35f, 0.08f, 0.08f);
        [Tooltip("Optional art for the up state. Leave empty to keep the current sprite and only tint.")]
        [SerializeField] private Sprite _unpressedSprite;
        [Tooltip("Optional art for the down state. Set both colours to white when using real art.")]
        [SerializeField] private Sprite _pressedSprite;

        [Header("Events")]
        [Tooltip("Fired once when the button is pressed. Hook up what it powers here.")]
        [SerializeField] private UnityEvent _onPressed = new UnityEvent();

        private bool _isPressed;

        /// <summary>Raised from code when the button is pressed, with the actor that pressed it.</summary>
        public event Action<IActor> Pressed;

        private void OnEnable()
        {
            LevelReset.Register(this);
            ApplyVisual();
        }

        private void OnDisable()
        {
            LevelReset.Unregister(this);
        }

        /// <summary>Presses the button if it is not already down.</summary>
        public void Interact(IActor actor)
        {
            if (_isPressed)
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
            if (_renderer == null)
            {
                return;
            }

            _renderer.color = _isPressed ? _pressedColor : _unpressedColor;
            Sprite sprite = _isPressed ? _pressedSprite : _unpressedSprite;
            if (sprite != null)
            {
                _renderer.sprite = sprite;
            }
        }
    }
}
