// ShakeOnEvent: lets a UnityEvent shake the camera (a door's On Opened, the generator's On Powered)
// without code. The shake comes from this object's position, so it fades when it happens off-screen.
// Visual only.
using GameJam.Core;
using UnityEngine;

namespace GameJam.Visuals
{
    /// <summary>Shakes the screen from this object's position when Shake() is called.</summary>
    public class ShakeOnEvent : MonoBehaviour
    {
        [Header("Shake")]
        [Tooltip("Furthest the camera moves, in world units. Tiny 0.02, small 0.04, medium 0.08, big 0.14.")]
        [SerializeField] private float _strength = 0.04f;
        [SerializeField] private float _duration = 0.2f;
        [Tooltip("On: fades with distance from the view. Off: full strength wherever the camera is.")]
        [SerializeField] private bool _positional = true;

        /// <summary>Requests this object's shake (ignored while the room is being reset).</summary>
        public void Shake()
        {
            if (LevelReset.IsResetting)
            {
                return;
            }

            if (_positional)
            {
                ScreenShake.ShakeAt(transform.position, _strength, _duration);
            }
            else
            {
                ScreenShake.Shake(_strength, _duration);
            }
        }
    }
}
