// HoverStairs: Chamber 3's four hovering plates. While the raise plate is held they flip up into a
// four-step staircase in front of the raised balcony; otherwise they lie flat on the floor (but they
// never drop while someone is still standing on them). They slide between three stops along the
// balcony: to the middle while the middle plate is held, else to the left while the left plate is
// held, else back to the right. Anyone on them is carried: the live player through
// PlayerMovement.AddCarry, clones through their recorded positions.
// Height is visual only: physics stays flat. The balcony's front edge is a row of wall blocks; the
// raised stairs remove the blocks in their columns so the top step leads onto the balcony, and
// ElevationField asks StepFraction to draw anyone on the steps at the right height.
using GameJam.Actors;
using GameJam.Core;
using GameJam.Interactables;
using GameJam.Player;
using GameJam.Visuals;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace GameJam.Machines
{
    /// <summary>Moving stairs that open a way through the balcony's front edge where they stand.</summary>
    [DefaultExecutionOrder(-10)]
    [RequireComponent(typeof(Rigidbody2D))]
    public class HoverStairs : MonoBehaviour, IResettable
    {
        [Header("References")]
        [SerializeField] private TickClock _clock;
        [Tooltip("Used to measure positions in grid cells.")]
        [SerializeField] private GridLayout _grid;
        [Tooltip("Gives the balcony height the steps climb to.")]
        [SerializeField] private ElevationField _elevation;
        [Tooltip("Held: the plates flip up into stairs.")]
        [SerializeField] private HoldPlate _raisePlate;
        [Tooltip("Held: the stairs slide to the left stop.")]
        [SerializeField] private HoldPlate _leftPlate;
        [Tooltip("Held: the stairs slide to the middle stop (wins over the left plate).")]
        [SerializeField] private HoldPlate _middlePlate;
        [SerializeField] private Transform _rightStop;
        [SerializeField] private Transform _leftStop;
        [SerializeField] private Transform _middleStop;
        [Tooltip("The stairs' footprint; anyone inside is on the stairs, and the live player is carried.")]
        [SerializeField] private ActorZone _riders;
        [SerializeField] private PlayerMovement _player;
        [Tooltip("Rails along the stairs' sides, so you get on at the bottom and off at the top only.")]
        [SerializeField] private Collider2D[] _sideRails;
        [Tooltip("Blocks just outside the rails on the edge row, closing edge columns the stairs only partly cover while sliding.")]
        [SerializeField] private Collider2D[] _edgeFillers;

        [Header("Balcony front edge")]
        [Tooltip("One blocker per grid column of the balcony's front edge.")]
        [SerializeField] private Collider2D[] _edgeBlockers;
        [Tooltip("The front edge's wall blocks; the ones under the raised stairs are removed.")]
        [SerializeField] private Tilemap _edgeTiles;
        [Tooltip("Grid row of the front edge (the top step's row).")]
        [SerializeField] private int _edgeRow = 14;

        [Header("Steps")]
        [Tooltip("The step plates, bottom step first. Each is lifted one rise per step while raised.")]
        [SerializeField] private Transform[] _steps;
        [Tooltip("Grid row of the bottom step.")]
        [SerializeField] private int _bottomRow = 11;
        [Tooltip("Width of the stairs along the balcony, in grid columns.")]
        [SerializeField] private float _widthCells = 2f;

        [Header("Tuning")]
        [Tooltip("Units moved per tick while sliding between stops.")]
        [SerializeField] private float _slideSpeed = 0.035f;

        [Header("Visuals")]
        [SerializeField] private Color _loweredColor = new Color(0.3f, 0.9f, 0.45f, 0.35f);
        [SerializeField] private Color _raisedColor = new Color(0.45f, 1f, 0.6f, 1f);

        private Rigidbody2D _body;
        private PlayerActor _playerActor;
        private SpriteRenderer[][] _stepSprites;
        private Vector3[] _stepRest;
        private int[] _edgeColumns;
        private TileBase[] _edgeTileArt;
        private bool[] _edgeOpen;

        /// <summary>True while the plates are flipped up into stairs.</summary>
        public bool IsRaised { get; private set; }

        private void Awake()
        {
            _body = GetComponent<Rigidbody2D>();
            _playerActor = _player.GetComponent<PlayerActor>();

            _stepSprites = new SpriteRenderer[_steps.Length][];
            _stepRest = new Vector3[_steps.Length];
            for (int i = 0; i < _steps.Length; i++)
            {
                _stepSprites[i] = _steps[i].GetComponentsInChildren<SpriteRenderer>();
                _stepRest[i] = _steps[i].localPosition;
            }

            // Each edge blocker knows its own column, and the wall tile it hides, so the list can be in any order.
            _edgeColumns = new int[_edgeBlockers.Length];
            _edgeTileArt = new TileBase[_edgeBlockers.Length];
            _edgeOpen = new bool[_edgeBlockers.Length];
            for (int i = 0; i < _edgeBlockers.Length; i++)
            {
                _edgeColumns[i] = Mathf.FloorToInt(CellOf(_edgeBlockers[i].bounds.center).x);
                _edgeTileArt[i] = _edgeTiles.GetTile(new Vector3Int(_edgeColumns[i], _edgeRow, 0));
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
            ApplyRaised(false);
            UpdateEdge(_rightStop.position, true);
        }

        /// <summary>
        /// How far up the stairs a world position is: 0 to 1 by whole steps, or -1 if it is not on
        /// the raised stairs. ElevationField uses it to draw characters climbing.
        /// </summary>
        public float StepFraction(Vector2 worldPosition)
        {
            if (!IsRaised)
            {
                return -1f;
            }

            Vector3 cell = CellOf(worldPosition);
            float centreColumn = CellOf(_body.position).x;
            bool inColumns = Mathf.Abs(cell.x - centreColumn) <= _widthCells * 0.5f;
            float row = cell.y - _bottomRow;
            if (!inColumns || row < 0f || row >= _steps.Length)
            {
                return -1f;
            }

            return (Mathf.Floor(row) + 1f) / _steps.Length;
        }

        private void FixedUpdate()
        {
            if (!_clock.IsRunning)
            {
                return;
            }

            // 1. Flip up with the raise plate; only flip down once nobody is standing on the steps,
            //    so the edge never closes on top of someone.
            bool raised = _raisePlate.IsActive || (IsRaised && _riders.HasLivingActor);
            if (raised != IsRaised)
            {
                ApplyRaised(raised);
            }

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

            // 4. Open the balcony's front edge only under the raised stairs.
            UpdateEdge(next, false);
        }

        /// <summary>Flips the steps up or down: rails, fillers, step heights and colours.</summary>
        private void ApplyRaised(bool raised)
        {
            IsRaised = raised;
            SetEnabled(_sideRails, raised);
            SetEnabled(_edgeFillers, raised);

            // Raised steps climb one rise each; lowered plates lie flat and faint on the floor.
            Color colour = raised ? _raisedColor : _loweredColor;
            for (int i = 0; i < _steps.Length; i++)
            {
                float rise = raised ? _elevation.Height * (i + 1) / _steps.Length : 0f;
                _steps[i].localPosition = _stepRest[i] + new Vector3(0f, rise, 0f);
                for (int s = 0; s < _stepSprites[i].Length; s++)
                {
                    _stepSprites[i][s].color = colour;
                }
            }
        }

        /// <summary>
        /// Opens the edge columns the raised stairs overlap and closes the rest, tiles included.
        /// Only changed columns are touched, unless <paramref name="force"/> redraws them all.
        /// While sliding, a partly covered column is closed off by the edge fillers instead.
        /// </summary>
        private void UpdateEdge(Vector2 stairsPosition, bool force)
        {
            float centreColumn = CellOf(stairsPosition).x;
            float reach = (_widthCells + 1f) * 0.5f - 0.01f;
            for (int i = 0; i < _edgeBlockers.Length; i++)
            {
                bool open = IsRaised && Mathf.Abs(_edgeColumns[i] + 0.5f - centreColumn) < reach;
                if (open == _edgeOpen[i] && !force)
                {
                    continue;
                }

                _edgeOpen[i] = open;
                _edgeBlockers[i].enabled = !open;
                _edgeTiles.SetTile(new Vector3Int(_edgeColumns[i], _edgeRow, 0), open ? null : _edgeTileArt[i]);
            }
        }

        private static void SetEnabled(Collider2D[] colliders, bool enabled)
        {
            for (int i = 0; i < colliders.Length; i++)
            {
                colliders[i].enabled = enabled;
            }
        }

        /// <summary>A world position in fractional grid cells.</summary>
        private Vector3 CellOf(Vector3 worldPosition)
        {
            return _grid.LocalToCellInterpolated(_grid.WorldToLocal(worldPosition));
        }
    }
}
