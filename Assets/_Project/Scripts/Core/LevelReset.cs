// LevelReset: a registry of every IResettable in the loaded level. Resettable components
// register in OnEnable and unregister in OnDisable, so GameStateController can reset the whole
// room with one call and nothing has to search the scene.
using System.Collections.Generic;

namespace GameJam.Core
{
    /// <summary>Tracks all active <see cref="IResettable"/> objects and resets them together.</summary>
    public static class LevelReset
    {
        private static readonly List<IResettable> Resettables = new List<IResettable>();

        /// <summary>Adds an object to the reset list. Call from OnEnable.</summary>
        public static void Register(IResettable resettable)
        {
            if (!Resettables.Contains(resettable))
            {
                Resettables.Add(resettable);
            }
        }

        /// <summary>Removes an object from the reset list. Call from OnDisable.</summary>
        public static void Unregister(IResettable resettable)
        {
            Resettables.Remove(resettable);
        }

        /// <summary>Returns every registered object to its starting state.</summary>
        public static void ResetAll()
        {
            for (int i = 0; i < Resettables.Count; i++)
            {
                Resettables[i].ResetState();
            }
        }
    }
}
