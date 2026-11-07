// RebindTarget: one key the player can change in the pause menu's settings, such as "Interact"
// or the "Up" part of Move. PlayerInputReader builds the list; KeyRebinder changes the key and
// the settings rows and HUD labels show KeyName. A changed key is saved in PlayerPrefs and
// applied again whenever a chamber loads.
using UnityEngine;
using UnityEngine.InputSystem;

namespace GameJam.Player
{
    /// <summary>A single rebindable binding: an action plus the index of the binding to change.</summary>
    public class RebindTarget
    {
        /// <summary>Creates a target for one binding of an action.</summary>
        public RebindTarget(string label, InputAction action, int bindingIndex)
        {
            Label = label;
            Action = action;
            BindingIndex = bindingIndex;
        }

        /// <summary>Name shown in the settings list.</summary>
        public string Label { get; }

        /// <summary>The action that owns the binding.</summary>
        public InputAction Action { get; }

        /// <summary>Index of the binding inside <see cref="Action"/>.</summary>
        public int BindingIndex { get; }

        /// <summary>The key as the player sees it, for example "E" or "Space".</summary>
        public string KeyName => Action.GetBindingDisplayString(BindingIndex);

        /// <summary>The control path in use right now, including any rebind, for example "&lt;Keyboard&gt;/e".</summary>
        public string Path => Action.bindings[BindingIndex].effectivePath;

        // One PlayerPrefs entry per binding. Binding indices are fixed by the code defaults, so they match in every scene.
        private string SaveKey => $"bind.{Action.name}.{BindingIndex}";

        /// <summary>Changes the key and saves it.</summary>
        public void SetPath(string path)
        {
            Action.ApplyBindingOverride(BindingIndex, path);
            PlayerPrefs.SetString(SaveKey, path);
            PlayerPrefs.Save();
        }

        /// <summary>Applies the saved key, if the player changed this binding before.</summary>
        public void LoadSaved()
        {
            string path = PlayerPrefs.GetString(SaveKey, string.Empty);
            if (!string.IsNullOrEmpty(path))
            {
                Action.ApplyBindingOverride(BindingIndex, path);
            }
        }

        /// <summary>Goes back to the default key and forgets the saved one.</summary>
        public void ResetToDefault()
        {
            Action.RemoveBindingOverride(BindingIndex);
            PlayerPrefs.DeleteKey(SaveKey);
            PlayerPrefs.Save();
        }
    }
}
