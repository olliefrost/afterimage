# GUI and clone slots

**Purpose:** The in-level HUD for every level. It shows the restart controls, every saved clone
and what state each clone is in, and lets the player pick a clone to re-record. It also defines
the clone slot rules the HUD depends on (limit, overwrite, re-record, death).

Placeholder art for now; the user will provide final icon and button art (see "Art").

## Screen layout

```
+--------------------------------------------------------------+
|                                                         [1]  |
|                                                         [2]  |
|                    (game view)                          [3]  |
|                                                         [4]  |
|                                                         [5]  |
|                                                              |
|                                              [T]  [  R  ]    |
+--------------------------------------------------------------+
```

- **Clone column:** right edge, vertically centred, clone 1 at the top. Only saved clones are
  shown; the column grows downward as clones are added (max 5).
- **Restart buttons:** bottom right. R (new clone) is the large button; T (retry) is a smaller
  button to its left.
- **Level complete panel:** centred, unchanged.
- Nothing else on screen. The game view stays as clear as possible.

## Restart buttons

| Button | Key | Label | Does |
|---|---|---|---|
| New clone | R | key badge "R" plus a clone glyph | Ends the run, saves it (see "Clone slot rules"), restarts the attempt |
| Retry | T | key badge "T" plus a loop arrow | Ends the run without saving it, restarts the attempt |

- Both are clickable as well as bound to their key. The key letter is drawn on the button.
- The R button pulses after the live player presses a hinted button before making any clone
  (the Tutorial1 cue), and stops once the first clone exists.
- When a clone is selected for re-recording, the R button shows that clone's colour and number
  ("R → 3"), so it is clear which slot it will overwrite.

## Clone icons

One icon per saved clone, in slot order. Each icon has:
- a body in the clone's colour (placeholder: rounded diamond with a soft glow),
- a small dark key badge on its left showing its number key (1-5),
- the same colour used for that clone's in-world light (`AfterimageGlow`), so icon and clone
  match at a glance.

| Slot | Key | Colour |
|---|---|---|
| 1 | 1 | Blue |
| 2 | 2 | Green |
| 3 | 3 | Yellow |
| 4 | 4 | Purple |
| 5 | 5 | Orange |

Red is reserved for death. The live player keeps its cyan implant glow and has no icon.
Colours are an inspector list so they can be tuned without code.

### Icon states

| State | When | Look |
|---|---|---|
| Active | Clone is replaying or standing at the end of its run | Full colour, gentle glow |
| Selected | Player pressed its number to re-record it | Brighter, outlined and gently pulsing; R button takes its colour |
| Dead | Clone died this attempt | Short shake (about 0.4 s), then red with a skull mark; the clone's light also turns red |

A dead icon returns to Active when the next attempt starts.

## Clone slot rules (game logic)

- **Limit:** at most 5 clones.
- **R with nothing selected:** the run is saved into the next empty slot. If all 5 are full, it
  overwrites slot 5, so the 5th clone is the one that keeps being re-recorded.
- **Selecting a clone:** pressing 1-5 (or clicking an icon) selects that slot for re-recording.
  The current run keeps going. Pressing the same number again deselects it; pressing another
  number moves the selection. Numbers for empty slots do nothing.
- **R with a clone selected:** the run overwrites that slot's recording, then the selection clears.
  Every other clone keeps its path and replays as before.
- **While selected:** the selected clone does not spawn on the next attempt (T or a fresh start),
  because the player is replacing it and should not play alongside its old path. If it is selected
  mid-run, this run's copy keeps playing until the restart.
- **T:** discards the run. The selection is kept.
- **Death (not built yet):** when a hazard kills a clone it stops replaying for the rest of the
  attempt and the HUD shows it as Dead. Death resets when the next attempt starts. The death
  event is added when the first hazard or enemy is built.

## Keybinds

| Key | Action |
|---|---|
| R | New clone (or overwrite the selected / 5th slot) |
| T | Retry without saving |
| 1-5 | Select / deselect that clone for re-recording |

All are rebindable inspector actions on `PlayerInputReader`.

## Art

- Placeholders are generated in-engine and named `placeholder_ui_*`; final art replaces them file
  by file as `spr_ui_<thing>_<state>` (for example `spr_ui_cloneicon_dead`, `spr_ui_button_r`).
- Placeholder style: dark translucent panels with thin cyan-grey borders, like a cracked relic
  display; icons tinted from one white sprite so a single asset serves all five colours.
- Pixel scale matches the game (Point filter, 2x at 1080p), so the HUD looks part of the world.
- Needed from the user: clone icon (white, tintable), dead overlay (skull), selected outline,
  key badge, R button, T button.

## Open questions

- Should the R button or the icons show the clone count against a par once a par exists?
- Does a clone that died still count towards a level's clone par?
