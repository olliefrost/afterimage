// SignalCable: a floor cable from a source (generator, plate) to what it drives. It glows and runs a
// small pulse along its path while the source is on, and can light up extra sprites at its end
// (such as a stair stop marker), so the player sees what powers what. Visual only.
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

        private ISignalSource _signal;
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
            CachePath();
            Apply(false);
        }

        private void Update()
        {
            bool on = _signal != null && _signal.IsOn;
            if (on != _wasOn)
            {
                Apply(on);
            }

            // The pulse loops along the cable while it is on, like Tutorial1's signal wire.
            if (on && _pulse != null && _totalLength > 0f)
            {
                _pulseDistance = Mathf.Repeat(_pulseDistance + _pulseSpeed * Time.deltaTime, _totalLength);
                _pulse.position = PointAlong(_pulseDistance);
            }
        }

        /// <summary>Recolours the cable and its end sprites, and shows the pulse only while on.</summary>
        private void Apply(bool on)
        {
            _wasOn = on;
            _line.startColor = on ? _onColour : _offColour;
            _line.endColor = on ? _onColour : _offColour;
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
