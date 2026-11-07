// PlayFromTitle: editor-only helper. While on (the default), pressing Play always starts from
// Title.unity, the first scene in Build Settings, whichever scene is open, so the editor runs the
// game the way a build does. Toggle it with GameJam > Play From Title, for example to test one
// chamber directly. The choice is saved per machine in EditorPrefs. Never included in builds.
using UnityEditor;
using UnityEditor.SceneManagement;

namespace GameJam.EditorTools
{
    /// <summary>Points Unity's play-mode start scene at the title screen while the menu toggle is on.</summary>
    [InitializeOnLoad]
    public static class PlayFromTitle
    {
        private const string MenuPath = "GameJam/Play From Title";
        private const string PrefKey = "GameJam.PlayFromTitle";
        private const string TitlePath = "Assets/_Project/Scenes/Title.unity";

        static PlayFromTitle()
        {
            // Wait until the editor has finished loading assets before looking up the scene.
            EditorApplication.delayCall += Apply;
        }

        private static bool IsOn
        {
            get => EditorPrefs.GetBool(PrefKey, true);
            set => EditorPrefs.SetBool(PrefKey, value);
        }

        [MenuItem(MenuPath)]
        private static void Toggle()
        {
            IsOn = !IsOn;
            Apply();
        }

        [MenuItem(MenuPath, true)]
        private static bool ToggleValidate()
        {
            Menu.SetChecked(MenuPath, IsOn);
            return true;
        }

        /// <summary>Sets or clears the play-mode start scene to match the toggle.</summary>
        private static void Apply()
        {
            EditorSceneManager.playModeStartScene = IsOn ? AssetDatabase.LoadAssetAtPath<SceneAsset>(TitlePath) : null;
        }
    }
}
