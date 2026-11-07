// HoverStairs: Chamber 3's four hovering plates. While the raise plate is held they flip up into a
// staircase across the raised ledge's wall face; otherwise they hover flat and can't be walked on.
// They slide between three stops along the back wall: to the middle while the middle plate is held,
// else to the left while the left plate is held, else back to the right. Anyone on them is carried:
// the live player through PlayerMovement.AddCarry, clones through their recorded positions.
// Height is faked: the ledges are floor further back, and the face strip in front of them is solid
// except where the raised stairs cover it.
using GameJam.Actors;
using GameJam.Core;
using GameJam.Interactables;
using GameJam.Player;
using UnityEngine;

namespace GameJam.Machines
{
    /// <summary>Moving stairs that open a way across the ledge face where they stand.</summary>
    [DefaultExecutionOrder(-10)]
    [RequireComponent(typeof(Rigidbody2D))]
    public class HoverStairs : MonoBehaviour, IResettable
    {
        [Header("References")]
        [SerializeField] private TickClock _clock;
        [Tooltip("Used to measure positions along the back wall in grid cells.")]
        [SerializeField] private GridLayout _grid;
        [Tooltip("Held: the plates flip up into stairs.")]
        [SerializeField] private HoldPlate _raisePlate;
        [Tooltip("Held: the stairs slide to the left stop.")]
        [SerializeField] private HoldPlate _leftPlate;
        [Tooltip("Held: the stairs slide to the middle stop (wins over the left plate).")]
        [SerializeField] private HoldPlate _middlePlate;
        [SerializeField] private Transform _rightStop;
        [SerializeField] private Transform _leftStop;
        [SerializeField] private Transform _middleStop;
        [Tooltip("The stairs' footprint; the live player inside it is carried.")]
        [SerializeField] private ActorZone _riders;
        [SerializeField] private PlayerMovement _player;
        [Tooltip("Walls on the stairs' open sides, so riders can't step off into the face strip.")]
        [SerializeField] private Collider2D[] _sideWalls;
        [Tooltip("One blocker per grid column of the ledge face; the ones under the raised stairs open up.")]
        [SerializeField] private Collider2D[] _faceBlockers;
        [SerializeField] private SpriteRenderer[] _plates;

        [Header("Tuning")]
        [Tooltip("Units moved per tick while sliding between stops.")]
        [SerializeField] private float _slideSpeed = 0.035f;
        [Tooltip("Width of the stairs along the back wall, in grid columns.")]
        [SerializeField] private float _widthCells = 2f;

        [Header("Visuals")]
        [SerializeField] private Color _hoveringColor = new Color(0.3f, 0.9f, 0.45f, 0.35f);
        [SerializeField] private Color _raisedColor = new Color(0.45f, 1f, 0.6f, 1f);

        private Rigidbody2D _body;
        private PlayerActor _playerActor;
        private float[] _blockerColumns;
        private bool _isRaised;

        private void Awake()
        {
            _body = GetComponent<Rigidbody2D>();
            _playerActor = _player.GetComponent<PlayerActor>();

            // Each face blocker's position along the back wall, in grid columns, for the overlap test.
            _blockerColumns = new float[_faceBlockers.Length];
            for (int i = 0; i < _faceBlockers.Length; i++)
            {
                _blockerColumns[i] = ColumnOf(_faceBlockers[i].bounds.center);
            }
        }

        private void OnEnable()
        {
            LevelReset.Register(this);
            ResetState();
        }

        private void OnDisable()
        {
            LevelReset.Unregister(this);
        }

        /// <summary>Puts the stairs back at the right stop, flat, for the next attempt.</summary>
        public void ResetState()
        {
            _body.position = _rightStop.position;
            transform.position = _rightStop.position;
            SetRaised(false);
            UpdateFaceBlockers(_rightStop.position);
        }

        private void FixedUpdate()
        {
            if (!_clock.IsRunning)
            {
                return;
            }

            // 1. Flip up or down with the raise plate.
            SetRaised(_raisePlate.IsActive);

            // 2. Slide towards the stop the held plates ask for.
            Vector2 target = _middlePlate.IsActive ? _middleStop.position
                : _leftPlate.IsActive ? _leftStop.position
                : _rightStop.position;
            Vector2 current = _body.position;
            Vector2 next = Vector2.MoveTowards(current, target, _slideSpeed);
            _body.MovePosition(next);

            // 3. Carry the live player if they are standing on the stairs.
            if (next != current && _riders.Contains(_playerActor))
            {
                _player.AddCarry(next - current);
            }

            // 4. Open the face strip only under the raised stairs.
            UpdateFaceBlockers(next);
        }

        private void SetRaised(bool raised)
        {
            _isRaised = raised;
            for (int i = 0; i < _sideWalls.Length; i++)
            {
                _sideWalls[i].enabled = raised;
            }

            Color colour = raised ? _raisedColor : _hoveringColor;
            for (int i = 0; i < _plates.Length; i++)
            {
                _plates[i].color = colour;
            }
        }

        /// <summary>Disables the face blockers the raised stairs overlap, so the stairs can be climbed.</summary>
        private void UpdateFaceBlockers(Vector2 stairsPosition)
        {
            float stairsColumn = ColumnOf(stairsPosition);
            float reach = (_widthCells + 1f) * 0.5f - 0.01f;
            for (int i = 0; i < _faceBlockers.Length; i++)
            {
                bool underStairs = Mathf.Abs(_blockerColumns[i] - stairsColumn) < reach;
                _faceBlockers[i].enabled = !(_isRaised && underStairs);
            }
        }

        /// <summary>A world position's grid column (cell x, fractional) along the back wall.</summary>
        private float ColumnOf(Vector3 worldPosition)
        {
            return _grid.LocalToCellInterpolated(_grid.WorldToLocal(worldPosition)).x;
        }
    }
}
