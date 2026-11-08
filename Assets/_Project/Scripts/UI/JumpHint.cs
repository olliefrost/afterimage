// JumpHint: the "Oh wait, I can jump!" moment in Chamber 4, where jumping first appears. A
// PlayerTrigger by the first gap calls Show(); the line appears once per visit to the chamber (not
// every attempt) through the shared DialogueBox, with the jump key as currently bound. It hides on
// the player's first jump or after a few seconds. Visual only: jumping works whether or not it shows.
using GameJam.Player;
using UnityEngine;

namespace GameJam.UI
{
    /// <summary>Shows the one-off jump hint and hides it once the player has jumped.</summary>
    public class JumpHint : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private DialogueBox _dialogue;
        [SerializeField] private PlayerInputReader _input;
        [SerializeField] private PlayerJump _playerJump;

        [Header("Text")]
        [SerializeField] private string _speaker = "YOU";
        [Tooltip("{0} becomes the jump key, e.g. SPACE.")]
        [SerializeField] private string _line = "Oh wait, I can jump! [{0}]";

        [Header("Timing")]
        [Tooltip("Seconds of play before the hint hides by itself (it also hides on the first jump).")]
        [SerializeField] private float _showSeconds = 5f;

        private bool _shown;
        private float _hideTimer;

        private void OnEnable()
        {
            _playerJump.Jumped += OnJumped;
        }

        private void OnDisable()
        {
            _playerJump.Jumped -= OnJumped;
        }

        /// <summary>Shows the hint, the first time only.</summary>
        public void Show()
        {
            if (_shown || _input.JumpTarget == null)
            {
                return;
            }

            _shown = true;
            _hideTimer = _showSeconds;
            _dialogue.Show(_speaker, string.Format(_line, _input.JumpTarget.KeyName.ToUpperInvariant()));
        }

        private void Update()
        {
            if (_hideTimer <= 0f)
            {
                return;
            }

            // Scaled time, so the hint waits while the game is paused.
            _hideTimer -= Time.deltaTime;
            if (_hideTimer <= 0f)
            {
                _dialogue.Hide(false);
            }
        }

        private void OnJumped()
        {
            if (_hideTimer > 0f)
            {
                _hideTimer = 0f;
                _dialogue.Hide(false);
            }
        }
    }
}
