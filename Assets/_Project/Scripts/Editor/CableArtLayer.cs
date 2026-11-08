// CableArtLayer: editor-only tool (GameJam > Lay Cable Art) that gives every floor-level wire in the open
// scene the shared drawn-cable look: rubber cable pieces from Art/Machinery/Cables along each SignalWire
// and SignalCable line, junction boxes at its ends and bends, and the wire's own thin line riding on top
// to show the signal. Re-runnable: it rebuilds one "CableArt" root each time, so run it again after
// moving a wire. Wires drawn on raised levels (sorting order above -90, as in Chamber 4) are skipped,
// since their builder lays their art with the right heights. Never included in builds.
using System.Collections.Generic;
using GameJam.Interactables;
using GameJam.Visuals;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace GameJam.EditorTools
{
    /// <summary>Lays cable sprite pieces under the open scene's floor-level wires.</summary>
    public static class CableArtLayer
    {
        private const string RootName = "CableArt";
        private const string ArtFolder = "Assets/_Project/Art/Machinery/Cables/";
        // Wires sorted above this draw on raised levels; their scene builder lays their art.
        private const int MaxFloorOrder = -90;
        // One floor piece covers one cell along the iso diagonal (0.5 world units across); a wall piece is 0.5 tall.
        private const float CellWidth = 0.5f;
        private const float WallPieceHeight = 0.5f;

        [MenuItem("GameJam/Lay Cable Art")]
        private static void Lay()
        {
            var run = AssetDatabase.LoadAssetAtPath<Sprite>(ArtFolder + "spr_cable_run_x.png");
            var wall = AssetDatabase.LoadAssetAtPath<Sprite>(ArtFolder + "spr_cable_run_wall.png");
            var junction = AssetDatabase.LoadAssetAtPath<Sprite>(ArtFolder + "spr_cable_junction.png");
            if (run == null || wall == null || junction == null)
            {
                Debug.LogError("CableArtLayer: cable sprites are missing from " + ArtFolder);
                return;
            }

            // 1. Start from a clean root, so re-running never doubles the art.
            var old = GameObject.Find(RootName);
            if (old != null)
            {
                Undo.DestroyObjectImmediate(old);
            }

            var root = new GameObject(RootName);
            Undo.RegisterCreatedObjectUndo(root, "Lay Cable Art");

            // 2. Lay pieces under every floor-level wire; shared trunks are only drawn once.
            var placed = new HashSet<Vector3Int>();
            int wires = 0, skipped = 0;
            foreach (var line in Object.FindObjectsByType<LineRenderer>(FindObjectsInactive.Include))
            {
                if (line.GetComponent<SignalWire>() == null && line.GetComponent<SignalCable>() == null)
                {
                    continue;
                }

                if (line.sortingOrder > MaxFloorOrder)
                {
                    skipped++;
                    continue;
                }

                var group = new GameObject(line.name).transform;
                group.SetParent(root.transform, false);
                LayAlong(line, group, run, wall, junction, placed);
                wires++;
            }

            EditorSceneManager.MarkSceneDirty(root.scene);
            Debug.Log($"CableArtLayer: laid cable art under {wires} wires ({skipped} raised wires left to their builder).");
        }

        /// <summary>Lays runs along each straight leg of a wire, then junction boxes on its ends and bends.</summary>
        private static void LayAlong(LineRenderer line, Transform group, Sprite run, Sprite wall, Sprite junction, HashSet<Vector3Int> placed)
        {
            Vector3[] points = WorldPoints(line);
            int pieceOrder = line.sortingOrder - 2;

            for (int i = 1; i < points.Length; i++)
            {
                Vector2 a = points[i - 1], b = points[i];
                Vector2 d = b - a;

                // Straight up or down a wall face: half-unit pieces from the lower end, the last one overlapping.
                if (Mathf.Abs(d.x) < 0.01f && Mathf.Abs(d.y) > 0.01f)
                {
                    float lo = Mathf.Min(a.y, b.y), hi = Mathf.Max(a.y, b.y);
                    int count = Mathf.Max(1, Mathf.CeilToInt((hi - lo) / WallPieceHeight - 0.01f));
                    for (int k = 0; k < count; k++)
                    {
                        float y = hi - lo < WallPieceHeight ? (lo + hi) * 0.5f : Mathf.Min(lo + WallPieceHeight * (k + 0.5f), hi - WallPieceHeight * 0.5f);
                        Piece(group, "Wall", wall, new Vector2(a.x, y), false, line, pieceOrder, placed, 0);
                    }

                    continue;
                }

                // Along the floor: legs follow an iso diagonal (rise half the run). Tiny jogs only get junctions.
                bool diagonal = Mathf.Abs(Mathf.Abs(d.y) - Mathf.Abs(d.x) * 0.5f) < 0.02f + Mathf.Abs(d.x) * 0.05f;
                float cells = Mathf.Abs(d.x) / CellWidth;
                if (!diagonal || cells < 0.25f)
                {
                    continue;
                }

                // Along cell x the world line rises to the right; along cell y it rises to the left (mirrored art).
                bool alongX = Mathf.Sign(d.x) == Mathf.Sign(d.y);
                int pieces = Mathf.Max(1, Mathf.CeilToInt(cells - 0.01f));
                for (int k = 0; k < pieces; k++)
                {
                    float t = cells <= 1f ? 0.5f : Mathf.Lerp(0.5f / cells, 1f - 0.5f / cells, pieces == 1 ? 0.5f : (float)k / (pieces - 1));
                    Piece(group, alongX ? "RunX" : "RunY", run, Vector2.Lerp(a, b, t), !alongX, line, pieceOrder, placed, 1);
                }
            }

            // Junction boxes on the ends and bends, drawn over the runs.
            for (int i = 0; i < points.Length; i++)
            {
                bool end = i == 0 || i == points.Length - 1;
                bool bend = !end && Vector3.Cross(points[i] - points[i - 1], points[i + 1] - points[i]).sqrMagnitude > 0.000001f;
                if (end || bend)
                {
                    Piece(group, "Junction", junction, points[i], false, line, pieceOrder + 1, placed, 2);
                }
            }
        }

        /// <summary>Adds one cable sprite unless an identical piece already sits there (shared trunks).</summary>
        private static void Piece(Transform group, string name, Sprite sprite, Vector2 position, bool flip, LineRenderer line, int order, HashSet<Vector3Int> placed, int kind)
        {
            var key = new Vector3Int(Mathf.RoundToInt(position.x * 32f), Mathf.RoundToInt(position.y * 32f), kind);
            if (!placed.Add(key))
            {
                return;
            }

            var go = new GameObject(name);
            go.transform.SetParent(group, false);
            go.transform.position = position;
            var renderer = go.AddComponent<SpriteRenderer>();
            renderer.sprite = sprite;
            renderer.flipX = flip;
            renderer.sortingLayerID = line.sortingLayerID;
            renderer.sortingOrder = order;
            renderer.spriteSortPoint = SpriteSortPoint.Pivot;   // a renderer added from code sorts by its Center otherwise
        }

        /// <summary>The wire's path in world space, whichever space its LineRenderer stores it in.</summary>
        private static Vector3[] WorldPoints(LineRenderer line)
        {
            var points = new Vector3[line.positionCount];
            line.GetPositions(points);
            if (!line.useWorldSpace)
            {
                for (int i = 0; i < points.Length; i++)
                {
                    points[i] = line.transform.TransformPoint(points[i]);
                }
            }

            return points;
        }
    }
}
