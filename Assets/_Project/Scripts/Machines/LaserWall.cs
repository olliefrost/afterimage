// LaserWall: the laser fence across Chamber 3's conveyor. FuelBelt asks IsBlocked as each crate
// crosses it and calls Burn() if the crate is destroyed; HydraulicBlock calls SetBlocked while it
// sits in the beam. Actors can't reach the belt, so the wall only ever burns fuel.
using System;
using GameJam.Core;
using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace GameJam.Machines
{
    /// <summary>A beam across the belt that burns fuel unless something blocks it.</summary>
    public class LaserWall : MonoBehaviour, IResettable
    {
        [Header("References")]
        [SerializeField] private TickClock _clock;
        [Tooltip("Draws the beam from the emitter to the far side, or only to the block while blocked.")]
        [SerializeField] private LineRenderer _beam;
        [SerializeField] private Transform _emitter;
        [SerializeField] private Transform _farEnd;
        [Tooltip("Where the beam stops while the hydraulic block is in the way.")]
        [SerializeField] private Transform _blockedEnd;
        [Tooltip("Flares up briefly each time a crate burns, so the player sees why the generator is starving.")]
        [SerializeField] private Light2D _burnFlash;

        [Header("Burn flash")]
        [SerializeField] private float _flashIntensity = 3f;
        [Tooltip("Ticks the burn flash takes to fade out.")]
        [Min(1)]
        [SerializeField] private int _flashTicks = 12;

        private int _flashTicksLeft;

        /// <summary>Raised each time a crate burns in the beam.</summary>
        public event Action Burned;

        /// <summary>True while the hydraulic block is holding the beam back.</summary>
        public bool IsBlocked { get; private set; }

        private void Awake()
        {
            _beam.useWorldSpace = true;
            _beam.positionCount = 2;
        }

        private void OnEnable()
        {
            LevelReset.Register(this);
            ResetState();
        }

        private void OnDisable()
        {
            LevelReset.Unregister(this);
        }

        /// <summary>Blocks or unblocks the beam and redraws it to match.</summary>
        public void SetBlocked(bool blocked)
        {
            if (IsBlocked == blocked)
            {
                return;
            }

            IsBlocked = blocked;
            DrawBeam();
        }

        /// <summary>Shows a crate burning in the beam.</summary>
        public void Burn()
        {
            _flashTicksLeft = _flashTicks;
            _burnFlash.intensity = _flashIntensity;
            Burned?.Invoke();
        }

        /// <summary>Unblocks the beam and clears the flash for the next attempt.</summary>
        public void ResetState()
        {
            IsBlocked = false;
            _flashTicksLeft = 0;
            _burnFlash.intensity = 0f;
            DrawBeam();
        }

        private void FixedUpdate()
        {
            if (!_clock.IsRunning || _flashTicksLeft <= 0)
            {
                return;
            }

            _flashTicksLeft--;
            _burnFlash.intensity = _flashIntensity * _flashTicksLeft / _flashTicks;
        }

        private void DrawBeam()
        {
            _beam.SetPosition(0, _emitter.position);
            _beam.SetPosition(1, IsBlocked ? _blockedEnd.position : _farEnd.position);
        }
    }
}
