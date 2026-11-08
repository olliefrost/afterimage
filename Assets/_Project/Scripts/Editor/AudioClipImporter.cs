// AudioClipImporter: editor-only. When a new audio file is first imported under Assets/_Project/Audio/,
// it gets the project's import settings from Ideas/sounds.md so nobody sets them by hand: music and
// long ambience stream as Vorbis, everything else (including stings) is a mono clip decompressed on load. It only
// touches first imports, so later inspector tweaks stay.
using UnityEditor;
using UnityEngine;

namespace GameJam.EditorTools
{
    /// <summary>Applies audio import settings to newly added clips in the Audio folder.</summary>
    public class AudioClipImporter : AssetPostprocessor
    {
        private const string AudioFolder = "Assets/_Project/Audio/";
        private const float MusicQuality = 0.7f;

        /// <summary>Runs before Unity imports a clip; sets load type, mono and compression on first import only.</summary>
        private void OnPreprocessAudio()
        {
            if (!assetPath.StartsWith(AudioFolder) || !assetImporter.importSettingsMissing)
            {
                return;
            }

            var importer = (AudioImporter)assetImporter;
            var settings = importer.defaultSampleSettings;

            // Long tracks stream from disk; the room tone and wind are long loops too.
            // Stings are short one-shots, so they decompress on load like effects.
            bool streamed = (assetPath.Contains("/Music/") && !assetPath.Contains("_sting"))
                || assetPath.EndsWith("amb_lab_hum.mp3")
                || assetPath.EndsWith("amb_wind.mp3");

            if (streamed)
            {
                settings.loadType = AudioClipLoadType.Streaming;
                settings.compressionFormat = AudioCompressionFormat.Vorbis;
                settings.quality = MusicQuality;
            }
            else
            {
                settings.loadType = AudioClipLoadType.DecompressOnLoad;
                settings.compressionFormat = AudioCompressionFormat.ADPCM;
                importer.forceToMono = true;
                settings.preloadAudioData = true;
            }

            importer.defaultSampleSettings = settings;
        }
    }
}
