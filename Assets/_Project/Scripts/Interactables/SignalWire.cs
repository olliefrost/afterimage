// SignalWire: shows cause and effect. Send() runs a bright charge front and pulse along the wire's
// LineRenderer path, lighting the wire behind it, then flashes the whole wire and fires OnArrived (wired
// in the inspector, e.g. to Door.Open); once arrived it stays lit with a looping pulse. It uses the same
// look as Visuals/SignalCable (grey off, cyan on), so every wire in the game reads alike. Driven by
// fixed ticks so a clone's press opens the door on exactly the same tick as the original run; the
// flicker, flash and looping pulse are visual only.
using GameJam.Core;
using GameJam.Visuals;
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
        [SerializeField] private Color _idleColor = new Color(0.18f, 0.2f, 0.22f);
        [SerializeField] private Color _poweredColor = new Color(0.35f, 0.85f, 1f);

        [Header("Feedback")]
        [Tooltip("Colour of the travelling front, and of the whole wire's flash when the signal arrives.")]
        [SerializeField] private Color _arrivalFlashColor = new Color(0.85f, 1f, 1f);
        [SerializeField] private float _arrivalFlashTime = 0.35f;
        [Tooltip("How much the travelling pulse flickers in size (0.3 = up to 30%).")]
        [SerializeField] private float _pulseFlicker = 0.3f;
        [Tooltip("Units per second the pulse loops along the wire once the signal has arrived.")]
        [SerializeField] private float _pulseSpeed = 2.5f;
        [Tooltip("Screen shake from the wire's end when the signal arrives.")]
        [SerializeField] private float _arrivalShake = 0.025f;

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
        private Vector3 _pulseScale;
        private float _flashTimeLeft;
        private float _pulseDistance;

        // Reused so lighting the wire each tick does not allocate new key arrays.
        private Gradient _gradient;
        private GradientColorKey[] _colorKeys;
        private GradientAlphaKey[] _alphaKeys;

        private void Awake()
        {
            _line = GetComponent<LineRenderer>();
            _pulseScale = _pulse.localScale;
            _gradient = new Gradient();
            _colorKeys = new GradientColorKey[5];
            _alphaKeys = new[] { new GradientAlphaKey(1f, 0f), new GradientAlphaKey(1f, 1f) };
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
            _flashTimeLeft = 0f;
            _pulse.gameObject.SetActive(false);
            _pulse.localScale = _pulseScale;
            SetLineColor(_idleColor);
        }

        private void FixedUpdate()
        {
            if (!_isSending || !_clock.IsRunning)
            {
                return;
            }

            // 1. Advance the pulse along the path by one tick, lighting the wire behind it.
            _elapsed += Time.fixedDeltaTime;
            float progress = Mathf.Clamp01(_elapsed / _travelTime);
            _pulse.position = PointAlongPath(progress * _totalLength);
            LightUpTo(progress);

            // 2. The pulse crackles: its size jumps a little every tick (visual only).
            float flicker = 1f + _pulseFlicker * (Mathf.PerlinNoise(_elapsed * 40f, 0.5f) - 0.5f) * 2f;
            _pulse.localScale = _pulseScale * flicker;

            // 3. On arrival, flash the whole wire and tell whatever is listening.
            if (progress >= 1f)
            {
                _isSending = false;
                _isPowered = true;
                _pulse.localScale = _pulseScale;
                _pulseDistance = _totalLength;
                _flashTimeLeft = _arrivalFlashTime;
                SetLineColor(_arrivalFlashColor);
                ScreenShake.ShakeAt(_points[_points.Length - 1], _arrivalShake, 0.15f);
                _onArrived.Invoke();
            }
        }

        private void Update()
        {
            if (!_isPowered)
            {
                return;
            }

            // 1. While lit, the pulse keeps looping from the source end, like a powered SignalCable.
            if (_totalLength > 0f)
            {
                _pulseDistance = Mathf.Repeat(_pulseDistance + _pulseSpeed * Time.deltaTime, _totalLength);
                _pulse.position = PointAlongPath(_pulseDistance);
            }

            // 2. The arrival flash fades back to the powered colour over real frames.
            if (_flashTimeLeft > 0f)
            {
                _flashTimeLeft -= Time.deltaTime;
                float t = 1f - Mathf.Clamp01(_flashTimeLeft / _arrivalFlashTime);
                SetLineColor(Color.Lerp(_arrivalFlashColor, _poweredColor, Ease.OutCubic(t)));
            }
        }

        /// <summary>Colours the wire powered up to <paramref name="progress"/> (0-1 of its length), with a bright front at that point, and idle after.</summary>
        private void LightUpTo(float progress)
        {
            float edge = Mathf.Clamp(progress, 0.02f, 0.97f);
            _colorKeys[0] = new GradientColorKey(_poweredColor, 0f);
            _colorKeys[1] = new GradientColorKey(_poweredColor, edge - 0.02f);
            _colorKeys[2] = new GradientColorKey(_arrivalFlashColor, edge);
            _colorKeys[3] = new GradientColorKey(_idleColor, edge + 0.02f);
            _colorKeys[4] = new GradientColorKey(_idleColor, 1f);
            _gradient.SetKeys(_colorKeys, _alphaKeys);
            _line.colorGradient = _gradient;
        }

        /// <summary>Colours the whole wire one colour, reusing the same gradient.</summary>
        private void SetLineColor(Color color)
        {
            for (int i = 0; i < _colorKeys.Length; i++)
            {
                _colorKeys[i] = new GradientColorKey(color, i / (float)(_colorKeys.Length - 1));
            }

            _gradient.SetKeys(_colorKeys, _alphaKeys);
            _line.colorGradient = _gradient;
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
    }
}
