// DoorAudio: on the Door prefab. Plays the shutter sound when the door opens or closes. The silent
// closing every door does when the room resets is skipped, so a new attempt does not rattle.
using GameJam.Core;
using GameJam.Interactables;
using UnityEngine;

namespace GameJam.Audio
{
    /// <summary>Plays door open and close sounds from the Door's events.</summary>
    [RequireComponent(typeof(Door))]
    public class DoorAudio : MonoBehaviour
    {
        [SerializeField] private Door _door;
        [SerializeField] private SoundCue _open;
        [SerializeField] private SoundCue _close;

        private GameStateController _state;

        private void Awake()
        {
            if (_door == null)
            {
                _door = GetComponent<Door>();
            }

            _state = FindAnyObjectByType<GameStateController>();
        }

        private void OnEnable()
        {
            _door.Opened += HandleOpened;
            _door.Closed += HandleClosed;
        }

        private void OnDisable()
        {
            _door.Opened -= HandleOpened;
            _door.Closed -= HandleClosed;
        }

        private void HandleOpened()
        {
            AudioManager.Play(_open);
        }

        private void HandleClosed()
        {
            // The room reset and the intro slam close doors on their own; only a real close (e.g. power lost) makes noise.
            if (_state != null && (_state.State == GameState.Resetting || _state.State == GameState.Intro))
            {
                return;
            }

            AudioManager.Play(_close);
        }
    }
}
