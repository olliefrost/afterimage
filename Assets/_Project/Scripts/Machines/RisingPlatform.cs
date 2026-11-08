// RisingPlatform: a platform that rises out of a chasm to bridge it. Chamber 4 has two kinds: the dip
// platform on a scissor-lift (HoldPlate.OnActivated calls Raise, OnReleased calls Lower) and the
// timed platforms PlatformSequence raises one after another (they drop again after _autoLowerTicks).
// While it is fully up its JumpGap object is off, so people can walk onto it, and FallZone treats it
// as solid ground. When it drops, everyone standing on it falls and dies (anyone mid-jump over it is
// spared), then the gap comes back. Gameplay only; PlatformLiftVisuals draws the motion from Progress.
using GameJam.Actors;
using GameJam.Core;
using GameJam.Hazards;
using UnityEngine;

namespace GameJam.Machines
{
    /// <summary>A chasm platform that is walkable only while raised.</summary>
    public class RisingPlatform : MonoBehaviour, IResettable, ISignalSource
    {
        [Header("References")]
        [SerializeField] private TickClock _clock;
        [Tooltip("The platform's top surface; anyone inside stands on the platform.")]
        [SerializeField] private ActorZone _surface;
        [Tooltip("The chasm edge blocker (a JumpGap object) that is off while the platform is up.")]
        [SerializeField] private GameObject _gap;

        [Header("Tuning")]
        [Tooltip("Ticks the platform takes to rise or drop (50 per second). It is walkable only once fully up.")]
        [Min(1)]
        [SerializeField] private int _moveTicks = 25;
        [Tooltip("Ticks it stays up before dropping by itself. 0 = stays up until Lower() is called.")]
        [Min(0)]
        [SerializeField] private int _autoLowerTicks = 0;

        private Collider2D _surfaceArea;
        private bool _wantUp;
        private int _ticksUp;

        /// <summary>Raised when the platform starts to rise.</summary>
        public event System.Action Rising;

        /// <summary>Raised when the platform starts to drop.</summary>
        public event System.Action Lowering;

        /// <summary>0 = down in the chasm, 1 = fully up. Moves one step per tick.</summary>
        public float Progress { get; private set; }

        /// <summary>True only while fully up: the only time it is solid ground.</summary>
        public bool IsRaised => Progress >= 1f;

        /// <summary>True while the platform is about to drop (last quarter of its timer), for a warning flicker.</summary>
        public bool IsAboutToDrop => _wantUp && _autoLowerTicks > 0 && _ticksUp >= _autoLowerTicks * 3 / 4;

        /// <inheritdoc />
        public bool IsOn => IsRaised;

        /// <summary>True if <paramref name="actor"/> is standing on this platform while it is up.</summary>
        public bool Supports(IActor actor)
        {
            return IsRaised && _surface.Contains(actor);
        }

        /// <summary>True if <paramref name="point"/> lies on the platform's footprint, raised or not (for drawing heights).</summary>
        public bool Covers(Vector2 point)
        {
            return _surfaceArea.OverlapPoint(point);
        }

        private void Awake()
        {
            _surfaceArea = _surface.GetComponent<Collider2D>();
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

        /// <summary>Starts the platform rising (and its drop timer, if it has one).</summary>
        public void Raise()
        {
            // Only the first call starts a rise; a repeat call restarts the drop timer but is not a new rise.
            bool startsRising = !_wantUp;
            _wantUp = true;
            _ticksUp = 0;
            if (startsRising)
            {
                Rising?.Invoke();
            }
        }

        /// <summary>Drops the platform: anyone on it falls, and the gap closes again.</summary>
        public void Lower()
        {
            if (!_wantUp)
            {
                return;
            }

            _wantUp = false;
            Lowering?.Invoke();

            // Whoever stands on it now has nothing under them. A jumper passing over is fine.
            if (IsRaised)
            {
                _surface.KillAll(ActorJump.IsActorAirborne, Falling.Kill);
            }

            _gap.SetActive(true);
        }

        /// <summary>Puts the platform back down in the chasm for the next attempt.</summary>
        public void ResetState()
        {
            _wantUp = false;
            _ticksUp = 0;
            Progress = 0f;
            _gap.SetActive(true);
        }

        private void FixedUpdate()
        {
            // A drop still finishes while the room is frozen after a death, so the platform is seen to
            // fall away under whoever was on it (the reset puts it back).
            bool finishingDrop = !_wantUp && Progress > 0f;
            if (!_clock.IsRunning && !finishingDrop)
            {
                return;
            }

            // 1. Move one step towards up or down.
            float step = 1f / _moveTicks;
            Progress = Mathf.Clamp01(Progress + (_wantUp ? step : -step));

            if (!_wantUp)
            {
                return;
            }

            // 2. Fully up: open the way across.
            if (IsRaised && _gap.activeSelf)
            {
                _gap.SetActive(false);
            }

            // 3. Timed platforms drop by themselves.
            _ticksUp++;
            if (_autoLowerTicks > 0 && _ticksUp >= _autoLowerTicks)
            {
                Lower();
            }
        }
    }
}
