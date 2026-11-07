// TickClock: the shared fixed-step counter that keeps the player's recording and every
// clone's replay in step. GameStateController starts and stops it.
// Runs before all other gameplay scripts so every FixedUpdate in a step sees the same tick.
using UnityEngine;

namespace GameJam.Core
{
    /// <summary>
    /// Counts fixed physics steps since the current attempt started. Gameplay that must replay
    /// identically reads <see cref="CurrentTick"/> instead of using time.
    /// </summary>
    [DefaultExecutionOrder(-100)]
    public class TickClock : MonoBehaviour
    {
        /// <summary>Tick of the current fixed step. The first running step of an attempt is tick 0.</summary>
        public int CurrentTick { get; private set; } = -1;

        /// <summary>True while an attempt is being played. Actors do nothing while it is false.</summary>
        public bool IsRunning { get; private set; }

        /// <summary>Starts a fresh attempt from tick 0 on the next fixed step.</summary>
        public void Restart()
        {
            CurrentTick = -1;
            IsRunning = true;
        }

        /// <summary>Freezes the clock, for example while resetting or after the level is complete.</summary>
        public void Stop()
        {
            IsRunning = false;
        }

        private void FixedUpdate()
        {
            if (IsRunning)
            {
                CurrentTick++;
            }
        }
    }
}
