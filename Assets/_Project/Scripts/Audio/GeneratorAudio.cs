// GeneratorAudio: on Chamber 3's PowerGenerator. Plays the start-up and shut-down sounds when power
// changes, and keeps a low hum looping on its own source for as long as the room is powered.
using GameJam.Core;
using GameJam.Machines;
using UnityEngine;

namespace GameJam.Audio
{
    /// <summary>Plays the generator's power on, power off and running hum.</summary>
    public class GeneratorAudio : MonoBehaviour
    {
        [SerializeField] private PowerGenerator _generator;
        [SerializeField] private SoundCue _powerOn;
        [SerializeField] private SoundCue _powerOff;
        [Tooltip("Loops while the room is powered.")]
        [SerializeField] private AudioClip _hum;
        [Range(0f, 1f)]
        [SerializeField] private float _humVolume = 0.3f;

        private AudioSource _humSource;
        private GameStateController _state;

        private void Awake()
        {
            _state = FindAnyObjectByType<GameStateController>();
            _humSource = gameObject.AddComponent<AudioSource>();
            _humSource.clip = _hum;
            _humSource.volume = _humVolume;
            _humSource.loop = true;
            _humSource.playOnAwake = false;
        }

        private void OnEnable()
        {
            _generator.PoweredChanged += HandlePoweredChanged;
            SetHum(_generator.IsPowered);
        }

        private void OnDisable()
        {
            _generator.PoweredChanged -= HandlePoweredChanged;
            _humSource.Stop();
        }

        private void HandlePoweredChanged(bool powered)
        {
            // The room reset cuts the power silently; only a real power change makes the start-up or shut-down sound.
            bool isReset = _state != null && _state.State == GameState.Resetting;
            if (!isReset)
            {
                AudioManager.Play(powered ? _powerOn : _powerOff);
            }

            SetHum(powered);
        }

        private void SetHum(bool powered)
        {
            if (powered && _hum != null)
            {
                if (!_humSource.isPlaying)
                {
                    _humSource.Play();
                }
            }
            else
            {
                _humSource.Stop();
            }
        }
    }
}
