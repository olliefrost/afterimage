// LightStrip: a row of soft pools of light that guides the eye, like the ceiling-breach pool over
// Tutorial1's pit. The Ending lights them one after another from the door towards the sister's
// hologram, then cuts them all when the trap springs. EndingSequence drives it. Visual only.
using System.Collections;
using GameJam.Audio;
using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace GameJam.Visuals
{
    /// <summary>Switches a line of lights on in order, and off all at once.</summary>
    public class LightStrip : MonoBehaviour
    {
        [Header("Lights")]
        [Tooltip("The pools in order, nearest the door first.")]
        [SerializeField] private Light2D[] _lights;
        [Tooltip("Seconds each pool takes to fade up once it starts.")]
        [SerializeField] private float _fadeTime = 0.35f;
        [Tooltip("Optional click played as each pool starts to come on.")]
        [SerializeField] private SoundCue _lightOn;

        private float[] _fullIntensity;

        private void Awake()
        {
            // Remember the designed brightness, then start dark.
            _fullIntensity = new float[_lights.Length];
            for (int i = 0; i < _lights.Length; i++)
            {
                _fullIntensity[i] = _lights[i].intensity;
                _lights[i].intensity = 0f;
            }
        }

        /// <summary>Fades the pools up one after another, spread over <paramref name="seconds"/>.</summary>
        public IEnumerator TurnOnInOrder(float seconds)
        {
            // 1. Pool i starts at its slice of the total time and takes _fadeTime to reach full.
            float fade = Mathf.Max(0.01f, _fadeTime);
            float step = _lights.Length > 1 ? Mathf.Max(0f, seconds - fade) / (_lights.Length - 1) : 0f;
            int clicked = 0;
            for (float t = 0f; t < seconds; t += Time.deltaTime)
            {
                // A click as each pool starts, in order.
                while (clicked < _lights.Length && t >= clicked * step)
                {
                    AudioManager.Play(_lightOn);
                    clicked++;
                }

                for (int i = 0; i < _lights.Length; i++)
                {
                    float k = Mathf.Clamp01((t - i * step) / fade);
                    _lights[i].intensity = _fullIntensity[i] * Ease.OutCubic(k);
                }

                yield return null;
            }

            SetAll(true);
        }

        /// <summary>Puts every pool at full brightness, or off, straight away.</summary>
        public void SetAll(bool on)
        {
            for (int i = 0; i < _lights.Length; i++)
            {
                _lights[i].intensity = on ? _fullIntensity[i] : 0f;
            }
        }
    }
}
