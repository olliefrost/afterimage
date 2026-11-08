// GameSettings: player options that are not key bindings, saved in PlayerPrefs so they survive
// restarts. The Settings page in the pause menu changes them; visual systems read them (CameraFollow
// scales every screen shake by ShakeScale).
using System;
using UnityEngine;

namespace GameJam.Core
{
    /// <summary>The screen shake strengths the player can pick, from the Settings page.</summary>
    public enum ShakeLevel
    {
        Full = 0,
        Low = 1,
        Off = 2
    }

    /// <summary>Saved player options (currently just screen shake strength).</summary>
    public static class GameSettings
    {
        private const string ShakeKey = "settings.shake";

        /// <summary>Raised when any setting changes, so open menus can refresh their labels.</summary>
        public static event Action Changed;

        /// <summary>The chosen screen shake strength (Full by default).</summary>
        public static ShakeLevel Shake
        {
            get => (ShakeLevel)PlayerPrefs.GetInt(ShakeKey, (int)ShakeLevel.Full);
            set
            {
                PlayerPrefs.SetInt(ShakeKey, (int)value);
                PlayerPrefs.Save();
                Changed?.Invoke();
            }
        }

        /// <summary>Multiplier applied to every screen shake: 1 for Full, 0.5 for Low, 0 for Off.</summary>
        public static float ShakeScale
        {
            get
            {
                switch (Shake)
                {
                    case ShakeLevel.Low: return 0.5f;
                    case ShakeLevel.Off: return 0f;
                    default: return 1f;
                }
            }
        }

        /// <summary>Steps to the next shake level (Full, Low, Off, then back to Full).</summary>
        public static void CycleShake()
        {
            Shake = (ShakeLevel)(((int)Shake + 1) % 3);
        }
    }
}
