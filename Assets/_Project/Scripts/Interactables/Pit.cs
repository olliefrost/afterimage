// Pit: a one-way drop. When the live player walks onto the hole they land at the bottom and a
// cage of edge colliders stops them climbing out; the clone key is the only way out.
// Clones need no handling: their recorded positions already include the drop.
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
            if (actor == null || actor.IsClone || _isOccupied)
            {
                return;
            }

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

        /// <summary>Opens the pit again for the next attempt.</summary>
        public void ResetState()
        {
            _isOccupied = false;
            _cage.enabled = false;
        }
    }
}
