// PowerGenerator: Chamber 3's generator. FuelBelt calls Feed() each time a fuel crate reaches it;
// the room stays powered for a short while after each crate, so it only stays on while fuel keeps
// arriving. Hold plates, the hover stairs, the exit and the room lights all check or listen to it.
using System;
using GameJam.Core;
using UnityEngine;
using UnityEngine.Events;

namespace GameJam.Machines
{
    /// <summary>Powers the room while fuel keeps arriving, on the shared tick clock.</summary>
    public class PowerGenerator : MonoBehaviour, IResettable
    {
        [Header("References")]
        [SerializeField] private TickClock _clock;

        [Header("Tuning")]
        [Tooltip("Ticks the room stays powered after each fuel crate. Keep it longer than the belt's spawn interval.")]
        [Min(1)]
        [SerializeField] private int _poweredTicksPerFuel = 150;

        [Header("Events")]
        [SerializeField] private UnityEvent _onPowered = new UnityEvent();
        [SerializeField] private UnityEvent _onUnpowered = new UnityEvent();

        private int _poweredUntilTick = -1;

        /// <summary>True while the room has power.</summary>
        public bool IsPowered { get; private set; }

        /// <summary>Raised when power turns on (true) or off (false).</summary>
        public event Action<bool> PoweredChanged;

        private void OnEnable()
        {
            LevelReset.Register(this);
        }

        private void OnDisable()
        {
            LevelReset.Unregister(this);
        }

        /// <summary>Burns one fuel crate: powers the room, or keeps it powered for longer.</summary>
        public void Feed()
        {
            _poweredUntilTick = _clock.CurrentTick + _poweredTicksPerFuel;
            SetPowered(true);
        }

        /// <summary>Cuts the power for the next attempt.</summary>
        public void ResetState()
        {
            _poweredUntilTick = -1;
            SetPowered(false);
        }

        private void FixedUpdate()
        {
            // Power runs out once no crate has arrived for a while.
            if (_clock.IsRunning && IsPowered && _clock.CurrentTick > _poweredUntilTick)
            {
                SetPowered(false);
            }
        }

        private void SetPowered(bool powered)
        {
            if (IsPowered == powered)
            {
                return;
            }

            IsPowered = powered;
            PoweredChanged?.Invoke(powered);
            (powered ? _onPowered : _onUnpowered).Invoke();
        }
    }
}
