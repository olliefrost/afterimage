// AudioManager: the one place that plays sound. It creates itself before the first scene loads and
// survives scene changes, so nothing needs wiring in a scene. It owns a small pool of AudioSources
// for sound effects (SoundCue plays, with a gap and a copy limit per cue) and two sources for music
// that crossfade when the track changes. Audio never affects gameplay, so it is not tick-timed.
// Used by the bridge components, SceneMusic and GameAudio.
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace GameJam.Audio
{
    /// <summary>Plays pooled sound effects and crossfading music through static calls.</summary>
    public class AudioManager : MonoBehaviour
    {
        private const int PoolSize = 16;
        private const int UiPoolSize = 4;
        private const float MusicFadeSeconds = 1.5f;
        private const float DuckedMusicScale = 0.35f;
        private const float DuckSpeed = 4f;
        private const float LoopFadeSeconds = 0.08f;

        private static AudioManager _instance;

        private readonly Dictionary<SoundCue, float> _lastPlayed = new Dictionary<SoundCue, float>();
        private AudioSource[] _sfxSources;
        private SoundCue[] _sfxCues;
        private AudioListener _fallbackListener;
        private AudioSource _loopSource;
        private bool _loopFading;
        private AudioSource[] _musicSources;
        private float[] _musicLevels;
        private int _activeMusic;
        private AudioClip _track;
        private float[] _trackVolumes;
        private float _musicFade = MusicFadeSeconds;
        private float _duck = 1f;
        private float _duckTarget = 1f;

        /// <summary>Creates the manager before any scene loads so it is always there.</summary>
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Create()
        {
            if (_instance != null)
            {
                return;
            }

            GameObject host = new GameObject("AudioManager");
            DontDestroyOnLoad(host);
            _instance = host.AddComponent<AudioManager>();
        }

        /// <summary>Plays a cue as a 2D sound. <paramref name="volumeScale"/> turns it down, e.g. for clones.</summary>
        public static void Play(SoundCue cue, float volumeScale = 1f)
        {
            if (_instance != null)
            {
                _instance.PlayCue(cue, volumeScale);
            }
        }

        /// <summary>Plays a cue as a 2D sound that is still heard while the game is paused (menu and pause sounds).</summary>
        public static void PlayUi(SoundCue cue, float volumeScale = 1f)
        {
            if (_instance != null)
            {
                _instance.PlayCue(cue, volumeScale, true);
            }
        }

        /// <summary>Starts a looping sound effect (the rewind whir) until <see cref="StopLoop"/>. Only one loop plays at a time.</summary>
        public static void StartLoop(SoundCue cue)
        {
            if (_instance != null && cue != null && cue.HasClips)
            {
                // A loop that is already sounding (and not being faded out) keeps going instead of restarting.
                if (_instance._loopSource.isPlaying && !_instance._loopFading)
                {
                    return;
                }

                AudioSource source = _instance._loopSource;
                source.clip = cue.PickClip();
                source.volume = cue.PickVolume();
                source.pitch = 1f;
                source.Play();
                _instance._loopFading = false;
            }
        }

        /// <summary>Fades the running loop out quickly and stops it, so it ends without a click.</summary>
        public static void StopLoop()
        {
            if (_instance != null && _instance._loopSource.isPlaying)
            {
                _instance._loopFading = true;
            }
        }

        /// <summary>Starts a music track with a crossfade. Does nothing if that track is already playing.</summary>
        public static void PlayMusic(AudioClip track, float volume = 0.5f)
        {
            if (_instance != null)
            {
                _instance.StartTrack(track, volume);
            }
        }

        /// <summary>Fades the music out to silence over <paramref name="seconds"/> (the Ending's trap).</summary>
        public static void StopMusic(float seconds)
        {
            if (_instance != null)
            {
                _instance._musicFade = Mathf.Max(0.01f, seconds);
                _instance._track = null;
            }
        }

        /// <summary>Turns the music down (pause menu) or back up.</summary>
        public static void DuckMusic(bool ducked)
        {
            if (_instance != null)
            {
                _instance._duckTarget = ducked ? DuckedMusicScale : 1f;
            }
        }

        private void Awake()
        {
            // 1. A pool of 2D sources for sound effects.
            //    The last few sources are for UI and ignore the listener pause, so menus work while paused.
            _sfxSources = new AudioSource[PoolSize + UiPoolSize];
            _sfxCues = new SoundCue[PoolSize + UiPoolSize];
            for (int i = 0; i < _sfxSources.Length; i++)
            {
                bool isUi = i >= PoolSize;
                _sfxSources[i] = CreateSource((isUi ? "Ui" : "Sfx") + i, isUi);
                _sfxSources[i].loop = false;
            }

            _loopSource = CreateSource("SfxLoop", false);
            _loopSource.loop = true;

            // 2. Two looping sources for music. They ignore the listener pause, so music plays under the pause menu.
            _musicSources = new[] { CreateSource("MusicA", true), CreateSource("MusicB", true) };
            _musicLevels = new float[2];
            _trackVolumes = new float[2];

            // 3. Unity plays nothing without an AudioListener, and our cameras have none: keep our own
            //    unless a scene supplies one.
            _fallbackListener = gameObject.AddComponent<AudioListener>();
            SceneManager.sceneLoaded += HandleSceneLoaded;
        }

        private void OnDestroy()
        {
            SceneManager.sceneLoaded -= HandleSceneLoaded;
        }

        private void HandleSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            // Only one listener may be enabled, so step aside if the new scene has its own.
            _fallbackListener.enabled = !HasSceneListener();
        }

        private static bool HasSceneListener()
        {
            foreach (AudioListener listener in FindObjectsByType<AudioListener>())
            {
                if (listener.gameObject.scene.name != "DontDestroyOnLoad")
                {
                    return true;
                }
            }

            return false;
        }

        private void Update()
        {
            // 1. Fade a stopping effect loop to silence, then stop it.
            if (_loopFading)
            {
                _loopSource.volume = Mathf.MoveTowards(_loopSource.volume, 0f, Time.unscaledDeltaTime / LoopFadeSeconds);
                if (_loopSource.volume <= 0f)
                {
                    _loopSource.Stop();
                    _loopFading = false;
                }
            }

            // 2. Ease the pause duck and crossfade the music sources.
            _duck = Mathf.MoveTowards(_duck, _duckTarget, DuckSpeed * Time.unscaledDeltaTime);
            float step = Time.unscaledDeltaTime / _musicFade;
            for (int i = 0; i < _musicSources.Length; i++)
            {
                // The active source fades up and the other fades out, then stops.
                float target = i == _activeMusic && _track != null ? 1f : 0f;
                _musicLevels[i] = Mathf.MoveTowards(_musicLevels[i], target, step);
                _musicSources[i].volume = _musicLevels[i] * _trackVolumes[i] * _duck;
                if (_musicLevels[i] <= 0f && _musicSources[i].isPlaying)
                {
                    _musicSources[i].Stop();
                }
            }
        }

        private AudioSource CreateSource(string sourceName, bool isMusic)
        {
            GameObject child = new GameObject(sourceName);
            child.transform.SetParent(transform);
            AudioSource source = child.AddComponent<AudioSource>();
            source.playOnAwake = false;
            source.loop = isMusic;
            source.ignoreListenerPause = isMusic;
            return source;
        }

        /// <summary>Plays one cue on a free source (from the UI pool if <paramref name="ui"/>), unless it played too recently or too many copies are sounding.</summary>
        private void PlayCue(SoundCue cue, float volumeScale, bool ui = false)
        {
            if (cue == null || !cue.HasClips)
            {
                return;
            }

            // 1. Respect the cue's minimum gap.
            float now = Time.unscaledTime;
            if (_lastPlayed.TryGetValue(cue, out float last) && now - last < cue.MinGap)
            {
                return;
            }

            // 2. Find a free source and count the copies of this cue already sounding.
            int first = ui ? PoolSize : 0;
            int end = ui ? PoolSize + UiPoolSize : PoolSize;
            int free = -1;
            int copies = 0;
            for (int i = first; i < end; i++)
            {
                if (_sfxSources[i].isPlaying)
                {
                    copies += _sfxCues[i] == cue ? 1 : 0;
                }
                else if (free < 0)
                {
                    free = i;
                }
            }

            if (free < 0 || copies >= cue.MaxCopies)
            {
                return;
            }

            // 3. Play a random variant at a random pitch.
            AudioSource source = _sfxSources[free];
            source.clip = cue.PickClip();
            source.volume = cue.PickVolume() * volumeScale;
            source.pitch = cue.PickPitch();
            source.Play();
            _sfxCues[free] = cue;
            _lastPlayed[cue] = now;
        }

        /// <summary>Crossfades to a new track: it starts on the idle source while the old one fades out.</summary>
        private void StartTrack(AudioClip track, float volume)
        {
            if (track == null)
            {
                return;
            }

            // The same track keeps playing; just apply its volume.
            if (track == _track)
            {
                _trackVolumes[_activeMusic] = volume;
                return;
            }

            // Each source keeps its own volume, so the fading-out track does not jump to the new one's level.
            _musicFade = MusicFadeSeconds;
            _track = track;
            _activeMusic = 1 - _activeMusic;
            _trackVolumes[_activeMusic] = volume;
            AudioSource source = _musicSources[_activeMusic];
            source.clip = track;
            source.Play();
        }
    }
}
