// AfterimageReplayer: drives one clone. Each tick it moves to the recorded position and repeats
// any recorded interact through its own Interactor. Once the run ends it stands still.
// Spawned and set up by AfterimageManager at the start of each attempt.
using GameJam.Actors;
using GameJam.Core;
using UnityEngine;

namespace GameJam.Afterimage
{
    /// <summary>A clone that replays a <see cref="Recording"/> tick by tick.</summary>
    [DefaultExecutionOrder(50)]
    [RequireComponent(typeof(Rigidbody2D))]
    public class AfterimageReplayer : MonoBehaviour, IActor
    {
        [Header("References")]
        [SerializeField] private Interactor _interactor;

        private Rigidbody2D _body;
        private TickClock _clock;
        private Recording _recording;

        /// <inheritdoc />
        public Transform Transform => transform;

        /// <inheritdoc />
        public bool IsClone => true;

        /// <summary>Facing on the current tick, for sprites.</summary>
        public Vector2 Facing { get; private set; } = Vector2.down;

        private void Awake()
        {
            _body = GetComponent<Rigidbody2D>();
        }

        /// <summary>Gives the clone its run and clock and places it at the run's first position.</summary>
        public void Init(Recording recording, TickClock clock)
        {
            _recording = recording;
            _clock = clock;

            if (_recording.Frames.Count > 0)
            {
                Vector2 start = _recording.Frames[0].Position;
                _body.position = start;
                transform.position = start;
            }
        }

        private void FixedUpdate()
        {
            if (_recording == null || !_clock.IsRunning)
            {
                return;
            }

            // After the last recorded tick the clone simply stays where it ended.
            int tick = _clock.CurrentTick;
            if (tick >= _recording.Frames.Count)
            {
                return;
            }

            ActorFrame frame = _recording.Frames[tick];
            _body.MovePosition(frame.Position);
            Facing = frame.Facing;

            if (frame.Interacted)
            {
                _interactor.TryInteract(this, frame.Position);
            }
        }
    }
}
