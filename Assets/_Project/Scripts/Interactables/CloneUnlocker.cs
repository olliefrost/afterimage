// CloneUnlocker: unlocks the clone controls the first time the live player falls into a pit.
// Used in Tutorial1, where GameStateController starts with clones locked: the pit is a one-way drop,
// so the moment the player is trapped is the moment they are given the clone key. Clones falling in
// do nothing. Sits in the chamber scene (it needs scene references), not on the Pit prefab.
using GameJam.Core;
using UnityEngine;

namespace GameJam.Interactables
{
    /// <summary>Calls <see cref="GameStateController.UnlockClones"/> when the live player enters the pit.</summary>
    public class CloneUnlocker : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private GameStateController _game;
        [SerializeField] private Pit _pit;

        private void OnEnable()
        {
            _pit.ActorEntered += HandleActorEntered;
        }

        private void OnDisable()
        {
            _pit.ActorEntered -= HandleActorEntered;
        }

        private void HandleActorEntered(IActor actor)
        {
            if (!actor.IsClone)
            {
                _game.UnlockClones();
            }
        }
    }
}
