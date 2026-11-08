// HydraulicBlock: the piston beside Chamber 3's laser wall. A fixed base pushes the block out along
// its channel while someone stands on the pedal (a HoldPlate wired to it); when the pedal is
// released the block springs back against the base. Once fully out it caps the laser lens and
// blocks the beam. It moves on ticks rather than physics pushing, so a clone holding the pedal
// replays exactly. PistonArm draws the arm between base and block.
using GameJam.Core;
using GameJam.Interactables;
using UnityEngine;

namespace GameJam.Machines
{
    /// <summary>A sprung piston that blocks the laser wall only while its pedal is held.</summary>
    public class HydraulicBlock : MonoBehaviour, IResettable
    {
        [Header("References")]
        [SerializeField] private TickClock _clock;
        [Tooltip("The pedal that drives the piston; it must be held for the block to stay out.")]
        [SerializeField] private HoldPlate _pedal;
        [Tooltip("The moving block art.")]
        [SerializeField] private Transform _block;
        [Tooltip("Where the block rests against the base.")]
        [SerializeField] private Transform _retractedPoint;
        [Tooltip("Where the block caps the laser lens.")]
        [SerializeField] private Transform _extendedPoint;
        [SerializeField] private LaserWall _laserWall;

        [Header("Tuning")]
        [Tooltip("Ticks to push the block fully out onto the lens.")]
        [Min(1)]
        [SerializeField] private int _extendTicks = 25;
        [Tooltip("Ticks for the block to spring back to the base when the pedal is released.")]
        [Min(1)]
        [SerializeField] private int _retractTicks = 12;

        // 0 = against the base, 1 = fully out on the lens.
        private float _extension;

        private void OnEnable()
        {
            LevelReset.Register(this);
            ResetState();
        }

        private void OnDisable()
        {
            LevelReset.Unregister(this);
        }

        /// <summary>Springs the block back to the base for the next attempt.</summary>
        public void ResetState()
        {
            _extension = 0f;
            _block.position = _retractedPoint.position;
        }

        private void FixedUpdate()
        {
            if (!_clock.IsRunning)
            {
                return;
            }

            // 1. Push out while the pedal is held, spring back while it is not.
            float step = _pedal.IsActive ? 1f / _extendTicks : -1f / _retractTicks;
            _extension = Mathf.Clamp01(_extension + step);
            _block.position = Vector3.Lerp(_retractedPoint.position, _extendedPoint.position, _extension);

            // 2. Only a fully extended block caps the lens.
            _laserWall.SetBlocked(_extension >= 1f);
        }
    }
}
