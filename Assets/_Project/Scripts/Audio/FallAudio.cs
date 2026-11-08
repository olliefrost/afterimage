// FallAudio: one per chamber with chasms (on GameSystems). Plays a falling sound whenever a hazard
// drops someone into a chasm (a missed jump, or a rising platform dropping with people on it),
// from the static Falling.Fell event. Fell is announced before the kill; a clone's fall is quieter.
using GameJam.Core;
using GameJam.Hazards;
using UnityEngine;

namespace GameJam.Audio
{
    /// <summary>Plays a sound when an actor falls into a chasm.</summary>
    public class FallAudio : MonoBehaviour
    {
        [SerializeField] private SoundCue _fall;
        [Tooltip("Volume scale when a clone falls.")]
        [Range(0f, 1f)]
        [SerializeField] private float _cloneVolumeScale = 0.5f;

        private void OnEnable()
        {
            Falling.Fell += HandleFell;
        }

        private void OnDisable()
        {
            Falling.Fell -= HandleFell;
        }

        private void HandleFell(IActor actor)
        {
            AudioManager.Play(_fall, actor.IsClone ? _cloneVolumeScale : 1f);
        }
    }
}
