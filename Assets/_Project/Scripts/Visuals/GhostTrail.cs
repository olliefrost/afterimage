// GhostTrail: a small pool of fading sprite copies ("afterimages") left behind a moving character.
// RewindEffect stamps one every few frames while time runs backwards, and a longer-lived one in the
// new clone's colour when a run is saved. Visual only: nothing here touches gameplay.
using UnityEngine;

namespace GameJam.Visuals
{
    /// <summary>Stamps copies of a sprite that fade out over a set time, reusing a fixed pool.</summary>
    public class GhostTrail : MonoBehaviour
    {
        [Header("Pool")]
        [Tooltip("Most ghosts on screen at once; the oldest is reused when they run out.")]
        [SerializeField] private int _poolSize = 32;
        [Tooltip("Unlit sprite material, so ghosts glow in the dark chamber.")]
        [SerializeField] private Material _material;

        private SpriteRenderer[] _ghosts;
        private Color[] _colours;
        private float[] _age;
        private float[] _life;
        private int _next;

        private void Awake()
        {
            // 1. Build the pool once; ghosts are only ever re-used, never created during play.
            _ghosts = new SpriteRenderer[_poolSize];
            _colours = new Color[_poolSize];
            _age = new float[_poolSize];
            _life = new float[_poolSize];
            for (int i = 0; i < _poolSize; i++)
            {
                var go = new GameObject($"Ghost {i}");
                go.transform.SetParent(transform, false);
                _ghosts[i] = go.AddComponent<SpriteRenderer>();
                _ghosts[i].sharedMaterial = _material;
                _ghosts[i].enabled = false;
            }
        }

        /// <summary>Leaves a copy of <paramref name="source"/>'s current sprite where it stands, fading over <paramref name="life"/> seconds.</summary>
        public void Spawn(SpriteRenderer source, Color colour, float life)
        {
            SpriteRenderer ghost = _ghosts[_next];
            ghost.sprite = source.sprite;
            ghost.flipX = source.flipX;
            ghost.sortingLayerID = source.sortingLayerID;
            ghost.sortingOrder = source.sortingOrder;
            ghost.color = colour;
            ghost.transform.SetPositionAndRotation(source.transform.position, source.transform.rotation);
            ghost.transform.localScale = source.transform.lossyScale;
            ghost.enabled = true;

            _colours[_next] = colour;
            _age[_next] = 0f;
            _life[_next] = Mathf.Max(0.01f, life);
            _next = (_next + 1) % _poolSize;
        }

        private void Update()
        {
            // Fade each live ghost towards clear, then hide it.
            for (int i = 0; i < _poolSize; i++)
            {
                if (!_ghosts[i].enabled)
                {
                    continue;
                }

                _age[i] += Time.deltaTime;
                float left = 1f - _age[i] / _life[i];
                if (left <= 0f)
                {
                    _ghosts[i].enabled = false;
                    continue;
                }

                Color c = _colours[i];
                c.a *= left;
                _ghosts[i].color = c;
            }
        }
    }
}
