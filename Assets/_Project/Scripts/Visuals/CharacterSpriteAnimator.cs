// CharacterSpriteAnimator: sits on a character's Visual child (player or clone). It works out
// facing from how the character moved this frame, picks one of 8 directions from a
// CharacterSpriteSet, loops that direction's run frames while moving and shows its idle sprite when
// still. Directions without run frames fall back to the idle sprite with a small walk bob.
// Visual only, so it needs no link to input or recordings.
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

        // Ignore tiny moves (physics jitter) so the sprite does not flicker between directions.
        private const float MinMoveSqr = 0.000001f;
        private const float DegreesPerDirection = 360f / CharacterSpriteSet.DirectionCount;

        /// <summary>
        /// While true (the rewind between attempts) the character faces against its motion and its run
        /// plays backwards, so it looks like a tape running in reverse rather than running away.
        /// </summary>
        public bool FaceAgainstMotion { get; set; }

        private Transform _body;
        private Vector3 _lastBodyPosition;
        private Vector3 _restLocalPosition;
        private int _direction;
        private float _walkTime;

        private void Awake()
        {
            _body = transform.parent != null ? transform.parent : transform;
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
                if (FaceAgainstMotion)
                {
                    frame = sprites.Run.Length - 1 - frame;
                }

                _renderer.sprite = sprites.Run[frame];
            }
            else
            {
                _renderer.sprite = sprites.Idle;
            }

            float bob = isMoving && !hasRun ? Mathf.Abs(Mathf.Sin(_walkTime * _bobSpeed)) * _bobHeight : 0f;
            transform.localPosition = _restLocalPosition + new Vector3(0f, bob, 0f);
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
