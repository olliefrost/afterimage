// PlatformLiftVisuals: draws a RisingPlatform moving. The top slides from _loweredOffset (down in the
// chasm) to its rest position as Progress goes 0 to 1, and an optional lift (the dip platform's
// scissor-lift lattice, a tiled sprite) stretches from the chasm floor up to the top. Before a timed
// platform drops, the top flickers as a warning. Visual only: RisingPlatform decides everything.
using GameJam.Machines;
using UnityEngine;

namespace GameJam.Visuals
{
    /// <summary>Moves a platform's sprites to match how far it has risen.</summary>
    public class PlatformLiftVisuals : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private RisingPlatform _platform;
        [Tooltip("The platform's top sprite, placed at its fully raised position.")]
        [SerializeField] private SpriteRenderer _top;
        [Tooltip("Optional lift under the top (scissor lattice or pillar). Use Draw Mode Tiled; its pivot sits at the chasm floor.")]
        [SerializeField] private SpriteRenderer _lift;

        [Header("Tuning")]
        [Tooltip("Where the top sits when fully lowered, relative to its raised position.")]
        [SerializeField] private Vector2 _loweredOffset = new Vector2(0f, -1.25f);
        [Tooltip("Flicker speed of the drop warning, in flashes per second.")]
        [SerializeField] private float _warningRate = 8f;
        [Range(0f, 1f)]
        [SerializeField] private float _warningAlpha = 0.45f;

        private Vector3 _topRest;
        private Color _topColour;

        private void Awake()
        {
            _topRest = _top.transform.localPosition;
            _topColour = _top.color;
        }

        private void LateUpdate()
        {
            // 1. Ease the top between down and up (smoothstep reads as a machine starting and stopping).
            float t = Mathf.SmoothStep(0f, 1f, _platform.Progress);
            _top.transform.localPosition = _topRest + (Vector3)(_loweredOffset * (1f - t));

            // 2. Stretch the lift from the chasm floor to the underside of the top.
            if (_lift != null)
            {
                Vector2 size = _lift.size;
                size.y = Mathf.Max(0f, _top.transform.localPosition.y - _lift.transform.localPosition.y);
                _lift.size = size;
            }

            // 3. Warn before a timed drop.
            Color colour = _topColour;
            if (_platform.IsAboutToDrop && Mathf.Repeat(Time.time * _warningRate, 1f) < 0.5f)
            {
                colour.a *= _warningAlpha;
            }

            _top.color = colour;
        }
    }
}
