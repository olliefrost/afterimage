// PowerLights: fades a group of lights up when the room is powered and down when it is not, so
// the player sees the moment Chamber 3's generator starts. Visual only: listens to PowerGenerator.
using GameJam.Machines;
using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace GameJam.Visuals
{
    /// <summary>Brightens its lights while the room has power.</summary>
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

        private float[] _poweredIntensities;
        private float _level;

        private void Awake()
        {
            _poweredIntensities = new float[_lights.Length];
            for (int i = 0; i < _lights.Length; i++)
            {
                _poweredIntensities[i] = _lights[i].intensity;
            }

            Apply();
        }

        private void Update()
        {
            float target = _generator.IsPowered ? 1f : 0f;
            if (Mathf.Approximately(_level, target))
            {
                return;
            }

            _level = Mathf.MoveTowards(_level, target, Time.deltaTime / _fadeSeconds);
            Apply();
        }

        private void Apply()
        {
            float fraction = Mathf.Lerp(_unpoweredFraction, 1f, _level);
            for (int i = 0; i < _lights.Length; i++)
            {
                _lights[i].intensity = _poweredIntensities[i] * fraction;
            }
        }
    }
}
