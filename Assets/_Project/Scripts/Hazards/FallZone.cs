// FallZone: the floor of a chasm (Chamber 4's dip and the platform chasm). Any living actor standing
// in it falls and dies, unless they are mid-jump or on one of its raised platforms. Walking can't
// get anyone in (JumpGap edges stop them), so this catches a missed jump, and a clone whose replayed
// path crosses a chasm whose platform isn't up this time. Runs late in the tick, after movement and
// platforms, so it judges where everyone ended up.
using GameJam.Actors;
using GameJam.Core;
using GameJam.Machines;
using UnityEngine;

namespace GameJam.Hazards
{
    /// <summary>Kills anyone standing over a chasm with no platform under them.</summary>
    [DefaultExecutionOrder(65)]
    public class FallZone : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private TickClock _clock;
        [Tooltip("The chasm area.")]
        [SerializeField] private ActorZone _zone;
        [Tooltip("Platforms in this chasm; standing on a raised one is safe.")]
        [SerializeField] private RisingPlatform[] _platforms;

        private System.Predicate<IActor> _isSafe;

        private void Awake()
        {
            // Cached so the per-tick check doesn't allocate a new delegate.
            _isSafe = IsSafe;
        }

        private void FixedUpdate()
        {
            if (_clock.IsRunning && _zone.HasLivingActor)
            {
                _zone.KillAll(_isSafe);
            }
        }

        /// <summary>Safe while in the air or on a raised platform.</summary>
        private bool IsSafe(IActor actor)
        {
            if (ActorJump.IsActorAirborne(actor))
            {
                return true;
            }

            for (int i = 0; i < _platforms.Length; i++)
            {
                if (_platforms[i].Supports(actor))
                {
                    return true;
                }
            }

            return false;
        }
    }
}
