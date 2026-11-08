// LaserPostVisuals: animates a laser post's sprite from its LaserEmitter's tick schedule: the idle
// frame while resting, the charge frames scrubbed as the shot builds, and the fire frames while the
// beam shows. Sits on the Laser prefab's Emitter child. The frames are drawn facing north-east
// (up-right on screen); a laser aiming up-left shows them mirrored, so one prefab serves both
// Tutorial2 corridors. Each shot gives a small screen kick. Visual only: it reads the emitter, never drives it.
using GameJam.Hazards;
using UnityEngine;

namespace GameJam.Visuals
{
    /// <summary>Shows the laser post frame that matches the emitter's phase, mirrored to face its aim.</summary>
    public class LaserPostVisuals : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private LaserEmitter _emitter;
        [SerializeField] private SpriteRenderer _renderer;

        [Header("Frames (drawn facing north-east)")]
        [SerializeField] private Sprite _idle;
        [Tooltip("Played across the charge, first to last, as the shot builds.")]
        [SerializeField] private Sprite[] _charge;
        [Tooltip("Played across the beam ticks.")]
        [SerializeField] private Sprite[] _fire;

        [Header("Placement")]
        [Tooltip("Moves the post this far along its aim, off the low wall behind it. Moving along the aim never shifts the firing line.")]
        [SerializeField] private float _standForward = 0.45f;

        [Header("Feedback")]
        [Tooltip("Screen shake from the post each time it fires; fades when the laser is off-screen.")]
        [SerializeField] private float _fireShake = 0.035f;

        private LaserPhase _lastPhase;

        private void Awake()
        {
            // 1. Mirror the north-east art for lasers that aim left on screen.
            Vector2 aim = _emitter.Aim;
            _renderer.flipX = aim.x < 0f;

            // 2. Stand the post clear of the wall at the corridor end, keeping its prefab height offset.
            //    Snapped to whole art pixels (64 PPU) so the sprite stays crisp.
            Vector2 offset = aim * _standForward;
            offset = new Vector2(Mathf.Round(offset.x * 64f), Mathf.Round(offset.y * 64f)) / 64f;
            transform.localPosition += (Vector3)offset;

            // 3. The art only shows the back of the post, so a laser aiming down the screen has no frames yet.
            if (aim.y < 0f)
            {
                Debug.LogWarning($"{name}: laser post art only faces away from the camera; aim {aim} will look backwards.", this);
            }
        }

        private void LateUpdate()
        {
            _renderer.sprite = CurrentFrame();

            // A small kick the moment a shot goes off.
            if (_emitter.Phase == LaserPhase.Firing && _lastPhase != LaserPhase.Firing)
            {
                ScreenShake.ShakeAt(transform.position, _fireShake, 0.15f);
            }

            _lastPhase = _emitter.Phase;
        }

        /// <summary>The frame for the emitter's phase, falling back to the idle pose if a list is empty.</summary>
        private Sprite CurrentFrame()
        {
            switch (_emitter.Phase)
            {
                case LaserPhase.Charging:
                    return Scrub(_charge, _emitter.PhaseProgress);
                case LaserPhase.Firing:
                    return Scrub(_fire, _emitter.PhaseProgress);
                default:
                    return _idle;
            }
        }

        /// <summary>Picks the frame a fraction of the way through a list, so playback follows the ticks.</summary>
        private Sprite Scrub(Sprite[] frames, float progress)
        {
            if (frames == null || frames.Length == 0)
            {
                return _idle;
            }

            int index = Mathf.Min(frames.Length - 1, Mathf.FloorToInt(progress * frames.Length));
            return frames[index];
        }
    }
}
