// Falling: the one way hazards kill someone by dropping them into a chasm (FallZone for a missed jump
// or a walk over a chasm, RisingPlatform when it drops with people on it). It announces the fall before
// the kill, so CharacterSpriteAnimator can show the character dropping out of sight instead of just
// freezing where they stood. Gameplay is the same as IActor.Kill(); the event is for visuals only.
using System;
using GameJam.Core;

namespace GameJam.Hazards
{
    /// <summary>Kills an actor by a fall and tells listeners so they can draw it.</summary>
    public static class Falling
    {
        /// <summary>Raised just before a living actor is killed by a fall.</summary>
        public static event Action<IActor> Fell;

        /// <summary>Kills <paramref name="actor"/> as a fall (no-op if already dead).</summary>
        public static void Kill(IActor actor)
        {
            if (actor.IsDead)
            {
                return;
            }

            Fell?.Invoke(actor);
            actor.Kill();
        }
    }
}
