// SpeechBubble: a small world-space bubble above a character (a canvas child of the Player in the
// Ending scene, wired to EndingSequence; hidden until Show). Show pops it up with a line of text, Hide pops it away. The Ending uses it for her
// "...?" and "Sis?". Real time, visual only.
using System.Collections;
using GameJam.Audio;
using GameJam.Visuals;
using UnityEngine;
using UnityEngine.UI;

namespace GameJam.UI
{
    /// <summary>Pops a line of text above a character.</summary>
    public class SpeechBubble : MonoBehaviour
    {
        [Header("UI references")]
        [Tooltip("The bubble panel that scales in and out.")]
        [SerializeField] private RectTransform _bubble;
        [SerializeField] private CanvasGroup _group;
        [SerializeField] private Text _label;

        [Header("Audio")]
        [Tooltip("Played once as the bubble pops up (the text beeps).")]
        [SerializeField] private SoundCue _voice;

        [Header("Timing")]
        [SerializeField] private float _popTime = 0.18f;

        private Vector3 _restScale;
        private Coroutine _routine;

        private void Awake()
        {
            _restScale = _bubble.localScale;
            Apply(0f);
        }

        /// <summary>Pops the bubble up showing <paramref name="text"/>, replacing any line already shown.</summary>
        public void Show(string text)
        {
            _label.text = text;
            AudioManager.Play(_voice);
            Animate(1f);
        }

        /// <summary>Pops the bubble away.</summary>
        public void Hide()
        {
            Animate(0f);
        }

        private void Animate(float target)
        {
            if (_routine != null)
            {
                StopCoroutine(_routine);
            }

            _routine = StartCoroutine(PopTo(target));
        }

        private IEnumerator PopTo(float target)
        {
            float start = _group.alpha;
            for (float t = 0f; t < _popTime; t += Time.unscaledDeltaTime)
            {
                float k = t / _popTime;
                float shown = Mathf.LerpUnclamped(start, target, target > start ? Ease.BackOut(k) : k);
                Apply(shown);
                yield return null;
            }

            Apply(target);
        }

        /// <summary>Sets size and opacity for how far the bubble has popped in (0-1, may overshoot).</summary>
        private void Apply(float shown)
        {
            _bubble.localScale = _restScale * Mathf.Max(0f, shown);
            _group.alpha = Mathf.Clamp01(shown);
        }
    }
}
