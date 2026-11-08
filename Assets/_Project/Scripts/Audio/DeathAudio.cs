// DeathAudio: on the Player and Clone prefabs. Plays a death sound when its actor is killed: the
// live player's sound on the Player prefab, a glitchy cut-off on a Clone. Put it on the prefab root
// next to PlayerActor or AfterimageReplayer; only the matching reference is filled in.
using GameJam.Afterimage;
using GameJam.Player;
using UnityEngine;

namespace GameJam.Audio
{
    /// <summary>Plays a sound when the player or a clone dies.</summary>
    public class DeathAudio : MonoBehaviour
    {
        [Tooltip("Fill in on the Player prefab.")]
        [SerializeField] private PlayerActor _player;
        [Tooltip("Fill in on the Clone prefab.")]
        [SerializeField] private AfterimageReplayer _clone;
        [SerializeField] private SoundCue _death;

        private void OnEnable()
        {
            if (_player != null)
            {
                _player.Died += HandlePlayerDied;
            }

            if (_clone != null)
            {
                _clone.Died += HandleCloneDied;
            }
        }

        private void OnDisable()
        {
            if (_player != null)
            {
                _player.Died -= HandlePlayerDied;
            }

            if (_clone != null)
            {
                _clone.Died -= HandleCloneDied;
            }
        }

        private void HandlePlayerDied()
        {
            AudioManager.Play(_death);
        }

        private void HandleCloneDied(AfterimageReplayer replayer)
        {
            AudioManager.Play(_death);
        }
    }
}
