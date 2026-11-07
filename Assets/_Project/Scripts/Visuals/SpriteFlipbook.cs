// SpriteFlipbook: plays a list of sprites on a SpriteRenderer. Doors call PlayForward/PlayBackward
// from their opened/closed events; looping machinery (conveyor belts, console screens) sets Loop
// and Play On Enable. Visual only: gameplay never waits on it.
using UnityEngine;

namespace GameJam.Visuals
{
    /// <summary>Steps through sprite frames forward or backward, once or looping.</summary>
    public class SpriteFlipbook : MonoBehaviour
    {
        [SerializeField] private SpriteRenderer _renderer;
        [Tooltip("Frames in playback order. A door's frames run from closed to open.")]
        [SerializeField] private Sprite[] _frames;
        [SerializeField] private float _framesPerSecond = 12f;
        [Tooltip("Repeat forever (belts, screens). Off for one-shot animations like doors.")]
        [SerializeField] private bool _loop;
        [SerializeField] private bool _playOnEnable;

        private float _frame;
        private int _step;

        private void OnEnable()
        {
            if (_playOnEnable)
            {
                PlayForward();
            }
            else
            {
                ShowFirst();
            }
        }

        /// <summary>Plays from the current frame towards the last one.</summary>
        public void PlayForward()
        {
            _step = 1;
        }

        /// <summary>Plays from the current frame back towards the first one (e.g. a door closing).</summary>
        public void PlayBackward()
        {
            _step = -1;
        }

        /// <summary>Stops and shows the first frame.</summary>
        public void ShowFirst()
        {
            _step = 0;
            Show(0f);
        }

        /// <summary>Stops and shows the last frame.</summary>
        public void ShowLast()
        {
            _step = 0;
            Show(_frames.Length - 1);
        }

        /// <summary>Stops and shows the frame a fraction of the way through (0 first, 1 last), for scripted scrubbing.</summary>
        public void ShowAt(float fraction)
        {
            _step = 0;
            Show(Mathf.Round(Mathf.Clamp01(fraction) * (_frames.Length - 1)));
        }

        private void Update()
        {
            if (_step == 0 || _frames.Length == 0)
            {
                return;
            }

            // Advance, then wrap when looping or stop at either end.
            float next = _frame + _step * _framesPerSecond * Time.deltaTime;
            if (_loop)
            {
                next = Mathf.Repeat(next, _frames.Length);
            }
            else if (next <= 0f || next >= _frames.Length - 1)
            {
                next = Mathf.Clamp(next, 0f, _frames.Length - 1);
                _step = 0;
            }

            Show(next);
        }

        private void Show(float frame)
        {
            _frame = frame;
            if (_frames.Length > 0)
            {
                _renderer.sprite = _frames[Mathf.Clamp((int)frame, 0, _frames.Length - 1)];
            }
        }
    }
}
