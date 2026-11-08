// ScreenTransition: the black wipe that covers every scene change (ScreenTransition prefab, one in
// the title and in every chamber). SceneFlow asks it to cover the screen: a black band with soft
// edges sweeps in from the left, an optional caption ("CHAMBER COMPLETED") fades in and out on the
// black, then SceneFlow loads the next scene. The new scene's instance starts covered and sweeps the
// band off to the right, revealing the chamber with the player already at spawn.
// Visual only, and it runs in real time so it still plays from the paused menu (time scale 0).
using System.Collections;
using GameJam.Core;
using UnityEngine;
using UnityEngine.UI;

namespace GameJam.UI
{
    /// <summary>Wipes the screen to black (with an optional caption) before a scene loads, and off again after.</summary>
    public class ScreenTransition : MonoBehaviour, ISceneTransition
    {
        [Header("UI references")]
        [Tooltip("The black band. Its anchors are moved across the screen; it is exactly one screen wide.")]
        [SerializeField] private RectTransform _wipe;
        [Tooltip("Soft edge on the band's left side (fades from clear to black).")]
        [SerializeField] private Image _leftEdge;
        [Tooltip("Soft edge on the band's right side (fades from black to clear).")]
        [SerializeField] private Image _rightEdge;
        [Tooltip("Text shown on the black between the wipes.")]
        [SerializeField] private Text _caption;
        [Tooltip("Invisible full-screen graphic that swallows clicks while a transition runs.")]
        [SerializeField] private Graphic _blocker;

        [Header("Wipe")]
        [Tooltip("Seconds for the band to sweep on or off the screen.")]
        [SerializeField] private float _wipeTime = 0.5f;
        [Tooltip("Width of each soft edge in reference pixels (1920x1080).")]
        [SerializeField] private float _edgeWidth = 240f;
        [Tooltip("Seconds the new scene stays black before the band sweeps off, so it settles first.")]
        [SerializeField] private float _arrivalHold = 0.15f;

        [Header("Caption")]
        [SerializeField] private float _captionFadeIn = 0.3f;
        [SerializeField] private float _captionHold = 1.2f;
        [SerializeField] private float _captionFadeOut = 0.3f;

        // Longest frame step used for the animation, so the hitch of a scene load cannot skip the wipe.
        private const float MaxStep = 1f / 30f;

        // The soft-edge ramp, built once and shared by every scene's transition.
        private static Sprite _ramp;

        /// <inheritdoc/>
        public bool IsBusy { get; private set; }

        private void Awake()
        {
            // 1. Soft edges: one generated alpha ramp, mirrored for the right edge.
            if (_ramp == null)
            {
                _ramp = CreateRampSprite();
            }

            _leftEdge.sprite = _ramp;
            _rightEdge.sprite = _ramp;
            _rightEdge.rectTransform.localScale = new Vector3(-1f, 1f, 1f);
            SetEdgeWidth(_leftEdge.rectTransform, -1f);
            SetEdgeWidth(_rightEdge.rectTransform, 1f);
            SetCaptionAlpha(0f);

            // 2. Arriving from a transition: start covered and sweep off; otherwise start clear.
            bool arriving = SceneFlow.ConsumeArrivingBlack();
            SetBand(arriving ? 0f : -1f);
            SetBusy(arriving);
            if (arriving)
            {
                StartCoroutine(WipeOff());
            }
        }

        private void OnEnable()
        {
            SceneFlow.Transition = this;
        }

        private void OnDisable()
        {
            if (ReferenceEquals(SceneFlow.Transition, this))
            {
                SceneFlow.Transition = null;
            }
        }

        /// <inheritdoc/>
        public void Play(int buildIndex, string caption)
        {
            if (IsBusy)
            {
                return;
            }

            SetBusy(true);
            StartCoroutine(WipeOnAndLoad(buildIndex, caption));
        }

        /// <summary>Sweeps the band on from the left, shows the caption on the black, then loads the scene.</summary>
        private IEnumerator WipeOnAndLoad(int buildIndex, string caption)
        {
            // 1. Band sweeps in from the left until it covers the screen.
            yield return Animate(_wipeTime, t => SetBand(t - 1f));

            // 2. Caption fades in, holds and fades out on the black.
            if (!string.IsNullOrEmpty(caption))
            {
                _caption.text = caption;
                yield return Animate(_captionFadeIn, SetCaptionAlpha);
                yield return Animate(_captionHold, null);
                yield return Animate(_captionFadeOut, t => SetCaptionAlpha(1f - t));
            }

            // 3. Load; the next scene's transition starts covered.
            SceneFlow.LoadNow(buildIndex, true);
        }

        /// <summary>Holds the black briefly, then sweeps the band off to the right.</summary>
        private IEnumerator WipeOff()
        {
            yield return Animate(_arrivalHold, null);
            yield return Animate(_wipeTime, SetBand);
            SetBusy(false);
        }

        /// <summary>Calls step with an eased 0..1 value over the given real-time seconds (step may be null to just wait).</summary>
        private static IEnumerator Animate(float seconds, System.Action<float> step)
        {
            for (float elapsed = 0f; elapsed < seconds; elapsed += Mathf.Min(Time.unscaledDeltaTime, MaxStep))
            {
                step?.Invoke(Mathf.SmoothStep(0f, 1f, elapsed / seconds));
                yield return null;
            }

            step?.Invoke(1f);
        }

        /// <summary>
        /// Places the band: -1 is fully off to the left, 0 covers the screen, 1 is fully off to the
        /// right. The band is one screen wide by its anchors, and is shifted by one edge width per
        /// screen so the soft edges also clear the screen at both ends.
        /// </summary>
        private void SetBand(float position)
        {
            _wipe.anchorMin = new Vector2(position, 0f);
            _wipe.anchorMax = new Vector2(position + 1f, 1f);
            _wipe.anchoredPosition = new Vector2(position * _edgeWidth, 0f);
        }

        /// <summary>Sizes a soft edge to sit just outside the band on one side (-1 left, 1 right).</summary>
        private void SetEdgeWidth(RectTransform edge, float side)
        {
            edge.anchorMin = new Vector2(side < 0f ? 0f : 1f, 0f);
            edge.anchorMax = new Vector2(side < 0f ? 0f : 1f, 1f);
            // Centre pivot, so the right edge's mirroring (scale x -1) flips it in place.
            edge.pivot = new Vector2(0.5f, 0.5f);
            edge.anchoredPosition = new Vector2(side * _edgeWidth * 0.5f, 0f);
            edge.sizeDelta = new Vector2(_edgeWidth, 0f);
        }

        private void SetCaptionAlpha(float alpha)
        {
            Color colour = _caption.color;
            colour.a = alpha;
            _caption.color = colour;
        }

        /// <summary>Marks a transition as running and swallows clicks for its whole length.</summary>
        private void SetBusy(bool busy)
        {
            IsBusy = busy;
            _blocker.raycastTarget = busy;
        }

        /// <summary>
        /// Builds a white sprite whose alpha ramps from clear on the left to opaque on the right,
        /// tinted black by the edge images. Made in code so the wipe needs no art asset.
        /// </summary>
        private static Sprite CreateRampSprite()
        {
            const int width = 64;
            var texture = new Texture2D(width, 1, TextureFormat.RGBA32, false)
            {
                wrapMode = TextureWrapMode.Clamp,
                filterMode = FilterMode.Bilinear,
                name = "ScreenTransitionRamp"
            };

            for (int x = 0; x < width; x++)
            {
                float alpha = Mathf.SmoothStep(0f, 1f, (x + 0.5f) / width);
                texture.SetPixel(x, 0, new Color(1f, 1f, 1f, alpha));
            }

            texture.Apply();
            return Sprite.Create(texture, new Rect(0f, 0f, width, 1f), new Vector2(0.5f, 0.5f));
        }
    }
}
