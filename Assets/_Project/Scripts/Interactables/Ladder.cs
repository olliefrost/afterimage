// Ladder: a climbable strip from the floor up to a raised ledge (Chamber 4's ladder to the plate).
// Walking into it starts climbing: PlayerMovement locks the player to the ladder's axis at climb
// speed, ElevationField draws anyone on it at a height that grows from the bottom to the top, and
// CharacterSpriteAnimator shows the climb frames. Like all heights, the climb is visual: physics
// only moves the actor a short way across the ladder's footprint. Side rails (walls in the scene)
// stop people stepping off halfway.
using System.Collections.Generic;
using UnityEngine;

namespace GameJam.Interactables
{
    /// <summary>A floor-to-ledge climb with a fixed axis and climb time.</summary>
    [RequireComponent(typeof(Collider2D))]
    public class Ladder : MonoBehaviour
    {
        private static readonly List<Ladder> _all = new List<Ladder>();

        [Header("References")]
        [Tooltip("Floor end of the ladder (on the floor plan).")]
        [SerializeField] private Transform _bottom;
        [Tooltip("Ledge end of the ladder (on the floor plan).")]
        [SerializeField] private Transform _top;

        [Header("Tuning")]
        [Tooltip("Seconds to climb from bottom to top.")]
        [Min(0.1f)]
        [SerializeField] private float _climbSeconds = 1.2f;
        [Tooltip("Matches PlayerMovement's iso vertical scale, so keys map onto the ladder the way they look on screen.")]
        [SerializeField] private float _isoVerticalScale = 0.5f;

        private Collider2D _area;

        /// <summary>Unit floor-plan direction from the bottom to the top.</summary>
        public Vector2 Axis { get; private set; }

        /// <summary>The ladder's axis as it looks on screen (iso squash undone), for reading movement keys.</summary>
        public Vector2 ScreenAxis { get; private set; }

        /// <summary>Floor-plan units per second while climbing, so the whole climb takes the set time.</summary>
        public float ClimbSpeed { get; private set; }

        /// <summary>The ladder at <paramref name="point"/>, or null if there is none.</summary>
        public static Ladder At(Vector2 point)
        {
            for (int i = 0; i < _all.Count; i++)
            {
                if (_all[i]._area.OverlapPoint(point))
                {
                    return _all[i];
                }
            }

            return null;
        }

        /// <summary>How far up the ladder <paramref name="point"/> is: 0 at the bottom, 1 at the top.</summary>
        public float Fraction(Vector2 point)
        {
            Vector2 span = (Vector2)(_top.position - _bottom.position);
            return Mathf.Clamp01(Vector2.Dot(point - (Vector2)_bottom.position, span) / span.sqrMagnitude);
        }

        private void Awake()
        {
            _area = GetComponent<Collider2D>();
            Vector2 span = _top.position - _bottom.position;
            Axis = span.normalized;
            ScreenAxis = new Vector2(span.x, span.y / _isoVerticalScale).normalized;
            ClimbSpeed = span.magnitude / _climbSeconds;
        }

        private void OnEnable()
        {
            _all.Add(this);
        }

        private void OnDisable()
        {
            _all.Remove(this);
        }
    }
}
