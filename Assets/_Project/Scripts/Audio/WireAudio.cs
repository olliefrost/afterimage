// WireAudio: on the SignalWire prefab. Plays a crackle as the signal leaves the button and a zap
// when it reaches the door, from SignalWire.Sent and Arrived.
using GameJam.Interactables;
using UnityEngine;

namespace GameJam.Audio
{
    /// <summary>Plays wire pulse and arrival sounds.</summary>
    [RequireComponent(typeof(SignalWire))]
    public class WireAudio : MonoBehaviour
    {
        [SerializeField] private SignalWire _wire;
        [SerializeField] private SoundCue _pulse;
        [SerializeField] private SoundCue _arrive;

        private void Awake()
        {
            if (_wire == null)
            {
                _wire = GetComponent<SignalWire>();
            }
        }

        private void OnEnable()
        {
            _wire.Sent += HandleSent;
            _wire.Arrived += HandleArrived;
        }

        private void OnDisable()
        {
            _wire.Sent -= HandleSent;
            _wire.Arrived -= HandleArrived;
        }

        private void HandleSent()
        {
            AudioManager.Play(_pulse);
        }

        private void HandleArrived()
        {
            AudioManager.Play(_arrive);
        }
    }
}
