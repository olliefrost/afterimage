# Sound design

**Purpose:** The plan for all music and sound effects (GitHub issues #11 "Add music" and #12 "Add
SFX"): what the game should sound like, every sound it needs and the code event that triggers it,
where to get free jam-safe assets, and how to wire audio in without touching gameplay. Nothing is
built yet: `Assets/_Project/Audio/` is empty and no script plays audio.

> **Status after the jam:** this plan is implemented. Clips were generated with ElevenLabs rather
> than taken from free libraries (see `CREDITS.md`), and the audio code is described under "Audio"
> in `CLAUDE.md`. The text below is the original plan and is kept for reference.

## Tone

- **World:** a deserted, overgrown relic lab. A low electrical room hum, distant drips, wind through
  ceiling breaches, buzzing lamps that fail. Machinery sounds worn: clunky, rattly, slightly out of
  true. Nothing clean or new.
- **Afterimage tech:** glitchy and "rewound". Tape stops, reversed swells, granular shimmer,
  bit-crushed edges. It should sound like the tech is barely holding together, more so as clones
  pile up.
- **Theme "Time":** a ticking or clock motif, used sparingly: the rewind between attempts and saving
  a clone. Not a constant tick, which gets tiring.
- **Mix:** quiet and sparse. The camera is close and vision is a small pool of light, so sound tells
  the player about things off-screen (a door opening, a clone pressing a button).

## Mixer

One `Audio/Main.mixer`:

```
Master
├── Music
├── Ambience
└── SFX
    ├── World    (doors, buttons, wires, pits, player footsteps)
    ├── Clones   (everything a clone causes; a few dB down, light low-pass / bit-crush)
    └── UI       (menus, HUD, dialogue)
```

- Expose a volume parameter per group so a volume slider in Settings is a small job later.
- The Clones group makes clone sounds read as "echoes" of the player without extra assets.
- A `Paused` snapshot ducks and low-passes Music and Ambience; the default snapshot restores them.

## Sound effects

Priority: **M** = needed for the Must have level (Tutorial1), **S** = Should have (enemies, later
chambers), **X** = stretch.

### Player and movement

| Sound | Trigger | Code hook | Pri | Notes |
|---|---|---|---|---|
| `sfx_step_*` | Each foot contact while running | `Visuals/CharacterSpriteAnimator` run frames (step on chosen frames) | M | 3-4 variants, random pitch ±5%. Concrete/grit underfoot; a softer set for grass/rubble later |
| `sfx_pit_fall` | Live player or clone drops into the pit | `Pit.ActorEntered` | M | Short scrape and drop, then `sfx_pit_land` thud |
| `sfx_pit_land` | Landing in the pit | `Pit.ActorEntered` (after a short delay) | M | Heavy, slightly echoey |
| `sfx_cage_close` | Pit cages the live player | `Pit.ActorEntered` (player only) | S | Metallic bars |

### Interactables

| Sound | Trigger | Code hook | Pri | Notes |
|---|---|---|---|---|
| `sfx_button_press` | Button pressed and latched | `PressButton.Pressed` / `_onPressed` | M | Clunk plus a mechanical latch click |
| `sfx_wire_pulse` (loop) | Signal travelling along the wire | `SignalWire.Send()` until `_onArrived` | M | Electric crackle/hum; positioned along the wire if cheap, else at the wire start |
| `sfx_wire_arrive` | Signal reaches the door | `SignalWire._onArrived` | M | Short zap/spark |
| `sfx_door_open` | Door shutter rolls up | `Door._onOpened` | M | Motor whine plus rattling shutter, length matched to the flipbook |
| `sfx_door_close` | Door shutter drops | `Door._onClosed` | M | Rattle down |
| `sfx_door_slam` | Shutter hits the floor hard (intro) | `IntroSequence` slam step (with `CameraFollow.Shake`) | M | Big impact with a metal ring-out |
| `sfx_exit` | Player walks through the level exit | `LevelExit` (`_onPlayerExited`) | M | Airlock hiss or rising whoosh |

### Afterimages

| Sound | Trigger | Code hook | Pri | Notes |
|---|---|---|---|---|
| `sfx_clone_save` | R saves the run | `GameStateController.RequestClone` | M | Rising "capture" tone with a tick |
| `sfx_retry` | T throws the run away | `GameStateController.RequestRetry` | M | Shorter, flatter, falling version of the save |
| `sfx_rewind` | Tape rewind between attempts | `Visuals/RewindEffect.Play` (start of the coroutine) | M | Reversed tape whir, ends on a snap at the flash |
| `sfx_clone_spawn` | Clone beams in at attempt start | `Visuals/CloneMaterialise` | M | Soft shimmer; one shared sound, not one per clone |
| `sfx_clone_select` | Pick a clone with 1-5 or a click | `PlayerInputReader.SelectCloneRequested`, `CloneIconView.Clicked` | M | Blip; pitch rises with the slot number |
| `sfx_clone_death` | Clone dies | `AfterimageManager.CloneDied` | S | Glitch/crunch, cut off abruptly |
| `sfx_clones_unlocked` | Clone controls appear (Tutorial1 pit) | `GameStateController.ClonesUnlockedNow` | M | Small sting as the HUD pops in |

Clone footsteps and the sounds clones cause (button presses) come from the same hooks, routed to the
Clones group. With 5 clones this can get busy (see open questions).

### Progress and story

| Sound | Trigger | Code hook | Pri | Notes |
|---|---|---|---|---|
| `sfx_level_complete` | Level complete panel | `GameStateController.StateChanged` (LevelComplete) | M | Short sting, music ducks under it |
| `sfx_guard_shout` | "HEY! GET BACK HERE!" line | `IntroSequence` dialogue step | S | Distant muffled shout, or a radio burst if we stay text-only |
| `sfx_sprint` | Player sprints in during the intro | `IntroSequence` sprint step | X | Faster footsteps reuse `sfx_step_*` |

### UI

| Sound | Trigger | Code hook | Pri | Notes |
|---|---|---|---|---|
| `sfx_ui_hover` / `sfx_ui_click` | Menu buttons | `MenuButton` prefab (one place for every menu button) | M | Quiet, dry, "old terminal" |
| `sfx_ui_pause` / `sfx_ui_resume` | Esc pause menu | `GameStateController.StateChanged` (Paused / Playing) | S | |
| `sfx_map_open` / `sfx_map_close` | Hold / release Tab | `PlayerInputReader.MapHeld` changes | S | CRT power-on blip |
| `sfx_rebind` | Key rebound in Settings | `KeyRebinder.Changed` | X | |
| `sfx_text_tick` | Each typed character in the dialogue box | `UI/DialogueBox` typing loop | S | Very short, random pitch, every 2nd character |

### Ambience

| Sound | Trigger | Code hook | Pri | Notes |
|---|---|---|---|---|
| `amb_lab_hum` (loop) | Always in a chamber | Scene ambience source | M | Low room tone, mono, seamless loop |
| `amb_drips`, `amb_wind` | Random one-shots | Scene ambience source, random interval | S | Wind near the ceiling breach over the pit |
| `sfx_lamp_buzz` | Lamp flickers | `Visuals/FlickerLight` | S | Electrical buzz and tick, positioned at the lamp |

### Later chambers and enemies (Should have)

Not needed until those features exist. From `chamber-3.md` and the general level ideas:
laser charge-up, fire and burn; enemy alert and attack; power generator start-up (a big "room
powers on" moment with lights and machinery kicking in); conveyor belt loop; fuel burning in the
laser; hydraulic block push and hiss-back; hovering stair plates flip and slide; pressure plate
down/up; damage/death for the player.

## Music

| Track | Where | Notes |
|---|---|---|
| `mus_title` | Title screen | Slow, lonely, a hint of the main motif |
| `mus_chamber` | Every chamber (one track to start) | Sparse ambient loop: low synth pads, broken/irregular percussion, occasional detuned bell. 2-3 min, seamless loop, never busy enough to distract from planning |
| `mus_chamber_tension` (stem) | Layered over `mus_chamber` | Optional: fades in once clones unlock or when 3+ clones are active (same tempo and key so it crossfades) |
| `mus_intro_sting` | Intro door slam | A hit to start the game |
| `mus_complete` | Level complete | 3-5 s sting, then the next chamber's music |

- **Across scenes:** one music player survives scene loads and crossfades when the track changes,
  so chamber transitions (issue #15) do not cut the music.
- **Pause:** keep the music playing, ducked and low-passed (the `Paused` snapshot).
- **Rewind:** briefly pitch the music down (tape-slow) during `RewindEffect`, back up on the flash.
  Cheap and ties the music to the time theme.

## Where to get free assets

Every file used gets a row in `CREDITS.md` (name, author, link, licence) when it is added. Prefer
CC0 so there is nothing to forget. Never use audio ripped from other games (Portal 2 is inspiration
only).

| Source | Licence | Good for |
|---|---|---|
| [Kenney](https://kenney.nl/assets?q=audio) (Sci-fi, Impact, Interface, UI Audio packs) | CC0 | UI clicks, impacts, sci-fi blips. Best first stop |
| [Freesound](https://freesound.org) | Per sound: filter to CC0 or CC-BY, skip CC-BY-NC | Specific foley: shutters, machinery hum, drips, electrical crackle |
| [OpenGameArt](https://opengameart.org) | Per asset (CC0, CC-BY, GPL); check each | SFX packs and loopable game music |
| [Sonniss GDC Game Audio Bundles](https://sonniss.com/gameaudiogdc) | Royalty-free, no attribution needed | High quality industrial, sci-fi and ambience libraries (large downloads) |
| [Pixabay sound effects](https://pixabay.com/sound-effects/) and [music](https://pixabay.com/music/) | Pixabay Content Licence | Ambience loops, stings, ambient music |
| [incompetech (Kevin MacLeod)](https://incompetech.com/music/royalty-free/) | CC-BY (credit required) | Dark ambient and tension music |
| [FreePD](https://freepd.com) | Public domain | Music |
| [jsfxr](https://sfxr.me), [ChipTone](https://sfbgames.itch.io/chiptone), Bfxr | Generated output is ours | Quick blips, zaps, UI and glitch sounds |
| [Audacity](https://www.audacityteam.org) | Free tool | Trim, normalise, reverse (rewind), pitch-shift, make seamless loops |

Tips: search Freesound for "roller shutter", "electrical hum", "tape rewind", "servo", "metal
clunk", "glitch". Layer two cheap sounds (a click plus a thud) rather than hunting for one perfect
sound.

## Implementation

To build later; nothing below exists yet.

- **Folder and namespace:** `Scripts/Audio/`, `GameJam.Audio`. Clips in `Audio/SFX`, `Audio/Music`,
  `Audio/Ambience`, named `sfx_<thing>_<variant>`, `mus_<name>`, `amb_<name>`.
- **`SoundCue` (ScriptableObject):** a list of clip variants, volume, pitch range, mixer group and a
  minimum gap between plays. Teammates tune sounds in the inspector without code.
- **`AudioManager` (on GameSystems):** a small pool of `AudioSource`s and `Play(cue)` /
  `Play(cue, position)`. Sounds are mostly 2D; world sounds can pan slightly by screen position
  (orthographic iso camera, listener on the camera). Caps how many copies of one cue play at once,
  so five clones pressing together do not stack five times.
- **Music player:** one `DontDestroyOnLoad` object with two sources for crossfades; each chamber
  says which track it wants.
- **Bridge components:** small one-job scripts on prefabs (`DoorAudio`, `ButtonAudio`,
  `FootstepAudio`, `WireAudio`) that listen to the hooks listed above. Where a UnityEvent already
  exists (`Door`, `PressButton`, `SignalWire`, `LevelExit`) wire it in the inspector; otherwise
  subscribe to the C# event in `OnEnable`. Gameplay scripts stay unchanged wherever possible.

Rules that fit our systems:

- **Audio never affects gameplay,** like `Visuals/`. Recording and replay stay deterministic, and
  sound timing does not need to be tick-exact.
- **Clones:** a sound caused by a clone (`IActor.IsClone`) goes to the Clones group.
- **Reset:** when an attempt resets (`StateChanged` to Resetting), stop every looping world sound
  (wire pulse, door motor) so nothing carries into the new attempt.
- **Pause:** `Time.timeScale = 0` does not stop audio. On Pause set `AudioListener.pause = true` and
  give UI sources `ignoreListenerPause = true`, so menu clicks still play.
- **Map:** no footsteps while `MapHeld` (the player stands still anyway).
- **Intro:** the intro plays before the clock starts, so its sounds are called from
  `IntroSequence` steps directly.
- **Import settings:** short SFX mono, Decompress On Load, ADPCM; music and long ambience Streaming,
  Vorbis around 70% quality.

## Build order

1. Footsteps, button press, door open/close, save/retry/rewind, level complete.
2. `mus_chamber` loop and `mus_title`.
3. Wire pulse, pit fall, clones unlocked sting, menu clicks, dialogue ticks, intro slam.
4. Ambience (hum, drips, wind) and lamp buzz.
5. Later chamber and enemy sounds once those features exist.

## Open questions

- Add a volume slider (Music / SFX) to Settings now, or after the level is done?
- One music track for every chamber, or one per chamber?
- Should clone footsteps be audible at all, or only the things clones press?
- Is the guard's line voiced (a recorded shout) or text only with a sound under it?
- Who on the team owns finding and editing sounds?
