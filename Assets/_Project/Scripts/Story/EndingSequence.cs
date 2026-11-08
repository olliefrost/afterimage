// EndingSequence: the whole bad ending in the Ending scene (Ideas/ending.md). She sprints in through
// the door (like IntroSequence), the camera zooms out, the guide lights come on towards the sister's
// hologram and she says "Sis?". The player then walks her the long way to it, and stepping on it
// springs the trap: the hologram glitches out, the lights cut, a spotlight bangs on and the cage
// drops. She says "...?", the camera pulls back to the two-way mirror, which only now lights up to
// show the scientists, and EndCredits takes over. Always plays; only the arrival can be skipped.
using System.Collections;
using GameJam.Audio;
using GameJam.Core;
using GameJam.Interactables;
using GameJam.Player;
using GameJam.UI;
using GameJam.Visuals;
using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace GameJam.Story
{
    /// <summary>Plays the arrival, the trap, the reveal and the credits of the final scene.</summary>
    public class EndingSequence : MonoBehaviour
    {
        [Header("Game references")]
        [SerializeField] private GameStateController _game;
        [SerializeField] private PlayerInputReader _input;
        [SerializeField] private PlayerActor _player;
        [SerializeField] private CameraFollow _camera;
        [SerializeField] private CameraPullBack _pullBack;

        [Header("Audio")]
        [Tooltip("The shutter hitting the floor behind her.")]
        [SerializeField] private SoundCue _doorSlam;
        [Tooltip("The sister's hologram glitching out.")]
        [SerializeField] private SoundCue _hologramGlitch;
        [Tooltip("Every lamp and the guide strip cutting out.")]
        [SerializeField] private SoundCue _blackout;
        [Tooltip("The cage hitting the floor.")]
        [SerializeField] private SoundCue _cageSlam;
        [Tooltip("Seconds the music takes to fade out when the hologram glitches (the silence before the trap).")]
        [SerializeField] private float _musicFadeOut = 0.3f;

        [Header("Arrival")]
        [SerializeField] private Door _door;
        [SerializeField] private DoorShutter _shutter;
        [Tooltip("Where the camera looks while the door is open.")]
        [SerializeField] private Transform _cameraFocus;
        [Tooltip("Points she runs through, starting outside the door; the last is just inside it. She then runs on to her spawn.")]
        [SerializeField] private Transform[] _path;
        [Tooltip("Running speed in units per second.")]
        [SerializeField] private float _sprintSpeed = 7f;
        [SerializeField] private float _openPause = 0.7f;

        [Header("Trap")]
        [SerializeField] private HologramFlicker _hologram;
        [Tooltip("Room lamps that cut out when the trap springs.")]
        [SerializeField] private Light2D[] _roomLamps;
        [Tooltip("Harsh white light that bangs on over her as the cage falls.")]
        [SerializeField] private Light2D _spotlight;
        [SerializeField] private CageDrop _cage;
        [Tooltip("Silence between the hologram vanishing and the lights cutting out.")]
        [SerializeField] private float _silenceTime = 0.4f;
        [SerializeField] private float _blackoutShakeStrength = 0.08f;

        [Tooltip("Units per second she is drawn to the hologram's centre while it glitches, so she ends up inside the cage.")]
        [SerializeField] private float _slideSpeed = 1.5f;
        [Tooltip("Safety limit in seconds for the glitch and slide, so the trap can never hang.")]
        [SerializeField] private float _trapTimeout = 3f;

        [Header("Speech")]
        [SerializeField] private SpeechBubble _bubble;
        [SerializeField] private string _firstSightLine = "Sis?";
        [SerializeField] private float _firstSightHold = 1.5f;
        [SerializeField] private string _trappedLine = "...?";
        [SerializeField] private float _lineHold = 1.8f;

        [Header("First sight (anticipation)")]
        [Tooltip("Pools of light that come on from the door towards the hologram.")]
        [SerializeField] private LightStrip _guideStrip;
        [Tooltip("Where the camera looks while it zooms out; about the middle of the guide strip so door and hologram both fit.")]
        [SerializeField] private Transform _revealFocus;
        [SerializeField] private float _revealSize = 5.5f;
        [Tooltip("Seconds to zoom out, and again to zoom back in.")]
        [SerializeField] private float _revealZoomTime = 1.8f;
        [Tooltip("Seconds the guide lights take to come on, door first.")]
        [SerializeField] private float _guideLightsTime = 2.2f;

        [Header("Reveal")]
        [Tooltip("The two-way mirror the camera drifts to.")]
        [SerializeField] private Transform _mirror;
        [Tooltip("The mirror's glass sprite: dark until the observers' lights come on.")]
        [SerializeField] private SpriteRenderer _glass;
        [Tooltip("Pale lit-room layer inside the glass pane, behind the silhouettes so they read dark against it. Invisible until the twist.")]
        [SerializeField] private SpriteRenderer _paneGlow;
        [Tooltip("Scientist silhouettes (masked to the pane): invisible until the twist.")]
        [SerializeField] private SpriteRenderer[] _scientists;
        [SerializeField, Range(0f, 1f)] private float _paneGlowOpacity = 0.9f;
        [SerializeField, Range(0f, 1f)] private float _glassDarkTint = 0.45f;
        [SerializeField] private float _pullBackSize = 6f;
        [SerializeField] private float _pullBackTime = 5f;
        [Tooltip("Lights behind the mirror, switched on during the pull-back to show the scientists.")]
        [SerializeField] private Light2D[] _observerLights;
        [SerializeField] private float _observerLightTarget = 1f;
        [SerializeField] private float _holdOnScientists = 2.5f;
        [SerializeField] private EndCredits _credits;

        private Rigidbody2D _playerBody;
        private Vector2 _spawn;
        private float _playSize;
        private Color[] _scientistColours;
        private bool _isArriving;
        private bool _trapSprung;

        private void Awake()
        {
            // Claim the chamber start before GameStateController.Start begins play.
            _isArriving = true;
            _game.BeginIntro();
        }

        private void Start()
        {
            _playerBody = _player.GetComponent<Rigidbody2D>();
            _spawn = _player.Transform.position;
            _playSize = Camera.main.orthographicSize;
            _spotlight.enabled = false;
            foreach (Light2D light in _observerLights)
            {
                light.intensity = 0f;
            }

            // The mirror starts as a plain dark pane: glass dimmed, silhouettes fully transparent.
            SetObservers(0f);
            StartCoroutine(Arrive());
        }

        private void OnEnable()
        {
            _input.SkipRequested += SkipArrival;
        }

        private void OnDisable()
        {
            _input.SkipRequested -= SkipArrival;
        }

        /// <summary>Called by the PlayerTrigger around the hologram when the live player steps on it.</summary>
        public void SpringTrap()
        {
            // Only while the player is really in control, so a reset or pause can't be overridden.
            if (!_trapSprung && !_isArriving && _game.State == GameState.Playing)
            {
                _trapSprung = true;
                StartCoroutine(Trap());
            }
        }

        /// <summary>She sprints in through the open door, the shutter slams and control passes to the player.</summary>
        private IEnumerator Arrive()
        {
            // 1. Door open, camera on it, she waits outside.
            _door.Open();
            _shutter.SetRaised(1f);
            _playerBody.simulated = false;
            _player.Teleport(_path[0].position);
            _camera.Follow(_cameraFocus, true);
            while (SceneFlow.IsChangingScene)
            {
                yield return null;
            }

            _input.SetSkipEnabled(true);
            yield return new WaitForSeconds(_openPause);

            // 2. She sprints through the doorway, the shutter slams, the camera turns to follow her.
            for (int i = 1; i < _path.Length; i++)
            {
                yield return RunTo(_path[i].position);
            }

            StartCoroutine(Slam());
            _camera.Follow(_player.Transform, false);
            yield return RunTo(_spawn);

            // 3. She sees the hologram for the first time.
            yield return Reveal();
        }

        /// <summary>
        /// The anticipation beat: the camera zooms out to show the whole way to the hologram, the guide
        /// lights come on towards it, she says "Sis?", then the camera returns to her and play begins.
        /// </summary>
        private IEnumerator Reveal()
        {
            // 1. Glide to the middle of the strip and zoom out so door and hologram are both in view.
            _camera.Follow(_revealFocus, false);
            StartCoroutine(_pullBack.ZoomTo(_revealSize, _revealZoomTime));
            yield return new WaitForSeconds(_revealZoomTime * 0.6f);

            // 2. The guide lights come on from the door towards the hologram.
            yield return _guideStrip.TurnOnInOrder(_guideLightsTime);

            // 3. She speaks.
            _bubble.Show(_firstSightLine);
            yield return new WaitForSeconds(_firstSightHold);
            _bubble.Hide();

            // 4. Back to her at the normal close zoom, then the player gets control.
            _camera.Follow(_player.Transform, false);
            yield return _pullBack.ZoomTo(_playSize, _revealZoomTime);
            FinishArrival();
        }

        /// <summary>Walks the character in a straight line at sprint speed until she arrives.</summary>
        private IEnumerator RunTo(Vector2 target)
        {
            while (Vector2.Distance(_playerBody.position, target) > 0.001f)
            {
                _player.Teleport(Vector2.MoveTowards(_playerBody.position, target, _sprintSpeed * Time.deltaTime));
                yield return null;
            }
        }

        /// <summary>Drops the shutter, shuts the door and shakes the camera.</summary>
        private IEnumerator Slam()
        {
            for (float t = 0f; t < 0.12f; t += Time.deltaTime)
            {
                float fall = t / 0.12f;
                _shutter.SetRaised(1f - fall * fall);
                yield return null;
            }

            _shutter.SetRaised(0f);
            _door.ResetState();
            AudioManager.Play(_doorSlam);
            ScreenShake.Shake(0.08f, 0.3f);
        }

        /// <summary>Any key during the arrival or the first-sight beat: jump to her spawn with control, lights on, normal zoom.</summary>
        private void SkipArrival()
        {
            if (!_isArriving)
            {
                return;
            }

            StopAllCoroutines();
            _shutter.SetRaised(0f);
            _door.ResetState();
            _player.Teleport(_spawn);
            _camera.Follow(_player.Transform, true);
            _pullBack.SnapZoom(_playSize);
            _guideStrip.SetAll(true);
            _bubble.Hide();
            FinishArrival();
        }

        /// <summary>Gives the player control: physics back on and the chamber's first attempt started.</summary>
        private void FinishArrival()
        {
            _isArriving = false;
            _input.SetSkipEnabled(false);
            _player.Teleport(_spawn);
            _playerBody.simulated = true;
            _game.FinishIntro();
        }

        /// <summary>The hologram vanishes, the lights die, the spotlight and cage slam down, then the reveal plays.</summary>
        private IEnumerator Trap()
        {
            // 1. Take control away for good and stop her where she stands.
            _game.EnterCutscene();
            _playerBody.linearVelocity = Vector2.zero;
            _playerBody.simulated = false;

            // 2. The hologram glitches out while she is drawn to its centre, so she is inside the cage
            //    whichever way she came in.
            _hologram.GlitchOut();
            AudioManager.Play(_hologramGlitch);
            AudioManager.StopMusic(_musicFadeOut);
            Vector2 centre = _hologram.transform.position;
            float slideSpeed = Mathf.Max(0.1f, _slideSpeed);
            for (float waited = 0f;
                 waited < _trapTimeout && (!_hologram.IsGone || Vector2.Distance(_playerBody.position, centre) > 0.001f);
                 waited += Time.deltaTime)
            {
                _player.Teleport(Vector2.MoveTowards(_playerBody.position, centre, slideSpeed * Time.deltaTime));
                yield return null;
            }

            yield return new WaitForSeconds(_silenceTime);

            // 3. Every lamp and the guide strip cut out with a clunk, and the spotlight bangs on as the cage falls.
            foreach (Light2D lamp in _roomLamps)
            {
                lamp.enabled = false;
            }

            _guideStrip.SetAll(false);

            AudioManager.Play(_blackout);
            ScreenShake.Shake(_blackoutShakeStrength, 0.3f);
            yield return new WaitForSeconds(0.5f);
            _spotlight.enabled = true;
            _cage.Drop();
            while (!_cage.HasLanded)
            {
                yield return null;
            }

            AudioManager.Play(_cageSlam);

            yield return new WaitForSeconds(0.6f);

            // 4. She is confused.
            _bubble.Show(_trappedLine);
            yield return new WaitForSeconds(_lineHold);
            _bubble.Hide();

            // 5. The camera pulls back to the mirror while the lights behind it come up.
            StartCoroutine(RaiseObserverLights());
            yield return _pullBack.Play(_mirror.position, _pullBackSize, _pullBackTime);
            yield return new WaitForSeconds(_holdOnScientists);

            // 6. Fade out to the end card and credits.
            yield return _credits.Play();
        }

        /// <summary>Fades the lights behind the mirror up over the pull-back, revealing the silhouettes.</summary>
        private IEnumerator RaiseObserverLights()
        {
            for (float t = 0f; t < _pullBackTime; t += Time.deltaTime)
            {
                float k = Mathf.Clamp01(t / _pullBackTime);
                foreach (Light2D light in _observerLights)
                {
                    light.intensity = _observerLightTarget * k;
                }

                SetObservers(k);
                yield return null;
            }

            SetObservers(1f);
        }

        /// <summary>
        /// Sets how revealed the observers are: 0 is a dark glass pane with no silhouettes, 1 is a lit
        /// pane with the silhouettes fully shown.
        /// </summary>
        private void SetObservers(float shown)
        {
            // The glass brightens from its dark tint to full colour as the room behind it lights up.
            float tint = Mathf.Lerp(_glassDarkTint, 1f, shown);
            _glass.color = new Color(tint, tint, tint, 1f);

            // The room behind the glass lights up: a pale layer inside the pane, with the silhouettes on it.
            Color glow = _paneGlow.color;
            glow.a = shown * _paneGlowOpacity;
            _paneGlow.color = glow;

            // Silhouettes keep their own near-black colour; only their opacity changes.
            if (_scientistColours == null)
            {
                _scientistColours = new Color[_scientists.Length];
                for (int i = 0; i < _scientists.Length; i++)
                {
                    _scientistColours[i] = _scientists[i].color;
                }
            }

            for (int i = 0; i < _scientists.Length; i++)
            {
                Color colour = _scientistColours[i];
                colour.a = shown;
                _scientists[i].color = colour;
            }
        }
    }
}
