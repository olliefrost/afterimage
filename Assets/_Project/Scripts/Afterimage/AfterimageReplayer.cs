// AfterimageReplayer: drives one clone. Each tick it moves to the recorded position and repeats
// any recorded interact through its own Interactor. Once the run ends it stands still; if it is
// killed it stops replaying for the rest of the attempt.
// Spawned and set up by AfterimageManager at the start of each attempt; CloneTint reads its colour.
using System;
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

        /// <summary>Which clone slot this clone replays (0 = clone 1).</summary>
        public int SlotIndex { get; private set; }

        /// <summary>The slot's colour, for the clone's tint and light.</summary>
        public Color SlotColour { get; private set; } = Color.white;

        /// <summary>True once the clone has died this attempt.</summary>
        public bool IsDead { get; private set; }

        /// <summary>The run this clone replays (set by Init), so the rewind can play it backwards.</summary>
        public Recording Run => _recording;

        /// <summary>Raised once when the clone dies.</summary>
        public event Action<AfterimageReplayer> Died;

        private void Awake()
        {
            _body = GetComponent<Rigidbody2D>();
        }

        /// <summary>Gives the clone its run, clock and slot, and places it at the run's first position.</summary>
        public void Init(Recording recording, TickClock clock, int slotIndex, Color slotColour)
        {
            _recording = recording;
            _clock = clock;
            SlotIndex = slotIndex;
            SlotColour = slotColour;

            if (_recording.Frames.Count > 0)
            {
                Vector2 start = _recording.Frames[0].Position;
                _body.position = start;
                transform.position = start;
            }
        }

        /// <summary>Kills the clone: it stops replaying where it stands until the next attempt. Hazards call this.</summary>
        [ContextMenu("Kill")]
        public void Kill()
        {
            if (IsDead)
            {
                return;
            }

            IsDead = true;
            Died?.Invoke(this);
        }

        private void FixedUpdate()
        {
            if (_recording == null || IsDead || !_clock.IsRunning)
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
