// PunchScale: a quick squash-and-stretch bump on a Transform or RectTransform. Wire Punch() to any
// UnityEvent (a button's On Pressed, a plate's On Activated) or call it from code (HUD buttons).
// It always returns to the scale it started with. Visual only.
using GameJam.Core;
using UnityEngine;

namespace GameJam.Visuals
{
    /// <summary>Squashes and stretches its target for a moment, then settles back to rest.</summary>
    public class PunchScale : MonoBehaviour
    {
        [Header("Target")]
        [Tooltip("What to punch. Empty = this object. Use the art child, not the object with colliders.")]
        [SerializeField] private Transform _target;

        [Header("Punch")]
        [Tooltip("Scale change at the peak, per axis. (0.15, -0.2) squashes it flat and wide, like a press.")]
        [SerializeField] private Vector2 _amount = new Vector2(0.15f, -0.2f);
        [Tooltip("Seconds until it is back at rest.")]
        [SerializeField] private float _duration = 0.3f;
        [Tooltip("How many times it wobbles past rest before settling.")]
        [SerializeField] private float _wobbles = 2f;
        [Tooltip("On for UI, so it still animates while the game is paused. Off for world art, so it freezes with the room.")]
        [SerializeField] private bool _unscaledTime;

        private Vector3 _restScale;
        private float _elapsed = -1f;
        private float _strength = 1f;

        private void Awake()
        {
            if (_target == null)
            {
                _target = transform;
            }

            _restScale = _target.localScale;
        }

        private void OnDisable()
        {
            _elapsed = -1f;
            _target.localScale = _restScale;
        }

        private void Update()
        {
            if (_elapsed < 0f)
            {
                return;
            }

            _elapsed += _unscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;
            float t = _elapsed / _duration;
            if (t >= 1f)
            {
                _elapsed = -1f;
                _target.localScale = _restScale;
                return;
            }

            float wave = Ease.Punch(t, _wobbles) * _strength;
            _target.localScale = new Vector3(
                _restScale.x * (1f + _amount.x * wave),
                _restScale.y * (1f + _amount.y * wave),
                _restScale.z);
        }

        /// <summary>Plays the punch at its normal strength, restarting it if one is already playing.</summary>
        public void Punch()
        {
            PunchWithStrength(1f);
        }

        /// <summary>Plays the punch scaled by <paramref name="strength"/> (1 = the inspector amount).</summary>
        public void PunchWithStrength(float strength)
        {
            // A plate releasing or a button popping up because the room reset is not an action.
            if (LevelReset.IsResetting)
            {
                return;
            }

            _strength = strength;
            _elapsed = 0f;
        }
    }
}
