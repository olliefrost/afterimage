// DialogueBox: a speech panel at the bottom centre of the screen (DialogueBox prefab). Show types a
// line out letter by letter under a speaker name; Hide fades it away. IntroSequence uses it for the
// guard's shout; it is generic so later speech can reuse it. Runs on real time so it never depends
// on the game's time scale. Visual only.
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace GameJam.UI
{
    /// <summary>Shows one line of speech with a typewriter effect and fades it out.</summary>
    public class DialogueBox : MonoBehaviour
    {
        [Header("UI references")]
        [Tooltip("Fades the whole box in and out.")]
        [SerializeField] private CanvasGroup _group;
        [SerializeField] private Text _speaker;
        [SerializeField] private Text _line;

        [Header("Timing")]
        [Tooltip("Letters typed per second.")]
        [SerializeField] private float _lettersPerSecond = 28f;
        [Tooltip("Seconds to fade in and out.")]
        [SerializeField] private float _fadeTime = 0.2f;

        private Coroutine _routine;

        /// <summary>True while the current line is still being typed out.</summary>
        public bool IsTyping { get; private set; }

        private void Awake()
        {
            _group.alpha = 0f;
        }

        /// <summary>Fades the box in and types the line out under the speaker's name.</summary>
        public void Show(string speaker, string line)
        {
            StopRoutine();
            _speaker.text = speaker;
            _routine = StartCoroutine(TypeLine(line));
        }

        /// <summary>Fades the box out, or hides it straight away if <paramref name="instant"/>.</summary>
        public void Hide(bool instant)
        {
            StopRoutine();
            IsTyping = false;
            if (instant)
            {
                _group.alpha = 0f;
            }
            else
            {
                _routine = StartCoroutine(FadeTo(0f));
            }
        }

        private IEnumerator TypeLine(string line)
        {
            // 1. Fade in with an empty line.
            IsTyping = true;
            _line.text = string.Empty;
            yield return FadeTo(1f);

            // 2. Reveal one more letter at the typing speed.
            float shown = 0f;
            while (shown < line.Length)
            {
                shown += Time.unscaledDeltaTime * _lettersPerSecond;
                _line.text = line.Substring(0, Mathf.Min(line.Length, (int)shown));
                yield return null;
            }

            IsTyping = false;
        }

        private IEnumerator FadeTo(float target)
        {
            float start = _group.alpha;
            for (float t = 0f; t < _fadeTime; t += Time.unscaledDeltaTime)
            {
                _group.alpha = Mathf.Lerp(start, target, t / _fadeTime);
                yield return null;
            }

            _group.alpha = target;
        }

        private void StopRoutine()
        {
            if (_routine != null)
            {
                StopCoroutine(_routine);
                _routine = null;
            }
        }
    }
}
