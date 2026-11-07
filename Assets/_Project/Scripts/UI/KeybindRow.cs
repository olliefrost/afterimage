// KeybindRow: one line in the pause menu's key settings, such as "INTERACT   [E]". Clicking the key
// button asks KeyRebinder for a new key; the button reads "PRESS A KEY" until one is pressed.
// SettingsPanel spawns one row per PlayerInputReader.RebindTargets entry and refreshes them.
using GameJam.Player;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace GameJam.UI
{
    /// <summary>Shows one rebindable key and starts its rebind when clicked.</summary>
    public class KeybindRow : MonoBehaviour
    {
        [Header("UI references")]
        [SerializeField] private Text _label;
        [SerializeField] private Button _keyButton;
        [SerializeField] private Text _keyText;

        [Header("Text")]
        [Tooltip("Shown on the key button while waiting for a new key.")]
        [SerializeField] private string _listeningText = "PRESS A KEY";

        private RebindTarget _target;
        private KeyRebinder _rebinder;

        private void OnDestroy()
        {
            _keyButton.onClick.RemoveListener(HandleClicked);
        }

        /// <summary>Binds the row to its key. Called once by SettingsPanel after spawning the row.</summary>
        public void Init(RebindTarget target, KeyRebinder rebinder)
        {
            _target = target;
            _rebinder = rebinder;
            _label.text = target.Label;
            _keyButton.onClick.AddListener(HandleClicked);
            Refresh();
        }

        /// <summary>Shows the current key, or the waiting text while this row is being rebound.</summary>
        public void Refresh()
        {
            _keyText.text = _rebinder.ActiveTarget == _target ? _listeningText : _target.KeyName.ToUpperInvariant();
        }

        private void HandleClicked()
        {
            // Deselect the button first, so pressing Space or Enter as the new key does not click it again.
            if (EventSystem.current != null)
            {
                EventSystem.current.SetSelectedGameObject(null);
            }

            _rebinder.StartRebind(_target);
        }
    }
}
