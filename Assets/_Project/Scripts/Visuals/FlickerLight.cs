// FlickerLight: makes a 2D light flicker like a half-broken relic lamp, with smooth noise plus
// short random dropouts. Visual only; it uses its own seeded generator so it never touches gameplay.
using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace GameJam.Visuals
{
    /// <summary>Varies a Light2D's intensity to look unstable.</summary>
    [RequireComponent(typeof(Light2D))]
    public class FlickerLight : MonoBehaviour
    {
        [Header("Flicker")]
        [Tooltip("Lowest intensity as a fraction of the light's starting intensity.")]
        [SerializeField, Range(0f, 1f)] private float _minFactor = 0.6f;
        [Tooltip("How fast the smooth flicker changes.")]
        [SerializeField] private float _noiseSpeed = 3f;

        [Header("Dropouts")]
        [Tooltip("Average dropouts per second, where the lamp almost cuts out.")]
        [SerializeField] private float _dropoutsPerSecond = 0.4f;
        [SerializeField] private Vector2 _dropoutDuration = new Vector2(0.05f, 0.2f);
        [SerializeField] private int _seed = 1;

        private Light2D _light;
        private System.Random _random;
        private float _baseIntensity;
        private float _noiseOffset;
        private float _dropoutTimer;

        private void Awake()
        {
            _light = GetComponent<Light2D>();
            _baseIntensity = _light.intensity;
            _random = new System.Random(_seed);
            _noiseOffset = (float)_random.NextDouble() * 100f;
        }

        private void Update()
        {
            // 1. Occasionally start a short dropout.
            if (_dropoutTimer <= 0f && _random.NextDouble() < _dropoutsPerSecond * Time.deltaTime)
            {
                _dropoutTimer = Mathf.Lerp(_dropoutDuration.x, _dropoutDuration.y, (float)_random.NextDouble());
            }

            // 2. Smooth noise normally, near-dark during a dropout.
            float noise = Mathf.PerlinNoise(_noiseOffset, Time.time * _noiseSpeed);
            float factor = Mathf.Lerp(_minFactor, 1f, noise);
            if (_dropoutTimer > 0f)
            {
                _dropoutTimer -= Time.deltaTime;
                factor = 0.1f;
            }

            _light.intensity = _baseIntensity * factor;
        }
    }
}
