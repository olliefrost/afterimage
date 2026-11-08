// PlayerInputReader: the only script that reads gameplay input (Input System package).
// It turns keys into plain data and events: PlayerMovement reads MoveInput, PlayerActor consumes
// interact presses, GameStateController listens for clone (R), retry (T), mark run (F) and clone
// select (1-5), PauseMenu listens for pause (Esc), and ChamberMap reads MapHeld (hold Tab). While the map is held
// the player stands still. IntroSequence turns on the skip action (any key or click) while the intro plays.
// PlayerJump consumes jump presses (Space), which exist only in chambers that tick _jumpEnabled
// (Chamber 4 onward); elsewhere the key does nothing and has no settings row.
// While the game is paused, GameStateController switches
// gameplay keys off here. It also lists the keys the player may rebind (RebindTargets) and applies
// any rebinds saved earlier; KeyRebinder changes them from the pause menu.
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace GameJam.Player
{
    /// <summary>
    /// Reads keyboard input and exposes it as data. Bindings can be changed in the inspector;
    /// any action left without bindings gets the default keys (WASD/arrows, E, R, T, F, 1-5, Tab, Esc).
    /// Saved rebinds from the settings menu are applied on top of those defaults. Runs early so the
    /// keys and rebind targets exist before the HUD reads them in its OnEnable.
    /// </summary>
    [DefaultExecutionOrder(-90)]
    public class PlayerInputReader : MonoBehaviour
    {
        [Header("Chamber abilities")]
        [Tooltip("Tick in Chamber 4 and every chamber after it: turns on the jump key and its settings row.")]
        [SerializeField] private bool _jumpEnabled = false;

        [Header("Actions (empty bindings fall back to the defaults)")]
        [SerializeField] private InputAction _move = new InputAction("Move", InputActionType.Value, expectedControlType: "Vector2");
        [SerializeField] private InputAction _interact = new InputAction("Interact", InputActionType.Button);
        [SerializeField] private InputAction _clone = new InputAction("Clone", InputActionType.Button);
        [SerializeField] private InputAction _retry = new InputAction("Retry", InputActionType.Button);
        [Tooltip("Marks the run so a death saves it as a clone. Defaults to F.")]
        [SerializeField] private InputAction _markRun = new InputAction("Mark Run", InputActionType.Button);
        [Tooltip("Jumps in the movement direction. Defaults to Space. Only active when Jump Enabled is ticked.")]
        [SerializeField] private InputAction _jump = new InputAction("Jump", InputActionType.Button);
        [Tooltip("Hold to show the chamber map. The player stands still while it is held.")]
        [SerializeField] private InputAction _map = new InputAction("Map", InputActionType.Button);
        [Tooltip("One action per clone slot; element 0 selects clone 1. Defaults to the number keys 1-5.")]
        [SerializeField] private InputAction[] _selectClone =
        {
            new InputAction("Select Clone 1", InputActionType.Button),
            new InputAction("Select Clone 2", InputActionType.Button),
            new InputAction("Select Clone 3", InputActionType.Button),
            new InputAction("Select Clone 4", InputActionType.Button),
            new InputAction("Select Clone 5", InputActionType.Button)
        };
        [Tooltip("Opens and closes the pause menu. Stays active while paused and cannot be rebound in game.")]
        [SerializeField] private InputAction _pause = new InputAction("Pause", InputActionType.Button);
        [Tooltip("Skips a cutscene such as the intro. Off except while one plays; not rebindable.")]
        [SerializeField] private InputAction _skip = new InputAction("Skip", InputActionType.Button);

        private readonly List<RebindTarget> _rebindTargets = new List<RebindTarget>();
        private readonly List<RebindTarget> _selectTargets = new List<RebindTarget>();

        // Set in Update, cleared when consumed in FixedUpdate, so a press between two physics steps is never lost.
        private bool _interactQueued;
        private bool _jumpQueued;
        private bool _gameplayEnabled = true;

        /// <summary>Current movement direction, each axis in -1..1.</summary>
        public Vector2 MoveInput { get; private set; }

        /// <summary>True while the map key is held down. Always false while paused.</summary>
        public bool MapHeld { get; private set; }

        /// <summary>Every key the player may rebind, in the order the settings menu lists them.</summary>
        public IReadOnlyList<RebindTarget> RebindTargets => _rebindTargets;

        /// <summary>The save clone key (R by default), for the HUD's key letter.</summary>
        public RebindTarget CloneTarget { get; private set; }

        /// <summary>The retry key (T by default), for the HUD's key letter.</summary>
        public RebindTarget RetryTarget { get; private set; }

        /// <summary>The mark run key (F by default), for the HUD's key letter.</summary>
        public RebindTarget MarkRunTarget { get; private set; }

        /// <summary>The jump key (Space by default), for the jump hint. Null in chambers without jumping.</summary>
        public RebindTarget JumpTarget { get; private set; }

        /// <summary>True in chambers where the player can jump.</summary>
        public bool JumpEnabled => _jumpEnabled;

        /// <summary>Raised when the clone key is pressed: end the run, keep it as a clone, restart.</summary>
        public event Action CloneRequested;

        /// <summary>Raised when the retry key is pressed: restart without keeping the run.</summary>
        public event Action RetryRequested;

        /// <summary>Raised when the mark key is pressed: toggle whether dying saves this run as a clone.</summary>
        public event Action MarkRunRequested;

        /// <summary>Raised with the slot index (0 = clone 1) when a clone select key is pressed.</summary>
        public event Action<int> SelectCloneRequested;

        /// <summary>Raised when the pause key is pressed, whether or not the game is paused.</summary>
        public event Action PauseRequested;

        /// <summary>Raised when any key or mouse button is pressed while skipping is on (see <see cref="SetSkipEnabled"/>).</summary>
        public event Action SkipRequested;

        private void Awake()
        {
            AddDefaultBindings();
            BuildRebindTargets();
        }

        private void OnEnable()
        {
            _pause.Enable();
            SetGameplayActionsEnabled(_gameplayEnabled);
        }

        private void OnDisable()
        {
            _pause.Disable();
            _skip.Disable();
            SetGameplayActionsEnabled(false);
        }

        private void Update()
        {
            // 1. While the map is open the player looks, not moves: no walking and no interacting.
            MapHeld = _map.IsPressed();
            MoveInput = MapHeld ? Vector2.zero : Vector2.ClampMagnitude(_move.ReadValue<Vector2>(), 1f);

            if (_interact.WasPressedThisFrame() && !MapHeld)
            {
                _interactQueued = true;
            }

            if (_jumpEnabled && _jump.WasPressedThisFrame() && !MapHeld)
            {
                _jumpQueued = true;
            }

            if (_clone.WasPressedThisFrame())
            {
                CloneRequested?.Invoke();
            }

            if (_retry.WasPressedThisFrame())
            {
                RetryRequested?.Invoke();
            }

            if (_markRun.WasPressedThisFrame())
            {
                MarkRunRequested?.Invoke();
            }

            for (int i = 0; i < _selectClone.Length; i++)
            {
                if (_selectClone[i].WasPressedThisFrame())
                {
                    SelectCloneRequested?.Invoke(i);
                }
            }

            // Pause before skip: an Esc that skips the intro must not also pause the game it just started.
            if (_pause.WasPressedThisFrame())
            {
                PauseRequested?.Invoke();
            }

            if (_skip.WasPressedThisFrame())
            {
                SkipRequested?.Invoke();
            }
        }

        /// <summary>Returns true once per interact press, then clears it. Call from FixedUpdate.</summary>
        public bool ConsumeInteract()
        {
            bool pressed = _interactQueued;
            _interactQueued = false;
            return pressed;
        }

        /// <summary>Returns true once per jump press, then clears it. Call from FixedUpdate.</summary>
        public bool ConsumeJump()
        {
            bool pressed = _jumpQueued;
            _jumpQueued = false;
            return pressed;
        }

        /// <summary>
        /// Switches every gameplay key on or off; the pause key always stays on. Used while paused,
        /// so keys pressed in the menu do nothing, and so the keys can be rebound (an action must be
        /// off while it is rebound).
        /// </summary>
        public void SetGameplayEnabled(bool enabled)
        {
            _gameplayEnabled = enabled;
            _interactQueued = false;
            _jumpQueued = false;
            MoveInput = Vector2.zero;
            MapHeld = false;
            if (isActiveAndEnabled)
            {
                SetGameplayActionsEnabled(enabled);
            }
        }

        /// <summary>Turns the skip action (any key or left click) on while a cutscene plays, and off after.</summary>
        public void SetSkipEnabled(bool enabled)
        {
            SetActionEnabled(_skip, enabled);
        }

        /// <summary>The key that picks a clone slot (0 = clone 1), for the HUD's key badges.</summary>
        public RebindTarget GetSelectTarget(int slot)
        {
            return _selectTargets[slot];
        }

        private void SetGameplayActionsEnabled(bool enabled)
        {
            SetActionEnabled(_move, enabled);
            SetActionEnabled(_interact, enabled);
            SetActionEnabled(_clone, enabled);
            SetActionEnabled(_retry, enabled);
            SetActionEnabled(_markRun, enabled);
            SetActionEnabled(_jump, enabled && _jumpEnabled);
            SetActionEnabled(_map, enabled);
            for (int i = 0; i < _selectClone.Length; i++)
            {
                SetActionEnabled(_selectClone[i], enabled);
            }
        }

        private static void SetActionEnabled(InputAction action, bool enabled)
        {
            if (enabled)
            {
                action.Enable();
            }
            else
            {
                action.Disable();
            }
        }

        /// <summary>
        /// Lists the rebindable keys and applies any saved rebinds. Move offers only the parts of its
        /// first composite (WASD); the arrow keys stay as a fixed second binding.
        /// </summary>
        private void BuildRebindTargets()
        {
            // 1. Movement directions, then the single-key actions, in menu order.
            _rebindTargets.Add(new RebindTarget("MOVE UP", _move, FindCompositePart(_move, "Up")));
            _rebindTargets.Add(new RebindTarget("MOVE DOWN", _move, FindCompositePart(_move, "Down")));
            _rebindTargets.Add(new RebindTarget("MOVE LEFT", _move, FindCompositePart(_move, "Left")));
            _rebindTargets.Add(new RebindTarget("MOVE RIGHT", _move, FindCompositePart(_move, "Right")));
            _rebindTargets.Add(new RebindTarget("INTERACT", _interact, 0));
            CloneTarget = new RebindTarget("SAVE CLONE", _clone, 0);
            _rebindTargets.Add(CloneTarget);
            RetryTarget = new RebindTarget("RETRY", _retry, 0);
            _rebindTargets.Add(RetryTarget);
            MarkRunTarget = new RebindTarget("MARK RUN", _markRun, 0);
            _rebindTargets.Add(MarkRunTarget);
            if (_jumpEnabled)
            {
                JumpTarget = new RebindTarget("JUMP", _jump, 0);
                _rebindTargets.Add(JumpTarget);
            }

            _rebindTargets.Add(new RebindTarget("MAP (HOLD)", _map, 0));
            for (int i = 0; i < _selectClone.Length; i++)
            {
                RebindTarget select = new RebindTarget($"PICK CLONE {i + 1}", _selectClone[i], 0);
                _selectTargets.Add(select);
                _rebindTargets.Add(select);
            }

            // 2. Put back the keys the player chose last time.
            for (int i = 0; i < _rebindTargets.Count; i++)
            {
                _rebindTargets[i].LoadSaved();
            }
        }

        /// <summary>Index of the first binding that is the named part of a composite, for example "Up" in WASD.</summary>
        private static int FindCompositePart(InputAction action, string part)
        {
            for (int i = 0; i < action.bindings.Count; i++)
            {
                InputBinding binding = action.bindings[i];
                if (binding.isPartOfComposite && string.Equals(binding.name, part, StringComparison.OrdinalIgnoreCase))
                {
                    return i;
                }
            }

            Debug.LogError($"PlayerInputReader: {action.name} has no composite part called {part}.");
            return 0;
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

            if (_markRun.bindings.Count == 0)
            {
                _markRun.AddBinding("<Keyboard>/f");
            }

            if (_jump.bindings.Count == 0)
            {
                _jump.AddBinding("<Keyboard>/space");
            }

            // Number row keys are named "1", "2" and so on, so element i defaults to key i + 1.
            for (int i = 0; i < _selectClone.Length; i++)
            {
                if (_selectClone[i].bindings.Count == 0)
                {
                    _selectClone[i].AddBinding($"<Keyboard>/{i + 1}");
                }
            }

            if (_map.bindings.Count == 0)
            {
                _map.AddBinding("<Keyboard>/tab");
            }

            if (_skip.bindings.Count == 0)
            {
                _skip.AddBinding("<Keyboard>/anyKey");
                _skip.AddBinding("<Mouse>/leftButton");
            }

            if (_pause.bindings.Count == 0)
            {
                _pause.AddBinding("<Keyboard>/escape");
            }
        }
    }
}
