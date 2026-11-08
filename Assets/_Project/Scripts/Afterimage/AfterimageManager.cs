// AfterimageManager: owns the clone slots (up to 5 saved runs, each with its own colour), which
// slot the player is controlling (recording into) this run, which slot they picked to control
// next, and the clone objects that replay the other slots. GameStateController calls EndRun,
// SelectSlot and RespawnClones; the HUD reads the slot state and listens to Changed and CloneDied.
// Slot rules are described in Ideas/gui.md.
using System;
using System.Collections.Generic;
using GameJam.Core;
using UnityEngine;

namespace GameJam.Afterimage
{
    /// <summary>
    /// Owns the saved runs (clone slots), the slot the player controls, the slot picked to control
    /// next, and the clone objects that replay every other slot each attempt.
    /// </summary>
    public class AfterimageManager : MonoBehaviour
    {
        /// <summary>Value of <see cref="PendingSlot"/> when no switch is picked.</summary>
        public const int NoSlot = -1;

        [Header("References")]
        [SerializeField] private TickClock _clock;
        [Tooltip("Prefab with an AfterimageReplayer, spawned once per saved run.")]
        [SerializeField] private AfterimageReplayer _clonePrefab;
        [Tooltip("Optional parent for spawned clones, to keep the hierarchy tidy.")]
        [SerializeField] private Transform _cloneParent;

        [Header("Slots")]
        [Tooltip("Most clones this chamber allows (Tutorial1 uses 2). Capped at the number of slot colours.")]
        [Min(1)]
        [SerializeField] private int _maxClones = 5;
        [Tooltip("Colour of each clone slot, used by its light and its HUD icon. Five slots at most.")]
        [SerializeField] private Color[] _slotColours =
        {
            new Color(0.25f, 0.55f, 1f),  // 1 blue
            new Color(0.3f, 0.9f, 0.4f),  // 2 green
            new Color(1f, 0.85f, 0.25f),  // 3 yellow
            new Color(0.7f, 0.4f, 1f),    // 4 purple
            new Color(1f, 0.55f, 0.15f)   // 5 orange
        };

        private readonly List<Recording> _slots = new List<Recording>();
        private readonly List<AfterimageReplayer> _clones = new List<AfterimageReplayer>();
        private readonly List<bool> _deadThisAttempt = new List<bool>();

        /// <summary>Most clones this chamber can hold.</summary>
        public int MaxClones => Mathf.Min(_maxClones, _slotColours.Length);

        /// <summary>Number of slot colours, the most clones any chamber can allow.</summary>
        public int SlotCount => _slotColours.Length;

        /// <summary>Number of saved runs.</summary>
        public int CloneCount => _slots.Count;

        /// <summary>
        /// Slot the player is controlling: this run is saved into it, and its clone does not spawn.
        /// Either a saved slot (re-recording it) or <see cref="CloneCount"/> (a new clone).
        /// </summary>
        public int ControlledSlot { get; private set; }

        /// <summary>Slot the player picked to control from the next restart, or <see cref="NoSlot"/>.</summary>
        public int PendingSlot { get; private set; } = NoSlot;

        /// <summary>This attempt's clones, indexed by slot; the controlled slot's entry is null.</summary>
        public IReadOnlyList<AfterimageReplayer> Clones => _clones;

        /// <summary>Raised when a run ends, the pick changes or the clones respawn.</summary>
        public event Action Changed;

        /// <summary>Raised with the slot index when that slot's clone dies this attempt.</summary>
        public event Action<int> CloneDied;

        /// <summary>Colour of a slot, shared by its clone's light and its HUD icon.</summary>
        public Color GetSlotColour(int slot)
        {
            return _slotColours[slot];
        }

        /// <summary>True if the slot's clone died during the current attempt.</summary>
        public bool IsSlotDead(int slot)
        {
            return slot < _deadThisAttempt.Count && _deadThisAttempt[slot];
        }

        /// <summary>True for the empty slot the next new clone would go into (none once the limit is reached).</summary>
        public bool IsNewSlot(int slot)
        {
            return slot == _slots.Count && _slots.Count < MaxClones;
        }

        /// <summary>
        /// Picks a slot to control from the next restart: any saved slot, or the new slot to go back
        /// to making new clones. Picking the pending slot again cancels it; picking the slot already
        /// controlled clears the pick. Returns false if the slot cannot be picked (nothing saved there
        /// and it is not the next new slot), so the HUD can show the refusal.
        /// </summary>
        public bool SelectSlot(int slot)
        {
            if (slot < 0 || (slot >= _slots.Count && !IsNewSlot(slot)))
            {
                return false;
            }

            PendingSlot = slot == PendingSlot || slot == ControlledSlot ? NoSlot : slot;
            Changed?.Invoke();
            return true;
        }

        /// <summary>
        /// Ends the current run. If <paramref name="keep"/> is true the run is saved into the controlled
        /// slot. Then control moves to the picked slot if there is one; otherwise every save (a new
        /// clone or a re-record) moves on to the next new slot, or stays put once all slots are full.
        /// A discarded run (retry) keeps control where it is.
        /// </summary>
        public void EndRun(Recording run, bool keep)
        {
            // 0. A run with no recorded ticks (saved on the very first tick) has nowhere to put the
            //    clone, so it is thrown away like a retry.
            keep = keep && run.Frames.Count > 0;

            // 1. Store the run in the slot the player was controlling.
            bool wasNewSlot = ControlledSlot == _slots.Count;
            if (keep)
            {
                if (wasNewSlot)
                {
                    _slots.Add(run);
                }
                else
                {
                    _slots[ControlledSlot] = run;
                }
            }

            // 2. Decide which slot the player controls next run.
            if (PendingSlot != NoSlot)
            {
                ControlledSlot = PendingSlot;
                PendingSlot = NoSlot;
            }
            else if (keep && _slots.Count < MaxClones)
            {
                ControlledSlot = _slots.Count;
            }

            Changed?.Invoke();
        }

        /// <summary>
        /// Removes last attempt's clones and spawns a fresh one for every saved slot, ready to replay
        /// from tick 0. The controlled slot is skipped, because the player is playing it.
        /// </summary>
        public void RespawnClones()
        {
            // 1. Clear last attempt's clones and their deaths.
            for (int i = 0; i < _clones.Count; i++)
            {
                if (_clones[i] != null)
                {
                    _clones[i].Died -= HandleCloneDied;
                    Destroy(_clones[i].gameObject);
                }
            }

            _clones.Clear();
            _deadThisAttempt.Clear();

            // 2. Spawn one clone per slot, keeping the lists indexed by slot.
            for (int i = 0; i < _slots.Count; i++)
            {
                _deadThisAttempt.Add(false);
                if (i == ControlledSlot)
                {
                    _clones.Add(null);
                    continue;
                }

                AfterimageReplayer clone = Instantiate(_clonePrefab, _cloneParent);
                clone.name = $"Clone {i + 1}";
                clone.Init(_slots[i], _clock, i, _slotColours[i]);
                clone.Died += HandleCloneDied;
                _clones.Add(clone);
            }

            Changed?.Invoke();
        }

        /// <summary>Marks the clone's slot as dead until the next attempt and tells the HUD.</summary>
        private void HandleCloneDied(AfterimageReplayer clone)
        {
            _deadThisAttempt[clone.SlotIndex] = true;
            CloneDied?.Invoke(clone.SlotIndex);
        }
    }
}
