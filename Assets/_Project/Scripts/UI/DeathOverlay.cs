// DeathOverlay: the screen side of a player death. When GameStateController reports the death, a red
// vignette and desaturation (a post-processing Volume) pulse in, a thin red band rolls down the
// screen, and a caption says what happened to the run: "RUN SAVED · CLONE N" in that clone's colour
// if the run was marked with F, otherwise "RUN LOST". It all fades out just as the rewind starts.
// Sits on the HUD (DeathOverlay child). Visual only.
using GameJam.Afterimage;
using GameJam.Core;
using GameJam.Visuals;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;

namespace GameJam.UI
{
    /// <summary>Red screen pulse and run caption shown for the moment between a death and the rewind.</summary>
    public class DeathOverlay : MonoBehaviour
    {
        [Header("Game references")]
        [SerializeField] private GameStateController _game;
        [SerializeField] private AfterimageManager _afterimages;

        [Header("UI references")]
        [Tooltip("Fades the band and caption in and out together.")]
        [SerializeField] private CanvasGroup _group;
        [Tooltip("Red vignette and desaturation profile (Data/Death_Volume.asset); its weight is animated.")]
        [SerializeField] private Volume _volume;
        [Tooltip("Thin red band that rolls down the screen once.")]
        [SerializeField] private RectTransform _band;
        [SerializeField] private Text _caption;

        [Header("Timing (seconds from the death)")]
        [SerializeField] private float _fadeIn = 0.08f;
        [Tooltip("When it starts fading out. Keep the fade inside GameStateController's Death Reset Delay (0.8).")]
        [SerializeField] private float _fadeOutStart = 0.62f;
        [SerializeField] private float _fadeOutEnd = 0.8f;
        [Tooltip("Seconds the red band takes to roll from the top of the screen to the bottom.")]
        [SerializeField] private float _bandTime = 0.45f;

        [Header("Look")]
        [Tooltip("Volume weight at the very start of the hit, before it settles.")]
        [SerializeField] private float _peakWeight = 1f;
        [SerializeField] private float _holdWeight = 0.7f;
        [SerializeField] private Color _lostColour = new Color(1f, 0.3f, 0.25f);
        [Tooltip("How far the caption overshoots as it pops in (0.3 = 30% bigger).")]
        [SerializeField] private float _captionPop = 0.3f;

        private RectTransform _captionRect;
        private float _startTime = -1f;

        private void Awake()
        {
            _captionRect = _caption.rectTransform;
            Hide();
        }

        private void OnEnable()
        {
            _game.PlayerDied += HandlePlayerDied;
        }

        private void OnDisable()
        {
            _game.PlayerDied -= HandlePlayerDied;
            Hide();
        }

        private void Update()
        {
            if (_startTime < 0f)
            {
                return;
            }

            // Real time, so a pause straight after the hit does not leave the screen red.
            float time = Time.unscaledTime - _startTime;
            if (time >= _fadeOutEnd)
            {
                Hide();
                return;
            }

            // 1. Overall visibility: quick fade in, hold, fade out before the rewind.
            float visible = Mathf.Clamp01(time / _fadeIn) * (1f - Mathf.InverseLerp(_fadeOutStart, _fadeOutEnd, time));
            _group.alpha = visible;

            // 2. Screen tint: a sharp peak at the hit settling to the hold level.
            float settle = Ease.OutCubic(Mathf.Clamp01(time / 0.3f));
            _volume.weight = Mathf.Lerp(_peakWeight, _holdWeight, settle) * visible;

            // 3. The band rolls down the screen once.
            float roll = Mathf.Clamp01(time / _bandTime);
            _band.anchorMin = new Vector2(0f, 1f - roll);
            _band.anchorMax = new Vector2(1f, 1f - roll);

            // 4. The caption pops in a little big and settles.
            float pop = 1f + _captionPop * (1f - Ease.OutCubic(Mathf.Clamp01(time / 0.25f)));
            _captionRect.localScale = new Vector3(pop, pop, 1f);
        }

        private void HandlePlayerDied(bool runSaved)
        {
            if (runSaved)
            {
                int slot = _afterimages.ControlledSlot;
                _caption.text = $"RUN SAVED · CLONE {slot + 1}";
                _caption.color = _afterimages.GetSlotColour(slot);
            }
            else
            {
                _caption.text = "RUN LOST";
                _caption.color = _lostColour;
            }

            _startTime = Time.unscaledTime;
        }

        private void Hide()
        {
            _startTime = -1f;
            _group.alpha = 0f;
            _volume.weight = 0f;
        }
    }
}
