// TitleLogo: brings the "Afterimage" logo on the title screen (Title.unity) to life. It slams in on
// load in a burst of glow, then floats gently, breathes a soft glow behind itself, and every few
// seconds glitches: copies in the clone slot colours jump sideways and fade, like afterimages of the
// logo. Sits on an empty RectTransform and builds its layers (echoes, glow, logo) as children in Awake,
// in that order, since uGUI draws later children on top. Runs on unscaled time and is visual only.
using UnityEngine;
using UnityEngine.UI;

namespace GameJam.UI
{
    /// <summary>Animates the title logo: entrance, float, glow pulse and periodic afterimage glitches.</summary>
    public class TitleLogo : MonoBehaviour
    {
        [Header("Art")]
        [SerializeField] private Sprite _sprite;

        [Header("Entrance")]
        [Tooltip("Seconds before the logo appears, so the backdrop is seen first.")]
        [SerializeField] private float _entranceDelay = 0.3f;
        [Tooltip("Seconds the logo takes to slam down to its rest size.")]
        [SerializeField] private float _entranceSeconds = 0.35f;
        [Tooltip("Scale the logo starts at before slamming down.")]
        [SerializeField] private float _entranceScale = 1.6f;

        [Header("Idle")]
        [Tooltip("How far the logo floats up and down, in canvas pixels.")]
        [SerializeField] private float _floatHeight = 6f;
        [SerializeField] private float _floatSpeed = 1.2f;
        [Tooltip("Glow copy behind the logo: tint, scale and pulse range of its alpha.")]
        [SerializeField] private Color _glowColour = new Color(0.3f, 0.95f, 1f, 1f);
        [SerializeField] private float _glowScale = 1.04f;
        [SerializeField] private Vector2 _glowAlpha = new Vector2(0.15f, 0.4f);
        [SerializeField] private float _glowSpeed = 1.7f;

        [Header("Glitch")]
        [Tooltip("Afterimage copies, in the clone slot colours.")]
        [SerializeField] private Color[] _echoColours =
        {
            new Color(0.25f, 0.55f, 1f), new Color(0.3f, 0.9f, 0.4f), new Color(0.7f, 0.4f, 1f),
        };
        [Tooltip("Random wait between glitches, in seconds.")]
        [SerializeField] private Vector2 _glitchInterval = new Vector2(2.5f, 5f);
        [Tooltip("How long one glitch lasts, in seconds.")]
        [SerializeField] private float _glitchSeconds = 0.35f;
        [Tooltip("Furthest an echo jumps sideways, in canvas pixels.")]
        [SerializeField] private float _glitchOffset = 18f;

        private RectTransform _rect;
        private Image _image;
        private Image _glow;
        private Image[] _echoes;
        private Vector2 _restPosition;
        private float _time;
        private float _nextGlitch;
        private float _glitchTime = -1f;

        private void Awake()
        {
            _rect = (RectTransform)transform;
            _restPosition = _rect.anchoredPosition;

            // Later children draw on top: echoes at the back, then the glow, then the logo itself.
            _echoes = new Image[_echoColours.Length];
            for (int i = 0; i < _echoes.Length; i++)
            {
                _echoes[i] = MakeLayer("Echo " + i);
            }

            _glow = MakeLayer("Glow");
            _glow.color = _glowColour;
            _glow.rectTransform.localScale = Vector3.one * _glowScale;
            _image = MakeLayer("Logo");
        }

        private void OnEnable()
        {
            _time = 0f;
            _glitchTime = -1f;
            _nextGlitch = _entranceDelay + _entranceSeconds + 1f;
            Apply();
        }

        private void Update()
        {
            _time += Time.unscaledDeltaTime;

            if (_glitchTime < 0f && _time >= _nextGlitch)
            {
                _glitchTime = 0f;
            }
            else if (_glitchTime >= 0f)
            {
                _glitchTime += Time.unscaledDeltaTime;
                if (_glitchTime > _glitchSeconds)
                {
                    _glitchTime = -1f;
                    _nextGlitch = _time + Random.Range(_glitchInterval.x, _glitchInterval.y);
                }
            }

            Apply();
        }

        /// <summary>Sets every layer of the logo for this moment.</summary>
        private void Apply()
        {
            // 1. Entrance: hidden, then slam from large to rest.
            float t = Mathf.Clamp01((_time - _entranceDelay) / _entranceSeconds);
            bool shown = _time >= _entranceDelay;
            float slam = 1f - (1f - t) * (1f - t) * (1f - t);
            float scale = Mathf.Lerp(_entranceScale, 1f, slam);
            float alpha = shown ? Mathf.Clamp01(t * 3f) : 0f;

            // 2. Idle float (starts once landed, so the slam stays clean).
            float settled = Mathf.Clamp01((_time - _entranceDelay - _entranceSeconds) * 2f);
            float bob = Mathf.Sin((_time - _entranceDelay) * _floatSpeed) * _floatHeight * settled;
            _rect.anchoredPosition = _restPosition + new Vector2(0f, bob);
            _rect.localScale = Vector3.one * scale;
            _image.color = new Color(1f, 1f, 1f, alpha);

            // A burst of glow as it lands stands in for an impact flash.
            float flash = shown ? Mathf.Clamp01(1f - (_time - _entranceDelay - _entranceSeconds) / 0.4f) : 0f;

            // 3. Glow breathes behind it.
            float pulse = 0.5f + 0.5f * Mathf.Sin(_time * _glowSpeed);
            Color glow = _glowColour;
            glow.a = Mathf.Lerp(_glowAlpha.x, _glowAlpha.y, pulse) * alpha + flash * 0.8f;
            _glow.rectTransform.localScale = Vector3.one * (_glowScale + flash * 0.08f);
            _glow.color = glow;

            // 4. Glitch: each echo jumps to a random side offset a few times and fades out.
            bool glitching = _glitchTime >= 0f;
            float fade = glitching ? 1f - _glitchTime / _glitchSeconds : 0f;
            int step = glitching ? Mathf.FloorToInt(_glitchTime * 20f) : 0;
            for (int i = 0; i < _echoes.Length; i++)
            {
                Color c = _echoColours[i];
                c.a = 0.75f * fade;
                _echoes[i].color = c;
                if (glitching)
                {
                    // Hashing the step keeps each jump fixed for a frame or two, which reads as a glitch.
                    float side = Mathf.Sin((step + 1) * 12.9898f + i * 78.233f) * 43758.5453f % 1f;
                    _echoes[i].rectTransform.anchoredPosition = new Vector2(side * _glitchOffset, (i - 1) * 2f);
                }
            }
        }

        /// <summary>Makes one layer of the logo: a child Image stretched over this rect, starting clear.</summary>
        private Image MakeLayer(string name)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Image));
            var rect = (RectTransform)go.transform;
            rect.SetParent(_rect, false);
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;

            var layer = go.GetComponent<Image>();
            layer.sprite = _sprite;
            layer.preserveAspect = true;
            layer.raycastTarget = false;
            layer.color = Color.clear;
            return layer;
        }
    }
}
