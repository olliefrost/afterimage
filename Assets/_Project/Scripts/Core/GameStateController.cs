// GameStateController: owns the level's flow (Playing, Resetting, LevelComplete).
// It listens for the clone/retry input, saves or discards the run, resets the room, respawns
// the clones and restarts the tick clock. One per level scene.
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
        Playing,
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

        [Header("Level complete")]
        [Tooltip("Seconds to show the level complete screen before loading the next scene in Build Settings.")]
        [SerializeField] private float _nextLevelDelay = 3f;

        /// <summary>Current state of the level.</summary>
        public GameState State { get; private set; } = GameState.Playing;

        /// <summary>Raised whenever <see cref="State"/> changes.</summary>
        public event Action<GameState> StateChanged;

        private void OnEnable()
        {
            _input.CloneRequested += RequestClone;
            _input.RetryRequested += RequestRetry;
        }

        private void OnDisable()
        {
            _input.CloneRequested -= RequestClone;
            _input.RetryRequested -= RequestRetry;
        }

        private void Start()
        {
            _clock.Restart();
            SetState(GameState.Playing);
        }

        /// <summary>Ends the run, keeps it as a new clone and starts the next attempt. Used by R and the HUD button.</summary>
        public void RequestClone()
        {
            StartNewAttempt(true);
        }

        /// <summary>Ends the run without keeping it and starts the next attempt. Used by T.</summary>
        public void RequestRetry()
        {
            StartNewAttempt(false);
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
            if (State != GameState.Playing)
            {
                return;
            }

            SetState(GameState.Resetting);
            _clock.Stop();

            // 1. Take the finished run out of the recorder, and keep it only if asked.
            Recording finishedRun = _recorder.TakeRecording();
            if (saveRun)
            {
                _afterimages.AddRecording(finishedRun);
            }

            // 2. Return the room and the player to their starting state.
            LevelReset.ResetAll();

            // 3. Spawn a fresh clone for every saved run, then count again from tick 0.
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
