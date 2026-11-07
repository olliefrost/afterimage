// Door: blocks the way until Open() is called (usually by SignalWire.OnArrived). Opening removes
// the blocking collider, recolours the door and turns on its "open" visuals (light spill or trail)
// so the exit reads clearly. Closes again on every new attempt.
using GameJam.Core;
using UnityEngine;

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
            _isOpen = true;
            ApplyState();
        }

        /// <summary>Closes the door for the next attempt.</summary>
        public void ResetState()
        {
            _isOpen = false;
            ApplyState();
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
