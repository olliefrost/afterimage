// ChamberMap: the hold-Tab overview of the whole chamber. On start it copies the room's floor and
// wall tilemaps into flat map tilemaps on the Map layer, and fits its own map camera to the room
// bounds. While PlayerInputReader.MapHeld is true the map camera draws over the normal view, which
// shows those tiles plus the MapMarker children on the gameplay prefabs (player, clones, buttons,
// pits, doors, exit). Visual only: nothing in gameplay reads it. One ChamberMap prefab per chamber.
using GameJam.Player;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace GameJam.Visuals
{
    /// <summary>Builds a simplified map of the chamber and shows it while the map key is held.</summary>
    public class ChamberMap : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private PlayerInputReader _input;
        [Tooltip("Camera that renders only the Map layer. Kept off until the map key is held.")]
        [SerializeField] private Camera _mapCamera;
        [Tooltip("The room's Grid; the map tilemaps are added under it so they line up with the real tiles.")]
        [SerializeField] private Grid _grid;
        [Tooltip("Tilemaps drawn as walkable floor on the map.")]
        [SerializeField] private Tilemap[] _floorSources;
        [Tooltip("Tilemaps drawn as walls on the map (walls and ledges).")]
        [SerializeField] private Tilemap[] _wallSources;
        [Tooltip("Box covering the room, the same one the main camera is clamped to. The map camera fits it.")]
        [SerializeField] private BoxCollider2D _roomBounds;

        [Header("Look")]
        [Tooltip("Plain diamond tile used for every map cell, tinted per tilemap.")]
        [SerializeField] private TileBase _mapTile;
        [Tooltip("Unlit sprite material, so the dark room lighting does not touch the map.")]
        [SerializeField] private Material _mapMaterial;
        [SerializeField] private Color _floorColour = new Color(0.12f, 0.17f, 0.22f);
        [SerializeField] private Color _wallColour = new Color(0.55f, 0.8f, 0.85f);
        [Tooltip("Empty space kept around the room on the map, in world units.")]
        [SerializeField] private float _margin = 0.5f;

        private void Start()
        {
            // 1. Floor first, walls on top, both on the Map layer.
            BuildLayer("MapFloor", _floorSources, _floorColour, -10);
            BuildLayer("MapWalls", _wallSources, _wallColour, -9);

            _mapCamera.enabled = false;
        }

        private void Update()
        {
            bool show = _input.MapHeld;
            if (show && !_mapCamera.enabled)
            {
                // 2. Frame the whole room each time the map opens, in case the window size changed.
                FitCamera();
            }

            _mapCamera.enabled = show;
        }

        /// <summary>Creates one flat map tilemap with a tile in every cell any source tilemap uses.</summary>
        private void BuildLayer(string layerName, Tilemap[] sources, Color colour, int sortingOrder)
        {
            GameObject layerObject = new GameObject(layerName, typeof(Tilemap), typeof(TilemapRenderer));
            layerObject.layer = LayerMask.NameToLayer("Map");
            layerObject.transform.SetParent(_grid.transform, false);

            Tilemap map = layerObject.GetComponent<Tilemap>();
            map.color = colour;
            TilemapRenderer mapRenderer = layerObject.GetComponent<TilemapRenderer>();
            mapRenderer.sharedMaterial = _mapMaterial;
            mapRenderer.sortingOrder = sortingOrder;

            for (int i = 0; i < sources.Length; i++)
            {
                foreach (Vector3Int cell in sources[i].cellBounds.allPositionsWithin)
                {
                    if (sources[i].HasTile(cell))
                    {
                        map.SetTile(cell, _mapTile);
                    }
                }
            }
        }

        /// <summary>Centres the map camera on the room and sizes it so the whole room fits on screen.</summary>
        private void FitCamera()
        {
            Bounds room = _roomBounds.bounds;
            float halfHeight = room.extents.y + _margin;
            float halfWidth = room.extents.x + _margin;
            _mapCamera.orthographicSize = Mathf.Max(halfHeight, halfWidth / _mapCamera.aspect);
            _mapCamera.transform.position = new Vector3(room.center.x, room.center.y, _mapCamera.transform.position.z);
        }
    }
}
