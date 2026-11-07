// CameraFollow: keeps the main camera on the player with a little smoothing, never showing past
// the room edges. Snaps instantly when the player teleports (new attempt, falling into the pit).
// Visual only: nothing in gameplay reads the camera.
using UnityEngine;

namespace GameJam.Visuals
{
    /// <summary>Smoothly follows a target and clamps the view to a room-bounds box.</summary>
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

        private Camera _camera;
        private Vector3 _velocity;

        private void Awake()
        {
            _camera = GetComponent<Camera>();
        }

        private void Start()
        {
            transform.position = ClampToRoom(DesiredPosition());
        }

        private void LateUpdate()
        {
            if (_target == null)
            {
                return;
            }

            // 1. Glide toward the target, or jump straight there after a teleport.
            Vector3 desired = DesiredPosition();
            Vector3 next;
            if (Vector2.Distance(transform.position, desired) > _snapDistance)
            {
                next = desired;
                _velocity = Vector3.zero;
            }
            else
            {
                next = Vector3.SmoothDamp(transform.position, desired, ref _velocity, _smoothTime);
            }

            // 2. Keep the whole view inside the room.
            transform.position = ClampToRoom(next);
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
