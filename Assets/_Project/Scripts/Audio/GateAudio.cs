// GateAudio: on Chamber 3's exit lock (the AllOnGate). Plays the lock release when every condition
// is met and the lock clunk when one fails again (for example the power runs out).
using GameJam.Machines;
using UnityEngine;

namespace GameJam.Audio
{
    /// <summary>Plays sounds when an AllOnGate opens and closes.</summary>
    public class GateAudio : MonoBehaviour
    {
        [SerializeField] private AllOnGate _gate;
        [SerializeField] private SoundCue _open;
        [SerializeField] private SoundCue _close;

        private void OnEnable()
        {
            _gate.Changed += HandleChanged;
        }

        private void OnDisable()
        {
            _gate.Changed -= HandleChanged;
        }

        private void HandleChanged(bool on)
        {
            AudioManager.Play(on ? _open : _close);
        }
    }
}
