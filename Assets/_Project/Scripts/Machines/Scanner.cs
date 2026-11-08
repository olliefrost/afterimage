// Scanner: the glass tube in Chamber 4's scanning chamber. A living actor standing inside it for
// _scanTicks in a row completes a scan, which fires OnScanned once per attempt (wired to the far-side
// door and PlatformSequence.Begin). Stepping out early restarts the scan. ScanProgress drives the
// light sweep visual. Tick-counted so a clone's scan finishes on the same tick as the original run.
using GameJam.Actors;
using GameJam.Core;
using UnityEngine;
using UnityEngine.Events;

namespace GameJam.Machines
{
    /// <summary>Completes a scan when someone stands inside long enough.</summary>
    public class Scanner : MonoBehaviour, IResettable, ISignalSource
    {
        [Header("References")]
        [SerializeField] private TickClock _clock;
        [Tooltip("The area inside the tube.")]
        [SerializeField] private ActorZone _zone;

        [Header("Tuning")]
        [Tooltip("Ticks someone must stand inside to complete a scan (50 per second).")]
        [Min(1)]
        [SerializeField] private int _scanTicks = 50;

        [Header("Events")]
        [Tooltip("Fired once when a scan completes, e.g. Door.Open on the far side and PlatformSequence.Begin.")]
        [SerializeField] private UnityEvent _onScanned = new UnityEvent();

        private int _ticksInside;

        /// <summary>True once a scan has completed this attempt.</summary>
        public bool IsScanned { get; private set; }

        /// <inheritdoc />
        public bool IsOn => IsScanned;

        /// <summary>0 to 1 while a scan is running, 1 once done, for the sweep visual.</summary>
        public float ScanProgress => IsScanned ? 1f : (float)_ticksInside / _scanTicks;

        private void OnEnable()
        {
            LevelReset.Register(this);
            ResetState();
        }

        private void OnDisable()
        {
            LevelReset.Unregister(this);
        }

        /// <summary>Clears the scan for the next attempt.</summary>
        public void ResetState()
        {
            _ticksInside = 0;
            IsScanned = false;
        }

        private void FixedUpdate()
        {
            if (!_clock.IsRunning || IsScanned)
            {
                return;
            }

            // Count up while someone stands inside; leaving restarts the scan.
            _ticksInside = _zone.HasLivingActor ? _ticksInside + 1 : 0;
            if (_ticksInside < _scanTicks)
            {
                return;
            }

            IsScanned = true;
            _onScanned.Invoke();
        }
    }
}
