// LadderClimber: sits on the player and clones and notes, once per tick, which Ladder (if any) the
// actor is standing on. PlayerMovement asks it to turn walking into climbing; on a clone it only
// feeds the climb animation, because the clone's replayed positions already follow the ladder.
using GameJam.Interactables;
using UnityEngine;

namespace GameJam.Actors
{
    /// <summary>Knows whether its actor is on a ladder this tick.</summary>
    [DefaultExecutionOrder(-6)]
    public class LadderClimber : MonoBehaviour
    {
        // Keys must point clearly along the ladder (not sideways to it) to climb, so a diagonal tap doesn't wobble.
        private const float MinAlignment = 0.3f;

        /// <summary>The ladder the actor is on, or null.</summary>
        public Ladder Current { get; private set; }

        /// <summary>True while on a ladder.</summary>
        public bool IsClimbing => Current != null;

        /// <summary>
        /// Turns screen-relative movement keys into a climb along the current ladder: up or down its axis
        /// at climb speed, or nothing for keys that point across it. Returns world velocity.
        /// </summary>
        public Vector2 ClimbVelocity(Vector2 input)
        {
            float along = Vector2.Dot(input, Current.ScreenAxis);
            if (Mathf.Abs(along) < MinAlignment)
            {
                return Vector2.zero;
            }

            return Current.Axis * (Mathf.Sign(along) * Current.ClimbSpeed);
        }

        private void FixedUpdate()
        {
            Current = Ladder.At(transform.position);
        }
    }
}
