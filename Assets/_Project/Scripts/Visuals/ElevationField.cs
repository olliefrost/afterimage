// ElevationField: answers "how high is the ground here?" for a level with raised areas (Chamber 3's
// balcony and hover stairs, Chamber 4's ladders). ActorElevation asks it every frame to lift characters' art. Visual only:
// physics and recordings stay flat. Levels without a field have height 0 everywhere.
using GameJam.Interactables;
using GameJam.Machines;
using UnityEngine;

namespace GameJam.Visuals
{
    /// <summary>The visual ground height at any point of the level.</summary>
    public class ElevationField : MonoBehaviour
    {
        [Header("References")]
        [Tooltip("Used to convert world positions into grid cells.")]
        [SerializeField] private GridLayout _grid;
        [Tooltip("Optional moving stairs that climb from the floor to the raised areas.")]
        [SerializeField] private HoverStairs _stairs;
        [Header("Raised areas")]
        [Tooltip("Height of the raised areas in units. The hover stairs' steps climb to this height too.")]
        [SerializeField] private float _height = 1.5f;
        [Tooltip("Raised areas in grid cells (x, y = first cell; width, height = cell counts).")]
        [SerializeField] private RectInt[] _raisedAreas;

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

        /// <summary>The ground height at a point: the stairs' step height first, then a ladder, then any raised area, else the floor.</summary>
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

            // 2. On a ladder: the climb so far.
            Ladder ladder = Ladder.At(worldPosition);
            if (ladder != null)
            {
                return _height * ladder.Fraction(worldPosition);
            }

            // 3. On a raised area: full height.
            Vector3 cell = _grid.LocalToCellInterpolated(_grid.WorldToLocal(worldPosition));
            for (int i = 0; i < _raisedAreas.Length; i++)
            {
                RectInt area = _raisedAreas[i];
                if (cell.x >= area.xMin && cell.x < area.xMax && cell.y >= area.yMin && cell.y < area.yMax)
                {
                    return _height;
                }
            }

            return 0f;
        }
    }
}
