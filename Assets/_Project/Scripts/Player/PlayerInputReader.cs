// PlayerInputReader: the only script that reads raw input (Input System package).
// It turns keys into plain data and events: PlayerMovement reads MoveInput, PlayerActor consumes
// interact presses, GameStateController listens for clone (R) and retry (T).
using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace GameJam.Player
{
    /// <summary>
    /// Reads keyboard input and exposes it as data. Bindings can be changed in the inspector;
    /// any action left without bindings gets the default keys (WASD/arrows, E, R, T).
    /// </summary>
    public class PlayerInputReader : MonoBehaviour
    {
        [Header("Actions (empty bindings fall back to the defaults)")]
        [SerializeField] private InputAction _move = new InputAction("Move", InputActionType.Value, expectedControlType: "Vector2");
        [SerializeField] private InputAction _interact = new InputAction("Interact", InputActionType.Button);
        [SerializeField] private InputAction _clone = new InputAction("Clone", InputActionType.Button);
        [SerializeField] private InputAction _retry = new InputAction("Retry", InputActionType.Button);

        // Set in Update, cleared when consumed in FixedUpdate, so a press between two physics steps is never lost.
        private bool _interactQueued;

        /// <summary>Current movement direction, each axis in -1..1.</summary>
        public Vector2 MoveInput { get; private set; }

        /// <summary>Raised when the clone key is pressed: end the run, keep it as a clone, restart.</summary>
        public event Action CloneRequested;

        /// <summary>Raised when the retry key is pressed: restart without keeping the run.</summary>
        public event Action RetryRequested;

        private void Awake()
        {
            AddDefaultBindings();
        }

        private void OnEnable()
        {
            _move.Enable();
            _interact.Enable();
            _clone.Enable();
            _retry.Enable();
        }

        private void OnDisable()
        {
            _move.Disable();
            _interact.Disable();
            _clone.Disable();
            _retry.Disable();
        }

        private void Update()
        {
            MoveInput = Vector2.ClampMagnitude(_move.ReadValue<Vector2>(), 1f);

            if (_interact.WasPressedThisFrame())
            {
                _interactQueued = true;
            }

            if (_clone.WasPressedThisFrame())
            {
                CloneRequested?.Invoke();
            }

            if (_retry.WasPressedThisFrame())
            {
                RetryRequested?.Invoke();
            }
        }

        /// <summary>Returns true once per interact press, then clears it. Call from FixedUpdate.</summary>
        public bool ConsumeInteract()
        {
            bool pressed = _interactQueued;
            _interactQueued = false;
            return pressed;
        }

        /// <summary>Gives every action without bindings its default keys, so the component works as soon as it is added.</summary>
        private void AddDefaultBindings()
        {
            if (_move.bindings.Count == 0)
            {
                _move.AddCompositeBinding("2DVector")
                    .With("Up", "<Keyboard>/w")
                    .With("Down", "<Keyboard>/s")
                    .With("Left", "<Keyboard>/a")
                    .With("Right", "<Keyboard>/d");
                _move.AddCompositeBinding("2DVector")
                    .With("Up", "<Keyboard>/upArrow")
                    .With("Down", "<Keyboard>/downArrow")
                    .With("Left", "<Keyboard>/leftArrow")
                    .With("Right", "<Keyboard>/rightArrow");
            }

            if (_interact.bindings.Count == 0)
            {
                _interact.AddBinding("<Keyboard>/e");
            }

            if (_clone.bindings.Count == 0)
            {
                _clone.AddBinding("<Keyboard>/r");
            }

            if (_retry.bindings.Count == 0)
            {
                _retry.AddBinding("<Keyboard>/t");
            }
        }
    }
}
