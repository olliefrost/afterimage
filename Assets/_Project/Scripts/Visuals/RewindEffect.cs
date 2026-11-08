// RewindEffect: the "tape rewind" shown between attempts. When the player presses R (save) or T
// (retry), GameStateController stops the clock and plays this before resetting the room: the screen
// tints cyan with scanlines and a rolling tracking band, the player and every clone run backwards
// along their recorded paths leaving fading afterimages, and a flash covers the reset. On a save, a
// bright ghost in the new clone's colour marks where the run ended. Any key skips it.
// Visual only: positions are only borrowed for the show; LevelReset puts everything back afterwards.
using System.Collections;
using System.Collections.Generic;
using GameJam.Afterimage;
using GameJam.Player;
using UnityEngine;
using UnityEngine.Rendering;

namespace GameJam.Visuals
{
    /// <summary>Plays the rewind between attempts; <see cref="Play"/> returns when the room may reset.</summary>
    public class RewindEffect : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private PlayerActor _player;
        [SerializeField] private PlayerInputReader _input;
        [SerializeField] private AfterimageManager _afterimages;
        [Tooltip("The player's 8-direction animator, turned to face against the motion while rewinding.")]
        [SerializeField] private CharacterSpriteAnimator _playerAnimator;
        [Tooltip("The player's visible sprite, copied for the afterimage trail.")]
        [SerializeField] private SpriteRenderer _playerSprite;
        [SerializeField] private GhostTrail _trail;

        [Header("Screen")]
        [Tooltip("Global Volume with the rewind look (desaturated, aberration, distortion); its weight is faded in and out.")]
        [SerializeField] private Volume _volume;
        [Tooltip("Scanlines, tint and REWIND label; faded in and out as one.")]
        [SerializeField] private CanvasGroup _overlay;
        [Tooltip("The bright noisy band that rolls up the screen like a VHS tracking error.")]
        [SerializeField] private RectTransform _trackingBand;
        [Tooltip("Full-screen flash that hides the moment the room resets.")]
        [SerializeField] private CanvasGroup _flash;

        [Header("Timing (seconds)")]
        [Tooltip("Rewind length for a very short run.")]
        [SerializeField] private float _minDuration = 0.55f;
        [Tooltip("Rewind length never exceeds this, however long the run was.")]
        [SerializeField] private float _maxDuration = 1.2f;
        [Tooltip("Extra rewind time per second of recorded run.")]
        [SerializeField] private float _perRecordedSecond = 0.03f;
        [SerializeField] private float _fadeIn = 0.12f;
        [SerializeField] private float _flashFade = 0.35f;
        [Tooltip("Key presses this soon after the rewind starts do not skip it (the R or T press itself).")]
        [SerializeField] private float _skipGrace = 0.15f;

        [Header("Afterimages")]
        [SerializeField] private float _ghostInterval = 0.04f;
        [SerializeField] private float _ghostLife = 0.4f;
        [SerializeField] private Color _ghostColour = new Color(0.45f, 0.95f, 1f, 0.55f);
        [Tooltip("How long the new clone's ghost stays where the saved run ended.")]
        [SerializeField] private float _savedGhostLife = 1.4f;
        [SerializeField] private float _trackingSpeed = 900f;

        private readonly List<(Transform clone, Rigidbody2D body, Recording run, int from)> _clones =
            new List<(Transform, Rigidbody2D, Recording, int)>();
        private Rigidbody2D _playerBody;
        private bool _skip;
        private float _elapsed;

        private void Awake()
        {
            _playerBody = _player.GetComponent<Rigidbody2D>();
            SetLook(0f);
            _flash.alpha = 0f;
        }

        /// <summary>
        /// Runs the rewind for <paramref name="run"/> (the attempt that just ended). Call before the
        /// run is handed to the clone slots, so the controlled slot's colour is still the saved clone's.
        /// </summary>
        public IEnumerator Play(Recording run, bool saved)
        {
            // 1. Take over the actors: no physics, so nothing is triggered while they fly back.
            int playerFrom = run.Frames.Count - 1;
            CollectClones(playerFrom);
            _playerBody.simulated = false;
            _playerAnimator.FaceAgainstMotion = true;
            if (saved)
            {
                Color slot = _afterimages.GetSlotColour(_afterimages.ControlledSlot);
                slot.a = 0.9f;
                _trail.Spawn(_playerSprite, slot, _savedGhostLife);
            }

            _skip = false;
            _input.SkipRequested += HandleSkip;
            _input.SetSkipEnabled(true);

            // 2. Rewind: everyone retraces their path to tick 0, accelerating like a tape spooling back.
            float seconds = run.Frames.Count * Time.fixedDeltaTime;
            float duration = Mathf.Clamp(_minDuration + seconds * _perRecordedSecond, _minDuration, _maxDuration);
            float nextGhost = 0f;
            for (_elapsed = 0f; _elapsed < duration && !_skip; _elapsed += Time.deltaTime)
            {
                float p = Smooth(_elapsed / duration);
                Pose(run, playerFrom, p);
                SetLook(Mathf.Clamp01(_elapsed / _fadeIn));
                RollTrackingBand(_elapsed);

                if (_elapsed >= nextGhost && playerFrom > 0)
                {
                    _trail.Spawn(_playerSprite, _ghostColour, _ghostLife);
                    nextGhost = _elapsed + _ghostInterval;
                }

                yield return null;
            }

            // 3. Land everyone on tick 0 and hand back: the caller resets the room under the flash.
            Pose(run, playerFrom, 1f);
            _input.SkipRequested -= HandleSkip;
            _input.SetSkipEnabled(false);
            _playerAnimator.FaceAgainstMotion = false;
            _playerBody.simulated = true;
            _clones.Clear();
            StartCoroutine(FlashOut());
        }

        /// <summary>Remembers each live clone, its run and the tick it has reached, and freezes its physics.</summary>
        private void CollectClones(int tickNow)
        {
            _clones.Clear();
            foreach (AfterimageReplayer clone in _afterimages.Clones)
            {
                if (clone == null || clone.Run == null || clone.Run.Frames.Count == 0)
                {
                    continue;
                }

                var body = clone.GetComponent<Rigidbody2D>();
                body.simulated = false;
                int from = Mathf.Min(tickNow, clone.Run.Frames.Count - 1);
                _clones.Add((clone.transform, body, clone.Run, from));
            }
        }

        /// <summary>Places the player and clones <paramref name="p"/> of the way back from where they were to tick 0.</summary>
        private void Pose(Recording run, int playerFrom, float p)
        {
            if (playerFrom >= 0)
            {
                _player.Teleport(run.Frames[Mathf.RoundToInt(Mathf.Lerp(playerFrom, 0, p))].Position);
            }

            foreach (var (clone, body, cloneRun, from) in _clones)
            {
                Vector2 at = cloneRun.Frames[Mathf.RoundToInt(Mathf.Lerp(from, 0, p))].Position;
                body.position = at;
                clone.position = at;
            }
        }

        /// <summary>Fades the cyan overlay and the rewind Volume together (0 off, 1 full).</summary>
        private void SetLook(float amount)
        {
            _overlay.alpha = amount;
            _volume.weight = amount;
        }

        /// <summary>Moves the tracking band up the screen, wrapping round, with a little jitter.</summary>
        private void RollTrackingBand(float t)
        {
            var canvas = (RectTransform)_trackingBand.parent;
            float height = canvas.rect.height + _trackingBand.rect.height;
            float y = Mathf.Repeat(t * _trackingSpeed, height) - height * 0.5f;
            float jitter = Mathf.Sin(t * 90f) * 3f;
            _trackingBand.anchoredPosition = new Vector2(jitter, y);
        }

        /// <summary>After the reset: flash, then fade the flash and the rewind look out together.</summary>
        private IEnumerator FlashOut()
        {
            for (float t = 0f; t < _flashFade; t += Time.deltaTime)
            {
                float left = 1f - t / _flashFade;
                _flash.alpha = left;
                SetLook(left);
                yield return null;
            }

            _flash.alpha = 0f;
            SetLook(0f);
        }

        private void HandleSkip()
        {
            if (_elapsed >= _skipGrace)
            {
                _skip = true;
            }
        }

        /// <summary>Ease in and out, so the rewind starts slow, spools fast, then settles.</summary>
        private static float Smooth(float x)
        {
            x = Mathf.Clamp01(x);
            return x * x * (3f - 2f * x);
        }
    }
}
