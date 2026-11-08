// PlayerMovement: moves the live player around the room on each fixed tick from PlayerInputReader.
// Its resulting position is what AfterimageRecorder samples for clones to replay. Moving platforms
// (Chamber 3's hover stairs) add their own movement through AddCarry before it runs each tick.
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

        private Rigidbody2D _body;
        private Vector2 _carry;

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
        }

        private void FixedUpdate()
        {
            if (!_clock.IsRunning)
            {
                _carry = Vector2.zero;
                return;
            }

            Vector2 direction = _input.MoveInput;
            if (direction.sqrMagnitude > 0.0001f)
            {
                Facing = direction.normalized;
            }

            // Screen-relative input, squashed vertically so movement matches the isometric floor.
            Vector2 velocity = new Vector2(direction.x, direction.y * _isoVerticalScale) * _moveSpeed;
            _body.MovePosition(_body.position + velocity * Time.fixedDeltaTime + _carry);
            _carry = Vector2.zero;
        }
    }
}
