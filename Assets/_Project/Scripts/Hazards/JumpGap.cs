// JumpGap: an invisible wall along the edge of a chasm. It stops anyone walking off the edge (nobody
// can walk or fall off a ledge), but a jump passes over it: PlayerJump asks All for every gap and
// ignores collisions with them while airborne. Where a jump lands is FallZone's business.
// RisingPlatform deactivates its own gap object while it is up, so people can walk across. Always
// toggle the whole object: Unity forgets ignored collisions when a collider is switched off, so a gap
// coming back raises Added for anyone mid-jump to ignore it again.
using System.Collections.Generic;
using UnityEngine;

namespace GameJam.Hazards
{
    /// <summary>A solid chasm edge that blocks walking but not jumping.</summary>
    [RequireComponent(typeof(Collider2D))]
    public class JumpGap : MonoBehaviour
    {
        private static readonly List<Collider2D> _all = new List<Collider2D>();

        private Collider2D _collider;

        /// <summary>Every gap collider in the loaded level.</summary>
        public static IReadOnlyList<Collider2D> All => _all;

        /// <summary>Raised when a gap appears, so an actor already mid-jump can ignore it too.</summary>
        public static event System.Action<Collider2D> Added;

        private void Awake()
        {
            _collider = GetComponent<Collider2D>();
        }

        private void OnEnable()
        {
            _all.Add(_collider);
            Added?.Invoke(_collider);
        }

        private void OnDisable()
        {
            _all.Remove(_collider);
        }
    }
}
