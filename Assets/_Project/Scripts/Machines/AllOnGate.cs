// AllOnGate: an AND gate. It turns on while every one of its sources is on (buttons pressed, plates
// held, room powered) and off again when any of them stops, firing an event each way. Chamber 3's exit
// door uses it: both ledge buttons pressed and the room powered opens the door, losing power closes it.
// Runs late in each tick (after players, clones and lasers) so clones' replayed presses are seen on
// the same tick as the original run. It is itself a signal source, so a cable can show the lock.
using System;
using GameJam.Core;
using UnityEngine;
using UnityEngine.Events;

namespace GameJam.Machines
{
    /// <summary>An AND gate over ISignalSources that opens while all of them are on.</summary>
    [DefaultExecutionOrder(70)]
    public class AllOnGate : MonoBehaviour, IResettable, ISignalSource
    {
        [Header("References")]
        [SerializeField] private TickClock _clock;
        [Tooltip("Components that implement ISignalSource, e.g. PressButton, HoldPlate, PowerGenerator.")]
        [SerializeField] private MonoBehaviour[] _sources;

        [Header("Events")]
        [Tooltip("Fired the tick every source becomes on together, e.g. Door.Open.")]
        [SerializeField] private UnityEvent _onAllOn = new UnityEvent();
        [Tooltip("Fired the tick that stops being true, e.g. Door.Close.")]
        [SerializeField] private UnityEvent _onNoLongerAllOn = new UnityEvent();

        private ISignalSource[] _signals;

        /// <summary>Raised when the gate turns on (true) or off (false).</summary>
        public event Action<bool> Changed;

        /// <summary>True while every source is on.</summary>
        public bool IsOn { get; private set; }

        private void Awake()
        {
            _signals = new ISignalSource[_sources.Length];
            for (int i = 0; i < _sources.Length; i++)
            {
                _signals[i] = _sources[i] as ISignalSource;
                if (_signals[i] == null)
                {
                    Debug.LogError($"{name}: source {i} does not implement ISignalSource, so this gate can never open.", this);
                    enabled = false;
                }
            }
        }

        private void OnEnable()
        {
            LevelReset.Register(this);
        }

        private void OnDisable()
        {
            LevelReset.Unregister(this);
        }

        /// <summary>Closes the gate for the next attempt (whatever it drives resets itself).</summary>
        public void ResetState()
        {
            IsOn = false;
        }

        private void FixedUpdate()
        {
            if (!_clock.IsRunning)
            {
                return;
            }

            bool allOn = true;
            for (int i = 0; i < _signals.Length && allOn; i++)
            {
                allOn = _signals[i].IsOn;
            }

            if (allOn == IsOn)
            {
                return;
            }

            IsOn = allOn;
            Changed?.Invoke(allOn);
            (allOn ? _onAllOn : _onNoLongerAllOn).Invoke();
        }
    }
}
