// WallOcclusion: makes individual wall tiles see-through while they hide the live player or the floor
// right around them (for example a pressure plate they are standing on), and turns them solid again
// once the player moves away. Only tiles not behind the player on the grid can fade, so nothing changes
// until a wall actually blocks the view. Snaps every wall solid on a level reset. Visual only; one per
// level scene.
using System.Collections.Generic;
using GameJam.Core;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace GameJam.Visuals
{
    /// <summary>Fades the wall tiles that stand between the camera and the player.</summary>
    public class WallOcclusion : MonoBehaviour, IResettable
    {
        /// <summary>One wall tile being faded.</summary>
        private struct FadedTile
        {
            public int MapIndex;
            public Vector3Int Cell;
            public float Alpha;
            public bool Wanted;
        }

        [Header("References")]
        [Tooltip("The live player's root, where its feet (and collider) are.")]
        [SerializeField] private Transform _player;
        [Tooltip("The player's sprite, so the check covers where they are drawn (including any height).")]
        [SerializeField] private SpriteRenderer _playerSprite;
        [SerializeField] private GridLayout _grid;
        [Tooltip("Tilemaps whose tiles can hide the player (walls, raised walls, ledge faces).")]
        [SerializeField] private Tilemap[] _walls;

        [Header("Look")]
        [Tooltip("Opacity of a wall tile while it hides the player.")]
        [Range(0f, 1f)]
        [SerializeField] private float _fadedAlpha = 0.3f;
        [Tooltip("Seconds a tile takes to fade out or back in.")]
        [SerializeField] private float _fadeSeconds = 0.25f;
        [Tooltip("Extra space around the player's sprite that is kept visible, so the floor they stand on (and a plate there) shows.")]
        [SerializeField] private float _viewPadding = 0.6f;
        [Tooltip("How many grid cells around the player are checked.")]
        [SerializeField] private int _searchRadius = 5;

        private readonly Dictionary<long, FadedTile> _faded = new Dictionary<long, FadedTile>();
        private readonly List<long> _keys = new List<long>();

        private void Awake()
        {
            if (_player == null || _playerSprite == null || _grid == null || _walls == null || _walls.Length == 0)
            {
                Debug.LogError($"{name}: WallOcclusion needs its player, player sprite, grid and wall tilemaps wired.", this);
                enabled = false;
            }
        }

        private void OnEnable()
        {
            LevelReset.Register(this);
        }

        private void OnDisable()
        {
            LevelReset.Unregister(this);
            RestoreAll();
        }

        /// <summary>Makes every faded wall solid at once for the next attempt.</summary>
        public void ResetState()
        {
            RestoreAll();
        }

        /// <summary>
        /// Each frame: marks the wall tiles that overlap the player's view area and are not behind
        /// them, then eases every tracked tile towards faded or solid.
        /// </summary>
        private void Update()
        {
            // 1. The area that must stay visible: the drawn player plus a margin around their feet.
            Rect view = ToRect(_playerSprite.bounds);
            view.xMin -= _viewPadding;
            view.yMin -= _viewPadding;
            view.xMax += _viewPadding;
            view.yMax += _viewPadding;
            Vector3Int playerCell = _grid.WorldToCell(_player.position);
            int playerDepth = playerCell.x + playerCell.y;

            // 2. Nothing is wanted until this frame's check says so.
            _keys.Clear();
            _keys.AddRange(_faded.Keys);
            for (int i = 0; i < _keys.Count; i++)
            {
                FadedTile tile = _faded[_keys[i]];
                tile.Wanted = false;
                _faded[_keys[i]] = tile;
            }

            // 3. Wall tiles on the player's row or nearer the camera that overlap the view are wanted faded.
            for (int m = 0; m < _walls.Length; m++)
            {
                Tilemap map = _walls[m];
                for (int x = -_searchRadius; x <= _searchRadius; x++)
                {
                    for (int y = -_searchRadius; y <= _searchRadius; y++)
                    {
                        Vector3Int cell = new Vector3Int(playerCell.x + x, playerCell.y + y, 0);
                        if (cell.x + cell.y > playerDepth)
                        {
                            continue;
                        }

                        Sprite sprite = map.GetSprite(cell);
                        if (sprite == null)
                        {
                            continue;
                        }

                        Vector3 centre = map.GetCellCenterWorld(cell);
                        Rect tileRect = ToRect(sprite.bounds);
                        tileRect.center += new Vector2(centre.x, centre.y);
                        if (tileRect.Overlaps(view))
                        {
                            Want(m, cell);
                        }
                    }
                }
            }

            // 4. Ease tracked tiles; forget the ones that are solid again.
            float step = _fadeSeconds > 0f ? Time.deltaTime / _fadeSeconds : 1f;
            _keys.Clear();
            _keys.AddRange(_faded.Keys);
            for (int i = 0; i < _keys.Count; i++)
            {
                FadedTile tile = _faded[_keys[i]];
                float target = tile.Wanted ? _fadedAlpha : 1f;
                if (!Mathf.Approximately(tile.Alpha, target))
                {
                    tile.Alpha = Mathf.MoveTowards(tile.Alpha, target, step);
                    SetAlpha(_walls[tile.MapIndex], tile.Cell, tile.Alpha);
                }

                if (!tile.Wanted && tile.Alpha >= 1f)
                {
                    _faded.Remove(_keys[i]);
                }
                else
                {
                    _faded[_keys[i]] = tile;
                }
            }
        }

        /// <summary>Marks a tile as wanted faded, starting to track it (and unlocking its colour) the first time.</summary>
        private void Want(int mapIndex, Vector3Int cell)
        {
            long key = Key(mapIndex, cell);
            if (!_faded.TryGetValue(key, out FadedTile tile))
            {
                // Only the colour lock is cleared, so other tile flags (like a locked transform) keep working.
                Tilemap map = _walls[mapIndex];
                map.SetTileFlags(cell, map.GetTileFlags(cell) & ~TileFlags.LockColor);
                tile = new FadedTile { MapIndex = mapIndex, Cell = cell, Alpha = 1f };
            }

            tile.Wanted = true;
            _faded[key] = tile;
        }

        /// <summary>Sets every tracked tile back to solid and stops tracking them.</summary>
        private void RestoreAll()
        {
            foreach (FadedTile tile in _faded.Values)
            {
                SetAlpha(_walls[tile.MapIndex], tile.Cell, 1f);
            }

            _faded.Clear();
        }

        /// <summary>Changes only the alpha of one tile's colour.</summary>
        private static void SetAlpha(Tilemap map, Vector3Int cell, float alpha)
        {
            Color colour = map.GetColor(cell);
            colour.a = alpha;
            map.SetColor(cell, colour);
        }

        /// <summary>A 2D screen-plane rectangle from 3D bounds, so depth (z) never affects the overlap test.</summary>
        private static Rect ToRect(Bounds bounds)
        {
            return new Rect(bounds.min.x, bounds.min.y, bounds.size.x, bounds.size.y);
        }

        /// <summary>A unique number for a tile in one of the wall maps.</summary>
        private static long Key(int mapIndex, Vector3Int cell)
        {
            return ((long)mapIndex << 40) | ((long)(cell.x + 0x8000) << 20) | (long)(cell.y + 0x8000);
        }
    }
}
