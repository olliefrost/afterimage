// HudButtonFeedback: makes a HUD run button (R, T, F) react: a bump when its action goes through
// and a sideways "no" shake when it is refused. HudController calls Punch and Deny from
// GameStateController.RunActionHandled, so keys and clicks look the same. It also owns the button's
// base scale (HudController's pulse sets it), so the pulse and the bump multiply instead of fighting.
// While nothing plays it leaves the scale alone, so CloneHudReveal can pop the button in.
// Visual only, real time (works while paused).
using GameJam.Visuals;
using UnityEngine;

namespace GameJam.UI
{
    /// <summary>Bump and refusal shake for one HUD button.</summary>
    [RequireComponent(typeof(RectTransform))]
    public class HudButtonFeedback : MonoBehaviour
    {
        [Header("Bump (action accepted)")]
        [SerializeField] private float _punchDuration = 0.3f;
        [Tooltip("How much bigger the button gets at the top of the bump (0.18 = 18%).")]
        [SerializeField] private float _punchAmount = 0.18f;

        [Header("Shake (action refused)")]
        [SerializeField] private float _denyDuration = 0.35f;
        [Tooltip("Furthest the button moves sideways, in canvas pixels.")]
        [SerializeField] private float _denyDistance = 10f;
        [SerializeField] private float _denySpeed = 55f;

        private RectTransform _rect;
        private Vector2 _restPosition;
        private float _baseScale = 1f;
        private bool _baseChanged;
        private float _punchElapsed = -1f;
        private float _denyElapsed = -1f;

        private void Awake()
        {
            _rect = (RectTransform)transform;
            _restPosition = _rect.anchoredPosition;
        }

        private void OnDisable()
        {
            _punchElapsed = -1f;
            _denyElapsed = -1f;
            _rect.anchoredPosition = _restPosition;
        }

        private void Update()
        {
            bool punching = _punchElapsed >= 0f;
            bool denying = _denyElapsed >= 0f;
            if (!punching && !denying && !_baseChanged)
            {
                return;
            }

            _baseChanged = false;
            float delta = Time.unscaledDeltaTime;

            // 1. Bump: scale up and wobble back, on top of the base (pulse) scale.
            float scale = _baseScale;
            if (punching)
            {
                _punchElapsed += delta;
                float t = _punchElapsed / _punchDuration;
                scale *= 1f + _punchAmount * Ease.Punch(t, 1.5f);
                if (t >= 1f)
                {
                    _punchElapsed = -1f;
                    _baseChanged = true;
                }
            }

            // 2. Refusal: a quick sideways shake that dies away, plus a slight shrink.
            float offset = 0f;
            if (denying)
            {
                _denyElapsed += delta;
                float t = _denyElapsed / _denyDuration;
                float fade = 1f - Mathf.Clamp01(t);
                offset = Mathf.Sin(_denyElapsed * _denySpeed) * _denyDistance * fade;
                scale *= 1f - 0.06f * fade;
                if (t >= 1f)
                {
                    _denyElapsed = -1f;
                    _baseChanged = true;
                }
            }

            _rect.localScale = new Vector3(scale, scale, 1f);
            _rect.anchoredPosition = _restPosition + new Vector2(offset, 0f);
        }

        /// <summary>Sets the resting scale (HudController's pulse); bumps play on top of it.</summary>
        public void SetBaseScale(float scale)
        {
            if (!Mathf.Approximately(scale, _baseScale))
            {
                _baseScale = scale;
                _baseChanged = true;
            }
        }

        /// <summary>Bump: the button's action went through.</summary>
        public void Punch()
        {
            _punchElapsed = 0f;
        }

        /// <summary>Shake: the button's action was refused.</summary>
        public void Deny()
        {
            _denyElapsed = 0f;
        }
    }
}
