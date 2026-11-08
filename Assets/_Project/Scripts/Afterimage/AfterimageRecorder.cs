// AfterimageRecorder: sits on the player and samples it every tick of an attempt.
// GameStateController takes the finished Recording when the run ends and hands it to AfterimageManager.
// Runs after the player's own scripts so the interact and jump flags for this tick are already set.
using GameJam.Actors;
using GameJam.Core;
using GameJam.Player;
using UnityEngine;

namespace GameJam.Afterimage
{
    /// <summary>Records the player's position, facing, interact presses and jump starts once per tick.</summary>
    [DefaultExecutionOrder(50)]
    [RequireComponent(typeof(Rigidbody2D))]
    public class AfterimageRecorder : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private TickClock _clock;
        [SerializeField] private PlayerMovement _movement;
        [SerializeField] private PlayerActor _actor;

        private Rigidbody2D _body;
        private Recording _current = new Recording();

        // Optional: only players that can jump have one.
        private ActorJump _jump;

        private void Awake()
        {
            _body = GetComponent<Rigidbody2D>();
            _jump = GetComponent<ActorJump>();
        }

        private void FixedUpdate()
        {
            if (!_clock.IsRunning)
            {
                return;
            }

            bool jumped = _jump != null && _jump.StartedThisTick;
            _current.Frames.Add(new ActorFrame(_body.position, _movement.Facing, _actor.InteractedThisTick, jumped));
        }

        /// <summary>Returns the run recorded so far and starts an empty one for the next attempt.</summary>
        public Recording TakeRecording()
        {
            Recording finished = _current;
            _current = new Recording();
            return finished;
        }
    }
}
