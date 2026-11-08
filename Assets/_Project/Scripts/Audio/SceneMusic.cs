// SceneMusic: one per scene (an empty object). Tells AudioManager which music track this scene
// wants when it starts; AudioManager crossfades from whatever was playing, and keeps playing if the
// next scene asks for the same track. An empty track leaves the current music alone.
using UnityEngine;

namespace GameJam.Audio
{
    /// <summary>Asks the AudioManager for this scene's music track.</summary>
    public class SceneMusic : MonoBehaviour
    {
        [Tooltip("Looping music for this scene (mus_title, mus_chamber). Empty keeps the current music.")]
        [SerializeField] private AudioClip _track;
        [Tooltip("Keep music well under the effects: 0.2 is about 14 dB down.")]
        [Range(0f, 1f)]
        [SerializeField] private float _volume = 0.2f;

        private void Start()
        {
            AudioManager.PlayMusic(_track, _volume);
        }
    }
}
