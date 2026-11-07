# GUI and clone slots

**Purpose:** The in-level HUD for every level. It shows the restart controls, every saved clone,
which clone the player is controlling right now, and what state each clone is in, and lets the
player pick a clone to re-record. It also defines the clone slot rules the HUD depends on (limit,
controlled clone, switching, death).

Placeholder art for now; the user will provide final icon and button art (see "Art").

## Screen layout

```
+--------------------------------------------------------------+
|                                                    > [1]     |  <- controlled clone
|                                                      [2]     |
|                    (game view)                       [3]     |
|                                                      (4)     |  <- ghost: new clone being recorded or picked
|                                                              |
|                                                              |
|                                              [T]  [  R  ]    |
+--------------------------------------------------------------+
```

- **Clone column:** right edge, vertically centred, clone 1 at the top. Every saved clone is shown.
  A faint ghost icon for the next new clone shows only while the player is recording it or has
  picked it (its number key), not while they are re-recording an older clone.
- **Restart buttons:** bottom right. R (save) is the large button; T (retry) is a smaller button to
  its left.
- **Level complete panel:** centred, unchanged.
- Nothing else on screen. The game view stays as clear as possible.

## Restart buttons

| Button | Key | Label | Caption | Does |
|---|---|---|---|---|
| Save | R | key "R" plus a clone glyph | "SAVE CLONE N", or "SAVE N → PLAY M" when clone M is picked | Saves the run into the controlled clone, then restarts (see "Clone slot rules") |
| Retry | T | key "T" plus a loop arrow | "RETRY", or "RETRY → M" when clone M is picked | Throws the run away, then restarts |

- Both are clickable as well as bound to their key. The key letter is drawn on the button.
- The R button's frame takes the colour of the controlled clone.
- The R button pulses after the live player presses a hinted button before making any clone
  (the Tutorial1 cue), and stops once the first clone exists.

## Clone icons

One icon per saved clone, in slot order, plus a ghost icon for the next new clone while it is
being recorded or picked. Each icon has:
- a body in the clone's colour (placeholder: faceted diamond with a soft glow),
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

Red is reserved for death. The live player has no icon of its own and uses neutral white light
(vision light and implant glow). The clone it controls does not spawn, so each clone colour
appears on only one character at a time. Colours are an inspector list so they can be tuned
without code.

### Icon looks and markers

| Look or marker | When | Look |
|---|---|---|
| Saved | Clone has a recorded run | Full colour, soft glow |
| Ghost | The next new clone, while it is being recorded or picked | Faint, see-through body |
| Controlled | The player is playing this clone this run | Framed row in the clone's colour, an arrow pointing at it, breathing glow |
| Picked next | The player pressed its number; it becomes controlled at the next restart | Blinking outline |
| Dead | Clone died this attempt | Short shake (about 0.4 s), then red with a skull mark; the clone's light also turns red |

A dead icon returns to normal when the next attempt starts.

## Clone slot rules (game logic)

- **Limit:** at most 5 clones.
- **Controlled clone:** the player always controls exactly one clone slot. The run is recorded
  into it, and that clone does not spawn. At the start it is the new slot (clone 1).
- **Picking a clone:** pressing 1-5 (or clicking an icon) picks a saved clone, or the ghost new
  slot, to control from the next restart. The current run keeps going. Pressing the picked number
  again cancels the pick; picking the clone already controlled does nothing.
- **R:** saves the run into the controlled clone (a new clone, or over the old path), then:
  - if a clone is picked, control switches to it;
  - else, after saving a new clone, control moves on to the next new slot, or stays on clone 5
    once all 5 exist (so clone 5 keeps being re-recorded);
  - else (re-recording an old clone) control stays on that clone, to keep refining it. Press the
    next free number (for example 3 with two clones) to go back to making new clones; its ghost
    icon appears when picked.
- **T:** throws the run away and restarts. A picked clone still becomes controlled.
- Every other clone keeps its path and replays as before.
- **Death (hook built, no hazard yet):** when a hazard kills a clone it stops replaying for the
  rest of the attempt and the HUD shows it as Dead. Death resets when the next attempt starts.

## Keybinds

| Key | Action |
|---|---|
| R | Save the run into the controlled clone and restart |
| T | Restart without saving |
| 1-5 | Pick that clone (or the new slot) to control from the next restart; again to cancel |

All are rebindable inspector actions on `PlayerInputReader`.

## Art

- Placeholders are generated in-engine and named `placeholder_ui_*`; final art replaces them file
  by file as `spr_ui_<thing>_<state>` (for example `spr_ui_cloneicon_dead`, `spr_ui_button_r`).
- Placeholder style: dark translucent panels with thin cyan-grey borders, like a cracked relic
  display; icons tinted from one white sprite so a single asset serves all five colours.
- Pixel scale matches the game (Point filter); buttons draw at 2x and clone icons at 3x on 1080p.
- Needed from the user: clone icon (white, tintable), dead overlay (skull), picked outline,
  controlled pointer and row frame, key badge, R button, T button.

## Open questions

- Should the R button or the icons show the clone count against a par once a par exists?
- Does a clone that died still count towards a level's clone par?
