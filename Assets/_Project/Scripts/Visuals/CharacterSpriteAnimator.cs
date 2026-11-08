// CharacterSpriteAnimator: sits on a character's Visual child (player or clone). It works out
// facing from how the character moved this frame, picks one of 8 directions from a
// CharacterSpriteSet, loops that direction's run frames while moving and shows its idle sprite when
// still. Directions without run frames fall back to the idle sprite with a small walk bob. It also
// adds the ground height from ActorElevation (if the Body has one), since it owns this sprite's
// position. Visual only, so it needs no link to input or recordings.
using System;
using UnityEngine;

namespace GameJam.Visuals
{
    /// <summary>Chooses an 8-direction sprite from movement and plays run frames while walking.</summary>
    public class CharacterSpriteAnimator : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private SpriteRenderer _renderer;
        [Tooltip("Shared idle and run sprites (Player and Clone use the same asset).")]
        [SerializeField] private CharacterSpriteSet _sprites;

        [Header("Facing")]
        [Tooltip("Matches PlayerMovement's iso vertical scale, so iso diagonals map to the diagonal sprites.")]
        [SerializeField] private float _isoVerticalScale = 0.5f;
        [Tooltip("Direction shown before the character first moves (6 = south, facing the camera).")]
        [Range(0, 7)]
        [SerializeField] private int _startDirection = 6;

        [Header("Walk bob (only for directions without run frames)")]
        [SerializeField] private float _bobHeight = 0.05f;
        [SerializeField] private float _bobSpeed = 16f;

        [Header("Footsteps")]
        [Tooltip("Run frames where a foot lands and a step sound plays. The run has 6 frames, so 0 and 3 is left foot, right foot.")]
        [SerializeField] private int[] _stepFrames = { 0, 3 };

        // Ignore tiny moves (physics jitter) so the sprite does not flicker between directions.
        private const float MinMoveSqr = 0.000001f;
        // A jump this big in one frame is a teleport (clone respawn, intro), not a step.
        private const float TeleportSqr = 0.25f;
        private const float DegreesPerDirection = 360f / CharacterSpriteSet.DirectionCount;

        /// <summary>
        /// While true (the rewind between attempts) the character faces against its motion and its run
        /// plays backwards, so it looks like a tape running in reverse rather than running away.
        /// </summary>
        public bool FaceAgainstMotion { get; set; }

        /// <summary>Raised on each foot contact while running forwards (used by FootstepAudio).</summary>
        public event Action Stepped;

        private int _lastRunFrame = -1;
        private Transform _body;
        private ActorElevation _elevation;
        private Vector3 _lastBodyPosition;
        private Vector3 _restLocalPosition;
        private int _direction;
        private float _walkTime;

        private void Awake()
        {
            _body = transform.parent != null ? transform.parent : transform;
            _elevation = GetComponentInParent<ActorElevation>();
            _lastBodyPosition = _body.position;
            _restLocalPosition = transform.localPosition;
            _direction = _startDirection;
        }

        private void LateUpdate()
        {
            if (_sprites == null)
            {
                return;
            }

            // 1. Read this frame's movement from the body.
            Vector2 delta = _body.position - _lastBodyPosition;
            _lastBodyPosition = _body.position;
            bool isMoving = delta.sqrMagnitude > MinMoveSqr;

            // 2. Face the way we moved; keep the last facing when standing still.
            if (isMoving)
            {
                _direction = DirectionFromDelta(FaceAgainstMotion ? -delta : delta);
                _walkTime += Time.deltaTime;
            }
            else
            {
                _walkTime = 0f;
            }

            // 3. Show a run frame, or the idle sprite plus a bob when this direction has no run frames.
            DirectionSprites sprites = _sprites.Get(_direction);
            bool hasRun = sprites.Run != null && sprites.Run.Length > 0;
            if (isMoving && hasRun)
            {
                int frame = (int)(_walkTime * _sprites.RunFramesPerSecond) % sprites.Run.Length;
                RaiseStepIfContact(frame, delta.sqrMagnitude);
                if (FaceAgainstMotion)
                {
                    frame = sprites.Run.Length - 1 - frame;
                }

                _renderer.sprite = sprites.Run[frame];
            }
            else
            {
                _renderer.sprite = sprites.Idle;
                _lastRunFrame = -1;
            }

            // 4. Stand on whatever ground height the level has here (Chamber 3's balcony and steps).
            float bob = isMoving && !hasRun ? Mathf.Abs(Mathf.Sin(_walkTime * _bobSpeed)) * _bobHeight : 0f;
            float ground = _elevation != null ? _elevation.Height : 0f;
            transform.localPosition = _restLocalPosition + new Vector3(0f, bob + ground, 0f);
        }

        /// <summary>
        /// Raises <see cref="Stepped"/> when the run frame changes onto one of <c>_stepFrames</c>. Skipped in the
        /// reversed rewind and on teleports, so only real running makes footsteps.
        /// </summary>
        private void RaiseStepIfContact(int frame, float moveSqr)
        {
            bool newFrame = frame != _lastRunFrame;
            _lastRunFrame = frame;
            if (newFrame && !FaceAgainstMotion && moveSqr < TeleportSqr && Array.IndexOf(_stepFrames, frame) >= 0)
            {
                Stepped?.Invoke();
            }
        }

        /// <summary>Maps a world move to a direction index (0 = east, counter-clockwise), undoing the iso squash.</summary>
        private int DirectionFromDelta(Vector2 delta)
        {
            float angle = Mathf.Atan2(delta.y / _isoVerticalScale, delta.x) * Mathf.Rad2Deg;
            int index = Mathf.RoundToInt(angle / DegreesPerDirection);
            return (index % CharacterSpriteSet.DirectionCount + CharacterSpriteSet.DirectionCount)
                % CharacterSpriteSet.DirectionCount;
        }
    }
}
