// SceneFlow: the one place that moves between the title screen and the chambers, and quits the
// game. TitleMenu and PauseMenu call it. Scene order follows Build Settings: the title is first,
// then the chambers in the order of Ideas/PROJECT_PLAN.md (GameStateController loads the next
// chamber itself when one is completed).
using UnityEngine;
using UnityEngine.SceneManagement;

namespace GameJam.Core
{
    /// <summary>Loads the title or a chamber and quits the game, always with time running again.</summary>
    public static class SceneFlow
    {
        /// <summary>Build index of the title screen.</summary>
        public const int TitleSceneIndex = 0;

        /// <summary>Build index of the first chamber, where a new game starts.</summary>
        public const int FirstChamberIndex = 1;

        /// <summary>Forgets the saved progress and starts the first chamber. Used by New Game and Restart Game.</summary>
        public static void NewGame()
        {
            SaveProgress.Clear();
            LoadScene(FirstChamberIndex);
        }

        /// <summary>Starts the saved chamber fresh, or the first chamber if nothing is saved.</summary>
        public static void ContinueGame()
        {
            LoadScene(SaveProgress.HasSave ? SaveProgress.Chamber : FirstChamberIndex);
        }

        /// <summary>Closes the game. In the editor it stops Play mode instead.</summary>
        public static void QuitGame()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }

        /// <summary>Loads a scene by build index, undoing any pause first so the new scene starts running.</summary>
        private static void LoadScene(int buildIndex)
        {
            Time.timeScale = 1f;
            SceneManager.LoadScene(buildIndex);
        }
    }
}
