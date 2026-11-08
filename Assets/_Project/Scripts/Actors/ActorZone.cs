// ActorZone: an area that knows which living actors (player or clones) are standing in it this tick.
// Hold plates, the hydraulic block's push spot, the hover stairs and Chamber 4's scanner, chasms,
// timed door and rising platforms read it (the last three kill whoever is inside). It queries its own
// collider every fixed step instead of relying on trigger messages, because clones are kinematic
// triggers and Unity sends no trigger messages between those and static colliders.
using System.Collections.Generic;
using GameJam.Core;
using UnityEngine;

namespace GameJam.Actors
{
    /// <summary>Lists the living actors overlapping this object's collider, refreshed once per tick.</summary>
    [DefaultExecutionOrder(-20)]
    [RequireComponent(typeof(Collider2D))]
    public class ActorZone : MonoBehaviour
    {
        private readonly List<Collider2D> _overlaps = new List<Collider2D>(16);
        private readonly List<IActor> _actors = new List<IActor>(8);
        private Collider2D _collider;
        private ContactFilter2D _filter;

        /// <summary>True if at least one living actor is inside the zone this tick.</summary>
        public bool HasLivingActor => _actors.Count > 0;

        private void Awake()
        {
            _collider = GetComponent<Collider2D>();
            _filter = ContactFilter2D.noFilter;
            _filter.useTriggers = true;
        }

        /// <summary>True if <paramref name="actor"/> is inside the zone this tick.</summary>
        public bool Contains(IActor actor)
        {
            return _actors.Contains(actor);
        }

        /// <summary>
        /// Kills every living actor in the zone this tick, for hazards that close on whoever is inside
        /// (a door shutting, a platform dropping). Skips anyone in <paramref name="spare"/>'s answer, e.g. mid-jump.
        /// </summary>
        public void KillAll(System.Predicate<IActor> spare = null)
        {
            // Kill() can trigger a level restart, so work from the end of a list nothing else changes this tick.
            for (int i = _actors.Count - 1; i >= 0; i--)
            {
                IActor actor = _actors[i];
                if (!actor.IsDead && (spare == null || !spare(actor)))
                {
                    actor.Kill();
                }
            }
        }

        private void FixedUpdate()
        {
            // Rebuild the list from scratch each tick, so actors that died or were removed never linger.
            _actors.Clear();
            int count = _collider.Overlap(_filter, _overlaps);
            for (int i = 0; i < count; i++)
            {
                IActor actor = _overlaps[i].GetComponentInParent<IActor>();
                if (actor != null && !actor.IsDead && !_actors.Contains(actor))
                {
                    _actors.Add(actor);
                }
            }
        }
    }
}
