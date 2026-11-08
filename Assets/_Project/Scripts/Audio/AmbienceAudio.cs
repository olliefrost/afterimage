// AmbienceAudio: one per chamber (an Ambience object in the scene). Loops the low room tone and,
// at random intervals, plays a distant drip or a gust of wind through the ceiling breaches. Uses its
// own AudioSources so the shared effect pool stays free; they pause with the game. Audio only.
using UnityEngine;

namespace GameJam.Audio
{
    /// <summary>Plays the chamber's room tone and random drips and wind.</summary>
    public class AmbienceAudio : MonoBehaviour
    {
        [Header("Room tone")]
        [SerializeField] private AudioClip _roomTone;
        [Range(0f, 1f)]
        [SerializeField] private float _roomToneVolume = 0.25f;

        [Header("Random one-shots")]
        [SerializeField] private AudioClip[] _drips;
        [SerializeField] private AudioClip _wind;
        [Range(0f, 1f)]
        [SerializeField] private float _dripVolume = 0.3f;
        [Range(0f, 1f)]
        [SerializeField] private float _windVolume = 0.25f;
        [Tooltip("Seconds between random sounds, picked at random between these two values.")]
        [SerializeField] private Vector2 _gapSeconds = new Vector2(6f, 16f);
        [Tooltip("Chance that a random sound is the wind rather than a drip.")]
        [Range(0f, 1f)]
        [SerializeField] private float _windChance = 0.2f;

        private AudioSource _toneSource;
        private AudioSource _shotSource;
        private float _nextShotTime;

        private void Awake()
        {
            // 1. One source for the looping room tone, one for the random sounds.
            _toneSource = gameObject.AddComponent<AudioSource>();
            _toneSource.clip = _roomTone;
            _toneSource.volume = _roomToneVolume;
            _toneSource.loop = true;
            _toneSource.playOnAwake = false;

            _shotSource = gameObject.AddComponent<AudioSource>();
            _shotSource.playOnAwake = false;
        }

        private void Start()
        {
            if (_roomTone != null)
            {
                _toneSource.Play();
            }

            ScheduleNext();
        }

        private void Update()
        {
            // 2. Play a random drip or wind when the timer is up. Scaled time, so it waits while paused.
            if (Time.time < _nextShotTime)
            {
                return;
            }

            bool useWind = _wind != null && (_drips.Length == 0 || Random.value < _windChance);
            if (useWind)
            {
                _shotSource.pitch = 1f;
                _shotSource.PlayOneShot(_wind, _windVolume);
            }
            else if (_drips.Length > 0)
            {
                _shotSource.pitch = Random.Range(0.9f, 1.1f);
                _shotSource.PlayOneShot(_drips[Random.Range(0, _drips.Length)], _dripVolume);
            }

            ScheduleNext();
        }

        private void ScheduleNext()
        {
            _nextShotTime = Time.time + Random.Range(_gapSeconds.x, _gapSeconds.y);
        }
    }
}
