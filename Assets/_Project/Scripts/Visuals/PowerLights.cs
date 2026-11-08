// PowerLights: fades a group of lights up when the room is powered and down when it is not, so
// the player sees the moment Chamber 3's generator starts. Powering up overshoots in a short surge
// before settling; losing power stutters the lights out like a failing supply.
// Visual only: listens to PowerGenerator.
using GameJam.Machines;
using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace GameJam.Visuals
{
    /// <summary>Brightens its lights while the room has power, with a surge on and a stutter off.</summary>
    public class PowerLights : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private PowerGenerator _generator;
        [Tooltip("Lights to fade. Their intensity set in the scene is their powered brightness.")]
        [SerializeField] private Light2D[] _lights;

        [Header("Look")]
        [Tooltip("Fraction of full brightness while unpowered (0 = off).")]
        [Range(0f, 1f)]
        [SerializeField] private float _unpoweredFraction = 0f;
        [Tooltip("Seconds to fade fully on or off.")]
        [SerializeField] private float _fadeSeconds = 0.6f;
        [Tooltip("Extra brightness at the peak of the power-up surge (0.6 = 60% brighter).")]
        [SerializeField] private float _surge = 0.6f;
        [Tooltip("Flickers per second while the power dies.")]
        [SerializeField] private float _stutterRate = 12f;

        private float[] _poweredIntensities;
        private float _level;

        private void Awake()
        {
            _poweredIntensities = new float[_lights.Length];
            for (int i = 0; i < _lights.Length; i++)
            {
                _poweredIntensities[i] = _lights[i].intensity;
            }

            Apply(1f);
        }

        private void Update()
        {
            bool powered = _generator.IsPowered;
            float target = powered ? 1f : 0f;
            if (Mathf.Approximately(_level, target))
            {
                return;
            }

            _level = Mathf.MoveTowards(_level, target, Time.deltaTime / _fadeSeconds);

            // Powering up: brightness climbs past full and drops back as the fade completes.
            // Powering down: the lights blink on and off while they fade.
            // Once the fade lands, settle at the plain level so a stutter frame never sticks.
            float multiplier;
            if (Mathf.Approximately(_level, target))
            {
                multiplier = 1f;
            }
            else if (powered)
            {
                multiplier = 1f + _surge * Mathf.Sin(_level * Mathf.PI);
            }
            else
            {
                multiplier = Mathf.Repeat(Time.time * _stutterRate, 1f) < 0.5f ? 1f : 0.3f;
            }

            Apply(multiplier);
        }

        /// <summary>Sets every light from the current level, times <paramref name="multiplier"/>.</summary>
        private void Apply(float multiplier)
        {
            float fraction = Mathf.Lerp(_unpoweredFraction, 1f, _level) * multiplier;
            for (int i = 0; i < _lights.Length; i++)
            {
                _lights[i].intensity = _poweredIntensities[i] * fraction;
            }
        }
    }
}
