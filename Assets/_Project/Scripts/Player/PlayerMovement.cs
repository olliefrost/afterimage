// PlayerMovement: moves the live player around the room on each fixed tick from PlayerInputReader.
// Its resulting position is what AfterimageRecorder samples for clones to replay.
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
        [SerializeField] private float _moveSpeed = 4f;

        private Rigidbody2D _body;

        /// <summary>Last non-zero movement direction, for sprites and recording.</summary>
        public Vector2 Facing { get; private set; } = Vector2.down;

        private void Awake()
        {
            _body = GetComponent<Rigidbody2D>();
        }

        private void FixedUpdate()
        {
            if (!_clock.IsRunning)
            {
                return;
            }

            Vector2 direction = _input.MoveInput;
            if (direction.sqrMagnitude > 0.0001f)
            {
                Facing = direction.normalized;
            }

            _body.MovePosition(_body.position + direction * (_moveSpeed * Time.fixedDeltaTime));
        }
    }
}
