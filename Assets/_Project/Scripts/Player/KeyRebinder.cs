// KeyRebinder: changes a key while the pause menu's settings are open. It waits for the next
// keyboard key (Esc cancels), saves it through the RebindTarget, and swaps keys if the new key was
// already used by another rebindable key. Lives on GameSystems next to PlayerInputReader; the
// settings rows start rebinds here, and the HUD key letters refresh when Changed fires.
using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace GameJam.Player
{
    /// <summary>Runs one interactive rebind at a time and resets keys to their defaults.</summary>
    public class KeyRebinder : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private PlayerInputReader _input;

        [Header("Tuning")]
        [Tooltip("Seconds to wait after a key press in case a better match follows, so a stray key is not grabbed.")]
        [SerializeField] private float _matchWaitSeconds = 0.1f;

        private InputActionRebindingExtensions.RebindingOperation _operation;
        private int _finishedFrame = -1;

        /// <summary>The key waiting for a new press, or null when no rebind is running.</summary>
        public RebindTarget ActiveTarget { get; private set; }

        /// <summary>
        /// True while waiting for a key, and on the frame a rebind ends, so the Esc that cancels a
        /// rebind does not also close the menu.
        /// </summary>
        public bool IsRebinding => _operation != null || Time.frameCount == _finishedFrame;

        /// <summary>Raised when a rebind starts or ends, or the keys are reset.</summary>
        public event Action Changed;

        private void OnDisable()
        {
            CancelRebind();
        }

        /// <summary>
        /// Waits for the next keyboard key and binds it to the target. The target's action must be
        /// switched off, which it is while the game is paused.
        /// </summary>
        public void StartRebind(RebindTarget target)
        {
            CancelRebind();

            string oldPath = target.Path;
            ActiveTarget = target;
            _operation = target.Action.PerformInteractiveRebinding(target.BindingIndex)
                .WithControlsHavingToMatchPath("<Keyboard>")
                .WithControlsExcluding("<Keyboard>/anyKey")
                .WithCancelingThrough("<Keyboard>/escape")
                .WithMatchingEventsBeingSuppressed()
                .OnMatchWaitForAnother(_matchWaitSeconds)
                .OnApplyBinding((operation, newPath) => ApplyKey(target, newPath, oldPath))
                .OnComplete(operation => Finish())
                .OnCancel(operation => Finish())
                .Start();

            Changed?.Invoke();
        }

        /// <summary>Stops a running rebind and keeps the old key.</summary>
        public void CancelRebind()
        {
            _operation?.Cancel();
        }

        /// <summary>Puts every rebindable key back to its default.</summary>
        public void ResetAll()
        {
            CancelRebind();
            for (int i = 0; i < _input.RebindTargets.Count; i++)
            {
                _input.RebindTargets[i].ResetToDefault();
            }

            Changed?.Invoke();
        }

        /// <summary>Binds the new key; any other key already on it takes this target's old key instead.</summary>
        private void ApplyKey(RebindTarget target, string newPath, string oldPath)
        {
            for (int i = 0; i < _input.RebindTargets.Count; i++)
            {
                RebindTarget other = _input.RebindTargets[i];
                if (other != target && string.Equals(other.Path, newPath, StringComparison.OrdinalIgnoreCase))
                {
                    other.SetPath(oldPath);
                }
            }

            target.SetPath(newPath);
        }

        /// <summary>Cleans up after a rebind that completed or was cancelled.</summary>
        private void Finish()
        {
            _operation.Dispose();
            _operation = null;
            ActiveTarget = null;
            _finishedFrame = Time.frameCount;
            Changed?.Invoke();
        }
    }
}
