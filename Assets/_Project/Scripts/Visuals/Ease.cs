// Ease: shared easing curves for visual tweens (HUD pop-ins, button punches, menu panels), so every
// animation in the game moves the same way. Pure maths, no state.
using UnityEngine;

namespace GameJam.Visuals
{
    /// <summary>Easing functions that map a 0-1 progress to an eased 0-1 value.</summary>
    public static class Ease
    {
        /// <summary>Fast start, gentle stop.</summary>
        public static float OutCubic(float t)
        {
            float u = 1f - Mathf.Clamp01(t);
            return 1f - u * u * u;
        }

        /// <summary>
        /// Overshoots past 1 then settles back ("back out"): things pop in. <paramref name="overshoot"/>
        /// 1.7 goes about 10% past, 0 not at all.
        /// </summary>
        public static float BackOut(float t, float overshoot = 1.7f)
        {
            float u = Mathf.Clamp01(t) - 1f;
            return 1f + (overshoot + 1f) * u * u * u + overshoot * u * u;
        }

        /// <summary>
        /// A punch: jumps to 1 straight away and wobbles back to 0 while fading out. Used for
        /// squash-and-stretch and shake-like bumps that end where they started.
        /// </summary>
        public static float Punch(float t, float wobbles = 2f)
        {
            t = Mathf.Clamp01(t);
            return Mathf.Cos(t * wobbles * Mathf.PI) * (1f - t) * (1f - t);
        }
    }
}
