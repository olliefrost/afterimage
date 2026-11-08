// GameStateController: owns the level's flow (Intro, Playing, Paused, Resetting, LevelComplete).
// It listens for the clone/retry/select/mark input, saves or discards the run, plays the optional
// RewindEffect, resets the room, respawns the clones and restarts the tick clock. One per level
// scene. The HUD buttons and clone icons call the same Request methods as the keys; PauseMenu
// calls Pause and Resume.
// On start it saves this chamber as the Continue point; on completion it stops the player and asks
// SceneFlow to wipe to the next chamber. In a chamber that teaches cloning, the
// clone controls stay locked until something calls UnlockClones (see CloneUnlocker).
// If a hazard kills the player, the room freezes for a moment so the hit reads, then restarts.
// The death saves the run as a clone only if the player marked the run first (F), which is how
// a clone is sent ahead to take a hit.
using System;
using System.Collections;
using GameJam.Afterimage;
using GameJam.Player;
using GameJam.Visuals;
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
        [Tooltip("The live player, so a hazard killing it restarts the attempt.")]
        [SerializeField] private PlayerActor _player;

        [Header("Player death")]
        [Tooltip("Seconds the room stays frozen after the player is killed, so the hit is visible, before it restarts.")]
        [SerializeField] private float _deathResetDelay = 0.8f;

        [Header("Clones")]
        [Tooltip("Off in a chamber that introduces cloning: R, T, 1-5 and the clone HUD stay locked until UnlockClones is called.")]
        [SerializeField] private bool _clonesStartUnlocked = true;

        [Header("Presentation")]
        [Tooltip("Optional rewind shown between attempts (Rewind prefab). Empty = the room resets instantly.")]
        [SerializeField] private RewindEffect _rewind;

        [Header("Level complete")]
        [Tooltip("Seconds the player stands still at the exit before the screen wipes to the next chamber.")]
        [SerializeField] private float _exitDelay = 0.4f;
        [Tooltip("Shown on the black between chambers.")]
        [SerializeField] private string _completeCaption = "CHAMBER COMPLETED";

        /// <summary>Current state of the level.</summary>
        public GameState State { get; private set; } = GameState.Playing;

        /// <summary>Raised whenever <see cref="State"/> changes.</summary>
        public event Action<GameState> StateChanged;

        /// <summary>
        /// True once the player may save, retry and pick clones in this chamber. Read straight from the
        /// serialized flag, so it is already right when other objects' OnEnable runs before this Awake.
        /// </summary>
        public bool ClonesUnlocked => _clonesStartUnlocked || _unlockedThisChamber;

        private bool _unlockedThisChamber;

        /// <summary>Raised once, when the clone controls unlock part-way through a chamber.</summary>
        public event Action ClonesUnlockedNow;

        /// <summary>True if this run is marked to become a clone when the player dies. Clears on every restart.</summary>
        public bool RunMarked { get; private set; }

        /// <summary>Raised whenever <see cref="RunMarked"/> changes, for the HUD's record button.</summary>
        public event Action<bool> RunMarkedChanged;

        private void OnEnable()
        {
            _input.CloneRequested += RequestClone;
            _input.RetryRequested += RequestRetry;
            _input.SelectCloneRequested += RequestSelect;
            _input.MarkRunRequested += ToggleRunMark;
            _player.Died += HandlePlayerDied;
        }

        private void OnDisable()
        {
            _input.CloneRequested -= RequestClone;
            _input.RetryRequested -= RequestRetry;
            _input.SelectCloneRequested -= RequestSelect;
            _input.MarkRunRequested -= ToggleRunMark;
            _player.Died -= HandlePlayerDied;
        }

        private IEnumerator Start()
        {
            SaveProgress.SaveChamber(SceneManager.GetActiveScene().buildIndex);

            // An intro claimed the start in Awake: the attempt begins when it calls FinishIntro.
            if (State == GameState.Intro)
            {
                yield break;
            }

            // Arrived behind a screen wipe: hold the chamber like an intro until it is visible, so
            // nothing moves or is recorded in the dark.
            if (SceneFlow.IsChangingScene)
            {
                BeginIntro();
                while (SceneFlow.IsChangingScene)
                {
                    yield return null;
                }

                FinishIntro();
                yield break;
            }

            _clock.Restart();
            SetState(GameState.Playing);
        }

        /// <summary>
        /// Holds the chamber in the Intro state: no clock, no recording, no gameplay keys. Called by
        /// IntroSequence in Awake, before this script's Start would begin play, and by Start while
        /// the arrival wipe still covers the screen.
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
        /// Marks or unmarks this run: a marked run is saved into the controlled clone if the player
        /// dies, so the clone walks into the same hit and shields later runs. R and T are unchanged.
        /// Used by F and the HUD's record button; locked with the other clone controls.
        /// </summary>
        public void ToggleRunMark()
        {
            if (State != GameState.Playing || !ClonesUnlocked)
            {
                return;
            }

            SetRunMarked(!RunMarked);
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

            _unlockedThisChamber = true;
            ClonesUnlockedNow?.Invoke();
        }

        /// <summary>
        /// Ends the level: freezes play and stops the player, then wipes to the next chamber (or the
        /// title after the last one) with the complete caption on the black.
        /// </summary>
        public void CompleteLevel()
        {
            if (State != GameState.Playing)
            {
                return;
            }

            _clock.Stop();
            _input.SetGameplayEnabled(false);
            SetState(GameState.LevelComplete);
            StartCoroutine(LeaveChamberAfterDelay());
        }

        /// <summary>Starts a new attempt from the player's request, keeping the finished run as a clone if asked.</summary>
        private void StartNewAttempt(bool saveRun)
        {
            if (State != GameState.Playing || !ClonesUnlocked)
            {
                return;
            }

            SetState(GameState.Resetting);
            _clock.Stop();
            StartCoroutine(ResetRoom(saveRun));
        }

        /// <summary>
        /// Freezes the room the moment the player is killed (so R, T and F are ignored too), then
        /// restarts, saving the run as a clone only if it was marked.
        /// </summary>
        private void HandlePlayerDied()
        {
            if (State != GameState.Playing)
            {
                return;
            }

            SetState(GameState.Resetting);
            _clock.Stop();
            StartCoroutine(ResetAfterDeath(RunMarked));
        }

        /// <summary>Holds the frozen death frame for a moment, then starts the next attempt.</summary>
        private IEnumerator ResetAfterDeath(bool saveRun)
        {
            yield return new WaitForSeconds(_deathResetDelay);
            yield return ResetRoom(saveRun);
        }

        /// <summary>
        /// Ends the run, plays the optional rewind, resets the room and starts the next attempt.
        /// Expects the clock to be stopped, so nothing is recorded while the rewind plays.
        /// </summary>
        private IEnumerator ResetRoom(bool saveRun)
        {
            Recording run = _recorder.TakeRecording();

            // 1. Show time running backwards (visual only) while the slots still describe the ended run.
            if (_rewind != null)
            {
                yield return _rewind.Play(run, saveRun);
            }

            // 2. Hand the finished run to the clone slots: saved into the controlled slot only if asked.
            _afterimages.EndRun(run, saveRun);

            // 3. Return the room and the player to their starting state.
            LevelReset.ResetAll();

            // 4. Spawn a clone for every saved run except the one now controlled, then count again from tick 0.
            _afterimages.RespawnClones();
            _clock.Restart();
            SetRunMarked(false);
            SetState(GameState.Playing);
        }

        /// <summary>Sets the run mark and tells the HUD, only when it actually changes.</summary>
        private void SetRunMarked(bool marked)
        {
            if (RunMarked == marked)
            {
                return;
            }

            RunMarked = marked;
            RunMarkedChanged?.Invoke(marked);
        }

        /// <summary>Holds the finished chamber for a moment, then hands over to SceneFlow's wipe.</summary>
        private IEnumerator LeaveChamberAfterDelay()
        {
            yield return new WaitForSeconds(_exitDelay);
            SceneFlow.CompleteChamber(SceneManager.GetActiveScene().buildIndex, _completeCaption);
        }

        private void SetState(GameState state)
        {
            State = state;
            StateChanged?.Invoke(state);
        }
    }
}
