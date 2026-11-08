// ActorJump: the jump state shared by the player and clones (Chamber 4 onward). Begin() starts a
// fixed-length hop: while airborne the actor ignores every JumpGap (chasm edges), FallZones and
// dropping platforms leave it alone, and the art follows ArcHeight. PlayerJump starts it from the
// Jump key and PlayerMovement carries the player along Direction; a clone starts it when its
// recording says it jumped, and its replayed positions do the moving. Ticks, not seconds, so a
// clone lands on exactly the tick the original run did.
using GameJam.Core;
using GameJam.Hazards;
using UnityEngine;

namespace GameJam.Actors
{
    /// <summary>Tracks one actor's jump: airborne time, direction and the visual arc.</summary>
    [DefaultExecutionOrder(-5)]
    public class ActorJump : MonoBehaviour
    {
        [Header("Tuning")]
        [Tooltip("Ticks a jump lasts (50 per second).")]
        [Min(1)]
        [SerializeField] private int _jumpTicks = 30;
        [Tooltip("Peak height of the drawn arc, in units. Visual only.")]
        [SerializeField] private float _arcHeight = 0.6f;

        private Collider2D[] _colliders;
        private int _ticksLeft;

        /// <summary>True while the actor is in the air.</summary>
        public bool IsAirborne => _ticksLeft > 0;

        /// <summary>True on the tick a jump started (what the recorder stores).</summary>
        public bool StartedThisTick { get; private set; }

        /// <summary>Unit direction of the current jump, screen-relative like movement input.</summary>
        public Vector2 Direction { get; private set; }

        /// <summary>Ticks a jump lasts, so movement can spread the hop distance across it.</summary>
        public int JumpTicks => _jumpTicks;

        /// <summary>Current drawn height above the ground, in units (0 when not jumping).</summary>
        public float ArcHeight
        {
            get
            {
                if (!IsAirborne)
                {
                    return 0f;
                }

                float t = 1f - (float)_ticksLeft / _jumpTicks;
                return 4f * _arcHeight * t * (1f - t);
            }
        }

        /// <summary>True if <paramref name="actor"/> is mid-jump. Actors without an ActorJump never are.</summary>
        public static bool IsActorAirborne(IActor actor)
        {
            ActorJump jump = actor.Transform.GetComponent<ActorJump>();
            return jump != null && jump.IsAirborne;
        }

        private void Awake()
        {
            _colliders = GetComponentsInChildren<Collider2D>();
        }

        private void OnEnable()
        {
            JumpGap.Added += OnGapAdded;
        }

        private void OnDisable()
        {
            JumpGap.Added -= OnGapAdded;
            Land();
        }

        /// <summary>Starts a jump in <paramref name="direction"/>. Ignored while already airborne.</summary>
        public void Begin(Vector2 direction)
        {
            if (IsAirborne)
            {
                return;
            }

            _ticksLeft = _jumpTicks;
            Direction = direction.sqrMagnitude > 0f ? direction.normalized : Vector2.down;
            StartedThisTick = true;
            SetGapsIgnored(true);
        }

        /// <summary>Ends any jump at once, e.g. when the room resets mid-air.</summary>
        public void Cancel()
        {
            Land();
        }

        private void FixedUpdate()
        {
            // The start flag lasts exactly one tick: this runs before the next tick's Begin can set it.
            StartedThisTick = false;

            if (!IsAirborne)
            {
                return;
            }

            _ticksLeft--;
            if (_ticksLeft == 0)
            {
                Land();
            }
        }

        /// <summary>Back on the ground: chasm edges block again.</summary>
        private void Land()
        {
            _ticksLeft = 0;
            SetGapsIgnored(false);
        }

        /// <summary>Turns collisions between this actor and every chasm edge off (in the air) or on.</summary>
        private void SetGapsIgnored(bool ignored)
        {
            for (int i = 0; i < JumpGap.All.Count; i++)
            {
                IgnoreGap(JumpGap.All[i], ignored);
            }
        }

        /// <summary>A gap that appears mid-jump (a platform dropping) must not stop the jump either.</summary>
        private void OnGapAdded(Collider2D gap)
        {
            if (IsAirborne)
            {
                IgnoreGap(gap, true);
            }
        }

        private void IgnoreGap(Collider2D gap, bool ignored)
        {
            for (int i = 0; i < _colliders.Length; i++)
            {
                Physics2D.IgnoreCollision(_colliders[i], gap, ignored);
            }
        }
    }
}
