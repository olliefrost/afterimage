// OcclusionFader: reusable see-through effect for anything that can hide the player (ground in
// front of a pit, a wall, a ceiling, a tall prop). Another script calls SetFaded(true) while the
// player is hidden and SetFaded(false) when they are clear. Visual only.
using UnityEngine;
using UnityEngine.Tilemaps;

namespace GameJam.Visuals
{
    /// <summary>Fades a group of sprites and tilemaps to a set opacity and back.</summary>
    public class OcclusionFader : MonoBehaviour
    {
        [Header("What fades")]
        [SerializeField] private SpriteRenderer[] _sprites = new SpriteRenderer[0];
        [SerializeField] private Tilemap[] _tilemaps = new Tilemap[0];

        [Header("Fade")]
        [Tooltip("Opacity while faded (0 = invisible, 1 = solid).")]
        [SerializeField, Range(0f, 1f)] private float _fadedAlpha = 0.25f;
        [Tooltip("Seconds a fade takes.")]
        [SerializeField] private float _fadeDuration = 0.3f;

        private float _amount;
        private float _target;

        private void Awake()
        {
            Apply(0f);
        }

        /// <summary>Fades out (true) or back to solid (false). Instant skips the transition, as on a reset.</summary>
        public void SetFaded(bool faded, bool instant = false)
        {
            _target = faded ? 1f : 0f;
            if (instant)
            {
                Apply(_target);
            }
        }

        private void Update()
        {
            if (Mathf.Approximately(_amount, _target))
            {
                return;
            }

            float step = _fadeDuration > 0f ? Time.deltaTime / _fadeDuration : 1f;
            Apply(Mathf.MoveTowards(_amount, _target, step));
        }

        /// <summary>Sets every renderer's opacity for a fade amount from 0 (solid) to 1 (faded).</summary>
        private void Apply(float amount)
        {
            _amount = amount;
            float alpha = Mathf.Lerp(1f, _fadedAlpha, amount);

            foreach (SpriteRenderer sprite in _sprites)
            {
                Color color = sprite.color;
                color.a = alpha;
                sprite.color = color;
            }

            foreach (Tilemap tilemap in _tilemaps)
            {
                Color color = tilemap.color;
                color.a = alpha;
                tilemap.color = color;
            }
        }
    }
}
