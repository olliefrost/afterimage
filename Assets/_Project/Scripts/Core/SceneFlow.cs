// SceneFlow: the one place that moves between the title screen and the chambers, and quits the
// game. A new game also flags the first chamber's intro to play (IntroSequence consumes it).
// TitleMenu, PauseMenu and GameStateController (on a chamber exit) call it. Scene order follows
// Build Settings: the title is first, then the chambers in the order of Ideas/PROJECT_PLAN.md.
// Every scene change goes through the scene's ScreenTransition (if it has one): the screen is wiped
// to black, the scene loads, and the new scene's transition starts black and wipes off.
using UnityEngine;
using UnityEngine.SceneManagement;

namespace GameJam.Core
{
    /// <summary>Loads the title or a chamber behind a screen wipe and quits the game, always with time running again.</summary>
    public static class SceneFlow
    {
        /// <summary>Build index of the title screen.</summary>
        public const int TitleSceneIndex = 0;

        /// <summary>Build index of the first chamber, where a new game starts.</summary>
        public const int FirstChamberIndex = 1;

        /// <summary>The current scene's screen transition, set while it is enabled. Null means instant loads.</summary>
        public static ISceneTransition Transition { get; set; }

        /// <summary>True while the screen is wiping to another scene; menus ignore input meanwhile.</summary>
        public static bool IsChangingScene => Transition != null && Transition.IsBusy;

        // Set by a new game, read once by the first chamber's IntroSequence.
        private static bool _introPending;

        // Set when a transition loaded the scene, read once by the new scene's transition so it starts black.
        private static bool _arrivingBlack;

        /// <summary>Forgets the saved progress and starts the first chamber with its intro. Used by New Game and Restart Game.</summary>
        public static void NewGame()
        {
            if (IsChangingScene)
            {
                return;
            }

            SaveProgress.Clear();
            _introPending = true;
            LoadScene(FirstChamberIndex, null);
        }

        /// <summary>Starts the saved chamber fresh, or the first chamber if nothing is saved. Never plays the intro.</summary>
        public static void ContinueGame()
        {
            if (IsChangingScene)
            {
                return;
            }

            _introPending = false;
            LoadScene(SaveProgress.HasSave ? SaveProgress.Chamber : FirstChamberIndex, null);
        }

        /// <summary>
        /// Leaves a completed chamber with the caption shown on the black: loads the next scene in
        /// Build Settings, or the title after the last chamber.
        /// </summary>
        public static void CompleteChamber(int chamberIndex, string caption)
        {
            int next = chamberIndex + 1;
            LoadScene(next < SceneManager.sceneCountInBuildSettings ? next : TitleSceneIndex, caption);
        }

        /// <summary>Ends the game after the Ending's credits: forgets the save (so Continue is gone) and wipes to the title.</summary>
        public static void FinishGame()
        {
            if (IsChangingScene)
            {
                return;
            }

            SaveProgress.Clear();
            LoadScene(TitleSceneIndex, null);
        }

        /// <summary>True once after a new game started, then false, so the intro plays a single time.</summary>
        public static bool ConsumeIntro()
        {
            bool pending = _introPending;
            _introPending = false;
            return pending;
        }

        /// <summary>True once after a transition loaded this scene, so the new scene's transition starts covered.</summary>
        public static bool ConsumeArrivingBlack()
        {
            bool arriving = _arrivingBlack;
            _arrivingBlack = false;
            return arriving;
        }

        /// <summary>
        /// Loads a scene straight away, undoing any pause first so the new scene starts running.
        /// Called by the transition once the screen is black (internal so other scripts go through
        /// the wipe instead).
        /// </summary>
        internal static void LoadNow(int buildIndex, bool fromTransition)
        {
            _arrivingBlack = fromTransition;
            Time.timeScale = 1f;
            SceneManager.LoadScene(buildIndex);
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

        /// <summary>Wipes to the scene through the transition, or loads it instantly if the scene has none.</summary>
        private static void LoadScene(int buildIndex, string caption)
        {
            if (Transition == null)
            {
                LoadNow(buildIndex, false);
                return;
            }

            // Play ignores the request if a scene change is already running.
            Transition.Play(buildIndex, caption);
        }
    }
}
