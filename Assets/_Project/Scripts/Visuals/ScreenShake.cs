// ScreenShake: the one place anything asks for a camera shake (button presses, doors, deaths, the
// generator powering up). It only forwards the request: CameraFollow listens, fades shakes that
// happen off-screen (usually clones) and applies the player's shake setting. Visual only.
using System;
using UnityEngine;

namespace GameJam.Visuals
{
    /// <summary>Static entry point for camera shakes, so effects never need a camera reference.</summary>
    public static class ScreenShake
    {
        /// <summary>
        /// Raised for every request: strength (world units), duration (seconds), world position and
        /// whether the position matters (false means full strength wherever the camera is).
        /// </summary>
        public static event Action<float, float, Vector2, bool> Requested;

        /// <summary>Shakes the screen at full strength, for things that happen to the player.</summary>
        public static void Shake(float strength, float duration)
        {
            Requested?.Invoke(strength, duration, Vector2.zero, false);
        }

        /// <summary>Shakes the screen for something at a world position; it fades away off-screen.</summary>
        public static void ShakeAt(Vector2 position, float strength, float duration)
        {
            Requested?.Invoke(strength, duration, position, true);
        }
    }
}
