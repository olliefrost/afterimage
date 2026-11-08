// PitAudio: on the Pit prefab. Plays the drop scrape when someone enters the pit and the landing
// thud a moment later. Clones replay the fall too, so theirs is quieter.
using System.Collections;
using GameJam.Core;
using GameJam.Interactables;
using UnityEngine;

namespace GameJam.Audio
{
    /// <summary>Plays the fall and landing sounds when an actor enters the pit.</summary>
    [RequireComponent(typeof(Pit))]
    public class PitAudio : MonoBehaviour
    {
        [SerializeField] private Pit _pit;
        [SerializeField] private SoundCue _fall;
        [SerializeField] private SoundCue _land;
        [Tooltip("Seconds between the fall sound and the landing thud.")]
        [SerializeField] private float _landDelay = 0.35f;
        [Tooltip("Volume scale when a clone falls in.")]
        [Range(0f, 1f)]
        [SerializeField] private float _cloneVolumeScale = 0.5f;

        private GameStateController _state;

        private void Awake()
        {
            if (_pit == null)
            {
                _pit = GetComponent<Pit>();
            }

            _state = FindAnyObjectByType<GameStateController>();
        }

        private void OnEnable()
        {
            _pit.ActorEntered += HandleEntered;
        }

        private void OnDisable()
        {
            _pit.ActorEntered -= HandleEntered;
        }

        private void HandleEntered(IActor actor)
        {
            float scale = actor.IsClone ? _cloneVolumeScale : 1f;
            AudioManager.Play(_fall, scale);
            StartCoroutine(LandAfterDelay(scale));
        }

        private IEnumerator LandAfterDelay(float scale)
        {
            yield return new WaitForSeconds(_landDelay);

            // If the attempt ended during the fall, the thud would land on the rewind: skip it.
            if (_state == null || _state.State == GameState.Playing)
            {
                AudioManager.Play(_land, scale);
            }
        }
    }
}
