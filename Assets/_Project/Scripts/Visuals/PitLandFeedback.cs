// PitLandFeedback: when the live player drops into a Pit, the camera shakes and the character
// squashes on landing (the PunchScale on the Player's Body/Visual), so the one-way drop feels heavy.
// Clones dropping in stay quiet. Sits on the Pit prefab. Visual only.
using GameJam.Core;
using GameJam.Interactables;
using UnityEngine;

namespace GameJam.Visuals
{
    /// <summary>Shake and landing squash for the live player dropping into a pit.</summary>
    public class PitLandFeedback : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Pit _pit;

        [Header("Feedback")]
        [SerializeField] private float _shakeStrength = 0.08f;
        [SerializeField] private float _shakeDuration = 0.3f;

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
            if (actor.IsClone)
            {
                return;
            }

            ScreenShake.Shake(_shakeStrength, _shakeDuration);

            // A one-off lookup on a rare event; the player carries a landing PunchScale on its art.
            PunchScale squash = actor.Transform.GetComponentInChildren<PunchScale>();
            if (squash != null)
            {
                squash.Punch();
            }
        }
    }
}
