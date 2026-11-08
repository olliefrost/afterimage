// PlatformAudio: on each Chamber 4 RisingPlatform. Plays a hydraulic rise sound when it starts to
// come up and a lowering sound when it starts to drop (a warning that anyone on it will fall).
using GameJam.Machines;
using UnityEngine;

namespace GameJam.Audio
{
    /// <summary>Plays rising and lowering sounds for a rising platform.</summary>
    public class PlatformAudio : MonoBehaviour
    {
        [SerializeField] private RisingPlatform _platform;
        [SerializeField] private SoundCue _rise;
        [SerializeField] private SoundCue _lower;

        private void OnEnable()
        {
            _platform.Rising += HandleRising;
            _platform.Lowering += HandleLowering;
        }

        private void OnDisable()
        {
            _platform.Rising -= HandleRising;
            _platform.Lowering -= HandleLowering;
        }

        private void HandleRising()
        {
            AudioManager.Play(_rise);
        }

        private void HandleLowering()
        {
            AudioManager.Play(_lower);
        }
    }
}
