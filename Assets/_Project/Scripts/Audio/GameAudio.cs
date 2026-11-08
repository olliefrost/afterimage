// GameAudio: one per chamber, on the GameSystems object. Plays the chamber-wide sounds from
// GameStateController events (clone saved, run thrown away, rewind, clone spawn and pick, run mark,
// level complete, clones unlocked), the pause, map and rebind sounds, and handles pause: sound
// effects freeze with the game and the music ducks.
using GameJam.Afterimage;
using GameJam.Core;
using GameJam.Player;
using UnityEngine;

namespace GameJam.Audio
{
    /// <summary>Plays the chamber's state sounds and applies the pause to the audio.</summary>
    public class GameAudio : MonoBehaviour
    {
        [SerializeField] private GameStateController _state;
        [SerializeField] private AfterimageManager _afterimages;
        [SerializeField] private PlayerInputReader _input;
        [SerializeField] private KeyRebinder _rebinder;

        [Header("Cues")]
        [Tooltip("R: the run is saved into a clone.")]
        [SerializeField] private SoundCue _cloneSave;
        [Tooltip("T (or an unmarked death): the run is thrown away.")]
        [SerializeField] private SoundCue _retry;
        [Tooltip("Loops for as long as the rewind between attempts plays.")]
        [SerializeField] private SoundCue _rewindLoop;
        [Tooltip("The tape snap as the rewind stops and the next attempt begins.")]
        [SerializeField] private SoundCue _rewindEnd;
        [SerializeField] private SoundCue _levelComplete;
        [Tooltip("The clone controls pop into the HUD for the first time.")]
        [SerializeField] private SoundCue _clonesUnlocked;
        [Tooltip("Clones beam in as the next attempt starts. One shared sound, not one per clone.")]
        [SerializeField] private SoundCue _cloneSpawn;
        [Tooltip("A clone is picked with keys 1-5 or a click.")]
        [SerializeField] private SoundCue _clonePick;
        [Tooltip("F marks the run so dying saves it.")]
        [SerializeField] private SoundCue _runMarkOn;
        [SerializeField] private SoundCue _runMarkOff;
        [SerializeField] private SoundCue _pause;
        [SerializeField] private SoundCue _resume;
        [Tooltip("Tab is pressed and released.")]
        [SerializeField] private SoundCue _mapOpen;
        [SerializeField] private SoundCue _mapClose;
        [Tooltip("A key is rebound in Settings.")]
        [SerializeField] private SoundCue _rebind;

        private bool _rewinding;
        private bool _mapWasHeld;
        private GameState _lastState = GameState.Intro;

        private void Awake()
        {
            if (_state == null)
            {
                _state = FindAnyObjectByType<GameStateController>();
            }

            if (_afterimages == null)
            {
                _afterimages = FindAnyObjectByType<AfterimageManager>();
            }

            if (_input == null)
            {
                _input = FindAnyObjectByType<PlayerInputReader>();
            }

            if (_rebinder == null)
            {
                _rebinder = FindAnyObjectByType<KeyRebinder>();
            }
        }

        private void OnEnable()
        {
            _state.AttemptEnding += HandleAttemptEnding;
            _state.StateChanged += HandleStateChanged;
            _state.ClonesUnlockedNow += HandleClonesUnlocked;
            _state.ClonePicked += HandleClonePicked;
            _state.RunMarkedChanged += HandleRunMarkedChanged;
            _rebinder.Changed += HandleRebound;
        }

        private void OnDisable()
        {
            _state.AttemptEnding -= HandleAttemptEnding;
            _state.StateChanged -= HandleStateChanged;
            _state.ClonesUnlockedNow -= HandleClonesUnlocked;
            _state.ClonePicked -= HandleClonePicked;
            _state.RunMarkedChanged -= HandleRunMarkedChanged;
            _rebinder.Changed -= HandleRebound;

            // Never leave sound frozen or the music ducked if the scene unloads while paused.
            AudioListener.pause = false;
            AudioManager.DuckMusic(false);
            AudioManager.StopLoop();
        }

        /// <summary>Plays a click as the map is held and released, only while playing.</summary>
        private void Update()
        {
            bool held = _input.MapHeld && _state.State == GameState.Playing;
            if (held != _mapWasHeld)
            {
                AudioManager.Play(held ? _mapOpen : _mapClose);
                _mapWasHeld = held;
            }
        }

        /// <summary>Plays the save or retry sound and starts the rewind whir, which runs until play resumes.</summary>
        private void HandleAttemptEnding(bool saved)
        {
            AudioManager.Play(saved ? _cloneSave : _retry);
            AudioManager.StartLoop(_rewindLoop);
            _rewinding = true;
        }

        /// <summary>
        /// Ends the rewind sound when the next attempt starts (with the clone spawn shimmer if there are
        /// clones), freezes sound effects and ducks the music while paused, and plays the pause, resume
        /// and level complete sounds.
        /// </summary>
        private void HandleStateChanged(GameState state)
        {
            if (_rewinding && state == GameState.Playing)
            {
                _rewinding = false;
                AudioManager.StopLoop();
                AudioManager.Play(_rewindEnd);
                if (_afterimages.CloneCount > 0)
                {
                    AudioManager.Play(_cloneSpawn);
                }
            }

            bool paused = state == GameState.Paused;
            AudioListener.pause = paused;
            AudioManager.DuckMusic(paused);

            // The pause sounds use the UI pool, which is still heard while paused.
            if (paused)
            {
                AudioManager.PlayUi(_pause);
            }
            else if (_lastState == GameState.Paused && state == GameState.Playing)
            {
                AudioManager.PlayUi(_resume);
            }

            _lastState = state;

            if (state == GameState.LevelComplete)
            {
                AudioManager.Play(_levelComplete);
            }
        }

        private void HandleClonesUnlocked()
        {
            AudioManager.Play(_clonesUnlocked);
        }

        private void HandleClonePicked(int slot)
        {
            AudioManager.Play(_clonePick);
        }

        private void HandleRunMarkedChanged(bool marked)
        {
            // The mark also clears itself on every restart; that is not the player's doing, so stay quiet.
            if (_state.State != GameState.Playing)
            {
                return;
            }

            AudioManager.Play(marked ? _runMarkOn : _runMarkOff);
        }

        private void HandleRebound()
        {
            AudioManager.PlayUi(_rebind);
        }
    }
}
