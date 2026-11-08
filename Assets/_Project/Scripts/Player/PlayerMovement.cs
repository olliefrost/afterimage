// PlayerMovement: moves the live player around the room on each fixed tick from PlayerInputReader.
// Its resulting position is what AfterimageRecorder samples for clones to replay. Moving platforms
// (Chamber 3's hover stairs) add their own movement through AddCarry before it runs each tick.
// On a ladder (LadderClimber) the keys climb instead of walk, and mid-jump (ActorJump) the keys are
// ignored while the player flies on in the jump's direction.
using GameJam.Actors;
using GameJam.Core;
using UnityEngine;

namespace GameJam.Player
{
    /// <summary>Top-down eight-way movement on a dynamic Rigidbody2D, so walls block the player.</summary>
    [RequireComponent(typeof(Rigidbody2D))]
    public class PlayerMovement : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private PlayerInputReader _input;
        [SerializeField] private TickClock _clock;

        [Header("Tuning")]
        [Tooltip("Walking speed in units per second.")]
        [SerializeField] private float _moveSpeed = 3.5f;
        [Tooltip("Scales vertical movement for the 2:1 isometric floor. 0.5 makes W+D follow the floor's grid lines.")]
        [SerializeField] private float _isoVerticalScale = 0.5f;
        [Tooltip("Speed through the air while jumping, in units per second. With the jump's length this sets how far a jump goes.")]
        [SerializeField] private float _jumpSpeed = 3f;

        private Rigidbody2D _body;
        private Vector2 _carry;

        // Optional: only on characters that can climb and jump (both are on the Player prefab).
        private LadderClimber _climber;
        private ActorJump _jump;

        /// <summary>Last non-zero movement direction, for sprites and recording.</summary>
        public Vector2 Facing { get; private set; } = Vector2.down;

        /// <summary>
        /// Moves the player along with a platform this tick, on top of their own walking. Platforms
        /// call it from FixedUpdate before this script runs.
        /// </summary>
        public void AddCarry(Vector2 delta)
        {
            _carry += delta;
        }

        private void Awake()
        {
            _body = GetComponent<Rigidbody2D>();
            _climber = GetComponent<LadderClimber>();
            _jump = GetComponent<ActorJump>();
        }

        private void FixedUpdate()
        {
            if (!_clock.IsRunning)
            {
                _carry = Vector2.zero;
                return;
            }

            Vector2 velocity = MoveVelocity();
            _body.MovePosition(_body.position + velocity * Time.fixedDeltaTime + _carry);
            _carry = Vector2.zero;
        }

        /// <summary>This tick's world velocity: flying on through a jump, climbing a ladder, or walking.</summary>
        private Vector2 MoveVelocity()
        {
            // 1. In the air the keys do nothing: the jump carries on the way it started.
            if (_jump != null && _jump.IsAirborne)
            {
                return ToFloor(_jump.Direction) * _jumpSpeed;
            }

            Vector2 direction = _input.MoveInput;
            if (direction.sqrMagnitude > 0.0001f)
            {
                Facing = direction.normalized;
            }

            // 2. On a ladder the keys move up or down it.
            if (_climber != null && _climber.IsClimbing)
            {
                return _climber.ClimbVelocity(direction);
            }

            // 3. Walking.
            return ToFloor(direction) * _moveSpeed;
        }

        /// <summary>Screen-relative input squashed vertically, so movement matches the isometric floor.</summary>
        private Vector2 ToFloor(Vector2 direction)
        {
            return new Vector2(direction.x, direction.y * _isoVerticalScale);
        }
    }
}
