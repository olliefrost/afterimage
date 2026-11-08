// SoundCueBuilder: editor-only menu item (GameJam > Audio > Create Missing Sound Cues). Makes a
// SoundCue asset in Audio/Cues for every clip in Audio/SFX (and every *_sting in Audio/Music) that
// has none, with the clip assigned, so new sounds only need hooking up. Existing cues are never touched. Footstep takes
// Numbered takes (sfx_step_NN, sfx_text_beep_NN) belong to a shared hand-made cue, so they are skipped.
using System.IO;
using UnityEditor;
using UnityEngine;
using GameJam.Audio;

namespace GameJam.EditorTools
{
    /// <summary>Creates a SoundCue for each SFX clip that does not have one yet.</summary>
    public static class SoundCueBuilder
    {
        private const string SfxFolder = "Assets/_Project/Audio/SFX";
        private const string MusicFolder = "Assets/_Project/Audio/Music";
        private const string CueFolder = "Assets/_Project/Audio/Cues";

        /// <summary>Scans the SFX folder and creates the missing cues.</summary>
        [MenuItem("GameJam/Audio/Create Missing Sound Cues")]
        private static void CreateMissing()
        {
            int created = 0;

            foreach (string guid in AssetDatabase.FindAssets("t:AudioClip", new[] { SfxFolder, MusicFolder }))
            {
                string clipPath = AssetDatabase.GUIDToAssetPath(guid);
                string cueName = Path.GetFileNameWithoutExtension(clipPath);
                string cuePath = $"{CueFolder}/{cueName}.asset";

                // 1. skip music tracks, old clips outside the sfx_ naming, numbered takes (sfx_step_01, sfx_text_beep_02: those share one hand-made cue) and anything that already has a cue
                bool isTrack = clipPath.StartsWith(MusicFolder) && !cueName.EndsWith("_sting");
                if (isTrack || !(cueName.StartsWith("sfx_") || cueName.EndsWith("_sting")) || System.Text.RegularExpressions.Regex.IsMatch(cueName, @"_\d\d$") || AssetDatabase.LoadAssetAtPath<SoundCue>(cuePath) != null)
                {
                    continue;
                }

                // 2. create the cue and assign its single clip
                var cue = ScriptableObject.CreateInstance<SoundCue>();
                var serialized = new SerializedObject(cue);
                var clips = serialized.FindProperty("_clips");
                clips.arraySize = 1;
                clips.GetArrayElementAtIndex(0).objectReferenceValue = AssetDatabase.LoadAssetAtPath<AudioClip>(clipPath);

                // 3. loops and UI sounds should not be pitch-shifted or jittered
                if (cueName.EndsWith("_loop") || cueName.EndsWith("_hum") || cueName.StartsWith("sfx_ui_"))
                {
                    serialized.FindProperty("_minPitch").floatValue = 1f;
                    serialized.FindProperty("_maxPitch").floatValue = 1f;
                    serialized.FindProperty("_volumeJitter").floatValue = 0f;
                }

                serialized.ApplyModifiedPropertiesWithoutUndo();
                AssetDatabase.CreateAsset(cue, cuePath);
                created++;
            }

            AssetDatabase.SaveAssets();
            Debug.LogWarning($"SoundCueBuilder: created {created} sound cues in {CueFolder}.");
        }
    }
}
