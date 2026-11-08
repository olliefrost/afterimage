// StairsAudio: on Chamber 3's HoverStairs. Plays the hover-plate whirr whenever the steps flip up or
// down or start sliding to another stop.
using GameJam.Machines;
using UnityEngine;

namespace GameJam.Audio
{
    /// <summary>Plays a sound when the hover stairs move.</summary>
    public class StairsAudio : MonoBehaviour
    {
        [SerializeField] private HoverStairs _stairs;
        [SerializeField] private SoundCue _move;

        private void OnEnable()
        {
            _stairs.Moved += HandleMoved;
        }

        private void OnDisable()
        {
            _stairs.Moved -= HandleMoved;
        }

        private void HandleMoved()
        {
            AudioManager.Play(_move);
        }
    }
}
