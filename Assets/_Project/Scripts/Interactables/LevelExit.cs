// LevelExit: a trigger past the level's exit door. When the live player walks in it fires
// OnPlayerExited (wired in the inspector to GameStateController.CompleteLevel). Clones are ignored.
using GameJam.Core;
using UnityEngine;
using UnityEngine.Events;

namespace GameJam.Interactables
{
    /// <summary>Ends the level when the live player reaches it.</summary>
    public class LevelExit : MonoBehaviour
    {
        [Header("Events")]
        [SerializeField] private UnityEvent _onPlayerExited = new UnityEvent();

        private void OnTriggerEnter2D(Collider2D other)
        {
            IActor actor = other.GetComponentInParent<IActor>();
            if (actor != null && !actor.IsClone)
            {
                _onPlayerExited.Invoke();
            }
        }
    }
}
