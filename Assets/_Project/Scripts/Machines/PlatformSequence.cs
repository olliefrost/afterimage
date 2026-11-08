// PlatformSequence: raises Chamber 4's chasm platforms one after another once the scanner fires
// Begin(): platform i starts rising _staggerTicks * i ticks after the scan. Each platform drops by
// itself (its own _autoLowerTicks), so the way up only exists as a moving window you chase by jumping.
using GameJam.Core;
using UnityEngine;

namespace GameJam.Machines
{
    /// <summary>Starts a row of platforms rising in order, a fixed number of ticks apart.</summary>
    public class PlatformSequence : MonoBehaviour, IResettable
    {
        [Header("References")]
        [SerializeField] private TickClock _clock;
        [Tooltip("Platforms in the order they rise.")]
        [SerializeField] private RisingPlatform[] _platforms;

        [Header("Tuning")]
        [Tooltip("Ticks between one platform starting to rise and the next (50 per second).")]
        [Min(1)]
        [SerializeField] private int _staggerTicks = 60;

        private bool _running;
        private int _ticksSinceBegin;
        private int _nextIndex;

        private void OnEnable()
        {
            LevelReset.Register(this);
            ResetState();
        }

        private void OnDisable()
        {
            LevelReset.Unregister(this);
        }

        /// <summary>Starts the sequence. Ignored if it is already running this attempt.</summary>
        public void Begin()
        {
            if (_running)
            {
                return;
            }

            _running = true;
            _ticksSinceBegin = 0;
            _nextIndex = 0;
        }

        /// <summary>Stops the sequence for the next attempt (the platforms reset themselves).</summary>
        public void ResetState()
        {
            _running = false;
            _ticksSinceBegin = 0;
            _nextIndex = 0;
        }

        private void FixedUpdate()
        {
            if (!_clock.IsRunning || !_running || _nextIndex >= _platforms.Length)
            {
                return;
            }

            // Raise the next platform once its turn comes (the first one on the tick of the scan).
            if (_ticksSinceBegin >= _nextIndex * _staggerTicks)
            {
                _platforms[_nextIndex].Raise();
                _nextIndex++;
            }

            _ticksSinceBegin++;
        }
    }
}
