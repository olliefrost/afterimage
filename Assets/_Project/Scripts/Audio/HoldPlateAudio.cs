// HoldPlateAudio: plays one cue when a HoldPlate goes down and another when it comes back up. On each
// pressure plate it plays the plate click; on the HydraulicBlock it listens to the same pedal for
// the piston push and spring-back, so one script serves both.
using GameJam.Interactables;
using UnityEngine;

namespace GameJam.Audio
{
    /// <summary>Plays sounds when a hold plate is pressed and released.</summary>
    public class HoldPlateAudio : MonoBehaviour
    {
        [SerializeField] private HoldPlate _plate;
        [SerializeField] private SoundCue _down;
        [SerializeField] private SoundCue _up;

        private void Awake()
        {
            if (_plate == null)
            {
                _plate = GetComponent<HoldPlate>();
            }
        }

        private void OnEnable()
        {
            _plate.ActiveChanged += HandleActiveChanged;
        }

        private void OnDisable()
        {
            _plate.ActiveChanged -= HandleActiveChanged;
        }

        private void HandleActiveChanged(bool active)
        {
            AudioManager.Play(active ? _down : _up);
        }
    }
}
