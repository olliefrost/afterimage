// IInteractable: anything an actor can use with the interact action (buttons, levers, terminals).
// Interactor finds these by their trigger collider and calls Interact.
using GameJam.Core;

namespace GameJam.Interactables
{
    /// <summary>An object that reacts when the player or a clone interacts with it.</summary>
    public interface IInteractable
    {
        /// <summary>Called when <paramref name="actor"/> interacts with this object.</summary>
        void Interact(IActor actor);
    }
}
