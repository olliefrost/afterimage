// SignalCable: a floor cable from a source (generator, plate) to what it drives. When the source
// turns on, a bright charge front races along the cable from the source end; while on it glows and
// runs a small pulse along its path; when it turns off it flickers out. It can light up extra
// sprites at its end (such as a stair stop marker), so the player sees what powers what. Visual only.
using GameJam.Core;
using UnityEngine;

namespace GameJam.Visuals
{
    /// <summary>Lights a LineRenderer path while its ISignalSource is on.</summary>
    [RequireComponent(typeof(LineRenderer))]
    public class SignalCable : MonoBehaviour
    {
        [Header("References")]
        [Tooltip("A component that implements ISignalSource, e.g. PowerGenerator or HoldPlate.")]
        [SerializeField] private MonoBehaviour _source;
        [Tooltip("Optional sprite that travels along the cable while it is on.")]
        [SerializeField] private Transform _pulse;
        [Tooltip("Optional sprites at the far end that light up with the cable (e.g. a stop marker).")]
        [SerializeField] private SpriteRenderer[] _endLights;

        [Header("Look")]
        [SerializeField] private Color _offColour = new Color(0.18f, 0.2f, 0.22f);
        [SerializeField] private Color _onColour = new Color(0.35f, 0.85f, 1f);
        [SerializeField] private Color _endOffColour = new Color(1f, 1f, 1f, 0.15f);
        [SerializeField] private Color _endOnColour = new Color(0.4f, 0.9f, 1f, 0.8f);
        [Tooltip("Units per second the pulse travels while the cable is on.")]
        [SerializeField] private float _pulseSpeed = 2.5f;

        [Header("Switching")]
        [Tooltip("Seconds the charge front takes to run the whole cable when it turns on.")]
        [SerializeField] private float _chargeTime = 0.3f;
        [Tooltip("Colour of the charge front, brighter than the on colour.")]
        [SerializeField] private Color _chargeColour = new Color(0.85f, 1f, 1f);
        [Tooltip("Seconds the cable flickers before going dark when it turns off.")]
        [SerializeField] private float _flickerTime = 0.25f;
        [Tooltip("Flickers per second while turning off.")]
        [SerializeField] private float _flickerRate = 14f;

        private ISignalSource _signal;
        private float _switchElapsed = -1f;

        // Reused so animating the cable does not allocate new key arrays.
        private Gradient _gradient;
        private GradientColorKey[] _colourKeys;
        private GradientAlphaKey[] _alphaKeys;
        private LineRenderer _line;
        private Vector3[] _points;
        private float[] _lengths;
        private float _totalLength;
        private float _pulseDistance;
        private bool _wasOn;

        private void Awake()
        {
            _signal = _source as ISignalSource;
            if (_signal == null)
            {
                Debug.LogError($"{name}: SignalCable source must implement ISignalSource.", this);
            }

            _line = GetComponent<LineRenderer>();
            _gradient = new Gradient();
            _colourKeys = new GradientColorKey[5];
            _alphaKeys = new[] { new GradientAlphaKey(1f, 0f), new GradientAlphaKey(1f, 1f) };
            CachePath();
            Apply(false);
            SetSolid(_offColour);
        }

        private void Update()
        {
            bool on = _signal != null && _signal.IsOn;
            if (on != _wasOn)
            {
                Apply(on);
                _switchElapsed = 0f;
            }

            AnimateSwitch(on);

            // The pulse loops along the cable while it is on, like Tutorial1's signal wire.
            if (on && _pulse != null && _totalLength > 0f)
            {
                _pulseDistance = Mathf.Repeat(_pulseDistance + _pulseSpeed * Time.deltaTime, _totalLength);
                _pulse.position = PointAlong(_pulseDistance);
            }
        }

        /// <summary>
        /// Plays the switch animation after a change: a bright front running along the cable when it
        /// turns on, a few flickers when it turns off, then the plain on or off colour.
        /// </summary>
        private void AnimateSwitch(bool on)
        {
            if (_switchElapsed < 0f)
            {
                return;
            }

            _switchElapsed += Time.deltaTime;
            float duration = on ? _chargeTime : _flickerTime;
            if (_switchElapsed >= duration)
            {
                _switchElapsed = -1f;
                SetSolid(on ? _onColour : _offColour);
                return;
            }

            float t = _switchElapsed / duration;
            if (on)
            {
                SetChargeFront(t);
            }
            else
            {
                bool lit = Mathf.Repeat(_switchElapsed * _flickerRate, 1f) < 0.5f;
                SetSolid(lit ? Color.Lerp(_offColour, _onColour, 1f - t) : _offColour);
            }
        }

        /// <summary>Lit behind the front, a bright band at <paramref name="front"/> (0-1 along the cable), off ahead of it.</summary>
        private void SetChargeFront(float front)
        {
            float edge = Mathf.Clamp(front, 0.02f, 0.97f);
            _colourKeys[0] = new GradientColorKey(_onColour, 0f);
            _colourKeys[1] = new GradientColorKey(_onColour, edge - 0.02f);
            _colourKeys[2] = new GradientColorKey(_chargeColour, edge);
            _colourKeys[3] = new GradientColorKey(_offColour, edge + 0.02f);
            _colourKeys[4] = new GradientColorKey(_offColour, 1f);
            _gradient.SetKeys(_colourKeys, _alphaKeys);
            _line.colorGradient = _gradient;
        }

        /// <summary>Colours the whole cable one colour, reusing the same gradient.</summary>
        private void SetSolid(Color colour)
        {
            for (int i = 0; i < _colourKeys.Length; i++)
            {
                _colourKeys[i] = new GradientColorKey(colour, i / (float)(_colourKeys.Length - 1));
            }

            _gradient.SetKeys(_colourKeys, _alphaKeys);
            _line.colorGradient = _gradient;
        }

        /// <summary>Recolours the end sprites and shows the pulse only while on; the cable itself is animated by AnimateSwitch.</summary>
        private void Apply(bool on)
        {
            _wasOn = on;
            if (_pulse != null)
            {
                _pulse.gameObject.SetActive(on);
            }

            if (_endLights != null)
            {
                for (int i = 0; i < _endLights.Length; i++)
                {
                    _endLights[i].color = on ? _endOnColour : _endOffColour;
                }
            }
        }

        /// <summary>Reads the cable's points (world space) and their running lengths for the pulse.</summary>
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

            _lengths = new float[_points.Length];
            for (int i = 1; i < _points.Length; i++)
            {
                _lengths[i] = _lengths[i - 1] + Vector3.Distance(_points[i - 1], _points[i]);
            }

            _totalLength = _points.Length > 0 ? _lengths[_points.Length - 1] : 0f;
        }

        /// <summary>The world point <paramref name="distance"/> units along the cable, for placing the pulse.</summary>
        private Vector3 PointAlong(float distance)
        {
            for (int i = 1; i < _points.Length; i++)
            {
                if (distance <= _lengths[i])
                {
                    float segment = _lengths[i] - _lengths[i - 1];
                    float t = segment > 0f ? (distance - _lengths[i - 1]) / segment : 1f;
                    return Vector3.Lerp(_points[i - 1], _points[i], t);
                }
            }

            return _points[_points.Length - 1];
        }
    }
}
