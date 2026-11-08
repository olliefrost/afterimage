// SoundCue: a ScriptableObject describing one sound effect (clip variants, volume, pitch spread and
// anti-stacking limits). Created in the editor (Create > GameJam > Sound Cue) so teammates can swap
// clips and tune levels without code. Played through AudioManager by the audio bridge components
// (FootstepAudio, ButtonAudio, DoorAudio, GameAudio). A cue with no clips is simply silent.
using UnityEngine;

namespace GameJam.Audio
{
    /// <summary>One tunable sound effect: random variant, volume, pitch spread and play limits.</summary>
    [CreateAssetMenu(menuName = "GameJam/Sound Cue", fileName = "sfx_new")]
    public class SoundCue : ScriptableObject
    {
        [Tooltip("Variants. One is picked at random each play. Leave empty to keep the cue silent.")]
        [SerializeField] private AudioClip[] _clips;
        [Range(0f, 1f)]
        [SerializeField] private float _volume = 1f;
        [Tooltip("Each play gets a random pitch between these two values (1 = original).")]
        [SerializeField] private float _minPitch = 0.9f;
        [SerializeField] private float _maxPitch = 1.1f;
        [Tooltip("Each play's volume is lowered by up to this fraction at random, so repeats do not sound identical.")]
        [Range(0f, 0.5f)]
        [SerializeField] private float _volumeJitter = 0.1f;
        [Tooltip("Shortest time in seconds between two plays of this cue.")]
        [SerializeField] private float _minGap = 0.05f;
        [Tooltip("How many copies of this cue may sound at once, so five clones do not stack five times.")]
        [SerializeField] private int _maxCopies = 3;

        /// <summary>True when at least one clip is assigned.</summary>
        public bool HasClips => _clips != null && _clips.Length > 0;

        public float MinGap => _minGap;
        public int MaxCopies => _maxCopies;

        // Runtime only: the variant played last, so the same one never plays twice in a row.
        private int _lastClip = -1;

        /// <summary>Picks a random clip variant that differs from the last one, or null if the cue has none.</summary>
        public AudioClip PickClip()
        {
            if (!HasClips)
            {
                return null;
            }

            int index = Random.Range(0, _clips.Length);
            if (_clips.Length > 1 && index == _lastClip)
            {
                index = (index + 1 + Random.Range(0, _clips.Length - 1)) % _clips.Length;
            }

            _lastClip = index;
            return _clips[index];
        }

        /// <summary>Picks this play's volume: the cue volume, lowered by a random amount up to the jitter.</summary>
        public float PickVolume()
        {
            return _volume * (1f - Random.Range(0f, _volumeJitter));
        }

        /// <summary>Picks a random pitch inside the cue's spread.</summary>
        public float PickPitch()
        {
            return Random.Range(_minPitch, _maxPitch);
        }
    }
}
