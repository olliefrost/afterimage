// ActorElevation: sits on a character's Body (player and clone) and lifts the Body's children
// (shadow, lights) to the ground height ElevationField reports, easing between heights so climbing
// steps looks smooth. A jumper never sinks mid-air (a gap below reads as height 0), so jumping up a
// row of platforms draws a clean rise. The sprite child is moved by CharacterSpriteAnimator, which adds Height itself.
// It leaves the Body alone, so the character still sorts by its feet on the floor plan and
// ActorPitDepth (which moves the Body) keeps working. Visual only.
using System.Collections.Generic;
using GameJam.Actors;
using UnityEngine;

namespace GameJam.Visuals
{
    /// <summary>Draws a character standing on raised ground without moving its collider.</summary>
    public class ActorElevation : MonoBehaviour
    {
        [Header("Tuning")]
        [Tooltip("Units per second the art rises or drops towards the ground height.")]
        [SerializeField] private float _climbSpeed = 6f;
        [Tooltip("A move longer than this in one frame is a teleport (e.g. a level reset): the height snaps instead of easing.")]
        [SerializeField] private float _teleportDistance = 0.5f;

        private readonly List<Transform> _lifted = new List<Transform>();
        private readonly List<Vector3> _restPositions = new List<Vector3>();
        private Vector2 _lastFeet;
        private ActorJump _jump;

        /// <summary>Current drawn height above the floor, in units.</summary>
        public float Height { get; private set; }

        private void Awake()
        {
            // Lift every child except the animated sprite, which applies Height on its own.
            foreach (Transform child in transform)
            {
                if (child.GetComponent<CharacterSpriteAnimator>() != null)
                {
                    continue;
                }

                _lifted.Add(child);
                _restPositions.Add(child.localPosition);
            }

            _jump = GetComponentInParent<ActorJump>();
            _lastFeet = Feet();
        }

        // Update, not LateUpdate: CharacterSpriteAnimator reads Height in its LateUpdate, so it must be current by then.
        private void Update()
        {
            // 1. Sample the ground where the collider is.
            Vector2 feet = Feet();
            bool teleported = (feet - _lastFeet).sqrMagnitude > _teleportDistance * _teleportDistance;
            _lastFeet = feet;
            float target = ElevationField.HeightAt(feet);
            if (_jump != null && _jump.IsAirborne)
            {
                target = Mathf.Max(target, Height);
            }

            if (Mathf.Approximately(Height, target))
            {
                return;
            }

            // 2. Ease towards it while walking or climbing; snap after a teleport so nobody sinks into a respawn.
            Height = teleported ? target : Mathf.MoveTowards(Height, target, _climbSpeed * Time.deltaTime);
            for (int i = 0; i < _lifted.Count; i++)
            {
                _lifted[i].localPosition = _restPositions[i] + new Vector3(0f, Height, 0f);
            }
        }

        /// <summary>The actor's root is where its collider is, so the ground is sampled there.</summary>
        private Vector2 Feet()
        {
            return transform.parent != null ? transform.parent.position : transform.position;
        }
    }
}
