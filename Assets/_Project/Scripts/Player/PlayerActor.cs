// PlayerActor: the live player's identity as an IActor, plus its reset, interaction and death.
// Each tick it consumes an interact press and uses the Interactor; AfterimageRecorder then reads
// InteractedThisTick so clones repeat the press on the same tick. When a hazard kills the player it
// raises Died, and GameStateController restarts the attempt without saving the run.
using System;
using GameJam.Actors;
using GameJam.Core;
using UnityEngine;

namespace GameJam.Player
{
    /// <summary>The live player: interacts on E, and returns to its spawn point on every new attempt.</summary>
    [RequireComponent(typeof(Rigidbody2D))]
    public class PlayerActor : MonoBehaviour, IActor, IResettable
    {
        [Header("References")]
        [SerializeField] private PlayerInputReader _input;
        [SerializeField] private TickClock _clock;
        [SerializeField] private Interactor _interactor;

        private Rigidbody2D _body;
        private Vector2 _spawnPosition;

        /// <inheritdoc />
        public Transform Transform => transform;

        /// <inheritdoc />
        public bool IsClone => false;

        /// <inheritdoc />
        public bool IsDead { get; private set; }

        /// <summary>True if the player pressed interact during the current tick.</summary>
        public bool InteractedThisTick { get; private set; }

        /// <summary>Raised once when the player is killed by a hazard.</summary>
        public event Action Died;

        private void Awake()
        {
            _body = GetComponent<Rigidbody2D>();
            _spawnPosition = _body.position;
        }

        private void OnEnable()
        {
            LevelReset.Register(this);
        }

        private void OnDisable()
        {
            LevelReset.Unregister(this);
        }

        private void FixedUpdate()
        {
            if (!_clock.IsRunning)
            {
                InteractedThisTick = false;
                return;
            }

            InteractedThisTick = _input.ConsumeInteract();
            if (InteractedThisTick)
            {
                _interactor.TryInteract(this, _body.position);
            }
        }

        /// <summary>Teleports the player back to where it stood when the level loaded.</summary>
        public void ResetState()
        {
            _body.position = _spawnPosition;
            _body.linearVelocity = Vector2.zero;
            transform.position = _spawnPosition;
            InteractedThisTick = false;
            IsDead = false;
            _input.ConsumeInteract();
        }

        /// <summary>Kills the player. GameStateController listens to <see cref="Died"/> and restarts the attempt.</summary>
        [ContextMenu("Kill")]
        public void Kill()
        {
            if (IsDead)
            {
                return;
            }

            IsDead = true;
            Died?.Invoke();
        }

        /// <summary>Moves the player instantly, for example when falling into a pit.</summary>
        public void Teleport(Vector2 position)
        {
            _body.position = position;
            _body.linearVelocity = Vector2.zero;
            transform.position = position;
        }
    }
}
