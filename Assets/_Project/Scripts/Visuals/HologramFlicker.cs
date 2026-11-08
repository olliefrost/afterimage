// HologramFlicker: makes the sister's hologram in the Ending look like an unstable projection: a
// soft alpha flicker with a small sideways jitter, plus GlitchOut, the stutter and stretch that
// ends it when the trap springs. EndingSequence calls GlitchOut. Visual only.
using System.Collections;
using UnityEngine;

namespace GameJam.Visuals
{
    /// <summary>Flickers a sprite like a hologram and can glitch it out of existence.</summary>
    public class HologramFlicker : MonoBehaviour
    {
        [Header("References")]
        [Tooltip("The sprite that flickers.")]
        [SerializeField] private SpriteRenderer _renderer;

        [Header("Flicker")]
        [SerializeField, Range(0f, 1f)] private float _minAlpha = 0.45f;
        [SerializeField, Range(0f, 1f)] private float _maxAlpha = 0.85f;
        [SerializeField] private float _flickerSpeed = 9f;
        [Tooltip("Sideways jitter in world units.")]
        [SerializeField] private float _jitter = 0.02f;

        [Header("Glitch out")]
        [SerializeField] private float _glitchTime = 0.45f;

        private Vector3 _restPosition;
        private Vector3 _restScale;
        private Color _baseColour;
        private bool _glitching;
        private float _seed;

        /// <summary>True once the hologram has fully glitched away.</summary>
        public bool IsGone { get; private set; }

        private void Awake()
        {
            _restPosition = _renderer.transform.localPosition;
            _restScale = _renderer.transform.localScale;
            _baseColour = _renderer.color;
            _seed = Random.value * 100f;
        }

        private void Update()
        {
            if (_glitching || IsGone)
            {
                return;
            }

            // 1. Smooth noise for the alpha, with a faster noise for the sideways jitter.
            float noise = Mathf.PerlinNoise(_seed, Time.time * _flickerSpeed);
            SetAlpha(Mathf.Lerp(_minAlpha, _maxAlpha, noise));
            float shake = (Mathf.PerlinNoise(_seed + 11f, Time.time * _flickerSpeed * 3f) - 0.5f) * 2f;
            _renderer.transform.localPosition = _restPosition + new Vector3(shake * _jitter, 0f, 0f);
        }

        /// <summary>Stutters, stretches sideways and vanishes, then leaves the sprite hidden.</summary>
        public void GlitchOut()
        {
            if (!_glitching && !IsGone)
            {
                StartCoroutine(Glitch());
            }
        }

        private IEnumerator Glitch()
        {
            _glitching = true;
            for (float t = 0f; t < _glitchTime; t += Time.deltaTime)
            {
                // 1. Random on/off stutter that gets wider and thinner as it goes.
                float k = t / _glitchTime;
                bool on = Random.value > k * 0.8f;
                SetAlpha(on ? 1f : 0.1f);

                // 2. Stretch sideways and squash vertically, with a bigger jump each frame.
                float stretch = 1f + k * 1.5f * Random.value;
                _renderer.transform.localScale = new Vector3(_restScale.x * stretch, _restScale.y * (1f - k * 0.7f), _restScale.z);
                _renderer.transform.localPosition = _restPosition + new Vector3((Random.value - 0.5f) * 0.2f * k, 0f, 0f);
                yield return null;
            }

            _renderer.enabled = false;
            IsGone = true;
            _glitching = false;
        }

        private void SetAlpha(float alpha)
        {
            Color colour = _baseColour;
            colour.a = alpha;
            _renderer.color = colour;
        }
    }
}
