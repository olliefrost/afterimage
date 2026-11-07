// SaveProgress: remembers the furthest chamber the player has reached, so the title screen can
// offer Continue. GameStateController saves the chamber's build index when a chamber starts;
// TitleMenu reads it and SceneFlow clears it for a new game. Clones are not saved: Continue
// starts the chamber fresh.
using UnityEngine;
using UnityEngine.SceneManagement;

namespace GameJam.Core
{
    /// <summary>Stores the build index of the last chamber started, in PlayerPrefs.</summary>
    public static class SaveProgress
    {
        private const string ChamberKey = "save.chamber";

        /// <summary>
        /// True if a chamber is saved and it still exists in Build Settings, so Continue can load it.
        /// </summary>
        public static bool HasSave
        {
            get
            {
                int chamber = Chamber;
                return chamber >= SceneFlow.FirstChamberIndex && chamber < SceneManager.sceneCountInBuildSettings;
            }
        }

        /// <summary>Build index of the saved chamber, or -1 if there is none.</summary>
        public static int Chamber => PlayerPrefs.GetInt(ChamberKey, -1);

        /// <summary>Saves the chamber the player has just started. Ignores scenes that are not chambers, such as the title.</summary>
        public static void SaveChamber(int buildIndex)
        {
            if (buildIndex < SceneFlow.FirstChamberIndex)
            {
                return;
            }

            PlayerPrefs.SetInt(ChamberKey, buildIndex);
            PlayerPrefs.Save();
        }

        /// <summary>Forgets the saved chamber, for a new game.</summary>
        public static void Clear()
        {
            PlayerPrefs.DeleteKey(ChamberKey);
            PlayerPrefs.Save();
        }
    }
}
