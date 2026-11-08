// JumpAudio: on the Player and Clone prefabs. Plays a take-off sound when the actor's jump starts and a
// footstep as it lands, from ActorJump.Started and Landed. Clones jump on the same tick as their
// recording, so theirs plays too, but quieter.
using GameJam.Actors;
using UnityEngine;

namespace GameJam.Audio
{
    /// <summary>Plays jump and landing sounds for one actor.</summary>
    [RequireComponent(typeof(ActorJump))]
    public class JumpAudio : MonoBehaviour
    {
        [SerializeField] private ActorJump _jump;
        [SerializeField] private SoundCue _takeOff;
        [Tooltip("Played when the jump ends; the footstep cue works well.")]
        [SerializeField] private SoundCue _land;
        [Tooltip("Volume scale for this actor (about 0.35 for clones, like their footsteps).")]
        [Range(0f, 1f)]
        [SerializeField] private float _volumeScale = 1f;

        private void Awake()
        {
            if (_jump == null)
            {
                _jump = GetComponent<ActorJump>();
            }
        }

        private void OnEnable()
        {
            _jump.Started += HandleStarted;
            _jump.Landed += HandleLanded;
        }

        private void OnDisable()
        {
            _jump.Started -= HandleStarted;
            _jump.Landed -= HandleLanded;
        }

        private void HandleStarted()
        {
            AudioManager.Play(_takeOff, _volumeScale);
        }

        private void HandleLanded()
        {
            AudioManager.Play(_land, _volumeScale);
        }
    }
}
