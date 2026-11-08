// Door: blocks the way until Open() is called (usually by SignalWire.OnArrived, or a gate such as
// Chamber 3's AllOnGate, which can also Close() it again). Opening removes the blocking collider,
// tints the door and turns on its "open" visuals (light spill or trail) so the exit reads clearly.
// Closes again on every new attempt. OnOpened/OnClosed drive animation
// (e.g. SpriteFlipbook.PlayForward/PlayBackward); the collider never waits for it.
using GameJam.Core;
using UnityEngine;
using UnityEngine.Events;

namespace GameJam.Interactables
{
    /// <summary>A door that opens on a signal and closes when the room resets.</summary>
    public class Door : MonoBehaviour, IResettable
    {
        [Header("References")]
        [Tooltip("Solid collider that stops actors while the door is closed.")]
        [SerializeField] private Collider2D _blocker;
        [SerializeField] private SpriteRenderer _renderer;
        [Tooltip("Shown only while the door is open, e.g. a light spilling through or a floor trail.")]
        [SerializeField] private GameObject _openVisuals;

        [Header("Visuals")]
        [SerializeField] private Color _closedColor = new Color(0.2f, 0.2f, 0.22f);
        [SerializeField] private Color _openColor = new Color(0.1f, 0.1f, 0.1f, 0.3f);

        [Header("Events")]
        [Tooltip("Fired when the door opens, e.g. SpriteFlipbook.PlayForward.")]
        [SerializeField] private UnityEvent _onOpened = new UnityEvent();
        [Tooltip("Fired when an open door closes (reset or Close), e.g. SpriteFlipbook.PlayBackward.")]
        [SerializeField] private UnityEvent _onClosed = new UnityEvent();

        private bool _isOpen;

        private void OnEnable()
        {
            LevelReset.Register(this);
            ApplyState();
        }

        private void OnDisable()
        {
            LevelReset.Unregister(this);
        }

        /// <summary>Opens the door and lets actors through.</summary>
        public void Open()
        {
            if (_isOpen)
            {
                return;
            }

            _isOpen = true;
            ApplyState();
            _onOpened.Invoke();
        }

        /// <summary>Shuts the door again, e.g. when the power behind its lock fails.</summary>
        public void Close()
        {
            bool wasOpen = _isOpen;
            _isOpen = false;
            ApplyState();
            if (wasOpen)
            {
                _onClosed.Invoke();
            }
        }

        /// <summary>Closes the door for the next attempt.</summary>
        public void ResetState()
        {
            Close();
        }

        private void ApplyState()
        {
            _blocker.enabled = !_isOpen;
            _renderer.color = _isOpen ? _openColor : _closedColor;
            if (_openVisuals != null)
            {
                _openVisuals.SetActive(_isOpen);
            }
        }
    }
}
