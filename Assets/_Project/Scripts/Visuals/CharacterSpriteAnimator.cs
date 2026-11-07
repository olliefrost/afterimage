// CharacterSpriteAnimator: sits on a character's Visual child (player or clone). It works out
// facing from how the character moved this frame, picks the front/back/side sprite and adds a
// small walk bob. Visual only, so it needs no link to input or recordings.
using UnityEngine;

namespace GameJam.Visuals
{
    /// <summary>Chooses a directional sprite from movement and bobs it while walking.</summary>
    public class CharacterSpriteAnimator : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private SpriteRenderer _renderer;

        [Header("Sprites")]
        [SerializeField] private Sprite _front;
        [SerializeField] private Sprite _back;
        [Tooltip("Side view facing right; flipped for left.")]
        [SerializeField] private Sprite _side;

        [Header("Walk bob")]
        [SerializeField] private float _bobHeight = 0.05f;
        [SerializeField] private float _bobSpeed = 16f;

        // Ignore tiny moves (physics jitter) so the sprite does not flicker between directions.
        private const float MinMoveSqr = 0.000001f;

        private Transform _body;
        private Vector3 _lastBodyPosition;
        private Vector3 _restLocalPosition;
        private float _walkTime;

        private void Awake()
        {
            _body = transform.parent != null ? transform.parent : transform;
            _lastBodyPosition = _body.position;
            _restLocalPosition = transform.localPosition;
        }

        private void LateUpdate()
        {
            // 1. Read this frame's movement from the body.
            Vector2 delta = _body.position - _lastBodyPosition;
            _lastBodyPosition = _body.position;
            bool isMoving = delta.sqrMagnitude > MinMoveSqr;

            // 2. Face the way we moved; keep the last facing when standing still.
            if (isMoving)
            {
                ApplyFacing(delta);
                _walkTime += Time.deltaTime;
            }
            else
            {
                _walkTime = 0f;
            }

            // 3. Bob up and down while walking.
            float bob = isMoving ? Mathf.Abs(Mathf.Sin(_walkTime * _bobSpeed)) * _bobHeight : 0f;
            transform.localPosition = _restLocalPosition + new Vector3(0f, bob, 0f);
        }

        private void ApplyFacing(Vector2 direction)
        {
            if (Mathf.Abs(direction.x) > Mathf.Abs(direction.y))
            {
                _renderer.sprite = _side;
                _renderer.flipX = direction.x < 0f;
            }
            else
            {
                _renderer.sprite = direction.y > 0f ? _back : _front;
                _renderer.flipX = false;
            }
        }
    }
}
