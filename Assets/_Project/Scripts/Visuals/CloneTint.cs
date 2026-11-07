// CloneTint: colours a clone in its slot colour (sprite tint and AfterimageGlow light) so each
// clone matches its HUD icon, and turns it red when it dies. Sits on the Clone prefab root.
// Visual only: it reads the slot colour from AfterimageReplayer, which AfterimageManager sets on spawn.
using GameJam.Afterimage;
using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace GameJam.Visuals
{
    /// <summary>Tints a clone's sprite and glow with its slot colour, or red once it is dead.</summary>
    public class CloneTint : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private AfterimageReplayer _replayer;
        [SerializeField] private SpriteRenderer _sprite;
        [SerializeField] private Light2D _glow;

        [Header("Look")]
        [Tooltip("How far the sprite tint is pulled towards white, so the character stays readable (0 = pure slot colour).")]
        [Range(0f, 1f)]
        [SerializeField] private float _spriteWhiteness = 0.35f;
        [SerializeField] private Color _deadColour = new Color(1f, 0.15f, 0.1f);

        // The prefab's sprite alpha is the afterimage's see-through look, so it is kept as is.
        private float _spriteAlpha;

        private void Awake()
        {
            _spriteAlpha = _sprite.color.a;
        }

        private void OnEnable()
        {
            _replayer.Died += HandleDied;
        }

        private void OnDisable()
        {
            _replayer.Died -= HandleDied;
        }

        private void Start()
        {
            // Start runs after AfterimageManager has called Init, so the slot colour is set by now.
            Apply(_replayer.IsDead ? _deadColour : _replayer.SlotColour);
        }

        private void HandleDied(AfterimageReplayer replayer)
        {
            Apply(_deadColour);
        }

        /// <summary>Sets the sprite tint (lightened, alpha kept) and the glow colour.</summary>
        private void Apply(Color colour)
        {
            Color tint = Color.Lerp(colour, Color.white, _spriteWhiteness);
            tint.a = _spriteAlpha;
            _sprite.color = tint;
            _glow.color = colour;
        }
    }
}
