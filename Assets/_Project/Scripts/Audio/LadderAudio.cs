// LadderAudio: on the Player and Clone prefabs. Plays a rung clank every so often while the actor
// climbs. It measures how far the actor moved along the ladder each tick, so it stays silent when
// they hang still, and works for clones because their replayed positions already follow the ladder.
using GameJam.Actors;
using UnityEngine;

namespace GameJam.Audio
{
    /// <summary>Plays rung sounds as an actor climbs a ladder.</summary>
    [RequireComponent(typeof(LadderClimber))]
    public class LadderAudio : MonoBehaviour
    {
        [SerializeField] private LadderClimber _climber;
        [SerializeField] private SoundCue _rung;
        [Tooltip("Distance climbed between two rung sounds, in units.")]
        [Min(0.05f)]
        [SerializeField] private float _stride = 0.4f;
        [Tooltip("Volume scale for this actor (about 0.5 for clones).")]
        [Range(0f, 1f)]
        [SerializeField] private float _volumeScale = 1f;

        private Vector2 _lastPosition;
        private float _climbed;

        private void Awake()
        {
            if (_climber == null)
            {
                _climber = GetComponent<LadderClimber>();
            }
        }

        private void OnEnable()
        {
            _lastPosition = transform.position;
            _climbed = 0f;
        }

        private void FixedUpdate()
        {
            Vector2 position = transform.position;
            float moved = Vector2.Distance(position, _lastPosition);
            _lastPosition = position;

            // 1. Only count movement made on the ladder; a teleport (reset) would otherwise count as climbing.
            if (!_climber.IsClimbing || moved > 1f)
            {
                _climbed = 0f;
                return;
            }

            // 2. Play a rung each time the stride is covered.
            _climbed += moved;
            if (_climbed >= _stride)
            {
                _climbed = 0f;
                AudioManager.Play(_rung, _volumeScale);
            }
        }
    }
}
