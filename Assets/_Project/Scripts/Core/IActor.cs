// IActor: anything that can act in the level, either the live player or a replaying clone.
// Interactables, triggers and hazards talk to this interface so they never depend on the player type.
using UnityEngine;

namespace GameJam.Core
{
    /// <summary>A body in the level that can interact with things and be killed: the player or a clone.</summary>
    public interface IActor
    {
        /// <summary>The actor's transform, for positions and distance checks.</summary>
        Transform Transform { get; }

        /// <summary>True for a replaying afterimage, false for the live player.</summary>
        bool IsClone { get; }

        /// <summary>True once the actor has been killed this attempt. Dead actors no longer block hazards.</summary>
        bool IsDead { get; }

        /// <summary>Kills the actor for the rest of the attempt. Hazards such as lasers call this.</summary>
        void Kill();
    }
}
