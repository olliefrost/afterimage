// IntroSequence: the opening of a new game in Tutorial1 (Ideas/PROJECT_PLAN.md, Story > Intro).
// The camera looks at the open entrance door with bright light outside, an unseen guard shouts,
// the character sprints in, the door's shutter slams down behind her and the camera glides to her. Then
// GameStateController.FinishIntro starts the first attempt, so nothing here is recorded.
// It runs only after New Game or Restart Game (SceneFlow.ConsumeIntro); any key or click skips it.
using System.Collections;
using GameJam.Core;
using GameJam.Interactables;
using GameJam.Player;
using GameJam.UI;
using GameJam.Visuals;
using UnityEngine;

namespace GameJam.Story
{
    /// <summary>Plays the sprint-in intro once at the start of a new game, then hands over control.</summary>
    public class IntroSequence : MonoBehaviour
    {
        [Header("Game references")]
        [SerializeField] private GameStateController _game;
        [SerializeField] private PlayerInputReader _input;
        [SerializeField] private PlayerActor _player;
        [SerializeField] private CameraFollow _camera;

        [Header("Scene references")]
        [Tooltip("The entrance door the character runs in through. Its open visuals are the bright light outside; the slam shuts it and it stays shut.")]
        [SerializeField] private Door _door;
        [Tooltip("The entrance door's shutter: rolled up while open, dropped for the slam.")]
        [SerializeField] private DoorShutter _shutter;
        [SerializeField] private DialogueBox _dialogue;
        [Tooltip("Where the camera looks while the door is open.")]
        [SerializeField] private Transform _cameraFocus;
        [Tooltip("Points the character runs through, starting outside the door; the last one is just inside it. She then runs on to her spawn point.")]
        [SerializeField] private Transform[] _path;

        [Header("Dialogue")]
        [SerializeField] private string _speaker = "???";
        [SerializeField] private string _line = "Come back here!";

        [Header("Timing")]
        [Tooltip("Seconds looking at the open door before the shout.")]
        [SerializeField] private float _openPause = 0.7f;
        [Tooltip("Seconds after the shout finishes typing before she runs in.")]
        [SerializeField] private float _afterShoutPause = 0.25f;
        [Tooltip("Running speed in units per second (walking is 3.5).")]
        [SerializeField] private float _sprintSpeed = 7f;
        [Tooltip("Seconds for the shutter to drop shut.")]
        [SerializeField] private float _slamDropTime = 0.12f;
        [Tooltip("Camera shake when the door slams: seconds and strength in world units.")]
        [SerializeField] private float _slamShakeTime = 0.3f;
        [SerializeField] private float _slamShakeStrength = 0.08f;

        [Header("Testing")]
        [Tooltip("Play the intro whenever this scene starts, not only after New Game.")]
        [SerializeField] private bool _alwaysPlay;

        private Rigidbody2D _playerBody;
        private Vector2 _spawn;
        private bool _isPlaying;

        private void Awake()
        {
            // Claim the chamber start before GameStateController.Start begins play.
            _isPlaying = SceneFlow.ConsumeIntro() || _alwaysPlay;
            if (_isPlaying)
            {
                _game.BeginIntro();
            }
        }

        private void Start()
        {
            if (!_isPlaying)
            {
                return;
            }

            _playerBody = _player.GetComponent<Rigidbody2D>();
            _spawn = _player.Transform.position;
            _input.SkipRequested += Skip;
            StartCoroutine(Play());
        }

        private void OnDisable()
        {
            _input.SkipRequested -= Skip;
        }

        private IEnumerator Play()
        {
            // 1. The door stands open on bright light, the camera on it, the character outside.
            _door.Open();
            _shutter.SetRaised(1f);
            _playerBody.simulated = false;
            _player.Teleport(_path[0].position);
            _camera.Follow(_cameraFocus, true);

            // Wait for the screen wipe from the title to clear, so the opening is seen and can't be skipped blind.
            while (SceneFlow.IsChangingScene)
            {
                yield return null;
            }

            _input.SetSkipEnabled(true);
            yield return new WaitForSeconds(_openPause);

            // 2. The guard shouts from outside.
            _dialogue.Show(_speaker, _line);
            while (_dialogue.IsTyping)
            {
                yield return null;
            }

            yield return new WaitForSeconds(_afterShoutPause);

            // 3. She sprints in through the doorway.
            for (int i = 1; i < _path.Length; i++)
            {
                yield return RunTo(_path[i].position);
            }

            // 4. The shutter slams down behind her while she runs on and the camera turns to follow her.
            StartCoroutine(Slam());
            _camera.Follow(_player.Transform, false);
            yield return RunTo(_spawn);

            Finish();
        }

        /// <summary>Moves the character in a straight line at sprint speed until she arrives.</summary>
        private IEnumerator RunTo(Vector2 target)
        {
            while (Vector2.Distance(_playerBody.position, target) > 0.001f)
            {
                _player.Teleport(Vector2.MoveTowards(_playerBody.position, target, _sprintSpeed * Time.deltaTime));
                yield return null;
            }
        }

        /// <summary>
        /// Drops the shutter fast, then on impact shuts the door (which also cuts the outside light),
        /// shakes the camera and clears the shout.
        /// </summary>
        private IEnumerator Slam()
        {
            for (float t = 0f; t < _slamDropTime; t += Time.deltaTime)
            {
                // Accelerate as it falls, like something heavy dropping.
                float fall = t / _slamDropTime;
                _shutter.SetRaised(1f - fall * fall);
                yield return null;
            }

            _shutter.SetRaised(0f);
            _door.ResetState();
            _camera.Shake(_slamShakeTime, _slamShakeStrength);
            _dialogue.Hide(false);
        }

        /// <summary>Any key or click: jump to the end state straight away.</summary>
        private void Skip()
        {
            if (!_isPlaying)
            {
                return;
            }

            StopAllCoroutines();
            _shutter.SetRaised(0f);
            _door.ResetState();
            _dialogue.Hide(true);
            _player.Teleport(_spawn);
            _camera.Follow(_player.Transform, true);
            Finish();
        }

        /// <summary>Puts the character on her spawn point with physics back on and starts the first attempt.</summary>
        private void Finish()
        {
            _isPlaying = false;
            _input.SetSkipEnabled(false);
            _input.SkipRequested -= Skip;
            _player.Teleport(_spawn);
            _playerBody.simulated = true;
            _game.FinishIntro();
        }
    }
}
