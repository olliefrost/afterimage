// LightFlash: briefly brightens a 2D light and lets it fade back, for sparks on a button press or a
// flash in a doorway. Wire Flash() to any UnityEvent. Use it on lights nothing else drives (not the
// PowerLights set), since it sets the intensity itself. Visual only.
using GameJam.Core;
using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace GameJam.Visuals
{
    /// <summary>Spikes a Light2D's intensity on Flash() and eases it back to its resting value.</summary>
    public class LightFlash : MonoBehaviour
    {
        [Header("References")]
        [Tooltip("The light to flash. Empty = a Light2D on this object.")]
        [SerializeField] private Light2D _light;

        [Header("Flash")]
        [Tooltip("Intensity added at the start of the flash.")]
        [SerializeField] private float _boost = 2f;
        [Tooltip("Seconds to fade back to rest.")]
        [SerializeField] private float _duration = 0.25f;

        private float _restIntensity;
        private float _elapsed = -1f;

        private void Awake()
        {
            if (_light == null)
            {
                _light = GetComponent<Light2D>();
            }

            _restIntensity = _light.intensity;
        }

        private void OnDisable()
        {
            _elapsed = -1f;
            _light.intensity = _restIntensity;
        }

        private void Update()
        {
            if (_elapsed < 0f)
            {
                return;
            }

            _elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(_elapsed / _duration);
            _light.intensity = _restIntensity + _boost * (1f - Ease.OutCubic(t));
            if (t >= 1f)
            {
                _elapsed = -1f;
            }
        }

        /// <summary>Starts (or restarts) the flash.</summary>
        public void Flash()
        {
            if (LevelReset.IsResetting)
            {
                return;
            }

            _elapsed = 0f;
        }
    }
}
