// CharacterSpriteSetBuilder: editor-only. Right-click a PixelLab character export folder (the one
// holding <State>/rotations and <State>/animations, e.g. Assets/Sprites/main_character) and choose
// Create > GameJam > Character Sprite Set From Folder. It gives every PNG pixel-art import settings
// (64 PPU, Point, uncompressed, pivot at the feet) and builds a CharacterSpriteSet asset in that folder
// for CharacterSpriteAnimator to use.
using System.IO;
using System.Linq;
using GameJam.Visuals;
using UnityEditor;
using UnityEngine;

namespace GameJam.EditorTools
{
    /// <summary>Builds a CharacterSpriteSet from a PixelLab character folder.</summary>
    public static class CharacterSpriteSetBuilder
    {
        private const string MenuPath = "Assets/Create/GameJam/Character Sprite Set From Folder";
        private const float PixelsPerUnit = 64f;

        [MenuItem(MenuPath, true)]
        private static bool CanBuild()
        {
            return AssetDatabase.IsValidFolder(SelectedPath());
        }

        [MenuItem(MenuPath)]
        private static void BuildFromSelection()
        {
            CharacterSpriteSet set = BuildFromFolder(SelectedPath());
            if (set != null)
            {
                Selection.activeObject = set;
            }
        }

        /// <summary>
        /// Fixes import settings for the folder's PNGs, then creates or refreshes
        /// <c>&lt;folder&gt;/&lt;folder name&gt;_SpriteSet.asset</c>. Returns null if the folder is not a PixelLab export.
        /// </summary>
        public static CharacterSpriteSet BuildFromFolder(string root)
        {
            // 1. Find the rotations folder and the first animation (e.g. Running) beside it.
            string rotations = Directory.GetDirectories(root, "rotations", SearchOption.AllDirectories)
                .Select(Normalise).FirstOrDefault();
            if (rotations == null)
            {
                Debug.LogError($"No 'rotations' folder under {root}; is this a PixelLab character export?");
                return null;
            }

            string animations = Normalise(Path.Combine(Path.GetDirectoryName(rotations), "animations"));
            string run = Directory.Exists(animations)
                ? Directory.GetDirectories(animations).Select(Normalise).FirstOrDefault()
                : null;

            // 2. Give every PNG pixel-art import settings so sprites are crisp and stand on their feet.
            string[] pngs = Directory.GetFiles(root, "*.png", SearchOption.AllDirectories).Select(Normalise).ToArray();
            AssetDatabase.StartAssetEditing();
            try
            {
                foreach (string png in pngs)
                {
                    ApplyImportSettings(png);
                }
            }
            finally
            {
                AssetDatabase.StopAssetEditing();
            }

            // 3. Collect the idle sprite and run frames for each of the 8 directions.
            var directions = new DirectionSprites[CharacterSpriteSet.DirectionCount];
            for (int i = 0; i < directions.Length; i++)
            {
                string name = CharacterSpriteSet.DirectionNames[i];
                directions[i].Idle = AssetDatabase.LoadAssetAtPath<Sprite>($"{rotations}/{name}.png");
                directions[i].Run = LoadFrames(run == null ? null : $"{run}/{name}");
                if (directions[i].Idle == null)
                {
                    Debug.LogWarning($"Missing idle sprite for '{name}' in {rotations}.");
                }
            }

            // 4. Save the set next to the art, refreshing an existing one so references to it survive.
            string assetPath = $"{root}/{Path.GetFileName(root)}_SpriteSet.asset";
            var set = AssetDatabase.LoadAssetAtPath<CharacterSpriteSet>(assetPath);
            if (set == null)
            {
                set = ScriptableObject.CreateInstance<CharacterSpriteSet>();
                AssetDatabase.CreateAsset(set, assetPath);
            }

            set.SetDirections(directions);
            EditorUtility.SetDirty(set);
            AssetDatabase.SaveAssets();
            return set;
        }

        private static void ApplyImportSettings(string path)
        {
            if (!(AssetImporter.GetAtPath(path) is TextureImporter importer))
            {
                return;
            }

            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.spritePixelsPerUnit = PixelsPerUnit;
            importer.filterMode = FilterMode.Point;
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            importer.mipmapEnabled = false;

            var settings = new TextureImporterSettings();
            importer.ReadTextureSettings(settings);
            settings.spriteAlignment = (int)SpriteAlignment.BottomCenter;
            importer.SetTextureSettings(settings);
            importer.SaveAndReimport();
        }

        /// <summary>Loads frame_*.png sprites in name order, or an empty array if the folder is missing.</summary>
        private static Sprite[] LoadFrames(string folder)
        {
            if (folder == null || !Directory.Exists(folder))
            {
                return new Sprite[0];
            }

            return Directory.GetFiles(folder, "frame_*.png")
                .Select(Normalise)
                .OrderBy(p => p)
                .Select(AssetDatabase.LoadAssetAtPath<Sprite>)
                .Where(s => s != null)
                .ToArray();
        }

        private static string SelectedPath()
        {
            return Selection.activeObject == null ? string.Empty : AssetDatabase.GetAssetPath(Selection.activeObject);
        }

        private static string Normalise(string path)
        {
            return path.Replace('\\', '/');
        }
    }
}
