// TimedDoor: opens a Door for a fixed number of ticks, then shuts it. Chamber 4's scanning chamber
// uses it: the signal button's SignalWire calls Trigger() when its pulse arrives, and the door stays
// open for 5 s. Anyone standing in the doorway when it shuts is crushed (killed), so a clone can't
// hold it open. A finished scan calls HoldOpen() so whoever was scanned can walk back out. Counting
// ticks (not seconds) keeps clones' replays in step with the original run.
using GameJam.Actors;
using GameJam.Core;
using GameJam.Interactables;
using UnityEngine;

namespace GameJam.Machines
{
    /// <summary>Holds a door open for a set time after a signal, then closes it.</summary>
    public class TimedDoor : MonoBehaviour, IResettable
    {
        [Header("References")]
        [SerializeField] private TickClock _clock;
        [SerializeField] private Door _door;
        [Tooltip("Optional: the doorway. Living actors inside it when the door shuts are killed.")]
        [SerializeField] private ActorZone _doorway;

        [Header("Tuning")]
        [Tooltip("Ticks the door stays open (50 per second).")]
        [Min(1)]
        [SerializeField] private int _openTicks = 250;

        private int _ticksLeft;
        private bool _held;

        /// <summary>True while the door is open.</summary>
        public bool IsOpen => _held || _ticksLeft > 0;

        /// <summary>0 to 1 through the open window, for a "closing soon" warning light. 0 once held open.</summary>
        public float OpenProgress => !_held && _ticksLeft > 0 ? 1f - (float)_ticksLeft / _openTicks : 0f;

        private void OnEnable()
        {
            LevelReset.Register(this);
            ResetState();
        }

        private void OnDisable()
        {
            LevelReset.Unregister(this);
        }

        /// <summary>Opens the door and starts the countdown. Ignored while it is already open.</summary>
        public void Trigger()
        {
            if (IsOpen)
            {
                return;
            }

            _ticksLeft = _openTicks;
            _door.Open();
        }

        /// <summary>Opens the door for the rest of the attempt, cancelling any countdown.</summary>
        public void HoldOpen()
        {
            _held = true;
            _ticksLeft = 0;
            _door.Open();
        }

        /// <summary>Stops the countdown for the next attempt (the Door closes itself on reset).</summary>
        public void ResetState()
        {
            _ticksLeft = 0;
            _held = false;
        }

        private void FixedUpdate()
        {
            if (!_clock.IsRunning || _held || _ticksLeft <= 0)
            {
                return;
            }

            _ticksLeft--;
            if (_ticksLeft > 0)
            {
                return;
            }

            // Time is up: crush anyone in the doorway, then shut it.
            if (_doorway != null)
            {
                _doorway.KillAll();
            }

            _door.Close();
        }
    }
}
