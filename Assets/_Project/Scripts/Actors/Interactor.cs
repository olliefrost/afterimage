// Interactor: shared by the player and clones. When its owner interacts, it finds the nearest
// IInteractable in range and uses it. Using the same component for both keeps replays identical.
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

        // Reused every call so interacting never allocates.
        private readonly Collider2D[] _hits = new Collider2D[8];
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
            int count = Physics2D.OverlapCircle(origin, _reach, _filter, _hits);

            // Pick the closest interactable among everything overlapping the reach circle.
            IInteractable closest = null;
            float closestDistance = float.MaxValue;
            for (int i = 0; i < count; i++)
            {
                IInteractable candidate = _hits[i].GetComponentInParent<IInteractable>();
                if (candidate == null)
                {
                    continue;
                }

                float distance = ((Vector2)_hits[i].transform.position - origin).sqrMagnitude;
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    closest = candidate;
                }
            }

            closest?.Interact(actor);
            return closest != null;
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, _reach);
        }
    }
}
