// TitleGhost: one clone ghost wandering the title screen's backdrop chamber (Title.unity). It walks a
// looped route, pauses, then rewinds back along it at speed, facing forward and leaving a trail of
// afterimages, just like a clone's run between attempts in the game. Moves its own transform, so the
// character art on its Body/Visual child (CharacterSpriteAnimator) picks the facing and run frames.
// Visual only: no recording or gameplay is involved.
using UnityEngine;

namespace GameJam.Visuals
{
    /// <summary>Walks a waypoint route, then rewinds it with a ghost trail, forever.</summary>
    public class TitleGhost : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private CharacterSpriteAnimator _animator;
        [Tooltip("The character sprite the trail copies.")]
        [SerializeField] private SpriteRenderer _sprite;
        [SerializeField] private GhostTrail _trail;

        [Header("Route")]
        [Tooltip("World points walked in order; the ghost starts at the first.")]
        [SerializeField] private Vector2[] _route;
        [SerializeField] private float _walkSpeed = 1.6f;
        [Tooltip("The rewind runs this many times faster than the walk.")]
        [SerializeField] private float _rewindSpeedUp = 4f;
        [Tooltip("Seconds to stand still at the end of the walk, and after the rewind.")]
        [SerializeField] private float _pauseSeconds = 1.2f;
        [Tooltip("Seconds before this ghost first moves, so several ghosts fall out of step.")]
        [SerializeField] private float _startDelay;

        [Header("Trail")]
        [SerializeField] private Color _colour = new Color(0.3f, 0.6f, 1f, 0.6f);
        [Tooltip("Seconds between trail copies while rewinding.")]
        [SerializeField] private float _stampEvery = 0.04f;
        [SerializeField] private float _trailLife = 0.5f;

        private float _distance;
        private float _length;
        private float _wait;
        private float _stampTimer;
        private bool _rewinding;

        private void Start()
        {
            if (_route.Length < 2)
            {
                Debug.LogWarning($"{name}: TitleGhost needs at least two route points.", this);
                enabled = false;
                return;
            }

            for (int i = 1; i < _route.Length; i++)
            {
                _length += Vector2.Distance(_route[i - 1], _route[i]);
            }

            _wait = _startDelay;
            transform.position = _route[0];
        }

        private void Update()
        {
            // 1. Stand still while paused.
            if (_wait > 0f)
            {
                _wait -= Time.deltaTime;
                return;
            }

            // 2. Walk forward, or rewind back to the start faster.
            float speed = _rewinding ? -_walkSpeed * _rewindSpeedUp : _walkSpeed;
            _distance = Mathf.Clamp(_distance + speed * Time.deltaTime, 0f, _length);
            transform.position = PointAt(_distance);

            // 3. While rewinding, face forward and leave afterimages behind.
            _animator.FaceAgainstMotion = _rewinding;
            if (_rewinding)
            {
                _stampTimer -= Time.deltaTime;
                if (_stampTimer <= 0f)
                {
                    _stampTimer = _stampEvery;
                    _trail.Spawn(_sprite, _colour, _trailLife);
                }
            }

            // 4. Turn around at either end of the route.
            if (!_rewinding && _distance >= _length)
            {
                _rewinding = true;
                _wait = _pauseSeconds;
            }
            else if (_rewinding && _distance <= 0f)
            {
                _rewinding = false;
                _wait = _pauseSeconds;
            }
        }

        /// <summary>The point <paramref name="distance"/> along the route.</summary>
        private Vector2 PointAt(float distance)
        {
            for (int i = 1; i < _route.Length; i++)
            {
                float leg = Vector2.Distance(_route[i - 1], _route[i]);
                if (distance <= leg)
                {
                    return Vector2.Lerp(_route[i - 1], _route[i], leg > 0f ? distance / leg : 1f);
                }

                distance -= leg;
            }

            return _route[_route.Length - 1];
        }
    }
}
