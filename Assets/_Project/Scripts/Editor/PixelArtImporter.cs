// PixelArtImporter: editor-only. When a new PNG is first imported under Assets/_Project/Art/, it
// gets the project's pixel-art settings (64 PPU, Point filter, no compression, no mipmaps) so
// teammates don't set them by hand. It only touches first imports, so later inspector tweaks stay.
using UnityEditor;
using UnityEngine;

namespace GameJam.EditorTools
{
    /// <summary>Applies pixel-art import settings to newly added sprites in the Art folder.</summary>
    public class PixelArtImporter : AssetPostprocessor
    {
        private const string ArtFolder = "Assets/_Project/Art/";
        private const float PixelsPerUnit = 64f;

        // PixelLab iso floor tiles are 64x64 with the top face's centre 26.5 px above the bottom edge.
        private static readonly Vector2 FloorTopFacePivot = new Vector2(0.5f, 26.5f / 64f);

        /// <summary>Runs before Unity imports a texture; sets sprite settings on first import only.</summary>
        private void OnPreprocessTexture()
        {
            if (!assetPath.StartsWith(ArtFolder) || !assetImporter.importSettingsMissing)
            {
                return;
            }

            var importer = (TextureImporter)assetImporter;
            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.spritePixelsPerUnit = PixelsPerUnit;
            importer.filterMode = FilterMode.Point;
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            importer.mipmapEnabled = false;
            importer.alphaIsTransparency = true;

            // Floor tiles pivot on the centre of their top face so it lands on the iso cell centre;
            // everything else stands on its feet.
            var settings = new TextureImporterSettings();
            importer.ReadTextureSettings(settings);
            if (assetPath.Contains("/Floors/"))
            {
                settings.spriteAlignment = (int)SpriteAlignment.Custom;
                settings.spritePivot = FloorTopFacePivot;
            }
            else
            {
                settings.spriteAlignment = (int)SpriteAlignment.BottomCenter;
            }
            importer.SetTextureSettings(settings);
        }
    }
}
