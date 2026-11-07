// IResettable: implemented by every component whose state changes during an attempt
// (doors, buttons, wires, pits, the player). LevelReset calls them all when a new attempt starts.
namespace GameJam.Core
{
    /// <summary>Something that can return itself to its starting state for a new attempt.</summary>
    public interface IResettable
    {
        /// <summary>Puts this object back exactly as it was when the level loaded.</summary>
        void ResetState();
    }
}
