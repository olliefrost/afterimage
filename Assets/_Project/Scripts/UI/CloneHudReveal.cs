// CloneHudReveal: hides the clone part of the HUD (R and T buttons, clone column) while the chamber
// has clones locked, then pops it in when GameStateController unlocks them: each part fades up,
// grows from small and overshoots a little before settling, one part after another, so the player
// notices the new controls. Sits on the HUD root. Visual only: GameStateController already ignores
// the clone keys while locked.
using GameJam.Core;
using GameJam.Visuals;
using UnityEngine;

namespace GameJam.UI
{
    /// <summary>Keeps the clone HUD hidden until clones unlock, then animates it in.</summary>
    public class CloneHudReveal : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private GameStateController _game;
        [Tooltip("Every HUD part that belongs to cloning. Each needs a CanvasGroup.")]
        [SerializeField] private CanvasGroup[] _parts;

        [Header("Animation")]
        [Tooltip("Seconds for each part to pop in.")]
        [SerializeField] private float _duration = 0.6f;
        [Tooltip("Seconds between one part starting and the next, so they arrive one after another.")]
        [SerializeField] private float _stagger = 0.12f;
        [Tooltip("Scale each part starts from.")]
        [SerializeField] private float _startScale = 0.4f;
        [Tooltip("Strength of the overshoot past full size before settling (1.7 is about 10%, 0 is none).")]
        [SerializeField] private float _overshoot = 1.7f;

        private RectTransform[] _rects;
        private float _revealTime = -1f;

        private void Awake()
        {
            _rects = new RectTransform[_parts.Length];
            for (int i = 0; i < _parts.Length; i++)
            {
                _rects[i] = (RectTransform)_parts[i].transform;
            }
        }

        private void OnEnable()
        {
            _game.ClonesUnlockedNow += HandleUnlocked;

            // Chambers that start unlocked show the HUD straight away; locked ones hide it.
            float shown = _game.ClonesUnlocked ? 1f : 0f;
            for (int i = 0; i < _parts.Length; i++)
            {
                Apply(i, shown, 1f);
            }
        }

        private void OnDisable()
        {
            _game.ClonesUnlockedNow -= HandleUnlocked;
        }

        private void Update()
        {
            if (_revealTime < 0f)
            {
                return;
            }

            // Visual only, and it should still play if the game is paused straight after, so real time.
            float elapsed = Time.unscaledTime - _revealTime;
            bool finished = true;
            for (int i = 0; i < _parts.Length; i++)
            {
                float t = Mathf.Clamp01((elapsed - i * _stagger) / _duration);
                finished &= t >= 1f;

                // 1. Scale: grow from small, overshoot a little, settle at full size (a "back out" ease).
                float scale = Mathf.LerpUnclamped(_startScale, 1f, Ease.BackOut(t, _overshoot));

                // 2. Fade up over the first third of the pop.
                Apply(i, Mathf.Clamp01(t * 3f), scale);
            }

            if (finished)
            {
                _revealTime = -1f;
            }
        }

        private void HandleUnlocked()
        {
            _revealTime = Time.unscaledTime;
        }

        /// <summary>Sets one part's opacity and scale; it only takes clicks once fully visible.</summary>
        private void Apply(int index, float alpha, float scale)
        {
            _parts[index].alpha = alpha;
            _parts[index].interactable = alpha >= 1f;
            _parts[index].blocksRaycasts = alpha > 0f;
            _rects[index].localScale = new Vector3(scale, scale, 1f);
        }
    }
}
