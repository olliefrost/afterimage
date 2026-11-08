// LaserEmitter: a wall laser that charges, fires one shot down its line and recharges, on a fixed
// tick cycle counted from the start of each attempt. A shot kills the first living actor in the beam
// (player or clone), so a clone in front shields everyone behind it. Walls stop the beam; dead
// clones and triggers do not. Placed in Tutorial2's corridors; reads the shared TickClock.
using System.Collections.Generic;
using GameJam.Core;
using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace GameJam.Hazards
{
    /// <summary>
    /// Fires on a deterministic tick schedule so clones meet the laser exactly as their original run did.
    /// Hitting someone does not change the schedule.
    /// </summary>
    [DefaultExecutionOrder(60)]
    public class LaserEmitter : MonoBehaviour, IResettable
    {
        // Hits closer than this to the muzzle are ignored, so the wall the emitter sits in never blocks it.
        private const float MinHitDistance = 0.02f;

        [Header("References")]
        [SerializeField] private TickClock _clock;
        [Tooltip("Where the beam starts. Place it just in front of the wall.")]
        [SerializeField] private Transform _muzzle;
        [Tooltip("Draws the beam for a few ticks after each shot. Uses world space with two points.")]
        [SerializeField] private LineRenderer _beam;
        [Tooltip("Optional glow that brightens while charging, so the player can read the timing.")]
        [SerializeField] private Light2D _chargeLight;

        [Header("Aim")]
        [Tooltip("World direction the beam fires in. Isometric corridors run along (1, 0.5), (-1, 0.5), (1, -0.5) or (-1, -0.5).")]
        [SerializeField] private Vector2 _direction = new Vector2(-1f, -0.5f);
        [Tooltip("Furthest the beam reaches if no wall stops it, in units.")]
        [SerializeField] private float _range = 20f;
        [Tooltip("Half-width of the lethal beam, in units. Wide enough to cover the corridor, so nobody can sidestep it.")]
        [Min(0f)]
        [SerializeField] private float _beamRadius = 0.35f;

        [Header("Timing (ticks, 50 per second)")]
        [Tooltip("Ticks from the start of the attempt before the first charge begins.")]
        [Min(0)]
        [SerializeField] private int _startDelayTicks = 0;
        [Tooltip("Ticks spent charging before each shot. The glow ramps up over this time.")]
        [Min(1)]
        [SerializeField] private int _chargeTicks = 75;
        [Tooltip("Ticks the beam stays visible after a shot. Only the first tick is lethal.")]
        [Min(0)]
        [SerializeField] private int _beamTicks = 10;
        [Tooltip("Ticks of rest after the beam before the next charge starts.")]
        [Min(0)]
        [SerializeField] private int _cooldownTicks = 15;

        [Header("Look")]
        [Tooltip("Charge light intensity at the moment of firing.")]
        [SerializeField] private float _chargeLightMaxIntensity = 2f;

        private readonly List<RaycastHit2D> _hits = new List<RaycastHit2D>(16);
        private ContactFilter2D _filter;
        private Vector2 _aim;

        private void Awake()
        {
            _aim = _direction.normalized;

            // Triggers must be included: clone bodies are triggers.
            _filter = ContactFilter2D.noFilter;
            _filter.useTriggers = true;

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

        /// <summary>Hides the beam and the charge glow for the next attempt; the schedule restarts with the clock.</summary>
        public void ResetState()
        {
            _beam.enabled = false;
            SetChargeLight(0f);
        }

        private void FixedUpdate()
        {
            if (!_clock.IsRunning)
            {
                return;
            }

            // 1. Work out where in its cycle the laser is on this tick.
            int cycleTick = _clock.CurrentTick - _startDelayTicks;
            if (cycleTick < 0)
            {
                return;
            }

            int period = _chargeTicks + _beamTicks + _cooldownTicks;
            int phase = cycleTick % period;

            // 2. Charging: brighten the glow towards the shot.
            if (phase < _chargeTicks)
            {
                _beam.enabled = false;
                SetChargeLight((phase + 1f) / _chargeTicks);
                return;
            }

            // 3. The shot itself: kill the first living actor in line and draw the beam to it.
            if (phase == _chargeTicks)
            {
                Fire();
                return;
            }

            // 4. Afterglow, then rest: the beam is harmless after its first tick.
            if (phase >= _chargeTicks + _beamTicks)
            {
                _beam.enabled = false;
                SetChargeLight(0f);
            }
        }

        /// <summary>Casts the beam, kills whoever it reaches first, and shows the beam up to that point.</summary>
        private void Fire()
        {
            Vector2 origin = _muzzle.position;

            // 1. A thin ray down the centre finds the wall that ends the beam.
            float stopDistance = _range;
            int count = Physics2D.Raycast(origin, _aim, _filter, _hits, _range);
            for (int i = 0; i < count; i++)
            {
                RaycastHit2D hit = _hits[i];
                if (IsSolidScenery(hit) && hit.distance >= MinHitDistance && hit.distance < stopDistance)
                {
                    stopDistance = hit.distance;
                }
            }

            // 2. A wide cast finds the nearest living actor before that wall. It ignores scenery, because
            //    a cast as wide as the corridor would graze the side walls.
            IActor target = null;
            count = Physics2D.CircleCast(origin, _beamRadius, _aim, _filter, _hits, stopDistance);
            for (int i = 0; i < count; i++)
            {
                RaycastHit2D hit = _hits[i];
                IActor actor = hit.collider.GetComponentInParent<IActor>();

                // Dead clones are already destroyed: the beam passes through them.
                if (actor != null && !actor.IsDead && hit.distance < stopDistance)
                {
                    stopDistance = hit.distance;
                    target = actor;
                }
            }

            // 3. Show the beam up to whatever stopped it, then apply the hit.
            _beam.SetPosition(0, origin);
            _beam.SetPosition(1, origin + _aim * stopDistance);
            _beam.enabled = true;
            SetChargeLight(1f);

            target?.Kill();
        }

        /// <summary>True for solid colliders that are not an actor or part of this emitter: walls and closed doors.</summary>
        private bool IsSolidScenery(RaycastHit2D hit)
        {
            Collider2D collider = hit.collider;
            return !collider.isTrigger
                && !collider.transform.IsChildOf(transform)
                && collider.GetComponentInParent<IActor>() == null;
        }

        private void SetChargeLight(float charge)
        {
            if (_chargeLight != null)
            {
                _chargeLight.intensity = charge * _chargeLightMaxIntensity;
            }
        }

        private void OnDrawGizmosSelected()
        {
            if (_muzzle == null)
            {
                return;
            }

            // Shows the aim line in the Scene view so the laser can be lined up with its corridor.
            Gizmos.color = Color.red;
            Vector3 start = _muzzle.position;
            Gizmos.DrawLine(start, start + (Vector3)(_direction.normalized * _range));
        }
    }
}
