// CameraFollow: keeps the main camera on the player with a little smoothing, never showing past
// the room edges. Snaps instantly when the player teleports (new attempt, falling into the pit).
// IntroSequence points it at other targets (Follow). Every screen shake arrives here through
// ScreenShake (buttons, doors, deaths, the intro slam); shakes that happen off-screen fade
// with distance, and the player's GameSettings shake level scales them all.
// Visual only: nothing in gameplay reads the camera.
using GameJam.Core;
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

        [Header("Shake")]
        [Tooltip("How fast the shake wobbles. Higher is more of a buzz, lower more of a sway.")]
        [SerializeField] private float _shakeFrequency = 28f;
        [Tooltip("A shake this far (units) outside the view is not felt at all; inside the view it is full strength.")]
        [SerializeField] private float _shakeFalloff = 4f;

        private Camera _camera;
        private Vector3 _velocity;
        private bool _isGliding;
        private Vector3 _restPosition;
        private float _shakeTimeLeft;
        private float _shakeDuration;
        private float _shakeStrength;
        private float _shakeSeed;

        private void Awake()
        {
            _camera = GetComponent<Camera>();
        }

        private void OnEnable()
        {
            ScreenShake.Requested += HandleShakeRequested;
        }

        private void OnDisable()
        {
            ScreenShake.Requested -= HandleShakeRequested;
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

        /// <summary>
        /// Shakes the view for a moment, fading out, for impacts like a door slamming. A weaker shake
        /// never cuts a stronger one short: whichever is stronger right now wins.
        /// </summary>
        private void StartShake(float strength, float duration)
        {
            strength *= GameSettings.ShakeScale;
            if (strength <= 0f || strength < CurrentShakeStrength())
            {
                return;
            }

            _shakeDuration = Mathf.Max(duration, 0.01f);
            _shakeTimeLeft = _shakeDuration;
            _shakeStrength = strength;
            _shakeSeed = Random.value * 100f;
        }

        /// <summary>Takes a ScreenShake request, fading it if its source is outside the view.</summary>
        private void HandleShakeRequested(float strength, float duration, Vector2 position, bool positional)
        {
            if (positional)
            {
                strength *= ShakeFalloff(position);
            }

            StartShake(strength, duration);
        }

        /// <summary>1 inside the view, falling to 0 at the falloff distance outside it.</summary>
        private float ShakeFalloff(Vector2 position)
        {
            float halfHeight = _camera.orthographicSize;
            float halfWidth = halfHeight * _camera.aspect;
            Vector2 fromCentre = position - (Vector2)_restPosition;
            float outsideX = Mathf.Max(0f, Mathf.Abs(fromCentre.x) - halfWidth);
            float outsideY = Mathf.Max(0f, Mathf.Abs(fromCentre.y) - halfHeight);
            float outside = Mathf.Sqrt(outsideX * outsideX + outsideY * outsideY);
            return 1f - Mathf.Clamp01(outside / _shakeFalloff);
        }

        /// <summary>The strength the running shake has left after fading, so a weaker request can't cut it short.</summary>
        private float CurrentShakeStrength()
        {
            return _shakeTimeLeft > 0f ? _shakeStrength * ShakeFade() : 0f;
        }

        /// <summary>1 at the start of a shake, easing to 0 at its end (squared, so it dies away smoothly).</summary>
        private float ShakeFade()
        {
            float left = Mathf.Clamp01(_shakeTimeLeft / _shakeDuration);
            return left * left;
        }

        /// <summary>This frame's shake offset: smooth noise rather than random jumps, shrinking as the shake ends.</summary>
        private Vector3 ShakeOffset()
        {
            if (_shakeTimeLeft <= 0f)
            {
                return Vector3.zero;
            }

            _shakeTimeLeft -= Time.unscaledDeltaTime;
            float time = Time.unscaledTime * _shakeFrequency;
            // Perlin noise mostly stays near the middle of 0-1, so stretch it to use the full -1..1 range.
            float x = Mathf.Clamp((Mathf.PerlinNoise(_shakeSeed, time) - 0.5f) * 4f, -1f, 1f);
            float y = Mathf.Clamp((Mathf.PerlinNoise(_shakeSeed + 37f, time) - 0.5f) * 4f, -1f, 1f);
            float size = _shakeStrength * ShakeFade();
            return new Vector3(x * size, y * size, 0f);
        }

        private Vector3 DesiredPosition()
        {
            Vector2 target = (Vector2)_target.position + _offset;
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
