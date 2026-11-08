// IInteractable: anything an actor can use with the interact action (buttons, levers, terminals).
// Interactor finds these by their trigger collider and calls Interact. The interact prompt asks
// CanInteract so it only shows a key over things that would actually react.
using GameJam.Core;

namespace GameJam.Interactables
{
    /// <summary>An object that reacts when the player or a clone interacts with it.</summary>
    public interface IInteractable
    {
        /// <summary>True if interacting now would do something (e.g. a button that is not already pressed).</summary>
        bool CanInteract(IActor actor);

        /// <summary>Called when <paramref name="actor"/> interacts with this object.</summary>
        void Interact(IActor actor);
    }
}
