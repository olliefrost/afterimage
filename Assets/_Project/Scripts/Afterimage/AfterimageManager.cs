// AfterimageManager: stores every saved run in a numbered clone slot and spawns one clone per slot
// at the start of each attempt. GameStateController calls AddRecording and RespawnClones; the HUD
// listens to CloneCountChanged. Slots are kept in order so a later feature can re-record one slot
// while the others keep replaying.
using System;
using System.Collections.Generic;
using GameJam.Core;
using UnityEngine;

namespace GameJam.Afterimage
{
    /// <summary>Owns the saved runs (clone slots) and the clone objects that replay them.</summary>
    public class AfterimageManager : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private TickClock _clock;
        [Tooltip("Prefab with an AfterimageReplayer, spawned once per saved run.")]
        [SerializeField] private AfterimageReplayer _clonePrefab;
        [Tooltip("Optional parent for spawned clones, to keep the hierarchy tidy.")]
        [SerializeField] private Transform _cloneParent;

        private readonly List<Recording> _slots = new List<Recording>();
        private readonly List<AfterimageReplayer> _clones = new List<AfterimageReplayer>();

        /// <summary>Number of saved runs, which is the number of clones each attempt.</summary>
        public int CloneCount => _slots.Count;

        /// <summary>Raised with the new count whenever a run is saved.</summary>
        public event Action<int> CloneCountChanged;

        /// <summary>Saves a finished run into a new clone slot.</summary>
        public void AddRecording(Recording recording)
        {
            _slots.Add(recording);
            CloneCountChanged?.Invoke(_slots.Count);
        }

        /// <summary>Removes last attempt's clones and spawns a fresh one for every slot, ready to replay from tick 0.</summary>
        public void RespawnClones()
        {
            for (int i = 0; i < _clones.Count; i++)
            {
                Destroy(_clones[i].gameObject);
            }

            _clones.Clear();

            for (int i = 0; i < _slots.Count; i++)
            {
                AfterimageReplayer clone = Instantiate(_clonePrefab, _cloneParent);
                clone.name = $"Clone {i + 1}";
                clone.Init(_slots[i], _clock);
                _clones.Add(clone);
            }
        }
    }
}
