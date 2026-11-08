// UiPopIn: animates a UI element in every time it is shown (enabled): it fades up and grows from a
// little smaller with a slight overshoot. Used on the pause menu's pages and backdrop and on the
// title menu buttons (with a delay each, so they arrive one after another). Real time, so it plays
// while the game is paused. Visual only.
using GameJam.Visuals;
using UnityEngine;

namespace GameJam.UI
{
    /// <summary>Fade and scale pop played each time the element becomes active.</summary>
    [RequireComponent(typeof(RectTransform))]
    public class UiPopIn : MonoBehaviour
    {
        [Header("Animation")]
        [Tooltip("Seconds to wait after being shown before popping in (for staggered lists).")]
        [SerializeField] private float _delay;
        [SerializeField] private float _duration = 0.3f;
        [Tooltip("Scale it grows from. 1 = fade only.")]
        [SerializeField] private float _startScale = 0.92f;
        [Tooltip("Overshoot past full size (1.7 is about 10%, 0 is none).")]
        [SerializeField] private float _overshoot = 1.2f;

        [Header("References")]
        [Tooltip("Optional fade. Empty = scale only.")]
        [SerializeField] private CanvasGroup _group;

        private RectTransform _rect;
        private float _elapsed = -1f;

        private void Awake()
        {
            _rect = (RectTransform)transform;
        }

        private void OnEnable()
        {
            _elapsed = 0f;
            Apply(0f);
        }

        private void OnDisable()
        {
            _elapsed = -1f;
            Apply(1f);
        }

        private void Update()
        {
            if (_elapsed < 0f)
            {
                return;
            }

            _elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01((_elapsed - _delay) / _duration);
            Apply(t);
            if (t >= 1f)
            {
                _elapsed = -1f;
            }
        }

        /// <summary>Sets the look for progress <paramref name="t"/> (0 hidden and small, 1 fully shown).</summary>
        private void Apply(float t)
        {
            float scale = Mathf.LerpUnclamped(_startScale, 1f, Ease.BackOut(t, _overshoot));
            _rect.localScale = new Vector3(scale, scale, 1f);
            if (_group != null)
            {
                _group.alpha = Mathf.Clamp01(t * 2f);
            }
        }
    }
}
