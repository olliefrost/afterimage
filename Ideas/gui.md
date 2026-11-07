# GUI and clone slots

**Purpose:** The in-level HUD for every level. It shows the restart controls, every saved clone,
which clone the player is controlling right now, and what state each clone is in, and lets the
player pick a clone to re-record. It also defines the clone slot rules the HUD depends on (limit,
controlled clone, switching, death), the hold-Tab chamber map (see "Chamber map"), and the title screen and Esc pause menu (see "Menus").

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
| Save | R | key "R" plus a clone glyph | "SAVE CLONE N", "SAVE CLONE N · MAX L" once the chamber's limit of L clones is reached, or "SAVE N → PLAY M" when clone M is picked | Saves the run into the controlled clone, then restarts (see "Clone slot rules") |
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
| Locked | The chamber's clone limit is reached (shown on the slot just past the limit) | Faint grey body with a padlock, no markers |

A dead icon returns to normal when the next attempt starts.

## Clone slot rules (game logic)

- **Limit:** at most 5 clones. A chamber can set a lower limit (Tutorial1 allows 2); the HUD and the
  number keys only offer that many.
- **Locked start:** a chamber that introduces cloning starts with the clone controls locked: R, T
  and 1-5 do nothing and the R/T buttons and clone column are hidden. When the chamber unlocks them
  (Tutorial1: the first time the player falls into the pit) they pop in one after another (fade up,
  grow with a slight overshoot). They stay unlocked for the rest of the chamber.
- **Controlled clone:** the player always controls exactly one clone slot. The run is recorded
  into it, and that clone does not spawn. At the start it is the new slot (clone 1).
- **Picking a clone:** pressing 1-5 (or clicking an icon) picks a saved clone, or the ghost new
  slot, to control from the next restart. The current run keeps going. Pressing the picked number
  again cancels the pick; picking the clone already controlled does nothing.
- **R:** saves the run into the controlled clone (a new clone, or over the old path), then:
  - if a clone is picked, control switches to it;
  - else control moves on to the next new clone, whether the run made a new clone or re-recorded
    an old one. Once all 5 exist there is no new slot, so control stays on the clone just saved.
  - To refine the same old clone again, press its number before pressing R.
- **T:** throws the run away and restarts. Control stays on the same clone (so T is how to retry a
  re-record), and a picked clone still becomes controlled.
- Every other clone keeps its path and replays as before.
- **Death (hook built, no hazard yet):** when a hazard kills a clone it stops replaying for the
  rest of the attempt and the HUD shows it as Dead. Death resets when the next attempt starts.

## Keybinds

| Key | Action |
|---|---|
| R | Save the run into the controlled clone and restart |
| T | Restart without saving |
| 1-5 | Pick that clone (or the new slot) to control from the next restart; again to cancel |
| Tab (hold) | Show the chamber map; release to return |
| Esc | Open or close the pause menu |

Movement (WASD), E, R, T, 1-5 and Tab can be rebound by the player in the pause menu's Settings; the
HUD's key letters and badges follow the new keys. The arrow keys and Esc are fixed.

## Chamber map

The play camera sits close (3x pixel scale) and the character only sees a small pool of light, so
the whole chamber is never on screen. Holding Tab swaps the view for a simplified map of the whole
chamber, so the player can keep track of every clone.

- **Shows:** the floor (dark blue-grey) and walls and ledges (pale cyan), generated from the
  chamber's own tiles, plus markers for key features: holes/pits (black), buttons (yellow dots),
  doors (red lines) and the exit (green).
- **Characters:** the player is a white dot; each clone is a dot in its slot colour (the same colour
  as its HUD icon and light), turning red when it dies.
- **Live:** the game keeps running while the map is open and the dots move in real time. The
  player's character stands still while Tab is held (that pause is part of the recorded run).
- The HUD stays on top, so the clone icons sit beside the map. Pausing closes the map.
- Not yet: door open/closed state, a legend, fog of war.

## Menus

- **Title screen** (`Title.unity`, the first scene): a placeholder "Game Name" logo with buttons
  beneath it: Continue (only when a save exists), New Game, Quit.
- **Continue:** the game remembers the last chamber started. Continue restarts that chamber fresh,
  without its clones. New Game and Restart Game start over from the first chamber.
- **Pause menu (Esc):** freezes the game and darkens the screen behind a centred panel: Resume,
  Settings, Restart Game (back to the start of the first chamber), Quit Game. Esc again resumes.
- **Settings:** one row per rebindable key. Click a key, then press the new one; Esc cancels.
  Choosing a key that another action already uses swaps the two. Reset to defaults and Back.
  Rebinds are saved between sessions.
- Same placeholder style as the HUD; every menu button shares one button prefab, so final art
  replaces them all at once.

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
