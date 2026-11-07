// SignalWire: shows cause and effect. Send() moves a red pulse along the wire's LineRenderer path,
// then fires OnArrived (wired in the inspector, e.g. to Door.Open). Driven by fixed ticks so a
// clone's press opens the door on exactly the same tick as the original run.
using GameJam.Core;
using UnityEngine;
using UnityEngine.Events;

namespace GameJam.Interactables
{
    /// <summary>A wire that carries a visible signal from a source (button) to a target (door).</summary>
    [RequireComponent(typeof(LineRenderer))]
    public class SignalWire : MonoBehaviour, IResettable
    {
        [Header("References")]
        [SerializeField] private TickClock _clock;
        [Tooltip("Small sprite that travels along the wire while the signal is moving.")]
        [SerializeField] private Transform _pulse;

        [Header("Tuning")]
        [Tooltip("Seconds the signal takes to travel the whole wire.")]
        [SerializeField] private float _travelTime = 1.2f;
        [SerializeField] private Color _idleColor = new Color(0.35f, 0.08f, 0.08f);
        [SerializeField] private Color _poweredColor = new Color(1f, 0.15f, 0.15f);

        [Header("Events")]
        [Tooltip("Fired when the signal reaches the end of the wire.")]
        [SerializeField] private UnityEvent _onArrived = new UnityEvent();

        private LineRenderer _line;
        private Vector3[] _points;
        private float[] _cumulativeLengths;
        private float _totalLength;
        private float _elapsed;
        private bool _isSending;
        private bool _isPowered;

        private void Awake()
        {
            _line = GetComponent<LineRenderer>();
            CachePath();
        }

        private void OnEnable()
        {
            LevelReset.Register(this);
            ResetState();
        }

        private void OnDisable()
        {
            LevelReset.Unregister(this);
        }

        /// <summary>Starts the signal travelling from the first point of the wire. Ignored if already sent.</summary>
        public void Send()
        {
            if (_isSending || _isPowered)
            {
                return;
            }

            _isSending = true;
            _elapsed = 0f;
            _pulse.gameObject.SetActive(true);
            _pulse.position = _points[0];
        }

        /// <summary>Turns the wire off and hides the pulse for the next attempt.</summary>
        public void ResetState()
        {
            _isSending = false;
            _isPowered = false;
            _elapsed = 0f;
            _pulse.gameObject.SetActive(false);
            SetLineColor(_idleColor);
        }

        private void FixedUpdate()
        {
            if (!_isSending || !_clock.IsRunning)
            {
                return;
            }

            // 1. Advance the pulse along the path by one tick.
            _elapsed += Time.fixedDeltaTime;
            float progress = Mathf.Clamp01(_elapsed / _travelTime);
            _pulse.position = PointAlongPath(progress * _totalLength);

            // 2. On arrival, light the whole wire and tell whatever is listening.
            if (progress >= 1f)
            {
                _isSending = false;
                _isPowered = true;
                _pulse.gameObject.SetActive(false);
                SetLineColor(_poweredColor);
                _onArrived.Invoke();
            }
        }

        /// <summary>Reads the wire's points (world space) and precomputes segment lengths for even pulse speed.</summary>
        private void CachePath()
        {
            _points = new Vector3[_line.positionCount];
            _line.GetPositions(_points);
            if (!_line.useWorldSpace)
            {
                for (int i = 0; i < _points.Length; i++)
                {
                    _points[i] = transform.TransformPoint(_points[i]);
                }
            }

            _cumulativeLengths = new float[_points.Length];
            for (int i = 1; i < _points.Length; i++)
            {
                _cumulativeLengths[i] = _cumulativeLengths[i - 1] + Vector3.Distance(_points[i - 1], _points[i]);
            }

            _totalLength = _cumulativeLengths[_points.Length - 1];
        }

        /// <summary>Returns the world point at <paramref name="distance"/> along the wire.</summary>
        private Vector3 PointAlongPath(float distance)
        {
            for (int i = 1; i < _points.Length; i++)
            {
                if (distance <= _cumulativeLengths[i])
                {
                    float segmentLength = _cumulativeLengths[i] - _cumulativeLengths[i - 1];
                    float t = segmentLength > 0f ? (distance - _cumulativeLengths[i - 1]) / segmentLength : 1f;
                    return Vector3.Lerp(_points[i - 1], _points[i], t);
                }
            }

            return _points[_points.Length - 1];
        }

        private void SetLineColor(Color color)
        {
            _line.startColor = color;
            _line.endColor = color;
        }
    }
}
