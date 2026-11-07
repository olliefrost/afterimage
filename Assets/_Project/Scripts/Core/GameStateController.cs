// GameStateController: owns the level's flow (Intro, Playing, Paused, Resetting, LevelComplete).
// It listens for the clone/retry/select input, saves or discards the run, resets the room,
// respawns the clones and restarts the tick clock. One per level scene. The HUD buttons and
// clone icons call the same Request methods as the keys; PauseMenu calls Pause and Resume.
// On start it saves this chamber as the Continue point. In a chamber that teaches cloning, the
// clone controls stay locked until something calls UnlockClones (see CloneUnlocker).
using System;
using System.Collections;
using GameJam.Afterimage;
using GameJam.Player;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace GameJam.Core
{
    /// <summary>The states a level can be in.</summary>
    public enum GameState
    {
        Intro,
        Playing,
        Paused,
        Resetting,
        LevelComplete
    }

    /// <summary>
    /// Runs attempts: each new attempt optionally keeps the last run as a clone, resets every
    /// <see cref="IResettable"/> and replays all saved clones from tick 0.
    /// </summary>
    public class GameStateController : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private TickClock _clock;
        [SerializeField] private PlayerInputReader _input;
        [SerializeField] private AfterimageRecorder _recorder;
        [SerializeField] private AfterimageManager _afterimages;

        [Header("Clones")]
        [Tooltip("Off in a chamber that introduces cloning: R, T, 1-5 and the clone HUD stay locked until UnlockClones is called.")]
        [SerializeField] private bool _clonesStartUnlocked = true;

        [Header("Level complete")]
        [Tooltip("Seconds to show the level complete screen before loading the next scene in Build Settings.")]
        [SerializeField] private float _nextLevelDelay = 3f;

        /// <summary>Current state of the level.</summary>
        public GameState State { get; private set; } = GameState.Playing;

        /// <summary>Raised whenever <see cref="State"/> changes.</summary>
        public event Action<GameState> StateChanged;

        /// <summary>True once the player may save, retry and pick clones in this chamber.</summary>
        public bool ClonesUnlocked { get; private set; }

        /// <summary>Raised once, when the clone controls unlock part-way through a chamber.</summary>
        public event Action ClonesUnlockedNow;

        private void Awake()
        {
            ClonesUnlocked = _clonesStartUnlocked;
        }

        private void OnEnable()
        {
            _input.CloneRequested += RequestClone;
            _input.RetryRequested += RequestRetry;
            _input.SelectCloneRequested += RequestSelect;
        }

        private void OnDisable()
        {
            _input.CloneRequested -= RequestClone;
            _input.RetryRequested -= RequestRetry;
            _input.SelectCloneRequested -= RequestSelect;
        }

        private void Start()
        {
            SaveProgress.SaveChamber(SceneManager.GetActiveScene().buildIndex);

            // An intro claimed the start in Awake: the attempt begins when it calls FinishIntro.
            if (State == GameState.Intro)
            {
                return;
            }

            _clock.Restart();
            SetState(GameState.Playing);
        }

        /// <summary>
        /// Holds the chamber in the Intro state: no clock, no recording, no gameplay keys. Called by
        /// IntroSequence in Awake, before this script's Start would begin play.
        /// </summary>
        public void BeginIntro()
        {
            _input.SetGameplayEnabled(false);
            SetState(GameState.Intro);
        }

        /// <summary>Ends the intro and starts the first attempt from tick 0, as a normal chamber start would.</summary>
        public void FinishIntro()
        {
            if (State != GameState.Intro)
            {
                return;
            }

            _input.SetGameplayEnabled(true);
            _clock.Restart();
            SetState(GameState.Playing);
        }

        private void OnDestroy()
        {
            // Never leave the game frozen if the scene unloads while paused.
            if (State == GameState.Paused)
            {
                Time.timeScale = 1f;
            }
        }

        /// <summary>
        /// Freezes the attempt: time stops, so the tick clock, the player and every clone hold their
        /// tick, and gameplay keys are ignored until <see cref="Resume"/>. Used by the pause menu.
        /// </summary>
        public void Pause()
        {
            if (State != GameState.Playing)
            {
                return;
            }

            Time.timeScale = 0f;
            _input.SetGameplayEnabled(false);
            SetState(GameState.Paused);
        }

        /// <summary>Continues a paused attempt from the tick it stopped on.</summary>
        public void Resume()
        {
            if (State != GameState.Paused)
            {
                return;
            }

            Time.timeScale = 1f;
            _input.SetGameplayEnabled(true);
            SetState(GameState.Playing);
        }

        /// <summary>
        /// Ends the run, saves it into the clone the player is controlling, switches to the picked
        /// clone if any, and starts the next attempt. Used by R and the HUD button.
        /// </summary>
        public void RequestClone()
        {
            StartNewAttempt(true);
        }

        /// <summary>
        /// Ends the run without keeping it, switches to the picked clone if any, and starts the next
        /// attempt. Used by T and the HUD button.
        /// </summary>
        public void RequestRetry()
        {
            StartNewAttempt(false);
        }

        /// <summary>Picks (or un-picks) the clone to control from the next restart. Used by keys 1-5 and the HUD icons.</summary>
        public void RequestSelect(int slot)
        {
            if (State != GameState.Playing || !ClonesUnlocked)
            {
                return;
            }

            _afterimages.SelectSlot(slot);
        }

        /// <summary>
        /// Turns the clone controls on for the rest of this chamber and tells the HUD to reveal them.
        /// Called by CloneUnlocker the first time the player falls into the pit; does nothing if already on.
        /// </summary>
        public void UnlockClones()
        {
            if (ClonesUnlocked)
            {
                return;
            }

            ClonesUnlocked = true;
            ClonesUnlockedNow?.Invoke();
        }

        /// <summary>Ends the level: freezes play, shows the complete screen and moves on if a next scene exists.</summary>
        public void CompleteLevel()
        {
            if (State != GameState.Playing)
            {
                return;
            }

            _clock.Stop();
            SetState(GameState.LevelComplete);
            StartCoroutine(LoadNextLevelAfterDelay());
        }

        /// <summary>Resets the room for a new attempt, keeping the finished run as a clone if asked.</summary>
        private void StartNewAttempt(bool saveRun)
        {
            if (State != GameState.Playing || !ClonesUnlocked)
            {
                return;
            }

            SetState(GameState.Resetting);
            _clock.Stop();

            // 1. Hand the finished run to the clone slots: saved into the controlled slot only if asked.
            _afterimages.EndRun(_recorder.TakeRecording(), saveRun);

            // 2. Return the room and the player to their starting state.
            LevelReset.ResetAll();

            // 3. Spawn a clone for every saved run except the one now controlled, then count again from tick 0.
            _afterimages.RespawnClones();
            _clock.Restart();
            SetState(GameState.Playing);
        }

        /// <summary>Waits on the complete screen, then loads the next scene if there is one.</summary>
        private IEnumerator LoadNextLevelAfterDelay()
        {
            yield return new WaitForSeconds(_nextLevelDelay);

            int nextIndex = SceneManager.GetActiveScene().buildIndex + 1;
            if (nextIndex < SceneManager.sceneCountInBuildSettings)
            {
                SceneManager.LoadScene(nextIndex);
            }
        }

        private void SetState(GameState state)
        {
            State = state;
            StateChanged?.Invoke(state);
        }
    }
}
