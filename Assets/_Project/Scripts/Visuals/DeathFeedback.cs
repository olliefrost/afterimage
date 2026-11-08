// DeathFeedback: the live player's death animation (placeholder until there is death art). When
// PlayerActor dies the character flashes white, turns red and shudders, then crumples down while
// flickering, the vision light turns red and dims, and the screen shakes. Just before
// GameStateController's death delay ends it recovers, so the rewind shows the normal character
// walking back in time. Sits on the Player's Body. Visual only: it touches sprites, scale and lights.
using GameJam.Core;
using GameJam.Player;
using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace GameJam.Visuals
{
    /// <summary>Plays a short tint, shudder and collapse on the player when they die, then restores them.</summary>
    public class DeathFeedback : MonoBehaviour, IResettable
    {
        [Header("References")]
        [SerializeField] private PlayerActor _player;
        [Tooltip("The character sprite (Body/Visual). Its scale is squashed, so its pivot should be at the feet.")]
        [SerializeField] private SpriteRenderer _sprite;
        [Tooltip("Lights on the character that turn red and dim (VisionLight, ImplantGlow).")]
        [SerializeField] private Light2D[] _lights;

        [Header("Timing (seconds from the hit)")]
        [Tooltip("White flash at the moment of the hit.")]
        [SerializeField] private float _flashTime = 0.06f;
        [Tooltip("Red shudder after the flash, until the collapse starts.")]
        [SerializeField] private float _shudderEnd = 0.3f;
        [Tooltip("When the collapse has finished.")]
        [SerializeField] private float _collapseEnd = 0.5f;
        [Tooltip("When the character starts recovering. Keep the recovery inside GameStateController's Death Reset Delay (0.8).")]
        [SerializeField] private float _recoverStart = 0.62f;
        [SerializeField] private float _recoverEnd = 0.78f;

        [Header("Look")]
        [SerializeField] private Color _hitColour = new Color(1f, 0.25f, 0.2f);
        [Tooltip("Scale of the crumpled character: wider and much shorter.")]
        [SerializeField] private Vector2 _collapsedScale = new Vector2(1.15f, 0.55f);
        [Tooltip("How far the width jitters during the shudder.")]
        [SerializeField] private float _shudderAmount = 0.12f;
        [SerializeField] private float _shudderSpeed = 60f;
        [Tooltip("Light brightness left at the bottom of the collapse (0-1 of normal).")]
        [SerializeField] private float _dimmedLight = 0.45f;

        [Header("Screen shake")]
        [SerializeField] private float _shakeStrength = 0.14f;
        [SerializeField] private float _shakeDuration = 0.35f;

        private Color _restColour;
        private Vector3 _restScale;
        private Color[] _lightColours;
        private float[] _lightIntensities;
        private float _elapsed = -1f;

        private void Awake()
        {
            _restColour = _sprite.color;
            _restScale = _sprite.transform.localScale;
            _lightColours = new Color[_lights.Length];
            _lightIntensities = new float[_lights.Length];
            for (int i = 0; i < _lights.Length; i++)
            {
                _lightColours[i] = _lights[i].color;
                _lightIntensities[i] = _lights[i].intensity;
            }
        }

        private void OnEnable()
        {
            _player.Died += HandleDied;
            LevelReset.Register(this);
        }

        private void OnDisable()
        {
            _player.Died -= HandleDied;
            LevelReset.Unregister(this);
            ResetState();
        }

        private void Update()
        {
            if (_elapsed < 0f)
            {
                return;
            }

            _elapsed += Time.deltaTime;
            float time = _elapsed;
            if (time >= _recoverEnd)
            {
                ResetState();
                return;
            }

            // 1. How "dead" the character looks right now: 1 through the hit and collapse, back to 0 while recovering.
            float recovery = Mathf.InverseLerp(_recoverStart, _recoverEnd, time);
            float dead = 1f - Ease.OutCubic(recovery);

            // 2. Colour: a white flash first, then the hit red, flickering during the collapse.
            Color colour = time < _flashTime ? Color.white : _hitColour;
            if (time > _shudderEnd && time < _collapseEnd && Mathf.Repeat(time * 20f, 1f) < 0.3f)
            {
                colour.a *= 0.4f;
            }
            _sprite.color = Color.Lerp(_restColour, colour, dead);

            // 3. Shape: a sideways shudder, then a squash down onto the floor.
            float collapse = Ease.OutCubic(Mathf.InverseLerp(_shudderEnd, _collapseEnd, time)) * dead;
            float shudder = time < _shudderEnd ? Mathf.Sin(time * _shudderSpeed) * _shudderAmount : 0f;
            _sprite.transform.localScale = new Vector3(
                _restScale.x * (Mathf.Lerp(1f, _collapsedScale.x, collapse) + shudder),
                _restScale.y * Mathf.Lerp(1f, _collapsedScale.y, collapse),
                _restScale.z);

            // 4. Lights: red at once, dimming as the character collapses.
            float brightness = Mathf.Lerp(1f, _dimmedLight, collapse);
            for (int i = 0; i < _lights.Length; i++)
            {
                _lights[i].color = Color.Lerp(_lightColours[i], _hitColour, dead);
                _lights[i].intensity = _lightIntensities[i] * brightness;
            }
        }

        /// <summary>Puts the sprite, scale and lights back to normal (also called by the level reset).</summary>
        public void ResetState()
        {
            _elapsed = -1f;
            _sprite.color = _restColour;
            _sprite.transform.localScale = _restScale;
            for (int i = 0; i < _lights.Length; i++)
            {
                _lights[i].color = _lightColours[i];
                _lights[i].intensity = _lightIntensities[i];
            }
        }

        private void HandleDied()
        {
            _elapsed = 0f;
            ScreenShake.Shake(_shakeStrength, _shakeDuration);
        }
    }
}
