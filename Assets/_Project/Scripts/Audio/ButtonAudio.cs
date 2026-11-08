// ButtonAudio: on the PressButton prefab. Plays a press sound when the button latches, quieter when
// a clone pressed it (clone replays still need to be heard, since they can happen off screen).
using GameJam.Interactables;
using UnityEngine;

namespace GameJam.Audio
{
    /// <summary>Plays a clunk when a PressButton is pressed.</summary>
    [RequireComponent(typeof(PressButton))]
    public class ButtonAudio : MonoBehaviour
    {
        [SerializeField] private PressButton _button;
        [SerializeField] private SoundCue _press;
        [Tooltip("Volume scale when a clone pressed the button.")]
        [Range(0f, 1f)]
        [SerializeField] private float _cloneVolumeScale = 0.6f;

        private void Awake()
        {
            if (_button == null)
            {
                _button = GetComponent<PressButton>();
            }
        }

        private void OnEnable()
        {
            _button.Pressed += HandlePressed;
        }

        private void OnDisable()
        {
            _button.Pressed -= HandlePressed;
        }

        private void HandlePressed(Core.IActor actor)
        {
            AudioManager.Play(_press, actor != null && actor.IsClone ? _cloneVolumeScale : 1f);
        }
    }
}
