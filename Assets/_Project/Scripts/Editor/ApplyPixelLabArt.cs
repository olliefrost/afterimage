// ApplyPixelLabArt: editor-only, one-off. Tools > GameJam > Apply PixelLab Art swaps the PixelLab art
// into the existing game: the main character (Player and Clone), the floor tiles, the press button
// and the door with its opening animation. It edits assets through Unity's APIs, so references and
// .meta files stay intact, and it is safe to run again. Walls are not swapped (they need a repaint).
using System.IO;
using System.Linq;
using System.Reflection;
using GameJam.Interactables;
using GameJam.Visuals;
using UnityEditor;
using UnityEditor.Events;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Tilemaps;

namespace GameJam.EditorTools
{
    /// <summary>Replaces placeholder art in prefabs and tiles with the generated PixelLab sprites.</summary>
    public static class ApplyPixelLabArt
    {
        private const string Prefabs = "Assets/_Project/Prefabs";
        private const string Art = "Assets/_Project/Art";
        private const string CharacterFolder = "Assets/Sprites/main_character";

        // The door faces south-west to sit in the back-right wall. Its pivot is the centre of its base,
        // which is placed on the centre of the old placeholder door's base.
        private const string DoorFacing = "sw";
        private static readonly Vector2 DoorPivot = new Vector2(63f / 128f, 20f / 128f);
        private static readonly Vector3 DoorVisualPosition = new Vector3(-0.5f, 0.25f, 0f);

        [MenuItem("Tools/GameJam/Apply PixelLab Art")]
        private static void Apply()
        {
            ApplyCharacter();
            ApplyFloorTiles();
            ApplyButton();
            ApplyDoor();
            AssetDatabase.SaveAssets();
            Debug.Log("PixelLab art applied: character, floor tiles, press button and door.");
        }

        /// <summary>Builds the character sprite set and gives it to the Player and Clone animators.</summary>
        private static void ApplyCharacter()
        {
            CharacterSpriteSet set = CharacterSpriteSetBuilder.BuildFromFolder(CharacterFolder);
            if (set == null)
            {
                return;
            }

            foreach (string prefab in new[] { "Player", "Clone" })
            {
                EditPrefab(prefab, root =>
                {
                    var animator = root.GetComponentInChildren<CharacterSpriteAnimator>(true);
                    if (animator == null)
                    {
                        Debug.LogWarning($"{prefab} has no CharacterSpriteAnimator.");
                        return;
                    }

                    var so = new SerializedObject(animator);
                    so.FindProperty("_sprites").objectReferenceValue = set;
                    so.ApplyModifiedPropertiesWithoutUndo();

                    // Show the south-facing idle in the editor too, not only once play starts.
                    var renderer = (SpriteRenderer)so.FindProperty("_renderer").objectReferenceValue;
                    if (renderer != null)
                    {
                        renderer.sprite = set.Get(6).Idle;
                    }
                });
            }
        }

        /// <summary>Points the existing floor Tile assets at the new floor sprites, so painted floors update.</summary>
        private static void ApplyFloorTiles()
        {
            SetTileSprite("tile_iso_floor", "spr_floor_panel");
            SetTileSprite("tile_iso_floor_cracked", "spr_floor_cracked");
            SetTileSprite("tile_iso_floor_moss", "spr_floor_moss");
            SetTileSprite("tile_iso_floor_grass", "spr_floor_grass");
        }

        /// <summary>Gives the press button the pedestal art and hides the old base/cap split.</summary>
        private static void ApplyButton()
        {
            Sprite idle = LoadSprite($"{Art}/Interactables/Buttons/spr_button_pedestal_idle_se.png");
            Sprite pressed = LoadSprite($"{Art}/Interactables/Buttons/spr_button_pedestal_pressed_se.png");
            if (idle == null || pressed == null)
            {
                return;
            }

            EditPrefab("PressButton", root =>
            {
                var button = root.GetComponentInChildren<PressButton>(true);
                var so = new SerializedObject(button);
                so.FindProperty("_unpressedSprite").objectReferenceValue = idle;
                so.FindProperty("_pressedSprite").objectReferenceValue = pressed;
                so.FindProperty("_unpressedColor").colorValue = Color.white;
                so.FindProperty("_pressedColor").colorValue = Color.white;
                so.ApplyModifiedPropertiesWithoutUndo();

                // The pedestal is one sprite, so every other art renderer (the old base or cap) is hidden.
                var main = (SpriteRenderer)so.FindProperty("_renderer").objectReferenceValue;
                if (main == null)
                {
                    Debug.LogWarning("PressButton has no renderer assigned.");
                    return;
                }

                main.sprite = idle;
                foreach (Transform child in root.transform)
                {
                    if ((child.name == "Base" || child.name == "Cap") && child != main.transform)
                    {
                        child.gameObject.SetActive(false);
                    }
                }
            });
        }

        /// <summary>Gives the door the blast-door art and a flipbook that opens and closes with it.</summary>
        private static void ApplyDoor()
        {
            string closedPath = $"{Art}/Interactables/Doors/spr_door_closed_{DoorFacing}.png";
            string[] framePaths = Directory.GetFiles($"{Art}/Interactables/Doors/Open", $"spr_door_open_{DoorFacing}_*.png")
                .Select(p => p.Replace('\\', '/'))
                .OrderBy(p => p)
                .ToArray();

            // 1. Pivot every door image on the centre of its base so all frames line up.
            foreach (string path in framePaths.Prepend(closedPath))
            {
                SetPivot(path, DoorPivot);
            }

            Sprite closed = LoadSprite(closedPath);
            Sprite[] frames = framePaths.Select(LoadSprite).Where(s => s != null).ToArray();
            if (closed == null || frames.Length == 0)
            {
                return;
            }

            EditPrefab("Door", root =>
            {
                // 2. Real art: no tint, closed sprite, placed on the old door's base.
                var door = root.GetComponentInChildren<Door>(true);
                var so = new SerializedObject(door);
                so.FindProperty("_closedColor").colorValue = Color.white;
                so.FindProperty("_openColor").colorValue = Color.white;
                so.ApplyModifiedPropertiesWithoutUndo();

                var renderer = (SpriteRenderer)so.FindProperty("_renderer").objectReferenceValue;
                renderer.sprite = closed;
                renderer.transform.localPosition = DoorVisualPosition;

                // 3. Flipbook on the same object, closed (frame 0) to open (last frame).
                var flipbook = renderer.GetComponent<SpriteFlipbook>();
                if (flipbook == null)
                {
                    flipbook = renderer.gameObject.AddComponent<SpriteFlipbook>();
                }
                var flip = new SerializedObject(flipbook);
                flip.FindProperty("_renderer").objectReferenceValue = renderer;
                SerializedProperty list = flip.FindProperty("_frames");
                list.arraySize = frames.Length;
                for (int i = 0; i < frames.Length; i++)
                {
                    list.GetArrayElementAtIndex(i).objectReferenceValue = frames[i];
                }

                flip.FindProperty("_loop").boolValue = false;
                flip.FindProperty("_playOnEnable").boolValue = false;
                flip.ApplyModifiedPropertiesWithoutUndo();

                // 4. Door events drive the flipbook; old listeners to it are cleared so reruns don't stack.
                Wire(door, "_onOpened", flipbook, flipbook.PlayForward);
                Wire(door, "_onClosed", flipbook, flipbook.PlayBackward);
            });
        }

        private static void Wire(Door door, string field, Object target, UnityAction action)
        {
            var evt = (UnityEvent)typeof(Door).GetField(field, BindingFlags.NonPublic | BindingFlags.Instance).GetValue(door);
            for (int i = evt.GetPersistentEventCount() - 1; i >= 0; i--)
            {
                if (evt.GetPersistentTarget(i) == target)
                {
                    UnityEventTools.RemovePersistentListener(evt, i);
                }
            }

            UnityEventTools.AddVoidPersistentListener(evt, action);
        }

        /// <summary>Opens a prefab, lets <paramref name="edit"/> change it, then saves it.</summary>
        private static void EditPrefab(string name, System.Action<GameObject> edit)
        {
            string path = $"{Prefabs}/{name}.prefab";
            GameObject root = PrefabUtility.LoadPrefabContents(path);
            try
            {
                edit(root);
                PrefabUtility.SaveAsPrefabAsset(root, path);
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(root);
            }
        }

        private static void SetTileSprite(string tileName, string spriteName)
        {
            var tile = AssetDatabase.LoadAssetAtPath<Tile>($"{Art}/Placeholders/Tiles/{tileName}.asset");
            Sprite sprite = LoadSprite($"{Art}/Environment/Floors/{spriteName}.png");
            if (tile == null || sprite == null)
            {
                Debug.LogWarning($"Could not swap {tileName} to {spriteName}.");
                return;
            }

            tile.sprite = sprite;
            EditorUtility.SetDirty(tile);
        }

        private static void SetPivot(string path, Vector2 pivot)
        {
            if (!(AssetImporter.GetAtPath(path) is TextureImporter importer))
            {
                return;
            }

            var settings = new TextureImporterSettings();
            importer.ReadTextureSettings(settings);
            settings.spriteAlignment = (int)SpriteAlignment.Custom;
            settings.spritePivot = pivot;
            importer.SetTextureSettings(settings);
            importer.SaveAndReimport();
        }

        private static Sprite LoadSprite(string path)
        {
            var sprite = AssetDatabase.LoadAssetAtPath<Sprite>(path);
            if (sprite == null)
            {
                Debug.LogWarning($"Missing sprite: {path}");
            }

            return sprite;
        }
    }
}
