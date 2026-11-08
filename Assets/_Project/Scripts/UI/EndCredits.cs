// EndCredits: the closing sequence of the Ending (a full-screen canvas in the Ending scene). Play
// fades the screen to black, types the end-card lines, scrolls the credits, then asks SceneFlow to
// finish the game (clears the save and wipes to the title). Holding any key or the mouse button
// speeds the credits up; Esc skips straight to the title. Real time, visual only.
using System.Collections;
using GameJam.Audio;
using GameJam.Core;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace GameJam.UI
{
    /// <summary>Fade to black, end card, scrolling credits, then back to the title.</summary>
    public class EndCredits : MonoBehaviour
    {
        [Header("UI references")]
        [Tooltip("Full-screen black image that fades in over the scene.")]
        [SerializeField] private Image _blackout;
        [Tooltip("Centred text for the end card lines.")]
        [SerializeField] private Text _cardText;
        [Tooltip("Text that scrolls up the screen. Set to anchor at the bottom centre.")]
        [SerializeField] private Text _creditsText;

        [Header("End card")]
        [SerializeField] private string[] _cardLines =
        {
            "SUBJECT 07 CONTAINED.",
            "RESETTING TRIAL...",
            "THE END"
        };
        [SerializeField] private float _fadeTime = 1.5f;
        [SerializeField] private float _lettersPerSecond = 20f;
        [Tooltip("Seconds each line stays after it is typed.")]
        [SerializeField] private float _lineHold = 1.1f;
        [Tooltip("Optional beep on every second typed letter of the end card.")]
        [SerializeField] private SoundCue _typeBeep;

        [Header("Credits")]
        [TextArea(4, 10)]
        [SerializeField] private string _credits = "DEVELOPED BY\nOllie\nFreddie\nAman\n\nART MADE WITH PIXELLAB\n\nUNIVERSITY OF BRISTOL GAME JAM 2026\nTHEME: TIME";
        [Tooltip("Reference pixels scrolled per second.")]
        [SerializeField] private float _scrollSpeed = 70f;
        [SerializeField] private float _fastMultiplier = 4f;
        [Tooltip("Optional music that starts with the closing sequence (mus_credits).")]
        [SerializeField] private AudioClip _music;
        [SerializeField, Range(0f, 1f)] private float _musicVolume = 0.5f;

        private bool _skip;
        private bool _playing;

        private void Awake()
        {
            _blackout.color = new Color(0f, 0f, 0f, 0f);
            _cardText.text = string.Empty;
            _creditsText.text = string.Empty;
        }

        private void Update()
        {
            // Esc skips from any phase (fade, end card or credits) straight to the title.
            if (_playing && Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
            {
                _skip = true;
            }
        }

        /// <summary>Runs the whole closing sequence. Returns when the title wipe has been started.</summary>
        public IEnumerator Play()
        {
            _playing = true;
            _skip = false;

            if (_music != null)
            {
                AudioManager.PlayMusic(_music, _musicVolume);
            }

            // 1. Fade the scene to black.
            for (float t = 0f; t < _fadeTime && !_skip; t += Time.unscaledDeltaTime)
            {
                _blackout.color = new Color(0f, 0f, 0f, t / _fadeTime);
                yield return null;
            }

            _blackout.color = Color.black;

            // 2. Type each end-card line, hold, then clear it.
            foreach (string line in _cardLines)
            {
                int lastCount = 0;
                for (float shown = 0f; shown < line.Length && !_skip; shown += Time.unscaledDeltaTime * _lettersPerSecond)
                {
                    int count = Mathf.Min(line.Length, (int)shown);
                    _cardText.text = line.Substring(0, count);
                    if (count > lastCount && count % 2 == 0)
                    {
                        AudioManager.Play(_typeBeep);
                    }

                    lastCount = count;
                    yield return null;
                }

                _cardText.text = line;
                for (float held = 0f; held < _lineHold && !_skip; held += Time.unscaledDeltaTime)
                {
                    yield return null;
                }
            }

            _cardText.text = string.Empty;

            // 3. Scroll the credits from below the screen until they have left the top.
            _creditsText.text = _credits;
            // The text's top edge starts at the screen bottom (text hangs below, off screen) and ends
            // once the whole block has passed the top. preferredHeight is right straight away, unlike rect.height.
            RectTransform rect = _creditsText.rectTransform;
            float height = _creditsText.preferredHeight;
            rect.sizeDelta = new Vector2(rect.sizeDelta.x, height);
            float end = 1080f + height;
            float y = 0f;
            while (y < end && !_skip)
            {
                bool fast = (Keyboard.current != null && Keyboard.current.anyKey.isPressed)
                    || (Mouse.current != null && Mouse.current.leftButton.isPressed);
                y += _scrollSpeed * (fast ? _fastMultiplier : 1f) * Time.unscaledDeltaTime;
                rect.anchoredPosition = new Vector2(0f, y);
                yield return null;
            }

            // 4. Back to the title, forgetting the save.
            _playing = false;
            SceneFlow.FinishGame();
        }
    }
}
