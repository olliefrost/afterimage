// CharacterSpriteAnimator: sits on a character's Visual child (player or clone). It works out
// facing from how the character moved this frame, picks one of 8 directions from a
// CharacterSpriteSet, loops that direction's run frames while moving and shows its idle sprite when
// still. Directions without run frames fall back to the idle sprite with a small walk bob. It also
// adds the ground height from ActorElevation (if the Body has one), since it owns this sprite's
// position. Mid-jump (ActorJump) it plays the jump frames along the arc; on a ladder (LadderClimber)
// it loops the climb frames and keeps the art on the ladder's face (Ladder.HugOffset). When a hazard
// drops the character into a chasm (Hazards.Falling) the sprite and its shadow fall out of sight and
// fade, until the actor lives again or the rewind takes over. Visual only, so it needs no link to
// input or recordings.
using GameJam.Actors;
using GameJam.Core;
using GameJam.Hazards;
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

        [Header("Falling into a chasm")]
        [Tooltip("Downward acceleration of the falling sprite, in units per second squared.")]
        [SerializeField] private float _fallGravity = 14f;
        [Tooltip("Seconds the falling sprite takes to fade out (the room restarts after the death delay).")]
        [SerializeField] private float _fallFadeSeconds = 0.6f;

        // Ignore tiny moves (physics jitter) so the sprite does not flicker between directions.
        private const float MinMoveSqr = 0.000001f;
        private const float DegreesPerDirection = 360f / CharacterSpriteSet.DirectionCount;

        /// <summary>
        /// While true (the rewind between attempts) the character faces against its motion and its run
        /// plays backwards, so it looks like a tape running in reverse rather than running away.
        /// </summary>
        public bool FaceAgainstMotion
        {
            get => _faceAgainstMotion;
            set
            {
                _faceAgainstMotion = value;
                if (value)
                {
                    EndFall();
                }
            }
        }

        private Transform _body;
        private ActorElevation _elevation;
        private ActorJump _jump;
        private LadderClimber _climber;
        private Vector3 _lastBodyPosition;
        private Vector3 _restLocalPosition;
        private int _direction;
        private float _walkTime;
        private bool _faceAgainstMotion;
        private IActor _actor;
        private SpriteRenderer _shadow;
        private bool _falling;
        private float _fallTime;
        private float _spriteAlpha;
        private float _shadowAlpha;

        private void Awake()
        {
            _body = transform.parent != null ? transform.parent : transform;
            _elevation = GetComponentInParent<ActorElevation>();
            _jump = GetComponentInParent<ActorJump>();
            _climber = GetComponentInParent<LadderClimber>();
            _actor = GetComponentInParent<IActor>();
            Transform shadow = _body.Find("Shadow");
            _shadow = shadow != null ? shadow.GetComponent<SpriteRenderer>() : null;
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

            // 3. Pick the sprite: jump frames in the air, climb frames on a ladder, else run or idle.
            DirectionSprites sprites = _sprites.Get(_direction);
            bool airborne = _jump != null && _jump.IsAirborne;
            bool climbing = !airborne && _climber != null && _climber.IsClimbing;
            bool animated = ShowFrame(sprites, isMoving, airborne, climbing);

            // 4. Stand on whatever ground height the level has here (balconies, ladders), plus the jump arc.
            //    Without frames for what the character is doing, a small bob shows it moving.
            float bob = isMoving && !animated ? Mathf.Abs(Mathf.Sin(_walkTime * _bobSpeed)) * _bobHeight : 0f;
            float ground = _elevation != null ? _elevation.Height : 0f;
            float arc = airborne ? _jump.ArcHeight : 0f;
            Vector2 hug = climbing && _climber.Current != null ? _climber.Current.HugOffset(_body.position) : Vector2.zero;
            transform.localPosition = _restLocalPosition + new Vector3(hug.x, hug.y + bob + ground + arc - FallDrop(), 0f);
        }

        private void OnEnable()
        {
            Falling.Fell += HandleFell;
        }

        private void OnDisable()
        {
            Falling.Fell -= HandleFell;
            EndFall();
        }

        /// <summary>Starts the fall when it is this character that went over the edge.</summary>
        private void HandleFell(IActor actor)
        {
            if (_falling || actor != _actor)
            {
                return;
            }

            _falling = true;
            _fallTime = 0f;
            _spriteAlpha = _renderer.color.a;
            _shadowAlpha = _shadow != null ? _shadow.color.a : 0f;
        }

        /// <summary>
        /// How far below the ground the falling sprite is this frame, fading it (and its shadow) as it
        /// goes. Ends the fall once the actor is alive again (the room reset).
        /// </summary>
        private float FallDrop()
        {
            if (!_falling)
            {
                return 0f;
            }

            if (_actor == null || !_actor.IsDead)
            {
                EndFall();
                return 0f;
            }

            _fallTime += Time.deltaTime;
            float fade = 1f - Mathf.Clamp01(_fallTime / _fallFadeSeconds);
            SetAlpha(_renderer, _spriteAlpha * fade);
            SetAlpha(_shadow, _shadowAlpha * fade);
            return 0.5f * _fallGravity * _fallTime * _fallTime;
        }

        /// <summary>Puts the sprite and shadow back as they were before the fall.</summary>
        private void EndFall()
        {
            if (!_falling)
            {
                return;
            }

            _falling = false;
            SetAlpha(_renderer, _spriteAlpha);
            SetAlpha(_shadow, _shadowAlpha);
            transform.localPosition = _restLocalPosition;
        }

        /// <summary>Sets one sprite's opacity, if it exists.</summary>
        private static void SetAlpha(SpriteRenderer sprite, float alpha)
        {
            if (sprite != null)
            {
                Color c = sprite.color;
                c.a = alpha;
                sprite.color = c;
            }
        }

        /// <summary>Shows this frame's sprite. Returns false when it fell back to the idle sprite while moving.</summary>
        private bool ShowFrame(DirectionSprites sprites, bool isMoving, bool airborne, bool climbing)
        {
            // The climb art faces up-left; every other state draws unmirrored.
            _renderer.flipX = false;

            // 1. In the air: the jump frames once across the jump.
            if (airborne)
            {
                bool hasJump = sprites.Jump != null && sprites.Jump.Length > 0;
                _renderer.sprite = hasJump
                    ? sprites.Jump[Mathf.Min((int)(_jump.Progress * sprites.Jump.Length), sprites.Jump.Length - 1)]
                    : sprites.Idle;
                return true;
            }

            // 2. On a ladder: loop the climb frames while moving, show the first one while still, mirrored
            //    for ladders that climb up-right on screen.
            Sprite[] climb = _sprites.Climb;
            if (climbing && climb != null && climb.Length > 0)
            {
                _renderer.flipX = _climber.Current != null && _climber.Current.ScreenAxis.x > 0f;
                _renderer.sprite = climb[(int)(_walkTime * _sprites.ClimbFramesPerSecond) % climb.Length];
                return true;
            }

            // 3. Walking: run frames, or idle (with the caller's bob) for directions without them.
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

            return !isMoving || hasRun;
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
