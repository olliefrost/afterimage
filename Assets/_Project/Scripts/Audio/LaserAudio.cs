// LaserAudio: on the Laser prefab. Plays the charge-up whine when the laser starts charging and the
// shot when it fires, from LaserEmitter.PhaseChanged. The emitter's tick schedule decides the timing,
// so the sound lines up with the beam on every attempt.
using GameJam.Hazards;
using UnityEngine;

namespace GameJam.Audio
{
    /// <summary>Plays laser charge and fire sounds from the emitter's phase changes.</summary>
    public class LaserAudio : MonoBehaviour
    {
        [SerializeField] private LaserEmitter _emitter;
        [SerializeField] private SoundCue _charge;
        [SerializeField] private SoundCue _fire;

        private void OnEnable()
        {
            _emitter.PhaseChanged += HandlePhaseChanged;
        }

        private void OnDisable()
        {
            _emitter.PhaseChanged -= HandlePhaseChanged;
        }

        private void HandlePhaseChanged(LaserPhase phase)
        {
            if (phase == LaserPhase.Charging)
            {
                AudioManager.Play(_charge);
            }
            else if (phase == LaserPhase.Firing)
            {
                AudioManager.Play(_fire);
            }
        }
    }
}
