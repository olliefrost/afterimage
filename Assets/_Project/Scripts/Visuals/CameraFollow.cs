// CameraFollow: keeps the main camera on the player with a little smoothing, never showing past
// the room edges. It follows the target's drawn height on raised ground (ElevationField), so the
// player stays on screen up on ledges. Snaps instantly when the player teleports (new attempt, falling into the pit).
// IntroSequence points it at other targets (Follow) and shakes it (Shake) during the intro.
// Visual only: nothing in gameplay reads the camera.
using UnityEngine;

namespace GameJam.Visuals
{
    /// <summary>Smoothly follows a target, clamps the view to a room-bounds box, and can shake.</summary>
    [RequireComponent(typeof(Camera))]
    public class CameraFollow : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Transform _target;
        [Tooltip("Box covering everything the camera may show. The view never leaves it.")]
        [SerializeField] private BoxCollider2D _roomBounds;

        [Header("Tuning")]
        [Tooltip("Seconds to catch up with the target. Lower is snappier.")]
        [SerializeField] private float _smoothTime = 0.15f;
        [Tooltip("Offset from the target's feet, so the character sits in the middle of the screen.")]
        [SerializeField] private Vector2 _offset = new Vector2(0f, 0.5f);
        [Tooltip("If the target is further than this from the camera, jump instead of gliding (teleports).")]
        [SerializeField] private float _snapDistance = 3f;
        [Tooltip("Seconds to glide to a new target set with Follow(target, false), however far away it is.")]
        [SerializeField] private float _glideTime = 0.6f;

        private Camera _camera;
        private Vector3 _velocity;
        private bool _isGliding;
        private Vector3 _restPosition;
        private float _shakeTimeLeft;
        private float _shakeDuration;
        private float _shakeStrength;

        private void Awake()
        {
            _camera = GetComponent<Camera>();
        }

        private void Start()
        {
            _restPosition = ClampToRoom(DesiredPosition());
            transform.position = _restPosition;
        }

        private void LateUpdate()
        {
            if (_target == null)
            {
                return;
            }

            // 1. Glide toward the target, or jump straight there after a teleport.
            Vector3 desired = DesiredPosition();
            if (_isGliding)
            {
                _restPosition = Vector3.SmoothDamp(_restPosition, desired, ref _velocity, _glideTime);
                _isGliding = Vector2.Distance(_restPosition, desired) > 0.02f;
            }
            else if (Vector2.Distance(_restPosition, desired) > _snapDistance)
            {
                _restPosition = desired;
                _velocity = Vector3.zero;
            }
            else
            {
                _restPosition = Vector3.SmoothDamp(_restPosition, desired, ref _velocity, _smoothTime);
            }

            // 2. Keep the whole view inside the room, then add any shake on top.
            _restPosition = ClampToRoom(_restPosition);
            transform.position = _restPosition + ShakeOffset();
        }

        /// <summary>
        /// Switches the camera to a new target. With <paramref name="snap"/> it jumps there this frame;
        /// otherwise it glides there over the glide time, even across the whole room.
        /// </summary>
        public void Follow(Transform target, bool snap)
        {
            _target = target;
            _velocity = Vector3.zero;
            _isGliding = !snap;
            if (snap)
            {
                _restPosition = ClampToRoom(DesiredPosition());
                transform.position = _restPosition;
            }
        }

        /// <summary>Shakes the view for a moment, fading out, for impacts like a door slamming.</summary>
        public void Shake(float duration, float strength)
        {
            _shakeDuration = Mathf.Max(duration, 0.01f);
            _shakeTimeLeft = _shakeDuration;
            _shakeStrength = strength;
        }

        /// <summary>This frame's random shake offset, shrinking to nothing as the shake ends.</summary>
        private Vector3 ShakeOffset()
        {
            if (_shakeTimeLeft <= 0f)
            {
                return Vector3.zero;
            }

            _shakeTimeLeft -= Time.unscaledDeltaTime;
            Vector2 offset = Random.insideUnitCircle * _shakeStrength * Mathf.Clamp01(_shakeTimeLeft / _shakeDuration);
            return new Vector3(offset.x, offset.y, 0f);
        }

        /// <summary>The camera position that centres the target as drawn.</summary>
        private Vector3 DesiredPosition()
        {
            // Aim at where the target is drawn: its floor-plan position lifted by the ground height there.
            Vector2 target = (Vector2)_target.position + _offset + new Vector2(0f, ElevationField.HeightAt(_target.position));
            return new Vector3(target.x, target.y, transform.position.z);
        }

        /// <summary>Moves a camera position so the visible rectangle stays inside the room bounds.</summary>
        private Vector3 ClampToRoom(Vector3 position)
        {
            if (_roomBounds == null)
            {
                return position;
            }

            Bounds room = _roomBounds.bounds;
            float halfHeight = _camera.orthographicSize;
            float halfWidth = halfHeight * _camera.aspect;

            position.x = ClampAxis(position.x, room.min.x + halfWidth, room.max.x - halfWidth, room.center.x);
            position.y = ClampAxis(position.y, room.min.y + halfHeight, room.max.y - halfHeight, room.center.y);
            return position;
        }

        /// <summary>Clamps one axis, centring on the room if the room is smaller than the view.</summary>
        private static float ClampAxis(float value, float min, float max, float centre)
        {
            return min > max ? centre : Mathf.Clamp(value, min, max);
        }
    }
}
