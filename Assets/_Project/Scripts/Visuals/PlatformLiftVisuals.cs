// PlatformLiftVisuals: draws a RisingPlatform moving. The top slides from _loweredOffset (down in the
// chasm) to its rest position as Progress goes 0 to 1, and an optional lift (the dip platform's
// scissor-lift lattice, any sprite) stretches from the chasm floor up to the top. Before a timed
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
        [Tooltip("The platform's top sprite, placed at its fully raised position. Child sprites (more tiles, its slab) move and flicker with it.")]
        [SerializeField] private SpriteRenderer _top;
        [Tooltip("Optional lift under the top (scissor lattice or pillar), placed at the chasm floor. Any pivot: it is stretched to fill the gap.")]
        [SerializeField] private SpriteRenderer _lift;

        [Header("Tuning")]
        [Tooltip("Where the top sits when fully lowered, relative to its raised position.")]
        [SerializeField] private Vector2 _loweredOffset = new Vector2(0f, -1.25f);
        [Tooltip("Flicker speed of the drop warning, in flashes per second.")]
        [SerializeField] private float _warningRate = 8f;
        [Range(0f, 1f)]
        [SerializeField] private float _warningAlpha = 0.45f;

        private Vector3 _topRest;
        private SpriteRenderer[] _topParts;
        private Color[] _partColours;
        private float _liftFloor;
        private float _liftSpriteHeight;
        private float _liftSpriteBottom;

        private void Awake()
        {
            _topRest = _top.transform.localPosition;
            _topParts = _top.GetComponentsInChildren<SpriteRenderer>();
            _partColours = new Color[_topParts.Length];
            for (int i = 0; i < _topParts.Length; i++)
            {
                _partColours[i] = _topParts[i].color;
            }
            if (_lift != null)
            {
                _liftFloor = _lift.transform.localPosition.y;
                _liftSpriteHeight = Mathf.Max(0.01f, _lift.sprite.bounds.size.y);
                _liftSpriteBottom = _lift.sprite.bounds.min.y;
            }
        }

        private void LateUpdate()
        {
            // 1. Ease the top between down and up (smoothstep reads as a machine starting and stopping).
            float t = Mathf.SmoothStep(0f, 1f, _platform.Progress);
            _top.transform.localPosition = _topRest + (Vector3)(_loweredOffset * (1f - t));

            // 2. Stretch the lift so its art spans the chasm floor up to the top, whatever its pivot.
            if (_lift != null)
            {
                float reach = Mathf.Max(0f, _top.transform.localPosition.y - _liftFloor);
                Transform lift = _lift.transform;
                float scale = reach / _liftSpriteHeight;
                lift.localScale = new Vector3(lift.localScale.x, scale, 1f);
                lift.localPosition = new Vector3(lift.localPosition.x, _liftFloor - _liftSpriteBottom * scale, lift.localPosition.z);
            }

            // 3. Warn before a timed drop by fading every part of the top in and out.
            bool dim = _platform.IsAboutToDrop && Mathf.Repeat(Time.time * _warningRate, 1f) < 0.5f;
            for (int i = 0; i < _topParts.Length; i++)
            {
                Color colour = _partColours[i];
                if (dim)
                {
                    colour.a *= _warningAlpha;
                }

                _topParts[i].color = colour;
            }
        }
    }
}
