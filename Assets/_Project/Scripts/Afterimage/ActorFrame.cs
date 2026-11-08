// ActorFrame: one tick of a recorded run. AfterimageRecorder writes one per tick and
// AfterimageReplayer applies them back in order.
using UnityEngine;

namespace GameJam.Afterimage
{
    /// <summary>
    /// What the player was doing on a single tick: where it stood, which way it faced, whether it
    /// interacted, and whether it started a jump (Chamber 4 onward).
    /// </summary>
    public readonly struct ActorFrame
    {
        public readonly Vector2 Position;
        public readonly Vector2 Facing;
        public readonly bool Interacted;
        public readonly bool Jumped;

        public ActorFrame(Vector2 position, Vector2 facing, bool interacted, bool jumped)
        {
            Position = position;
            Facing = facing;
            Interacted = interacted;
            Jumped = jumped;
        }
    }
}
