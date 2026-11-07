// ActorFrame: one tick of a recorded run. AfterimageRecorder writes one per tick and
// AfterimageReplayer applies them back in order.
using UnityEngine;

namespace GameJam.Afterimage
{
    /// <summary>What the player was doing on a single tick: where it stood, which way it faced, whether it interacted.</summary>
    public readonly struct ActorFrame
    {
        public readonly Vector2 Position;
        public readonly Vector2 Facing;
        public readonly bool Interacted;

        public ActorFrame(Vector2 position, Vector2 facing, bool interacted)
        {
            Position = position;
            Facing = facing;
            Interacted = interacted;
        }
    }
}
