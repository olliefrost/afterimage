// PitVisuals: sits on a Pit and makes it look deep. From outside, a shadow hides the inside and
// the ground in front covers it. Anyone who enters is drawn down at the pit floor (ActorPitDepth).
// Only when the live player is down there do the shadow and ground fade (OcclusionFader) and the
// pit light up, so a clone in the pit stays hidden. Visual only; listens to Pit's events.
using GameJam.Core;
using GameJam.Interactables;
using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace GameJam.Visuals
{
    /// <summary>Hides the inside of a pit and reveals it while the player is in it.</summary>
    public class PitVisuals : MonoBehaviour, IResettable
    {
        [Header("References")]
        [SerializeField] private Pit _pit;
        [Tooltip("Fades the dark overlay that hides the inside from outside.")]
        [SerializeField] private OcclusionFader _shadowFader;
        [Tooltip("Fades the ground slab in front of the pit that covers characters at the bottom.")]
        [SerializeField] private OcclusionFader _groundFader;
        [Tooltip("Light at the bottom of the pit, only on while the player is down there.")]
        [SerializeField] private Light2D _pitLight;

        [Header("Depth")]
        [Tooltip("How far below the floor the pit bottom is drawn, in units.")]
        [SerializeField] private float _depth = 1.25f;
        [Tooltip("Sorting order for characters in the pit: below the shadow and the front ground.")]
        [SerializeField] private int _actorSortingOrder = -70;

        private ActorPitDepth _playerDepth;

        private void OnEnable()
        {
            _pit.ActorEntered += HandleActorEntered;
            _pit.ActorExited += HandleActorExited;
            LevelReset.Register(this);
        }

        private void OnDisable()
        {
            _pit.ActorEntered -= HandleActorEntered;
            _pit.ActorExited -= HandleActorExited;
            LevelReset.Unregister(this);
        }

        /// <summary>Drops the actor's art into the pit, and reveals the pit if it is the player.</summary>
        private void HandleActorEntered(IActor actor)
        {
            ActorPitDepth depth = actor.Transform.GetComponentInChildren<ActorPitDepth>();
            if (depth == null || depth.IsInPit)
            {
                return;
            }

            depth.EnterPit(_depth, _actorSortingOrder);
            if (!actor.IsClone)
            {
                _playerDepth = depth;
                SetRevealed(true, false);
            }
        }

        /// <summary>Puts the actor's art back on the floor when it leaves.</summary>
        private void HandleActorExited(IActor actor)
        {
            ActorPitDepth depth = actor.Transform.GetComponentInChildren<ActorPitDepth>();
            if (depth != null)
            {
                depth.ExitPit();
            }

            if (!actor.IsClone)
            {
                _playerDepth = null;
                SetRevealed(false, true);
            }
        }

        /// <summary>Hides the pit again at the start of a new attempt.</summary>
        public void ResetState()
        {
            if (_playerDepth != null)
            {
                _playerDepth.ExitPit();
                _playerDepth = null;
            }

            SetRevealed(false, true);
        }

        /// <summary>Shows (fades out the cover, lights the pit) or hides the inside of the pit.</summary>
        private void SetRevealed(bool revealed, bool instant)
        {
            _shadowFader.SetFaded(revealed, instant);
            _groundFader.SetFaded(revealed, instant);
            _pitLight.enabled = revealed;
        }
    }
}
