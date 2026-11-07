# CLAUDE.md

## Project overview

A 2D, true isometric (2:1) HD pixel-art puzzle game for University of Bristol gamejam with the theme 'Time'. The player records afterimages (clones) of themselves. On every new attempt each earlier clone replays its recorded actions, and the player coordinates the clones to clear rooms: distract or defeat enemies, activate interactables, move objects and collect information.

**Setting and tone:** post-technology, deserted and overgrown, with half-broken relic tech. Puzzle chambers in the spirit of Portal 2. Inspiration only; never copy assets.

**Engine:** Unity (2D), C#, PC target.
- Unity version: 6
- Render pipeline: URP 17.6.0
- Input system: Input System package 1.20.1 (confirm Active Input Handling is set to the new system)

Read `ProjectSettings/ProjectVersion.txt` and `Packages/manifest.json` for accurate values.

**Context:** the jam runs 24 hours (midday Saturday to midday Sunday). Three teammates, each have Claude Code on their own clone of the repo `gamejam-26`.

**Scope source of truth:** the `Ideas/` folder in the repo root (see "Ideas folder" below).
- `Ideas/PROJECT_PLAN.md` holds the overall game idea, the scope lists and the high-level plan for each level.
- `Ideas/<level>.md` (for example `Ideas/tutorial1.md`) holds the detailed design of one level.

Scope lists, as defined in `Ideas/PROJECT_PLAN.md`:
- **Must have:** one complete level with interesting puzzles and working afterimage mechanics.
- **Should have:** enemies and inside/outside variations.
- **Stretch (do not build unless asked):** co-op, lore, easter eggs, NPCs with text speech.

**Priorities, in order:** the game always runs, the core afterimage mechanic, the Must have list, the Should have list, polish. Scope creep is the main project risk, so keep a playable version after every feature.

## Keeping this file current

This project changes quickly. Treat this file as live documentation that you maintain, not a fixed spec.

- Every time you read this file (at the start of a session and before starting a task), check it against the repo: folders, scenes, systems, commands, versions, the `Ideas/` file list and TBD entries. Fix anything stale in place. Fill in a TBD as soon as the answer is in the repo or the user states it.
- After finishing a task, update this file if you added, renamed or removed a system, folder, scene or major script, introduced a convention, made or were given a decision, found a bug or workaround, or changed a command or setup step.
- Edit in the section that fits. Replace outdated text instead of appending corrections. Keep the file short (under about 250 lines) and keep only lines that change what an agent should do. No changelog; write facts in the present tense.
- Three agents edit this file, so run `git pull --rebase` right before editing, make a separate commit (`docs: update CLAUDE.md ...`) and push soon after. On a merge conflict, keep both sides' facts.
- Mention in your task summary, in one line, what you changed here.
- Do not change the scope in `Ideas/PROJECT_PLAN.md` without the user's say-so, and never remove this section.

## Commands

Unity is editor-driven. Claude Code cannot normally run or see the game, so the user confirms in Unity that things compile and behave.

- **Open:** Unity Hub, open the repo root. Do not start Unity from the command line unless asked.
- **Compile check:** Unity recompiles when the editor regains focus. If Unity MCP tools are connected, read the console with them. Otherwise ask the user to report console errors and warnings.
- **Run:** press Play in the editor. First scene: `Assets/_Project/Scenes/Tutorial1.unity`; Build Settings order is Tutorial1, Tutorial2, Chamber3. Keys: WASD/arrows move, E interact, R save the run into the controlled clone, T retry without saving, F mark the run so dying saves it, 1-5 pick the clone to control from the next restart.
- **Build:** Windows standalone from Build Settings, output to `Builds/` (git-ignored). Confirm the settings before the first build and note them here.
- **Tests and lint:** none set up. If Unity Test Framework tests are added, put them under `Assets/_Project/Tests` and note how to run them here.
- **Git** runs in the terminal: `git status`, `git diff`, `git log`, `git pull --rebase`.

### Unity MCP (optional)

- If Unity MCP tools are connected in your session, use them for editor actions: reading the console, inspecting the hierarchy, and creating or editing GameObjects, components and prefabs. It is the only acceptable way for you to change scenes and prefabs.
- If they are not connected, do not try to set them up. Give the user numbered editor steps instead.
- The MCP for Unity package (`com.coplaydev.unity-mcp`, pinned v10.3.0) is committed in `Packages/manifest.json` so the team shares it. Its window (Window > MCP for Unity) must use the **stdio** transport to match the Claude Code server; HTTP mode shows "No Unity Editor instances found".
- MCP client config contains machine-specific paths and ports. Do not commit it, and check `git status` for it before every commit.
- Unity only renders and advances Play mode while its window has focus. Before an MCP screenshot, force a render (`EditorApplication.QueuePlayerLoopUpdate()` plus `Camera.main.Render()`) or you may get a stale frame. Manual renders log harmless "RenderPass dimensions" errors; clear the console afterwards.

## Project structure

Repo root:

- `Ideas/` design documents (not Unity assets, see below)
- `Assets/_Project/` all our own game content
- `CLAUDE.md`, `CREDITS.md` (not created yet; add it with the first external asset)

All our own game content lives under `Assets/_Project/`, kept apart from imported packages. Subfolders:

- `Scripts/<Feature>/`: `Core` (tick clock, interfaces, level reset, game state), `Player` (input, movement, actor), `Actors` (shared Interactor, ActorZone), `Afterimage` (record and replay), `Interactables`, `Hazards` (lasers), `Machines` (Chamber 3's generator, belt, laser wall, hydraulic block, hover stairs, power gate), `UI`, `Visuals` (camera follow, sprite facing, flicker lights; never affects gameplay)
- `Scenes/` (`Tutorial1.unity`, `Tutorial2.unity`, `Chamber3.unity`)
- `Prefabs/` (Player, Clone, PressButton, Pit, SignalWire, Door, LevelExit, HUD, Laser)
- `Art/Placeholders/` (generated `placeholder_iso_*` sprites, `Tiles/` tile assets, and `UI/` with the `placeholder_ui_*` HUD sprites)
- `Audio/`
- `Data/` (`Tutorial1_Volume.asset` post-processing profile)
- `Tests/` (not created yet)

- **Naming:** scripts, prefabs and scenes use PascalCase (`Player.prefab`, `Level01.unity`). Sprites are `spr_<thing>_<state>`, sound effects `sfx_<thing>`, music `mus_<name>`. Temporary art is prefixed `placeholder_`.
- Each level is one scene. Build gameplay objects as prefabs so scene changes stay small. Name each level scene after its design file (`Ideas/tutorial1.md` becomes `Tutorial1.unity`).
- Commit `.meta` files with their assets. Never delete, regenerate or hand-edit them, and move or rename assets only inside the Unity editor so references survive.

### Ideas folder

`Ideas/` sits in the repo root, outside `Assets/`, so Unity does not import it. It is the design source of truth.

- `Ideas/PROJECT_PLAN.md`: the overall game idea, scope lists (Must have, Should have, Stretch) and the high-level summary of each level and how they connect.
- `Ideas/<level>.md`: one file per level, lowercase, no spaces (`tutorial1.md`, `tutorial2.md`, `level1.md`). Each holds that level's purpose, mechanics taught, room layout, step-by-step flow, art and feedback notes, and open questions.
- `Ideas/Inspiration/`: look-and-feel reference images, each listed in `inspiration.md` with its source and what to take from it. Image names are lowercase snake_case. Reference only: never import these into `Assets/`.
- `Ideas/Sketches/`: hand-drawn level layouts (`tutorial1.png`).
- Current files: `PROJECT_PLAN.md`, `tutorial1.md`, `tutorial2.md`, `chamber-3.md` (sketch `chamber_3_design.jpg`), `gui.md` (HUD layout and clone slot rules for every level), `Inspiration/`, `Sketches/` (add new level files here as they are written).
- **Before building or changing a level,** read `Ideas/PROJECT_PLAN.md` and that level's file. Build what the level file describes. Open questions in a level file are not yours to decide: ask the user.
- **Do not edit files in `Ideas/` without the user's say-so.** If the implementation has to differ from a level file, say so in your task summary and let the user update the doc.
- If a level is mentioned in `Ideas/PROJECT_PLAN.md` but has no file yet, ask the user before inventing its design.

## Code conventions

- Standard C# style: PascalCase for types, methods and properties, camelCase for locals and parameters, `_camelCase` for private fields, four-space indents, braces on their own line. Namespace `GameJam.<Feature>`.
- One MonoBehaviour per file, file name equal to the class name. One script per entity or responsibility; split a script once it does two jobs.
- Expose inspector values with `[SerializeField] private`, never public fields. Add `[Header]` or `[Tooltip]` to tunable values so teammates can adjust them without reading code.
- Cache references in `Awake` or `Start`. Do not call `Find`, `FindObjectOfType` or `GetComponent` in `Update`, and avoid allocations in per-frame code. Prefer C# events or `UnityEvent` over direct references between systems.
- Gameplay that must replay identically runs on `FixedUpdate` and the shared tick counter, never on `Time.deltaTime`. Random values that affect gameplay use a seeded generator.
- Comments exist so teammates keep control of the code.
  - Start every script with a short header comment: what it does, what uses it, and where it sits in the game flow.
  - Put `///` summaries on classes and on every public and non-trivial private method, saying what it does and why.
  - Inside long methods, add short step comments that describe the flow ("1. record this tick", "2. apply to clone").
  - Comment the reason for the code, not what the syntax does.
  - Keep comments plain and accurate, and update or delete them when the code changes.
- No dead code, no commented-out blocks and no leftover `Debug.Log`. Keep `Debug.LogWarning` and `Debug.LogError` where they report real problems.
- Prefer the simplest implementation that works. This is a jam, and every teammate must be able to read the code.
- Clean code always with logical flow, modular where possible, and always informative clean comments, without being too long

## Key systems

Status: built and working in Tutorial1. Update each item when the code changes.

- **Tick clock** (`Core/TickClock`, execution order -100): counts fixed steps from 0 each attempt. Recording and replay index frames by this tick.
- **Afterimage system** (`Scripts/Afterimage`): `AfterimageRecorder` (on the player, order 50) stores one `ActorFrame` (position, facing, interacted) per tick in a `Recording`. `AfterimageManager` keeps recordings in numbered clone slots and respawns one `AfterimageReplayer` clone per slot each attempt. Clones replay positions and repeat interacts on the same tick, then stand still when their run ends.
- **Clone slots** (rules in `Ideas/gui.md`): `AfterimageManager` holds up to 5 slots; the clone limit is the length of its `_slotColours` list (blue, green, yellow, purple, orange; red means dead). `ControlledSlot` is the slot this run records into: a saved slot (re-recording) or `CloneCount` (new). Its clone does not spawn. `SelectSlot` (keys 1-5 or clicking an icon) sets `PendingSlot`. `EndRun(run, keep)` saves into the controlled slot if `keep`, then switches to the pending slot; with no pending slot it moves on to the next new slot after a new save (capped at the last slot) and stays put after a re-record. It raises `Changed` (end of run, pick, respawn) and `CloneDied(slot)`; `IsSlotDead` clears on respawn. `Visuals/CloneTint` on the Clone prefab colours the sprite and `AfterimageGlow` from the replayer's `SlotColour`, and turns them red on death.
- **Death:** hazards call `IActor.Kill()`. A killed clone (`AfterimageReplayer`) stops replaying, turns red and no longer blocks hazards. A killed player (`PlayerActor.Died`) freezes the room for `GameStateController._deathResetDelay`, then restarts: the run is saved into the controlled clone (like R) if `RunMarked`, otherwise thrown away (like T). `ToggleRunMark` (F or the HUD record button) sets `RunMarked`; it clears on every restart and raises `RunMarkedChanged`. This is how a clone is sent ahead to absorb a laser shot. Both components have a "Kill" context menu for testing. `GameStateController._player` must be wired in every level scene.
- **Actors:** `Core/IActor` (`Transform`, `IsClone`, `IsDead`, `Kill()`) is implemented by `PlayerActor` and `AfterimageReplayer`. Interactables (`Interactables/IInteractable`) and triggers talk only to `IActor`. `Actors/Interactor` is shared by player and clone and takes this tick's position, so both reach the same thing.
- **Level reset:** `Core/IResettable.ResetState()`. Resettables register with the static `Core/LevelReset` in `OnEnable` and unregister in `OnDisable`; `LevelReset.ResetAll()` resets the room.
- **Lasers** (`Hazards/LaserEmitter`, order 60): charge, fire one shot, show the beam, cool down, on a fixed tick cycle from each attempt's tick 0 (timings in ticks, 50 per second). The shot kills only the first living actor along `_direction` from `_muzzle`, within `_beamRadius` (0.35, covers a 2-cell corridor so nobody can sidestep it); a centre ray finds the wall that ends the beam, and triggers and dead clones never block it. Tutorial2 lasers: charge 80, beam 10, cooldown 30 ticks (first shot at tick 80, then every 120). Hits never change the schedule. `Interactables/PlayerTrigger` fires a UnityEvent when the live player enters (Tutorial2's exit door uses it to call `Door.Open`).
- **Zones and hold plates:** `Actors/ActorZone` lists the living actors overlapping its collider, refreshed each tick by an overlap query (order -20). It does not use trigger messages, because clones are kinematic triggers and get none against static colliders. `Interactables/HoldPlate` is active only while a living actor stands in its zone and its optional `PowerGenerator` has power; it does not latch.
- **Chamber 3 machines** (`Scripts/Machines`, all tick-timed): `FuelBelt` drops a crate every 60 ticks and carries it to `PowerGenerator.Feed()` in 200 ticks, burning it at `LaserWall` unless blocked. `HydraulicBlock` pushes its block out from a fixed base (arm drawn by `Visuals/PistonArm`) while its pedal `HoldPlate` is held, and springs back otherwise. The generator stays powered for 150 ticks after each crate; `Visuals/PowerLights` fades lights with it, and `PowerGate` passes an event only while powered (exit door). `HoverStairs` (order -10, kinematic) flips its 4 steps up while the raise plate is held and slides to the middle stop if the top-left plate is held, else the left stop if the top-right plate is held, else back right. It carries the live player through `PlayerMovement.AddCarry`, enables its side rails while raised, and removes the balcony edge's wall tiles and blockers (`LedgeFace`, `LedgeEdgeBlockers`) in the columns it covers.
- **Visual height** (physics stays flat): `Visuals/ElevationField` (one per level, optional) gives the ground height at a point: its `_raisedAreas` cell rects, plus the steps via `HoverStairs.StepFraction`. `Visuals/ActorElevation` on the Player and Clone `Body` lifts the Body's children to that height; `CharacterSpriteAnimator` adds `ActorElevation.Height` to its own bob. The Body itself never moves, so sorting stays on the floor plan. Raised art lives on `RaisedFloor`/`RaisedWalls` tilemaps offset +1.5 (one wall block's face height); their physics is on the collider-only `RaisedBlockers` tilemap at floor level.
- **Wiring visuals:** `Core/ISignalSource` (`IsOn`) is implemented by `PowerGenerator` and `HoldPlate`. `Visuals/SignalCable` lights a floor `LineRenderer` (sorting -96) with a travelling pulse while its source is on, and can tint end sprites (stair stop markers).
- **Planned extensions,** only after the Must have list works: a clone par and clones degrading as their count grows (playback jitter).
- **Input:** `Player/PlayerInputReader` is the only script that reads input (Input System package, inspector `InputAction` fields with code defaults). It exposes `MoveInput`, `ConsumeInteract()` and the `CloneRequested`/`RetryRequested`/`MarkRunRequested`/`SelectCloneRequested(slot)` events. Movement is screen-relative; `PlayerMovement._isoVerticalScale` (0.5) makes diagonals follow the iso floor.
- **Game state and scene flow:** `Core/GameStateController` owns Playing, Resetting and LevelComplete. `RequestClone` (R) saves the run, `RequestRetry` (T) discards it; both apply a pending switch, reset the room, respawn clones and restart the clock. `RequestSelect(slot)` picks the clone to control next. Keys and HUD buttons call the same methods, and all of them work only while Playing. LevelComplete loads the next Build Settings scene after a delay. Level order follows `Ideas/PROJECT_PLAN.md`.
- **Level wiring:** `PressButton` (latches) → `SignalWire` (red pulse along its LineRenderer, timed in ticks) → `Door.Open`, wired with inspector UnityEvents. `Pit` is a one-way drop that cages only the live player and raises `ActorEntered`/`ActorExited`. `LevelExit` calls `GameStateController.CompleteLevel`.
- **Pit depth (visual only):** `Visuals/PitVisuals` on the Pit draws anyone inside 1.25 units lower via `Visuals/ActorPitDepth` on the character's `Body` child (a `SortingGroup` holding Shadow and Visual). Colliders and recordings keep the floor position. Sorting orders: Floor -100, Hole -98, in-pit button -80, in-pit actors -70, `PitShadow` (unlit) -60, `PitFrontGround` -55, everything else 0. The shadow and front ground fade only while the live player is in the pit, so a clone down there stays hidden.
- **See-through occluders:** `Visuals/OcclusionFader` is the reusable fade for anything that can hide the player (ground, walls, ceilings, tall props). Give it sprites and/or tilemaps plus a faded opacity, and call `SetFaded(bool, instant)`. The pit's front ground uses it at 0.2, including the grass and rubble standing on that slab.
- **Long wall sprites** (doors, wall panels) sort from the far, higher end of their base, so characters standing anywhere in front draw on top. The door's pivot is its upper-left base corner.
- **Rendering (isometric 2:1):** one `Grid` per level with Isometric cell layout, cell size (1, 0.5). Tilemaps: `Floor` (Chunk mode, order -100), and `Walls`/`Ledges` (Individual mode, colliders merged by a `CompositeCollider2D`) so they Y-sort with characters. The Renderer2D sorts by Custom Axis (0, 1, 0). Sprite pivots go at the feet or footprint centre, with sprite sort point Pivot. The two far walls are full height; the near walls are low ledges.
- **Pixel art:** 64 PPU, 64x32 px floor tiles, characters about 32x64 px, Point filter, no compression, no mipmaps. Camera orthographic size 4.21875 gives an exact 2x pixel scale at 1080p.
- **Lighting:** URP 2D lights. The global light is near black (0.03) and the camera background pure black, so only lit areas read. The player's `Body` carries a soft `VisionLight` (the permanent vignette around the character) and a small pale `ImplantGlow`, both neutral white so the player never shares a clone slot colour; clones carry `AfterimageGlow`, tinted per slot by `CloneTint`. Point lights are the cues (the ceiling-breach pool over the pit is Tutorial1's "go here" clue). `Visuals/FlickerLight` handles broken lamps, and a global Volume uses `Data/Tutorial1_Volume.asset` (bloom, vignette, colour grade, grain).
- **UI** (layout in `Ideas/gui.md`): uGUI (Canvas plus legacy `Text`, 1920x1080 reference). `UI/HudController` handles the bottom-right F (record), T and R buttons and the level complete panel. The F button (in the HUD prefab) turns red with a blinking dot and reads "SAVES ON DEATH" while the run is marked. The R button is framed in the controlled clone's colour, its caption reads "SAVE CLONE N" or "SAVE N → PLAY M", and it pulses after the first hinted button press. `UI/CloneColumn` keeps five pre-placed `UI/CloneIconView` icons on the right edge in step with the slots: saved clones, plus a ghost for the next new one only while it is controlled or picked. `CloneIconView.Show(saved, dead, controlled, pending)` draws a framed row and arrow for the controlled clone, a blinking outline for the picked one, and a shake then red with a skull for a dead one. UI placeholder sprites live in `Art/Placeholders/UI/` at 50 PPU, so sliced borders draw at 2x. The EventSystem uses `InputSystemUIInputModule`.

## Workflow

- `main` must always open and run. Work on short-lived branches named `<area>/<short-description>`, for example `afterimage/record-replay`. Merge into `main` whenever something works, at least hourly. Run `git pull --rebase origin main` before merging or pushing. Never force-push `main`.
- Each teammate has their own clone and runs one Claude Code session in it. Do not use git worktrees, because each would need its own Unity import.
- Before starting: pull, run `git status`, and check that the files you plan to touch are not another person's current task. If you must change a shared system or someone else's scene, tell the user first.
- Scenes and prefabs are YAML and merge badly. One person edits a given scene at a time, and nobody re-saves a scene they did not mean to change. Never hand-edit `.unity` or `.prefab` files: use the Unity editor or Unity MCP, or give the user editor steps.
- Plan before coding when a task touches more than three files or any shared system: write a short plan and wait for the user's go-ahead. For smaller tasks, proceed.
- Commit after each finished, working task, once the user has confirmed it in Play mode. Push when the user asks. One logical change per commit.
- **Commit format:** `type(scope): imperative summary`, 72 characters or fewer, for example `feat(afterimage): record player position each fixed tick`. Types: `feat`, `fix`, `refactor`, `docs`, `chore`, `assets`. Add a body when the reason or a trade-off is not obvious from the diff.
- **A task is done when:** the code compiles with no new console errors or warnings, you have checked or listed what to test in Play mode, there are no stray debug logs, comments match the code, this file is current, and you have run the `code-review` skill on the change and fixed or reported its findings. Do the review at the end of every build task, before committing.
- **Report back briefly:** what changed, which files, exact steps to test in Unity, anything you could not verify, any deviation from a file in `Ideas/`, and any update made to this file. If something is unclear, ask one short question instead of guessing.

## Rules and limits

- Build only what the Must have list asks for unless the user says otherwise. Do not start Should have or Stretch features without being told. If a task seems to need extra scope, ask first.
- Do not add packages, plugins, Asset Store items or other dependencies without asking. Do not edit `Packages/manifest.json` or `ProjectSettings/` unless the task requires it and the user agrees.
- Never edit or commit `Library/`, `Temp/`, `Logs/`, `obj/`, `UserSettings/` or `Builds/`. Do not change third-party code or assets.
- Do not create or generate art or audio assets unless asked. Use simple placeholder shapes and sprites named `placeholder_*`.
- Use only assets the team made or whose licence allows use in a jam. Record every external asset in `CREDITS.md` when you add it: name, author, source link and licence. Do not use copyrighted assets from other games.
- Do not commit secrets or personal paths.

## Decisions and known issues

- **Decided:** Unity, C#, 2D sprites only with a true isometric 2:1 view (replaced the earlier 3/4 Stardew view), PC target. Scope and level designs live in `Ideas/` (`PROJECT_PLAN.md` plus one file per level).
- **Decided for Tutorial1:** the pit is a one-way drop (the clone key is the only way out), a clone stands still when its run ends, and the "go here" hint is a brighter area, not a trail of dots. The pit's inside is hidden in shadow, the ground turns see-through only for the live player, and vision is a light around the character, not a screen overlay.
- **Decided for Tutorial2:** a laser hit on the live player saves the run only if it was marked with F (T always discards), lasers run on a fixed tick timer that hits never reset, each attempt starts at Chamber 2's entrance, and Laser 2 stays dark until the turn. Layout (cells): landing (7..10, 12..15), Corridor 1 west along y 10..11, Corridor 2 south along x -3..-2, exit passage east along y -4..-3 with the door at x 0 opened by `DoorTrigger` (a `PlayerTrigger`).
- **Decided for Chamber3** (details in `Ideas/chamber-3.md`, Decisions): power gates the plates, stairs and door, and stays on only while fuel arrives; plates work by standing on them and the buttons are decoration; the back is a raised balcony with the exit door on it; the generator is wired to every plate and the door with lit cables. Layout (cells): floor (0..17, 0..13), balcony front edge row 14 (wall blocks), balcony bays at y 15..16 (left x 1..3, middle x 7..10 with the door, right x 14..16), exit passage (8..9, 17..18), steps on rows 11..14 with stops at column centres 3, 9 and 15. The belt's front wall has a floor-level channel on row 6: laser lens at x 10, block lane x 10..12, piston base at x 13; the pedal is on the open floor at (13, 4). The machines are scene objects, not prefabs yet.
- **Known issues and workarounds** (add each with a one-line workaround and remove it when fixed):
  - Painting tilemaps from editor code leaves stale wall colliders. Workaround: after `SetTile`, toggle the `TilemapCollider2D` off and on, call `ProcessTilemapChanges()`, then `CompositeCollider2D.GenerateGeometry()`, and save.
  - After editing a script, a compile can still load the old version (new fields missing). Workaround: request a scripts-only recompile and check the type with reflection before using it.
  - A newly created script can stay out of the compiled assembly (its MonoScript reports no class, no console error). Workaround: delete the new `.cs` and its `.meta`, refresh, write the file again, then refresh.