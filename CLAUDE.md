# CLAUDE.md

## Project overview

A 2D, top-down 3/4 view (Stardew Valley style perspective) HD pixel-art puzzle game for University of Bristol gamejam with the theme 'Time'. The player records afterimages (clones) of themselves. On every new attempt each earlier clone replays its recorded actions, and the player coordinates the clones to clear rooms: distract or defeat enemies, activate interactables, move objects and collect information.

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
- **Run:** press Play in the editor. Main scene: TBD (update once the first scene exists).
- **Build:** Windows standalone from Build Settings, output to `Builds/` (git-ignored). Confirm the settings before the first build and note them here.
- **Tests and lint:** none set up. If Unity Test Framework tests are added, put them under `Assets/_Project/Tests` and note how to run them here.
- **Git** runs in the terminal: `git status`, `git diff`, `git log`, `git pull --rebase`.

### Unity MCP (optional)

- If Unity MCP tools are connected in your session, use them for editor actions: reading the console, inspecting the hierarchy, and creating or editing GameObjects, components and prefabs. It is the only acceptable way for you to change scenes and prefabs.
- If they are not connected, do not try to set them up. Give the user numbered editor steps instead.
- MCP config contains machine-specific paths and ports. Do not commit it unless the team has agreed to share it, and check `git status` for it before every commit.

## Project structure

Repo root:

- `Ideas/` design documents (not Unity assets, see below)
- `Assets/_Project/` all our own game content
- `CLAUDE.md`, `CREDITS.md`

All our own game content lives under `Assets/_Project/`, kept apart from imported packages. Subfolders:

- `Scripts/<Feature>/` (for example Player, Afterimage, Interactables, Enemies, UI)
- `Scenes/`
- `Prefabs/`
- `Art/` (sprites, tilesets, animations)
- `Audio/`
- `Data/`
- `Tests/`

This layout is a starting plan. Update this section when the real structure differs.

- **Naming:** scripts, prefabs and scenes use PascalCase (`Player.prefab`, `Level01.unity`). Sprites are `spr_<thing>_<state>`, sound effects `sfx_<thing>`, music `mus_<name>`. Temporary art is prefixed `placeholder_`.
- Each level is one scene. Build gameplay objects as prefabs so scene changes stay small. Name each level scene after its design file (`Ideas/tutorial1.md` becomes `Tutorial1.unity`).
- Commit `.meta` files with their assets. Never delete, regenerate or hand-edit them, and move or rename assets only inside the Unity editor so references survive.

### Ideas folder

`Ideas/` sits in the repo root, outside `Assets/`, so Unity does not import it. It is the design source of truth.

- `Ideas/PROJECT_PLAN.md`: the overall game idea, scope lists (Must have, Should have, Stretch) and the high-level summary of each level and how they connect.
- `Ideas/<level>.md`: one file per level, lowercase, no spaces (`tutorial1.md`, `tutorial2.md`, `level1.md`). Each holds that level's purpose, mechanics taught, room layout, step-by-step flow, art and feedback notes, and open questions.
- Current files: `PROJECT_PLAN.md`, `tutorial1.md` (add new level files here as they are written).
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

Status: planned design. Update each item to match the code once it exists, and note anything the team changes.

- **Afterimage system** (`Scripts/Afterimage`): a recorder samples the player on each fixed tick during an attempt: position, facing and discrete action events (interact, attack, push), stamped with the tick number. When the player starts a new attempt the level resets, and each saved recording is replayed by a clone driven by the same tick counter. Clones replay recorded state and action events rather than raw input, so physics differences cannot desync them.
- **Actors:** interactables and enemies react to any actor (the player or a clone) through a shared interface, never to the player type directly. Interface name and location: TBD.
- **Level reset:** every component whose state changes during play (doors, plates, movable objects, enemies) implements a reset interface, so one call returns the room to its starting state. Name: TBD.
- **Planned extensions,** only after the Must have list works: a clone limit or par, clones degrading as their count grows (playback jitter), and editing an earlier clone's path.
- **Input:** read raw input in one component only (WASD to move, mouse to aim and interact) and expose it as data. No other script reads input directly. Input package: TBD.
- **Game state and scene flow:** a single controller owns the states Playing, Resetting and LevelComplete, and loads the next level or the end screen. A level can be replayed with fewer clones. Level order follows `Ideas/PROJECT_PLAN.md`. Location: TBD.
- **Rendering for the 3/4 view:** sort sprites by Y so objects overlap correctly (Transparency Sort Mode set to Custom Axis with (0, 1, 0), sprite pivots at the feet). Pixel art import settings: Point filter, no compression, one shared Pixels Per Unit value: TBD.
- **UI:** keep it minimal (clone counter, restart, level complete). Use one UI system for the whole project: TBD.

## Workflow

- `main` must always open and run. Work on short-lived branches named `<area>/<short-description>`, for example `afterimage/record-replay`. Merge into `main` whenever something works, at least hourly. Run `git pull --rebase origin main` before merging or pushing. Never force-push `main`.
- Each teammate has their own clone and runs one Claude Code session in it. Do not use git worktrees, because each would need its own Unity import.
- Before starting: pull, run `git status`, and check that the files you plan to touch are not another person's current task. If you must change a shared system or someone else's scene, tell the user first.
- Scenes and prefabs are YAML and merge badly. One person edits a given scene at a time, and nobody re-saves a scene they did not mean to change. Never hand-edit `.unity` or `.prefab` files: use the Unity editor or Unity MCP, or give the user editor steps.
- Plan before coding when a task touches more than three files or any shared system: write a short plan and wait for the user's go-ahead. For smaller tasks, proceed.
- Commit after each finished, working task, once the user has confirmed it in Play mode. Push when the user asks. One logical change per commit.
- **Commit format:** `type(scope): imperative summary`, 72 characters or fewer, for example `feat(afterimage): record player position each fixed tick`. Types: `feat`, `fix`, `refactor`, `docs`, `chore`, `assets`. Add a body when the reason or a trade-off is not obvious from the diff.
- **A task is done when:** the code compiles with no new console errors or warnings, you have checked or listed what to test in Play mode, there are no stray debug logs, comments match the code, and this file is current.
- **Report back briefly:** what changed, which files, exact steps to test in Unity, anything you could not verify, any deviation from a file in `Ideas/`, and any update made to this file. If something is unclear, ask one short question instead of guessing.

## Rules and limits

- Build only what the Must have list asks for unless the user says otherwise. Do not start Should have or Stretch features without being told. If a task seems to need extra scope, ask first.
- Do not add packages, plugins, Asset Store items or other dependencies without asking. Do not edit `Packages/manifest.json` or `ProjectSettings/` unless the task requires it and the user agrees.
- Never edit or commit `Library/`, `Temp/`, `Logs/`, `obj/`, `UserSettings/` or `Builds/`. Do not change third-party code or assets.
- Do not create or generate art or audio assets unless asked. Use simple placeholder shapes and sprites named `placeholder_*`.
- Use only assets the team made or whose licence allows use in a jam. Record every external asset in `CREDITS.md` when you add it: name, author, source link and licence. Do not use copyrighted assets from other games.
- Do not commit secrets or personal paths.

## Decisions and known issues

- **Decided:** Unity, C#, 2D sprites only with a 3/4 (Stardew Valley style) view, PC target. Scope and level designs live in `Ideas/` (`PROJECT_PLAN.md` plus one file per level).
- **Known issues and workarounds:** none yet. Add each with a one-line workaround and remove it when fixed.