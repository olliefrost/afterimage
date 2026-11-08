// CloneDeathFeedback: the moment a clone dies (e.g. it takes a laser shot for the player) it flashes
// white, glitches sideways and its glow bursts, then settles into the red CloneTint gives dead
// clones. Also shakes the screen a little if the clone is in view. Sits on the Clone prefab root.
// Visual only.
using GameJam.Afterimage;
using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace GameJam.Visuals
{
    /// <summary>A short white flash, glitch and glow burst when a clone dies.</summary>
    public class CloneDeathFeedback : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private AfterimageReplayer _replayer;
        [SerializeField] private SpriteRenderer _sprite;
        [SerializeField] private Light2D _glow;

        [Header("Look")]
        [SerializeField] private float _duration = 0.35f;
        [Tooltip("How far the width glitches at the start, as a fraction of its size.")]
        [SerializeField] private float _glitchAmount = 0.35f;
        [Tooltip("Times per second the glitch jumps.")]
        [SerializeField] private float _glitchRate = 30f;
        [Tooltip("Extra glow intensity at the start of the burst.")]
        [SerializeField] private float _glowBoost = 3f;

        [Header("Screen shake")]
        [SerializeField] private float _shakeStrength = 0.05f;
        [SerializeField] private float _shakeDuration = 0.2f;

        private Vector3 _restScale;
        private float _restGlow;
        private Color _settledColour;
        private float _elapsed = -1f;

        private void Awake()
        {
            _restScale = _sprite.transform.localScale;
            _restGlow = _glow.intensity;
        }

        private void OnEnable()
        {
            _replayer.Died += HandleDied;
        }

        private void OnDisable()
        {
            _replayer.Died -= HandleDied;
            // Only once the settled colour has been read (after the first frame of the effect).
            if (_elapsed > 0f)
            {
                Finish();
            }
        }

        private void Update()
        {
            if (_elapsed < 0f)
            {
                return;
            }

            // CloneTint turns the clone red in the same Died event; its colour is what the flash fades into.
            if (_elapsed == 0f)
            {
                _settledColour = _sprite.color;
            }

            _elapsed += Time.deltaTime;
            float t = _elapsed / _duration;
            if (t >= 1f)
            {
                Finish();
                return;
            }

            // 1. Flash: white fading into the dead red.
            Color white = Color.white;
            white.a = _settledColour.a;
            _sprite.color = Color.Lerp(white, _settledColour, Ease.OutCubic(t));

            // 2. Glitch: the width jumps between random sizes (sometimes mirrored), calming down.
            float step = Mathf.Floor(_elapsed * _glitchRate);
            float jitter = (Mathf.PerlinNoise(step * 0.73f, 3.1f) - 0.5f) * 2f * _glitchAmount * (1f - t);
            _sprite.transform.localScale = new Vector3(_restScale.x * (1f + jitter), _restScale.y, _restScale.z);

            // 3. Glow burst fading back.
            _glow.intensity = _restGlow + _glowBoost * (1f - t) * (1f - t);
        }

        private void HandleDied(AfterimageReplayer replayer)
        {
            _elapsed = 0f;
            ScreenShake.ShakeAt(transform.position, _shakeStrength, _shakeDuration);
        }

        /// <summary>Ends the effect on the settled dead look.</summary>
        private void Finish()
        {
            _elapsed = -1f;
            _sprite.color = _settledColour;
            _sprite.transform.localScale = _restScale;
            _glow.intensity = _restGlow;
        }
    }
}
