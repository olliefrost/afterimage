// InteractPrompt: a small key badge (the current interact key, E unless rebound) that floats above
// whatever the live player would use if they pressed interact now: it pops in over a usable button
// in reach and pops out when there is none, or when the button is already pressed or unpowered.
// Hidden while the clock is stopped (intro, rewind, death), while paused and while the map is held.
// Sits on the Player prefab with a small world-space canvas. Visual only: it asks the player's own
// Interactor, so it always points at what E would press.
using GameJam.Actors;
using GameJam.Core;
using GameJam.Interactables;
using GameJam.Player;
using GameJam.Visuals;
using UnityEngine;
using UnityEngine.UI;

namespace GameJam.UI
{
    /// <summary>Shows the interact key above the interactable the player can use right now.</summary>
    public class InteractPrompt : MonoBehaviour
    {
        [Header("Game references")]
        [SerializeField] private PlayerActor _player;
        [SerializeField] private Interactor _interactor;
        [SerializeField] private PlayerInputReader _input;
        [SerializeField] private TickClock _clock;
        [SerializeField] private KeyRebinder _rebinder;

        [Header("UI references")]
        [Tooltip("The world-space badge that moves to the target and pops in and out.")]
        [SerializeField] private RectTransform _badge;
        [SerializeField] private CanvasGroup _group;
        [SerializeField] private Text _keyLabel;

        [Header("Placement")]
        [Tooltip("Where the badge floats relative to the interactable, in world units.")]
        [SerializeField] private Vector2 _offset = new Vector2(0f, 0.55f);
        [Tooltip("How far the badge bobs up and down.")]
        [SerializeField] private float _bobHeight = 0.04f;
        [SerializeField] private float _bobSpeed = 4f;

        [Header("Pop")]
        [Tooltip("Seconds to pop fully in or out.")]
        [SerializeField] private float _popTime = 0.18f;

        private Transform _target;
        private float _shown;
        private Vector3 _restScale;
        private Rigidbody2D _body;

        private void Awake()
        {
            _body = _player.GetComponent<Rigidbody2D>();
            _restScale = _badge.localScale;
            Apply(0f);
        }

        private void OnEnable()
        {
            if (_rebinder != null)
            {
                _rebinder.Changed += RefreshKeyLabel;
            }

            RefreshKeyLabel();
        }

        private void Start()
        {
            // The input reader builds its key list in Awake, which may run after this OnEnable.
            RefreshKeyLabel();
        }

        private void OnDisable()
        {
            if (_rebinder != null)
            {
                _rebinder.Changed -= RefreshKeyLabel;
            }
        }

        private void LateUpdate()
        {
            // 1. Find what interact would use now, if the player can act at all.
            Transform target = null;
            if (CanPrompt())
            {
                // The rigidbody position, the same origin PlayerActor interacts from.
                IInteractable nearest = _interactor.FindNearest(_body.position, out Transform where);
                if (nearest != null && nearest.CanInteract(_player))
                {
                    target = where;
                }
            }

            // 2. Pop in towards a target (jumping straight to a new one), pop out when there is none.
            if (target != null)
            {
                _target = target;
            }

            float goal = target != null ? 1f : 0f;
            _shown = Mathf.MoveTowards(_shown, goal, Time.unscaledDeltaTime / _popTime);

            // 3. Float above the target with a gentle bob.
            if (_target != null)
            {
                float bob = Mathf.Sin(Time.unscaledTime * _bobSpeed) * _bobHeight;
                _badge.position = (Vector3)((Vector2)_target.position + _offset + new Vector2(0f, bob));
            }

            Apply(goal > 0f ? Ease.BackOut(_shown) : _shown);
        }

        /// <summary>True while the live player is in play: clock running, keys on, map closed, alive.</summary>
        private bool CanPrompt()
        {
            return _clock.IsRunning && _input.GameplayEnabled && !_input.MapHeld && !_player.IsDead;
        }

        /// <summary>Sets the badge's size and opacity for how far it has popped in (0-1, may overshoot).</summary>
        private void Apply(float shown)
        {
            _badge.localScale = _restScale * Mathf.Max(0f, shown);
            _group.alpha = Mathf.Clamp01(shown);
        }

        private void RefreshKeyLabel()
        {
            if (_input != null && _input.InteractTarget != null)
            {
                _keyLabel.text = _input.InteractTarget.KeyName.ToUpperInvariant();
            }
        }
    }
}
