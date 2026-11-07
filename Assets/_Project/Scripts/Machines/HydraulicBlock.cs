// HydraulicBlock: the block beside Chamber 3's laser wall. While a living actor stands in its push
// spot (an ActorZone behind it) the block slides into the beam and blocks it; when nobody holds it,
// it springs back out. It moves on ticks rather than physics pushing, so a clone holding it replays
// exactly.
using GameJam.Actors;
using GameJam.Core;
using UnityEngine;

namespace GameJam.Machines
{
    /// <summary>A sprung block that blocks the laser wall only while someone holds it in place.</summary>
    public class HydraulicBlock : MonoBehaviour, IResettable
    {
        [Header("References")]
        [SerializeField] private TickClock _clock;
        [Tooltip("Where an actor must stand to push and hold the block.")]
        [SerializeField] private ActorZone _pushSpot;
        [Tooltip("The moving block art.")]
        [SerializeField] private Transform _block;
        [SerializeField] private Transform _retractedPoint;
        [SerializeField] private Transform _extendedPoint;
        [SerializeField] private LaserWall _laserWall;

        [Header("Tuning")]
        [Tooltip("Ticks to push the block fully into the beam.")]
        [Min(1)]
        [SerializeField] private int _extendTicks = 25;
        [Tooltip("Ticks for the block to spring back out when let go.")]
        [Min(1)]
        [SerializeField] private int _retractTicks = 12;

        // 0 = fully out, 1 = fully in the beam.
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

        /// <summary>Springs the block back out for the next attempt.</summary>
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

            // 1. Push in while held, spring back while not.
            float step = _pushSpot.HasLivingActor ? 1f / _extendTicks : -1f / _retractTicks;
            _extension = Mathf.Clamp01(_extension + step);
            _block.position = Vector3.Lerp(_retractedPoint.position, _extendedPoint.position, _extension);

            // 2. Only a fully pushed-in block stops the beam.
            _laserWall.SetBlocked(_extension >= 1f);
        }
    }
}
