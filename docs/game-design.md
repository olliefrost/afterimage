# Game design

The overall design of *Afterimage*. Per-level designs are in [`levels/`](levels/), the story is in [`story.md`](story.md).

## Concept

| | |
| --- | --- |
| **Pitch** | An isometric puzzle game where you record your own actions and cooperate with the afterimages of your past selves to clear a deserted laboratory. |
| **Theme** | *Time*. Past versions of you complete rooms by blocking danger, holding down plates, pressing buttons and manipulating objects. |
| **Perspective** | True isometric (2:1), HD pixel art with 64x32 px floor tiles and 64 pixels per unit. |
| **Goal** | Solve each chamber's puzzle and reach its exit. |
| **Platform and engine** | PC (Windows), Unity 6 with URP 2D. |

## Core mechanic

- **Afterimages.** Save a run and an afterimage (clone) replays it, tick for tick, on every later attempt. Up to 5 clones, each with its own colour. Tutorial 1 allows 2, and cloning only unlocks once the player is trapped in its pit.
- **The controlled clone.** The player always controls exactly one clone slot, shown in the HUD. They can switch to an earlier clone to re-record its path while the others keep replaying. Full rules are in [`ui.md`](ui.md).
- **Danger.** Lasers kill whoever they hit first. A clone that dies stops replaying, so a clone can be sent ahead to absorb a shot. Marking a run with F makes dying save it as a clone.
- **Seeing the room.** The camera stays close and the character only sees a small pool of light, so the player relies on the hold-Tab chamber map (walls, key features and a live dot per clone) to track their clones.

### Controls

| Action | Key |
| --- | --- |
| Move (screen-relative) | WASD or arrow keys |
| Interact | E |
| Jump (Chamber 4 onward) | Space |
| Save the run into the controlled clone and restart | R |
| Retry without saving | T |
| Mark the run so dying saves it | F |
| Pick the clone to control from the next restart | 1-5 |
| Chamber map | Hold Tab |
| Pause menu | Esc |

The HUD's R, T and F buttons and clone icons can also be clicked. Gameplay keys can be rebound in the pause menu's Settings.

## Game loop and structure

1. The title screen offers Continue (only when a save exists), New Game and Quit.
2. New Game plays the intro (she breaks into the facility) and starts Tutorial 1.
3. In each chamber the player repeats: record a run, save it as a clone, and try again with every earlier clone replaying, until the exit is reached.
4. Chambers play in order, then the ending. Finishing the game clears the Continue save.

| # | Chamber | Idea | Clones needed |
| --- | --- | --- | --- |
| 1 | [Tutorial 1, "First Clone"](levels/01-tutorial1.md) | Buttons, doors and your first clone | 1 |
| 2 | [Tutorial 2, "Taking the Hit"](levels/02-tutorial2.md) | Charging lasers and sacrificing clones | 2 |
| 3 | [Chamber 3, "All Hands"](levels/03-chamber3.md) | A power room where each clone has a job | 4 |
| 4 | [Chamber 4, "Leap of Faith"](levels/04-chamber4.md) | Timed windows, jumping and ladders | 3 |
| 5 | [Ending, "Reunion"](levels/05-ending.md) | The trap | none |

### Level design toolkit

- Buttons and pressure plates (a button latches, a plate works only while held)
- Lasers that charge, fire and cool down on a fixed tick timer
- Doors, with a specific door that signals the next level
- Wires that visibly carry signals between interactables

## Art and audio

- **Style:** post-technology, deserted and overgrown, half-broken relic tech, in the spirit of Portal 2. See [`inspiration.md`](inspiration.md).
- **Art:** PixelLab pixel art (true 2:1 iso, muted palette), with a pixel font and UI built from PixelLab sprites. A few `placeholder_*` sprites made in-engine remain for icons and glyphs.
- **Audio:** sound effects and music generated with ElevenLabs, plus a synthesised credits track. The plan and implementation notes are in [`audio.md`](audio.md).
- **Credits and licences:** [`../CREDITS.md`](../CREDITS.md).
- **UI:** the HUD, menus and clone slot rules are in [`ui.md`](ui.md).

## Technology

- Unity 6 (`6000.6.8f1`), C#, URP 2D, Input System package, uGUI.
- A shared fixed-step tick clock records the player each tick and replays clones from it.
- One input reader, an actor interface so buttons, pits and exits react to the player and clones alike, and a level reset interface.
- Code architecture and conventions are in [`../CLAUDE.md`](../CLAUDE.md).

## Outcome

Made in 24 hours (midday Saturday to midday Sunday) by a team of three.

**Shipped:** five playable scenes (Tutorial 1, Tutorial 2, Chamber 3, Chamber 4 and the Ending) with the full afterimage mechanic, lasers and death, machines, jumping and ladders, a title screen, pause menu, key rebinding, music and sound effects.

**Not built:**
- Co-op, NPC dialogue and easter eggs (the Stretch list)
- Enemies beyond lasers, and outdoor levels
- A clone par (fewest clones per chamber), a time limit, and clones degrading as their number grows
