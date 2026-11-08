// Interactor: shared by the player and clones. When its owner interacts, it finds the nearest
// IInteractable in range and uses it. Using the same component for both keeps replays identical.
// The player's InteractPrompt asks it what is in reach each frame to show the interact key.
using System.Collections.Generic;
using GameJam.Core;
using GameJam.Interactables;
using UnityEngine;

namespace GameJam.Actors
{
    /// <summary>Finds and uses the closest interactable within reach of this actor.</summary>
    public class Interactor : MonoBehaviour
    {
        [Header("Tuning")]
        [Tooltip("How far from the actor's position an interactable can be used, in units.")]
        [SerializeField] private float _reach = 0.8f;

        [Tooltip("Layers that interactables live on.")]
        [SerializeField] private LayerMask _interactableLayers = ~0;

        private const int MaxCachedColliders = 64;

        // Reused every call so interacting never allocates.
        private readonly Collider2D[] _hits = new Collider2D[16];
        private readonly Dictionary<Collider2D, IInteractable> _interactables = new Dictionary<Collider2D, IInteractable>();
        private ContactFilter2D _filter;

        private void Awake()
        {
            _filter = new ContactFilter2D();
            _filter.SetLayerMask(_interactableLayers);
            _filter.useTriggers = true;
        }

        /// <summary>
        /// Uses the nearest interactable in reach of <paramref name="origin"/>. Callers pass this tick's
        /// position (live body or recorded frame) so the player and its clone reach exactly the same thing.
        /// Returns true if one was found.
        /// </summary>
        public bool TryInteract(IActor actor, Vector2 origin)
        {
            IInteractable closest = FindNearest(origin, out _);
            closest?.Interact(actor);
            return closest != null;
        }

        /// <summary>
        /// The interactable an interact at <paramref name="origin"/> would use (the closest in reach),
        /// or null. <paramref name="where"/> is its collider's transform, for placing the interact
        /// prompt. Shared by TryInteract and the prompt so they always agree.
        /// </summary>
        public IInteractable FindNearest(Vector2 origin, out Transform where)
        {
            int count = Physics2D.OverlapCircle(origin, _reach, _filter, _hits);

            // Pick the closest interactable among everything overlapping the reach circle.
            IInteractable closest = null;
            where = null;
            float closestDistance = float.MaxValue;
            for (int i = 0; i < count; i++)
            {
                IInteractable candidate = Lookup(_hits[i]);
                if (candidate == null)
                {
                    continue;
                }

                float distance = ((Vector2)_hits[i].transform.position - origin).sqrMagnitude;
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    closest = candidate;
                    where = _hits[i].transform;
                }
            }

            return closest;
        }

        /// <summary>
        /// The interactable a collider belongs to (or null), remembered per collider: the interact
        /// prompt asks every frame, so the component search runs only the first time.
        /// </summary>
        private IInteractable Lookup(Collider2D hit)
        {
            if (!_interactables.TryGetValue(hit, out IInteractable interactable))
            {
                // Clones are destroyed and respawned every attempt, so old entries pile up; start over now and then.
                if (_interactables.Count >= MaxCachedColliders)
                {
                    _interactables.Clear();
                }

                interactable = hit.GetComponentInParent<IInteractable>();
                _interactables[hit] = interactable;
            }

            return interactable;
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, _reach);
        }
    }
}
