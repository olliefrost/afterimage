// Recording: a whole recorded run, one ActorFrame per tick (index = tick number).
// Created by AfterimageRecorder, stored in a clone slot by AfterimageManager, played by AfterimageReplayer.
using System.Collections.Generic;

namespace GameJam.Afterimage
{
    /// <summary>The frames of one run, where <c>Frames[t]</c> is what happened on tick <c>t</c>.</summary>
    public class Recording
    {
        // Roughly a minute at the default 50 fixed steps per second, so normal runs never reallocate.
        private const int InitialCapacity = 3000;

        /// <summary>Frames in tick order.</summary>
        public List<ActorFrame> Frames { get; } = new List<ActorFrame>(InitialCapacity);
    }
}
