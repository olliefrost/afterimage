// FootstepAudio: on a character's Visual child (next to CharacterSpriteAnimator), on both the
// Player and Clone prefabs. Plays a footstep cue each time the animator reports a foot contact.
// The Clone prefab uses a lower volume scale so clones read as quiet echoes of the player.
using GameJam.Visuals;
using UnityEngine;

namespace GameJam.Audio
{
    /// <summary>Plays a footstep sound on each step the character animator reports.</summary>
    [RequireComponent(typeof(CharacterSpriteAnimator))]
    public class FootstepAudio : MonoBehaviour
    {
        [SerializeField] private CharacterSpriteAnimator _animator;
        [SerializeField] private SoundCue _step;
        [Tooltip("Player 1, clones lower (try 0.35).")]
        [Range(0f, 1f)]
        [SerializeField] private float _volumeScale = 1f;

        private void Awake()
        {
            if (_animator == null)
            {
                _animator = GetComponent<CharacterSpriteAnimator>();
            }
        }

        private void OnEnable()
        {
            _animator.Stepped += HandleStepped;
        }

        private void OnDisable()
        {
            _animator.Stepped -= HandleStepped;
        }

        private void HandleStepped()
        {
            AudioManager.Play(_step, _volumeScale);
        }
    }
}
