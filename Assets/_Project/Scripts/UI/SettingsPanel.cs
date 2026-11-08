// SettingsPanel: the settings page inside the pause menu. A Screen Shake button cycles
// GameSettings' shake strength (Full, Low, Off). On first open it spawns one KeybindRow per
// rebindable key from PlayerInputReader, keeps the rows in step with KeyRebinder, and has Reset to
// defaults and Back buttons. PauseMenu opens and closes it.
using System;
using System.Collections.Generic;
using GameJam.Core;
using GameJam.Player;
using UnityEngine;
using UnityEngine.UI;

namespace GameJam.UI
{
    /// <summary>Lists the rebindable keys and lets the player change or reset them.</summary>
    public class SettingsPanel : MonoBehaviour
    {
        [Header("UI references")]
        [Tooltip("Row prefab spawned once per rebindable key.")]
        [SerializeField] private KeybindRow _rowPrefab;
        [Tooltip("Parent with a layout group that stacks the rows.")]
        [SerializeField] private Transform _rowParent;
        [SerializeField] private Button _resetButton;
        [SerializeField] private Button _backButton;
        [Tooltip("Cycles the screen shake strength: Full, Low, Off.")]
        [SerializeField] private Button _shakeButton;
        [SerializeField] private Text _shakeLabel;

        private readonly List<KeybindRow> _rows = new List<KeybindRow>();
        private PlayerInputReader _input;
        private KeyRebinder _rebinder;

        /// <summary>True while the panel is showing.</summary>
        public bool IsOpen => gameObject.activeSelf;

        /// <summary>Raised when the player clicks Back.</summary>
        public event Action BackRequested;

        private void OnEnable()
        {
            _rebinder.Changed += RefreshRows;
            _resetButton.onClick.AddListener(_rebinder.ResetAll);
            _backButton.onClick.AddListener(HandleBackClicked);
            _shakeButton.onClick.AddListener(GameSettings.CycleShake);
            GameSettings.Changed += RefreshShakeLabel;
            RefreshRows();
            RefreshShakeLabel();
        }

        private void OnDisable()
        {
            _rebinder.Changed -= RefreshRows;
            _resetButton.onClick.RemoveListener(_rebinder.ResetAll);
            _backButton.onClick.RemoveListener(HandleBackClicked);
            _shakeButton.onClick.RemoveListener(GameSettings.CycleShake);
            GameSettings.Changed -= RefreshShakeLabel;
        }

        /// <summary>Shows the current screen shake strength on its button.</summary>
        private void RefreshShakeLabel()
        {
            _shakeLabel.text = "SCREEN SHAKE: " + GameSettings.Shake.ToString().ToUpperInvariant();
        }

        /// <summary>Gives the panel the scene's input objects. PauseMenu calls this before the panel is first opened.</summary>
        public void Init(PlayerInputReader input, KeyRebinder rebinder)
        {
            _input = input;
            _rebinder = rebinder;
        }

        /// <summary>Shows the panel, spawning the rows the first time.</summary>
        public void Open()
        {
            if (_rows.Count == 0)
            {
                for (int i = 0; i < _input.RebindTargets.Count; i++)
                {
                    KeybindRow row = Instantiate(_rowPrefab, _rowParent);
                    row.Init(_input.RebindTargets[i], _rebinder);
                    _rows.Add(row);
                }
            }

            gameObject.SetActive(true);
        }

        /// <summary>Hides the panel and stops any rebind that is waiting for a key.</summary>
        public void Close()
        {
            _rebinder.CancelRebind();
            gameObject.SetActive(false);
        }

        private void RefreshRows()
        {
            for (int i = 0; i < _rows.Count; i++)
            {
                _rows[i].Refresh();
            }
        }

        private void HandleBackClicked()
        {
            BackRequested?.Invoke();
        }
    }
}
