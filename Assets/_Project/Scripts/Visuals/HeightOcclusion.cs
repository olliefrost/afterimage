// HeightOcclusion: fades an OcclusionFader while the live player is drawn high up (the top step, the
// landing or the balcony), because things on the floor plan in front of them, like Chamber 3's
// balcony edge, then cover their legs. Same see-through effect as Tutorial1's pit; it snaps back to
// solid on a level reset. Visual only: reads the player's ActorElevation.
using GameJam.Core;
using UnityEngine;

namespace GameJam.Visuals
{
    /// <summary>Makes occluders see-through while the player stands on raised ground.</summary>
    public class HeightOcclusion : MonoBehaviour, IResettable
    {
        [Header("References")]
        [Tooltip("The live player's ActorElevation (on its Body).")]
        [SerializeField] private ActorElevation _player;
        [SerializeField] private OcclusionFader _fader;

        [Header("Tuning")]
        [Tooltip("Drawn height (units) above which the occluders fade. 1.4 with a 1.5 balcony: only the top step, landing and balcony.")]
        [SerializeField] private float _fadeAboveHeight = 1.4f;

        private bool _isFaded;

        private void Awake()
        {
            if (_player == null || _fader == null)
            {
                Debug.LogError($"{name}: HeightOcclusion needs its player and fader wired.", this);
                enabled = false;
            }
        }

        private void OnEnable()
        {
            LevelReset.Register(this);
        }

        private void OnDisable()
        {
            LevelReset.Unregister(this);
        }

        /// <summary>Makes the occluders solid at once for the next attempt, which starts on the floor.</summary>
        public void ResetState()
        {
            _isFaded = false;
            _fader.SetFaded(false, true);
        }

        private void Update()
        {
            bool high = _player.Height > _fadeAboveHeight;
            if (high == _isFaded)
            {
                return;
            }

            _isFaded = high;
            _fader.SetFaded(high);
        }
    }
}
