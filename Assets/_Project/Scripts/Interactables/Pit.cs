// Pit: a one-way drop. When the live player walks onto the hole they land at the bottom and a
// cage of edge colliders stops them climbing out; the clone key is the only way out.
// Clones need no handling: their recorded positions already include the drop.
// It also reports every actor entering and leaving, so visuals (PitVisuals) can react.
using System;
using GameJam.Core;
using UnityEngine;

namespace GameJam.Interactables
{
    /// <summary>A hole the player falls into and cannot leave until the next attempt.</summary>
    public class Pit : MonoBehaviour, IResettable
    {
        [Header("References")]
        [Tooltip("Where the player lands at the bottom of the pit.")]
        [SerializeField] private Transform _landingPoint;
        [Tooltip("Collider around the pit's inside edge, enabled once the player is down there.")]
        [SerializeField] private Collider2D _cage;

        private bool _isOccupied;

        /// <summary>Raised when the player or a clone enters the pit.</summary>
        public event Action<IActor> ActorEntered;

        /// <summary>Raised when the player or a clone leaves the pit (for example on a reset).</summary>
        public event Action<IActor> ActorExited;

        private void OnEnable()
        {
            LevelReset.Register(this);
            ResetState();
        }

        private void OnDisable()
        {
            LevelReset.Unregister(this);
        }

        /// <summary>Drops the live player into the pit when they step onto the hole.</summary>
        private void OnTriggerEnter2D(Collider2D other)
        {
            IActor actor = other.GetComponentInParent<IActor>();
            if (actor == null)
            {
                return;
            }

            if (!actor.IsClone && !_isOccupied)
            {
                // 1. Move the player straight to the bottom of the pit.
                Vector2 landing = _landingPoint.position;
                Rigidbody2D body = other.attachedRigidbody;
                if (body != null)
                {
                    body.position = landing;
                    body.linearVelocity = Vector2.zero;
                }

                actor.Transform.position = landing;

                // 2. Close the cage so the only way out is a new attempt.
                _isOccupied = true;
                _cage.enabled = true;
            }

            ActorEntered?.Invoke(actor);
        }

        /// <summary>Reports an actor leaving, which happens when it is moved back to spawn.</summary>
        private void OnTriggerExit2D(Collider2D other)
        {
            IActor actor = other.GetComponentInParent<IActor>();
            if (actor != null)
            {
                ActorExited?.Invoke(actor);
            }
        }

        /// <summary>Opens the pit again for the next attempt.</summary>
        public void ResetState()
        {
            _isOccupied = false;
            _cage.enabled = false;
        }
    }
}
