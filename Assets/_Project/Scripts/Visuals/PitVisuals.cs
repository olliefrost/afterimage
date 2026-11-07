// PitVisuals: sits on a Pit and makes it look deep. From outside, a shadow hides the inside and
// the ground in front covers it. Anyone who enters is drawn down at the pit floor (ActorPitDepth).
// Only when the live player is down there does the ground turn see-through and the pit light up,
// so a clone in the pit stays hidden. Visual only; listens to Pit's events.
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
        [Tooltip("Dark overlay over the opening that hides the inside from outside.")]
        [SerializeField] private SpriteRenderer _shadow;
        [Tooltip("Ground slab in front of the pit that covers characters standing at the bottom.")]
        [SerializeField] private SpriteRenderer _frontGround;
        [Tooltip("Light at the bottom of the pit, only on while the player is down there.")]
        [SerializeField] private Light2D _pitLight;

        [Header("Depth")]
        [Tooltip("How far below the floor the pit bottom is drawn, in units.")]
        [SerializeField] private float _depth = 1.25f;
        [Tooltip("Sorting order for characters in the pit: below the shadow and the front ground.")]
        [SerializeField] private int _actorSortingOrder = -70;

        [Header("Reveal")]
        [Tooltip("Shadow opacity while the player is in the pit.")]
        [SerializeField, Range(0f, 1f)] private float _revealedShadowAlpha = 0.15f;
        [Tooltip("Front ground opacity while the player is in the pit.")]
        [SerializeField, Range(0f, 1f)] private float _revealedGroundAlpha = 0.35f;
        [Tooltip("Seconds the reveal fade takes.")]
        [SerializeField] private float _revealDuration = 0.3f;

        private ActorPitDepth _playerDepth;
        private float _reveal;
        private bool _isRevealed;

        private void Awake()
        {
            ApplyReveal(0f);
        }

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
                _isRevealed = true;
                _pitLight.enabled = true;
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
                Hide();
            }
        }

        /// <summary>Hides the pit again at the start of a new attempt.</summary>
        public void ResetState()
        {
            if (_playerDepth != null)
            {
                _playerDepth.ExitPit();
            }

            Hide();
        }

        /// <summary>Snaps the pit back to its dark, covered look.</summary>
        private void Hide()
        {
            _playerDepth = null;
            _isRevealed = false;
            _pitLight.enabled = false;
            ApplyReveal(0f);
        }

        private void Update()
        {
            if (!_isRevealed || _reveal >= 1f)
            {
                return;
            }

            float step = _revealDuration > 0f ? Time.deltaTime / _revealDuration : 1f;
            ApplyReveal(Mathf.Min(1f, _reveal + step));
        }

        /// <summary>Sets how see-through the pit is, from 0 (hidden) to 1 (revealed).</summary>
        private void ApplyReveal(float reveal)
        {
            _reveal = reveal;
            SetAlpha(_shadow, Mathf.Lerp(1f, _revealedShadowAlpha, reveal));
            SetAlpha(_frontGround, Mathf.Lerp(1f, _revealedGroundAlpha, reveal));
        }

        private static void SetAlpha(SpriteRenderer renderer, float alpha)
        {
            Color color = renderer.color;
            color.a = alpha;
            renderer.color = color;
        }
    }
}
