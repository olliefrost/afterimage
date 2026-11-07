// PlayerTrigger: a trigger area that fires OnPlayerEntered when the live player walks in. Clones are
// ignored. Tutorial2 uses it to open the exit door automatically (wired to Door.Open); LevelExit
// behind the door then ends the level.
using GameJam.Core;
using UnityEngine;
using UnityEngine.Events;

namespace GameJam.Interactables
{
    /// <summary>Fires an inspector event when the live player enters its trigger collider.</summary>
    public class PlayerTrigger : MonoBehaviour
    {
        [Header("Events")]
        [SerializeField] private UnityEvent _onPlayerEntered = new UnityEvent();

        private void OnTriggerEnter2D(Collider2D other)
        {
            IActor actor = other.GetComponentInParent<IActor>();
            if (actor != null && !actor.IsClone)
            {
                _onPlayerEntered.Invoke();
            }
        }
    }
}
