# Contributing to Afterimage

Thanks for your interest. *Afterimage* was made in 24 hours for the University of Bristol Game Jam 2026, and the jam version is finished: four chambers, title, menus, audio and the ending. Bug fixes, polish and documentation improvements are very welcome. For anything bigger, such as a new chamber or mechanic, please open an issue first so we can talk it through before you spend time on it.

## Getting set up

1. Install **Unity 6** (`6000.6.8f1`, see `ProjectSettings/ProjectVersion.txt`) through Unity Hub.
2. Fork and clone the repository, then open the folder in Unity Hub. The first import takes a few minutes.
3. Press **Play**. The game starts from `Assets/_Project/Scenes/Title.unity` whichever scene is open. To test one chamber directly, switch off **GameJam > Play From Title**.
4. **Edit > Clear All PlayerPrefs** resets the Continue save, key rebinds and settings if you need a clean start.

Before changing anything, skim [`CLAUDE.md`](CLAUDE.md) for the architecture and conventions, and [`docs/`](docs/README.md) for the design. If you change a level, read [`docs/game-design.md`](docs/game-design.md) and that level's file in `docs/levels/` first.

## Making a change

1. Branch from `main` with a short name in the form `<area>/<short-description>`, for example `fix/laser-timing` or `docs/chamber3-notes`.
2. Keep each pull request to one fix or feature.
3. Test in Play mode and check the Console has no new errors or warnings.
4. Run `git pull --rebase origin main` before you push.
5. Open a pull request saying what you changed, why, and how to test it in Play mode. Screenshots or a short clip help a lot for visual changes.

## Commit messages

Use `type(scope): imperative summary`, 72 characters or fewer:

```
fix(lasers): stop the beam passing through dead clones
docs(chamber4): describe the platform timings
```

Types: `feat`, `fix`, `refactor`, `docs`, `chore`, `assets`.

## Unity rules

Scenes and prefabs are YAML files that merge badly, so:

- Edit `.unity` and `.prefab` files only in the Unity editor, never by hand.
- Do not re-save a scene you did not mean to change, and avoid editing the same scene as someone else at the same time.
- Commit every `.meta` file with its asset. Never delete or regenerate them, and move or rename assets only inside Unity so references survive.
- Never commit `Library/`, `Temp/`, `Logs/`, `obj/`, `UserSettings/` or `Builds/`.
- Ask before adding packages or editing `Packages/manifest.json` or `ProjectSettings/`.

## Code style

- Standard C#: PascalCase for types, methods and properties, camelCase for locals and parameters, `_camelCase` for private fields, four-space indents, braces on their own line.
- Namespaces follow the folder: `GameJam.<Feature>`, with scripts under `Assets/_Project/Scripts/<Feature>/`.
- One MonoBehaviour per file, named after the class.
- Expose inspector values with `[SerializeField] private` fields, with a `[Header]` or `[Tooltip]` on anything tunable.
- Anything that must replay identically for clones runs in `FixedUpdate` on the shared tick counter, never on `Time.deltaTime`.
- Start each script with a short header comment saying what it does and where it fits, and add `///` summaries to classes and public methods.
- No dead code, commented-out blocks or leftover `Debug.Log` calls.

Prefer the simplest thing that works. The code should stay readable for anyone picking it up.

## Art and audio

Do not add assets from other games or anything you do not have the rights to share. Temporary art goes in with a `placeholder_` prefix. Follow the naming in `CLAUDE.md`: sprites `spr_<thing>_<state>`, sound effects `sfx_<thing>`, music `mus_<name>`.

## Reporting bugs

Open an issue with:

- what you did, what you expected and what happened instead
- the chamber, and roughly how many clones were saved
- any Console errors, and a screenshot or clip if it is visual

## Licence

By contributing, you agree that your code is released under the project's [MIT Licence](LICENSE).
