// ElevationField: answers "how high is the ground here?" for a level with raised areas (Chamber 3's
// balcony and hover stairs, Chamber 4's stacked levels, ladders and rising platforms). ActorElevation asks it every
// frame to lift characters' art. Visual only: physics and recordings stay flat. Levels without a
// field have height 0 everywhere.
using GameJam.Interactables;
using GameJam.Machines;
using UnityEngine;

namespace GameJam.Visuals
{
    /// <summary>A rising platform and the height its top is drawn at once fully up.</summary>
    [System.Serializable]
    public struct PlatformHeight
    {
        public RisingPlatform Platform;
        [Tooltip("Height of the platform's top when fully raised, in units.")]
        public float Height;
    }

    /// <summary>A block of cells raised to its own height (Chamber 4's stacked levels).</summary>
    [System.Serializable]
    public struct LevelArea
    {
        [Tooltip("Cells covered (x, y = first cell; width, height = cell counts).")]
        public RectInt Cells;
        [Tooltip("Ground height of these cells, in units.")]
        public float Height;
    }

    /// <summary>The visual ground height at any point of the level.</summary>
    public class ElevationField : MonoBehaviour
    {
        [Header("References")]
        [Tooltip("Used to convert world positions into grid cells.")]
        [SerializeField] private GridLayout _grid;
        [Tooltip("Optional moving stairs that climb from the floor to the raised areas.")]
        [SerializeField] private HoverStairs _stairs;
        [Tooltip("Optional rising platforms (Chamber 4): anyone on one is drawn at its height, scaled by how far it has risen.")]
        [SerializeField] private PlatformHeight[] _platforms;
        [Header("Raised areas")]
        [Tooltip("Height of the raised areas in units. The hover stairs' steps climb to this height too.")]
        [SerializeField] private float _height = 1.5f;
        [Tooltip("Raised areas in grid cells (x, y = first cell; width, height = cell counts).")]
        [SerializeField] private RectInt[] _raisedAreas;
        [Tooltip("Optional areas with their own heights, for levels with several floors. Checked before the raised areas.")]
        [SerializeField] private LevelArea[] _levels;

        private static ElevationField _current;

        /// <summary>Height of the raised areas in units (the one source for every raised thing in the level).</summary>
        public float Height => _height;

        /// <summary>The ground height at <paramref name="worldPosition"/> in the current level, or 0 if it has no field.</summary>
        public static float HeightAt(Vector2 worldPosition)
        {
            return _current != null ? _current.Sample(worldPosition) : 0f;
        }

        private void OnEnable()
        {
            _current = this;
        }

        private void OnDisable()
        {
            if (_current == this)
            {
                _current = null;
            }
        }

        /// <summary>The ground height at a point: the stairs' step height first, then a rising platform, then a ladder, then any level area or raised area, else the floor.</summary>
        private float Sample(Vector2 worldPosition)
        {
            // 1. On the raised stairs: one rise per step.
            if (_stairs != null)
            {
                float step = _stairs.StepFraction(worldPosition);
                if (step >= 0f)
                {
                    return _height * step;
                }
            }

            // 2. On a rising platform: its height, scaled by how far it has risen.
            if (_platforms != null)
            {
                for (int i = 0; i < _platforms.Length; i++)
                {
                    if (_platforms[i].Platform.Covers(worldPosition))
                    {
                        return _platforms[i].Height * _platforms[i].Platform.Progress;
                    }
                }
            }

            // 3. On a ladder: between its bottom and top heights.
            Ladder ladder = Ladder.At(worldPosition);
            if (ladder != null)
            {
                return ladder.HeightAt(worldPosition);
            }

            // 4. On a level area: its own height.
            Vector3 cell = _grid.LocalToCellInterpolated(_grid.WorldToLocal(worldPosition));
            if (_levels != null)
            {
                for (int i = 0; i < _levels.Length; i++)
                {
                    if (Contains(_levels[i].Cells, cell))
                    {
                        return _levels[i].Height;
                    }
                }
            }

            // 5. On a raised area: full height.
            for (int i = 0; i < _raisedAreas.Length; i++)
            {
                if (Contains(_raisedAreas[i], cell))
                {
                    return _height;
                }
            }

            return 0f;
        }

        /// <summary>True if the fractional cell position lies inside the cell rect.</summary>
        private static bool Contains(RectInt area, Vector3 cell)
        {
            return cell.x >= area.xMin && cell.x < area.xMax && cell.y >= area.yMin && cell.y < area.yMax;
        }
    }
}
