// ScannerVisuals: brings Chamber 4's scanning chamber to life. Its lamp is dim while the chamber is
// shut, bright while the entry door's window is open (flashing a warning colour near the end), and
// green once a scan is done. While someone is being scanned, a light ring sweeps up and down the
// tube, and the booth turns see-through while anyone is inside so they stay visible behind the glass.
// Visual only: Scanner and TimedDoor decide everything.
using GameJam.Machines;
using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace GameJam.Visuals
{
    /// <summary>Drives the scanning chamber's lamp and sweep ring from the scanner and its timed door.</summary>
    public class ScannerVisuals : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Scanner _scanner;
        [Tooltip("The entry door's timer; its last stretch flashes the warning colour.")]
        [SerializeField] private TimedDoor _entryDoor;
        [Tooltip("The lamp inside the chamber.")]
        [SerializeField] private Light2D _lamp;
        [Tooltip("Ring sprite that sweeps up and down the tube while a scan runs.")]
        [SerializeField] private SpriteRenderer _sweep;
        [Tooltip("The booth art while shut (its door also hides it while open).")]
        [SerializeField] private SpriteRenderer _closedArt;
        [Tooltip("The booth art while open.")]
        [SerializeField] private SpriteRenderer _openArt;

        [Header("Colours")]
        [SerializeField] private Color _idleColour = new Color(0.35f, 0.6f, 0.7f);
        [SerializeField] private Color _openColour = new Color(0.45f, 0.9f, 1f);
        [SerializeField] private Color _warningColour = new Color(1f, 0.45f, 0.2f);
        [SerializeField] private Color _doneColour = new Color(0.45f, 1f, 0.55f);

        [Header("Tuning")]
        [Tooltip("Lamp intensity while shut and while open or scanning.")]
        [SerializeField] private Vector2 _lampIntensity = new Vector2(0.2f, 1f);
        [Tooltip("How far through the open window the warning flashing starts (0 to 1).")]
        [Range(0f, 1f)]
        [SerializeField] private float _warningFrom = 0.7f;
        [Tooltip("Warning flashes per second.")]
        [SerializeField] private float _warningRate = 6f;
        [Tooltip("Lowest and highest point of the sweep ring, relative to where it was placed.")]
        [SerializeField] private Vector2 _sweepRange = new Vector2(0f, 0.9f);
        [Tooltip("Sweeps (bottom to top and back) per second.")]
        [SerializeField] private float _sweepRate = 2f;
        [Tooltip("Booth opacity while someone is inside.")]
        [Range(0f, 1f)]
        [SerializeField] private float _occupiedAlpha = 0.25f;
        [Tooltip("Seconds the booth takes to fade in or out.")]
        [SerializeField] private float _fadeSeconds = 0.25f;

        private Vector3 _sweepRest;
        private float _boothAlpha = 1f;

        private void Awake()
        {
            _sweepRest = _sweep.transform.localPosition;
        }

        private void LateUpdate()
        {
            // 1. Lamp colour and brightness from the chamber's state.
            Color colour = _idleColour;
            float intensity = _lampIntensity.x;
            if (_scanner.IsScanned)
            {
                colour = _doneColour;
                intensity = _lampIntensity.y;
            }
            else if (_entryDoor.IsOpen || _scanner.ScanProgress > 0f)
            {
                bool warn = _entryDoor.IsOpen && _entryDoor.OpenProgress >= _warningFrom
                    && Mathf.Repeat(Time.time * _warningRate, 1f) < 0.5f;
                colour = warn ? _warningColour : _openColour;
                intensity = _lampIntensity.y * (1f + _scanner.ScanProgress);
            }

            _lamp.color = colour;
            _lamp.intensity = intensity;

            // 2. The sweep ring only shows while a scan is running.
            bool scanning = !_scanner.IsScanned && _scanner.ScanProgress > 0f;
            _sweep.enabled = scanning;
            if (scanning)
            {
                float t = Mathf.PingPong(Time.time * _sweepRate * 2f, 1f);
                _sweep.transform.localPosition = _sweepRest + new Vector3(0f, Mathf.Lerp(_sweepRange.x, _sweepRange.y, t), 0f);
            }

            // 3. See-through while occupied. The shut art stays hidden while the door is open.
            float target = _scanner.IsOccupied ? _occupiedAlpha : 1f;
            _boothAlpha = Mathf.MoveTowards(_boothAlpha, target, Time.deltaTime / Mathf.Max(_fadeSeconds, 0.01f) * (1f - _occupiedAlpha));
            SetAlpha(_openArt, _boothAlpha);
            SetAlpha(_closedArt, _entryDoor.IsOpen ? 0f : _boothAlpha);
        }

        /// <summary>Sets one sprite's opacity, if it is wired.</summary>
        private static void SetAlpha(SpriteRenderer sprite, float alpha)
        {
            if (sprite != null)
            {
                Color c = sprite.color;
                c.a = alpha;
                sprite.color = c;
            }
        }
    }
}
