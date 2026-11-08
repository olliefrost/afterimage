// LaserWallAudio: on Chamber 3's LaserWall. Plays the burn sizzle each time a fuel crate is
// destroyed in the beam, so the player hears why the generator is starving.
using GameJam.Machines;
using UnityEngine;

namespace GameJam.Audio
{
    /// <summary>Plays a sound when the laser wall burns a crate.</summary>
    public class LaserWallAudio : MonoBehaviour
    {
        [SerializeField] private LaserWall _wall;
        [SerializeField] private SoundCue _burn;

        private void OnEnable()
        {
            _wall.Burned += HandleBurned;
        }

        private void OnDisable()
        {
            _wall.Burned -= HandleBurned;
        }

        private void HandleBurned()
        {
            AudioManager.Play(_burn);
        }
    }
}
