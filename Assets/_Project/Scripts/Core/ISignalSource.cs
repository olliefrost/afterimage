// ISignalSource: anything that is simply on or off, such as a powered generator or a held plate.
// Visuals like SignalCable read it to light up, so the player sees what powers what.
namespace GameJam.Core
{
    /// <summary>Something with an on/off state that cables and lights can show.</summary>
    public interface ISignalSource
    {
        /// <summary>True while the source is on (powered, held, active).</summary>
        bool IsOn { get; }
    }
}
