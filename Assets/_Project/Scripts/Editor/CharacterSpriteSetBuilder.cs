// CharacterSpriteSetBuilder: editor-only. Right-click a PixelLab character export folder (the one
// holding <State>/rotations and <State>/animations, e.g. Assets/Sprites/main_character) and choose
// Create > GameJam > Character Sprite Set From Folder. It gives every PNG pixel-art import settings
// (64 PPU, Point, uncompressed, pivot under the lowest foot of each folder) and builds a CharacterSpriteSet asset in that folder
// for CharacterSpriteAnimator to use. Animation folders are matched by name: Running (or the first other
// folder), Jump (8 directions) and Climb (one north-west set, facing the wall).
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
            // 1. Find the rotations folder and the animation folders beside it.
            string rotations = Directory.GetDirectories(root, "rotations", SearchOption.AllDirectories)
                .Select(Normalise).FirstOrDefault();
            if (rotations == null)
            {
                Debug.LogError($"No 'rotations' folder under {root}; is this a PixelLab character export?");
                return null;
            }

            string animations = Normalise(Path.Combine(Path.GetDirectoryName(rotations), "animations"));
            string[] animationFolders = Directory.Exists(animations)
                ? Directory.GetDirectories(animations).Select(Normalise).ToArray()
                : new string[0];
            string jump = Named(animationFolders, "Jump");
            string climb = Named(animationFolders, "Climb");
            string run = Named(animationFolders, "Running")
                ?? animationFolders.FirstOrDefault(f => f != jump && f != climb);

            // 2. Give every PNG pixel-art import settings so sprites are crisp and stand on their feet.
            //    Idle and run images use different canvas sizes, so each folder is pivoted on the lowest
            //    foot in that folder; otherwise she would jump off her shadow when she starts running.
            string[] folders = Directory.GetFiles(root, "*.png", SearchOption.AllDirectories)
                .Select(p => Normalise(Path.GetDirectoryName(p))).Distinct().ToArray();
            AssetDatabase.StartAssetEditing();
            try
            {
                foreach (string folder in folders)
                {
                    string[] pngs = Directory.GetFiles(folder, "*.png").Select(Normalise).ToArray();
                    float feet = pngs.Min(FeetFromBottom);
                    foreach (string png in pngs)
                    {
                        ApplyImportSettings(png, feet);
                    }
                }
            }
            finally
            {
                AssetDatabase.StopAssetEditing();
            }

            // 3. Collect the idle sprite, run frames and jump frames for each of the 8 directions.
            var directions = new DirectionSprites[CharacterSpriteSet.DirectionCount];
            for (int i = 0; i < directions.Length; i++)
            {
                string name = CharacterSpriteSet.DirectionNames[i];
                directions[i].Idle = AssetDatabase.LoadAssetAtPath<Sprite>($"{rotations}/{name}.png");
                directions[i].Run = LoadFrames(run == null ? null : $"{run}/{name}");
                directions[i].Jump = LoadFrames(jump == null ? null : $"{jump}/{name}");
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
            set.SetClimb(LoadFrames(climb == null ? null : $"{climb}/north-west"));
            EditorUtility.SetDirty(set);
            AssetDatabase.SaveAssets();
            return set;
        }

        /// <summary>Pixel-art settings with the pivot <paramref name="feetFraction"/> of the height up, under the feet.</summary>
        private static void ApplyImportSettings(string path, float feetFraction)
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
            settings.spriteAlignment = (int)SpriteAlignment.Custom;
            settings.spritePivot = new Vector2(0.5f, feetFraction);
            importer.SetTextureSettings(settings);
            importer.SaveAndReimport();
        }

        /// <summary>
        /// How far up the image the lowest opaque pixel sits, as a fraction of its height. Read from the
        /// PNG file itself, so it works before the texture is imported.
        /// </summary>
        private static float FeetFromBottom(string path)
        {
            var texture = new Texture2D(2, 2);
            try
            {
                texture.LoadImage(File.ReadAllBytes(path));
                Color32[] pixels = texture.GetPixels32();
                int width = texture.width;
                for (int row = 0; row < texture.height; row++)
                {
                    for (int x = 0; x < width; x++)
                    {
                        if (pixels[row * width + x].a > 0)
                        {
                            return (float)row / texture.height;
                        }
                    }
                }

                return 0f;
            }
            finally
            {
                Object.DestroyImmediate(texture);
            }
        }

        /// <summary>The animation folder with this name (any case), or null.</summary>
        private static string Named(string[] folders, string name)
        {
            return folders.FirstOrDefault(f => string.Equals(Path.GetFileName(f), name, System.StringComparison.OrdinalIgnoreCase));
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
