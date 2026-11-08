// PlayerJump: starts the live player's jump when the jump key is pressed (Chamber 4 onward). It
// jumps the way the keys point, or the way the player last faced when standing still. No jumping
// from a ladder or mid-air. ActorJump keeps the jump going and the recorder stores the start tick,
// so clones jump on the same tick; PlayerMovement does the flying.
using GameJam.Actors;
using GameJam.Core;
using UnityEngine;

namespace GameJam.Player
{
    /// <summary>Turns jump presses into jumps for the live player.</summary>
    [DefaultExecutionOrder(-4)]
    [RequireComponent(typeof(ActorJump))]
    public class PlayerJump : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private PlayerInputReader _input;
        [SerializeField] private TickClock _clock;
        [SerializeField] private PlayerMovement _movement;
        [Tooltip("Optional: no jumping while climbing.")]
        [SerializeField] private LadderClimber _climber;

        private ActorJump _jump;

        /// <summary>Raised on the tick the player jumps (the jump hint hides on the first one).</summary>
        public event System.Action Jumped;

        private void Awake()
        {
            _jump = GetComponent<ActorJump>();
        }

        private void FixedUpdate()
        {
            // Always consume, so a press made while the jump isn't allowed doesn't fire later.
            bool pressed = _input.ConsumeJump();
            if (!pressed || !_clock.IsRunning || _jump.IsAirborne || (_climber != null && _climber.IsClimbing))
            {
                return;
            }

            Vector2 keys = _input.MoveInput;
            _jump.Begin(keys.sqrMagnitude > 0.0001f ? keys : _movement.Facing);
            Jumped?.Invoke();
        }
    }
}
