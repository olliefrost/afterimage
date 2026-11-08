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

        // Last shown state, so Refresh can animate only what changed.
        private int _shownCount;
        private int _shownControlled;
        private int _shownPending;

        private void OnEnable()
        {
            // 1. Bind each icon to its slot and listen for clicks (icons past the limit can show as locked).
            int count = Mathf.Min(_icons.Length, _afterimages.SlotCount);
            for (int i = 0; i < count; i++)
            {
                _icons[i].Init(i, _afterimages.GetSlotColour(i));
                _icons[i].Clicked += _game.RequestSelect;
            }

            // 2. Follow the slots and the keys from now on, starting with their current state.
            _afterimages.Changed += Refresh;
            _afterimages.CloneDied += HandleCloneDied;
            _rebinder.Changed += RefreshKeyLabels;
            _game.RunActionHandled += HandleRunAction;
            _shownCount = _afterimages.CloneCount;
            _shownControlled = _afterimages.ControlledSlot;
            _shownPending = _afterimages.PendingSlot;
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
            _game.RunActionHandled -= HandleRunAction;
        }

        /// <summary>Shows each clone's current select key on its badge.</summary>
        private void RefreshKeyLabels()
        {
            int count = Mathf.Min(_icons.Length, _afterimages.SlotCount);
            for (int i = 0; i < count; i++)
            {
                _icons[i].SetKeyLabel(_input.GetSelectTarget(i).KeyName.ToUpperInvariant());
            }
        }

        /// <summary>
        /// Shows every saved slot with its current markers. The ghost for the next new clone only
        /// shows while the player is recording it or has picked it, never just because it is free.
        /// Once the chamber's clone limit is reached, the icon just past it shows a lock.
        /// </summary>
        private void Refresh()
        {
            int controlled = _afterimages.ControlledSlot;
            int pending = _afterimages.PendingSlot;
            bool atLimit = _afterimages.CloneCount >= _afterimages.MaxClones;
            for (int i = 0; i < _icons.Length; i++)
            {
                bool isSaved = i < _afterimages.CloneCount;
                bool isUsedNewSlot = _afterimages.IsNewSlot(i) && (i == controlled || i == pending);
                bool isLocked = atLimit && i == _afterimages.MaxClones;
                _icons[i].gameObject.SetActive(isSaved || isUsedNewSlot || isLocked);
                if (isLocked)
                {
                    _icons[i].ShowLocked();
                }
                else if (isSaved || isUsedNewSlot)
                {
                    _icons[i].Show(isSaved, _afterimages.IsSlotDead(i), i == controlled, i == pending);
                }
            }

            AnimateChanges(controlled, pending);
        }

        /// <summary>
        /// Pops in a newly saved clone's icon and bumps the icon that just became controlled or
        /// picked, so each change catches the eye.
        /// </summary>
        private void AnimateChanges(int controlled, int pending)
        {
            int count = _afterimages.CloneCount;
            if (count > _shownCount && count - 1 < _icons.Length)
            {
                _icons[count - 1].Pop();
            }

            if (controlled != _shownControlled && IsShown(controlled))
            {
                _icons[controlled].Punch();
            }

            if (pending != _shownPending && IsShown(pending))
            {
                _icons[pending].Punch();
            }

            _shownCount = count;
            _shownControlled = controlled;
            _shownPending = pending;
        }

        /// <summary>
        /// A refused pick (1-5 or a click) gives that icon a small "no" shake. A slot past the clone
        /// limit has no icon of its own, so the padlock icon shakes instead.
        /// </summary>
        private void HandleRunAction(RunAction action, int slot, bool accepted)
        {
            if (action != RunAction.Select || accepted)
            {
                return;
            }

            if (IsShown(slot))
            {
                _icons[slot].Nudge();
            }
            else if (slot >= _afterimages.MaxClones && IsShown(_afterimages.MaxClones))
            {
                _icons[_afterimages.MaxClones].Nudge();
            }
        }

        private bool IsShown(int slot)
        {
            return slot >= 0 && slot < _icons.Length && _icons[slot].gameObject.activeSelf;
        }

        private void HandleCloneDied(int slot)
        {
            Refresh();
            _icons[slot].Shake();
        }
    }
}
