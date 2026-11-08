// CloneMaterialise: when a clone spawns at the start of an attempt it beams in rather than popping
// in: a ring in its slot colour ripples out on the floor, the sprite flickers in thin and tall, then
// settles to normal, and its glow flares and dims. Sits on the Clone prefab root. Visual only: the
// clone replays from tick 0 as usual while this plays.
using System.Collections;
using GameJam.Afterimage;
using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace GameJam.Visuals
{
    /// <summary>Plays the clone's short beam-in effect once, on spawn.</summary>
    public class CloneMaterialise : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private AfterimageReplayer _replayer;
        [Tooltip("The character sprite's transform (Body/Visual), stretched while beaming in.")]
        [SerializeField] private Transform _visual;
        [SerializeField] private SpriteRenderer _sprite;
        [SerializeField] private Light2D _glow;
        [Tooltip("Floor ring under the clone; tinted with the slot colour and stepped through the ring frames.")]
        [SerializeField] private SpriteRenderer _ring;
        [SerializeField] private Sprite[] _ringFrames;

        [Header("Timing")]
        [SerializeField] private float _duration = 0.5f;
        [Tooltip("Flicker rate (on/off switches per second) at the start of the beam-in.")]
        [SerializeField] private float _flickerRate = 30f;
        [Tooltip("Glow brightness at the start, as a multiple of its normal intensity.")]
        [SerializeField] private float _glowFlare = 4f;

        private void Start()
        {
            // Start runs after AfterimageManager.Init, so the slot colour is known.
            StartCoroutine(BeamIn());
        }

        private IEnumerator BeamIn()
        {
            Vector3 restScale = _visual.localScale;
            float restGlow = _glow.intensity;
            Color ringColour = _replayer.SlotColour;
            _ring.enabled = _ringFrames.Length > 0;

            for (float t = 0f; t < _duration; t += Time.deltaTime)
            {
                float k = t / _duration;

                // 1. Thin and tall, overshooting a little wider before settling: a beam resolving into a body.
                float width = Mathf.LerpUnclamped(0.1f, 1f, EaseOutBack(k));
                float height = Mathf.Lerp(1.5f, 1f, 1f - (1f - k) * (1f - k));
                _visual.localScale = new Vector3(restScale.x * width, restScale.y * height, restScale.z);

                // 2. Flicker in for the first two thirds, then stay solid.
                _sprite.enabled = k > 0.66f || ((int)(t * _flickerRate) & 1) == 0;

                // 3. Glow flares and dims back; the floor ring ripples out and fades.
                _glow.intensity = restGlow * Mathf.Lerp(_glowFlare, 1f, k);
                if (_ring.enabled)
                {
                    _ring.sprite = _ringFrames[Mathf.Min((int)(k * _ringFrames.Length), _ringFrames.Length - 1)];
                    ringColour.a = 1f - k;
                    _ring.color = ringColour;
                }

                yield return null;
            }

            _visual.localScale = restScale;
            _sprite.enabled = true;
            _glow.intensity = restGlow;
            _ring.enabled = false;
        }

        /// <summary>Overshoots past 1 then settles, for a springy arrival.</summary>
        private static float EaseOutBack(float x)
        {
            const float c1 = 1.70158f;
            const float c3 = c1 + 1f;
            float m = x - 1f;
            return 1f + c3 * m * m * m + c1 * m * m;
        }
    }
}
