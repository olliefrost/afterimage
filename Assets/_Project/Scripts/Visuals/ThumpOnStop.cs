// ThumpOnStop: watches something that moves (Chamber 3's hydraulic block, the hover stairs and their
// steps) and fires On Stopped when it comes to rest after moving, so a ShakeOnEvent or PunchScale can
// make the stop land with a thump. Jumps (a level reset teleporting it home) and freezes (the clock
// stopping for R, T or a death) are ignored. Visual only: it reads positions, never moves anything.
using GameJam.Core;
using UnityEngine;
using UnityEngine.Events;

namespace GameJam.Visuals
{
    /// <summary>Raises an event when a watched transform stops after moving.</summary>
    public class ThumpOnStop : MonoBehaviour
    {
        [Header("References")]
        [Tooltip("What to watch. Empty = this object.")]
        [SerializeField] private Transform _watched;
        [Tooltip("The chamber's clock. Movement frozen by the clock stopping (R, T, a death) is not a real stop.")]
        [SerializeField] private TickClock _clock;

        [Header("Detection")]
        [Tooltip("Slower than this (units per second) counts as still.")]
        [SerializeField] private float _stillSpeed = 0.05f;
        [Tooltip("Seconds it must stay still before the stop counts (movement runs on fixed ticks, so frames can repeat a position).")]
        [SerializeField] private float _stillTime = 0.06f;
        [Tooltip("Moving further than this in one frame is a teleport, not movement.")]
        [SerializeField] private float _jumpDistance = 0.5f;

        [Header("Events")]
        [SerializeField] private UnityEvent _onStopped = new UnityEvent();

        private Vector3 _lastPosition;
        private bool _isMoving;
        private float _stillFor;

        private void Awake()
        {
            if (_watched == null)
            {
                _watched = transform;
            }
        }

        private void OnEnable()
        {
            _lastPosition = _watched.position;
            _isMoving = false;
        }

        private void LateUpdate()
        {
            // Paused (time scale 0): nothing moves, nothing to judge.
            if (Time.deltaTime <= 0f)
            {
                return;
            }

            Vector3 position = _watched.position;
            float step = Vector3.Distance(position, _lastPosition);
            _lastPosition = position;

            // 1. The clock stopping freezes everything mid-move: forget the movement, no thump.
            if (_clock != null && !_clock.IsRunning)
            {
                _isMoving = false;
                return;
            }

            // 2. A teleport cancels any movement in progress without a thump.
            if (step > _jumpDistance)
            {
                _isMoving = false;
                return;
            }

            // 3. Moving: remember it. Still for long enough after moving: that's the stop.
            if (step / Time.deltaTime > _stillSpeed)
            {
                _isMoving = true;
                _stillFor = 0f;
                return;
            }

            _stillFor += Time.deltaTime;
            if (_isMoving && _stillFor >= _stillTime)
            {
                _isMoving = false;
                _onStopped.Invoke();
            }
        }
    }
}
