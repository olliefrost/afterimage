// CloneColumn: the column of clone icons on the right edge of the HUD. Shows one icon per saved
// clone plus a ghost icon for a new clone while it is being recorded or picked, marks the clone
// the player is controlling and the one picked to control next, shows deaths, and routes icon
// clicks to GameStateController.RequestSelect (the same as keys 1-5). Reads AfterimageManager,
// and labels each key badge with the clone's current select key (KeyRebinder can change it).
using GameJam.Afterimage;
using GameJam.Core;
using GameJam.Player;
using UnityEngine;

namespace GameJam.UI
{
    /// <summary>Keeps the HUD's clone icons in step with the clone slots.</summary>
    public class CloneColumn : MonoBehaviour
    {
        [Header("Game references")]
        [SerializeField] private GameStateController _game;
        [SerializeField] private AfterimageManager _afterimages;
        [SerializeField] private PlayerInputReader _input;
        [SerializeField] private KeyRebinder _rebinder;

        [Header("UI references")]
        [Tooltip("One pre-placed icon per clone slot, top to bottom. Icons past the next new clone are hidden.")]
        [SerializeField] private CloneIconView[] _icons;

        private void OnEnable()
        {
            // 1. Bind each icon to its slot and listen for clicks.
            int count = Mathf.Min(_icons.Length, _afterimages.MaxClones);
            for (int i = 0; i < count; i++)
            {
                _icons[i].Init(i, _afterimages.GetSlotColour(i));
                _icons[i].Clicked += _game.RequestSelect;
            }

            // 2. Follow the slots and the keys from now on, starting with their current state.
            _afterimages.Changed += Refresh;
            _afterimages.CloneDied += HandleCloneDied;
            _rebinder.Changed += RefreshKeyLabels;
            Refresh();
            RefreshKeyLabels();
        }

        private void OnDisable()
        {
            for (int i = 0; i < _icons.Length; i++)
            {
                _icons[i].Clicked -= _game.RequestSelect;
            }

            _afterimages.Changed -= Refresh;
            _afterimages.CloneDied -= HandleCloneDied;
            _rebinder.Changed -= RefreshKeyLabels;
        }

        /// <summary>Shows each clone's current select key on its badge.</summary>
        private void RefreshKeyLabels()
        {
            int count = Mathf.Min(_icons.Length, _afterimages.MaxClones);
            for (int i = 0; i < count; i++)
            {
                _icons[i].SetKeyLabel(_input.GetSelectTarget(i).KeyName.ToUpperInvariant());
            }
        }

        /// <summary>
        /// Shows every saved slot with its current markers. The ghost for the next new clone only
        /// shows while the player is recording it or has picked it, never just because it is free.
        /// </summary>
        private void Refresh()
        {
            int controlled = _afterimages.ControlledSlot;
            int pending = _afterimages.PendingSlot;
            for (int i = 0; i < _icons.Length; i++)
            {
                bool isSaved = i < _afterimages.CloneCount;
                bool isUsedNewSlot = _afterimages.IsNewSlot(i) && (i == controlled || i == pending);
                bool isVisible = isSaved || isUsedNewSlot;
                _icons[i].gameObject.SetActive(isVisible);
                if (isVisible)
                {
                    _icons[i].Show(isSaved, _afterimages.IsSlotDead(i), i == controlled, i == pending);
                }
            }
        }

        private void HandleCloneDied(int slot)
        {
            Refresh();
            _icons[slot].Shake();
        }
    }
}
