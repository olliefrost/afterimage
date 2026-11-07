// ActorPitDepth: sits on a character's Body child (player or clone). PitVisuals tells it when the
// character is down a pit; it then drops the Body art below the floor with a short fall and moves
// it behind the pit's shadow and front ground. Visual only: colliders and recordings never move.
using UnityEngine;
using UnityEngine.Rendering;

namespace GameJam.Visuals
{
    /// <summary>Draws a character lower and further back while it is inside a pit.</summary>
    [RequireComponent(typeof(SortingGroup))]
    public class ActorPitDepth : MonoBehaviour
    {
        [Header("Fall")]
        [Tooltip("Seconds the drop to the pit floor takes.")]
        [SerializeField] private float _fallDuration = 0.3f;

        private SortingGroup _sortingGroup;
        private Vector3 _restLocalPosition;
        private int _restSortingOrder;
        private float _targetDepth;
        private float _fallTime;
        private bool _isInPit;

        /// <summary>True while the character is drawn down a pit.</summary>
        public bool IsInPit => _isInPit;

        private void Awake()
        {
            _sortingGroup = GetComponent<SortingGroup>();
            _restLocalPosition = transform.localPosition;
            _restSortingOrder = _sortingGroup.sortingOrder;
        }

        /// <summary>
        /// Starts the fall to <paramref name="depth"/> units below the floor and sorts the
        /// character at <paramref name="sortingOrder"/>, behind the pit's covering art.
        /// </summary>
        public void EnterPit(float depth, int sortingOrder)
        {
            _isInPit = true;
            _targetDepth = depth;
            _fallTime = 0f;
            _sortingGroup.sortingOrder = sortingOrder;
        }

        /// <summary>Puts the character straight back on the floor, as on a new attempt.</summary>
        public void ExitPit()
        {
            _isInPit = false;
            _sortingGroup.sortingOrder = _restSortingOrder;
            transform.localPosition = _restLocalPosition;
        }

        private void Update()
        {
            if (!_isInPit)
            {
                return;
            }

            // Ease in so the drop speeds up like a fall.
            _fallTime += Time.deltaTime;
            float progress = _fallDuration > 0f ? Mathf.Clamp01(_fallTime / _fallDuration) : 1f;
            float depth = _targetDepth * progress * progress;
            transform.localPosition = _restLocalPosition + new Vector3(0f, -depth, 0f);
        }
    }
}
