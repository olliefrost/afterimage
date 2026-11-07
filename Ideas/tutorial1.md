# Tutorial Room 1: "First Clone"

**Purpose:** Introduce the core loop (cloning and cooperating with your past self) with no text-heavy tutorial, using level design and visual cues only.

**Mechanics taught:**
- Exploring and following visual hints
- Pressing a button
- Using the clone button to replay your previous run
- Cooperating with your own clone to solve a puzzle

## Room layout

Isometric chamber, as in `Sketches/tutorial1.png` (scene: `Assets/_Project/Scenes/Tutorial1.unity`).

- **Spawn area:** Centre-right, close to the back-right wall, mostly empty and only faintly lit, so the player's attention is pushed toward the hint.
- **Door:** Set into the back-right wall towards its right end, left in shadow so it doesn't read as the obvious route. A short corridor behind it leads to the level exit.
- **Hole:** In the centre of the room. The hint is a collapsed patch of ceiling above it: a soft, slightly warmer pool of daylight falls around the hole and plants grow thicker there. The room is otherwise dim and cold. There is no trail of dots.
- **Pit:** 3x3 floor tiles with its far inner walls visible. Holds a single button at the bottom. It is a one-way drop: the player cannot climb out.
- **Overgrown annex:** The left side of the room has more moss, crates, rubble and two glowing relic terminals. It is decoration only. Plants and props are spread through the whole room, not just one corner.

## Step-by-step flow

1. **Spawn.** The player enters the room and sees the shadowed door and the hole with its hint. The door isn't an obvious exit.
2. **Investigate.** The player is drawn to the brighter area and walks into the hole.
3. **Reveal.** The player lands at the bottom of the pit next to a lone button and can't get out. Pressing E presses the button, and the clone button in the corner of the HUD starts pulsing to suggest it's worth using.
4. **Button press.** A red pulse travels along a wire on the floor to the door and opens it.
5. **Clone.** The player presses R (or clicks the clone button) and returns to the spawn point. T restarts without keeping the run.
6. **Watch the past self.** The player sees their previous run play out: the clone walks into the hole, presses the button on the same tick, and the signal opens the door. The clone then stands still where its run ended.
7. **Exit.** The door is now clearly the right way to go: it fades open and warm light spills into the room. The player leaves through it and the level completes.

## Teaching beats

| Moment | What the player learns |
|---|---|
| Brighter area around the hole | Follow visual cues to find the path |
| Button in the pit | Interacting with objects has effects elsewhere |
| Pulsing clone button | The clone mechanic is something to use now |
| Signal running to the door | Cause and effect is shown visually |
| Watching the clone repeat the run | Clones replay your earlier actions |
| Door open and lit | Clear payoff, go this way |

## Art and feedback notes

- Use shadow and lighting to hide the door and hole bottom without making them unfair.
- The red signal should be clearly visible and readable as it travels, since it carries the cause and effect.
- Door open state needs a strong visual change (light spill or a trail).

## Decisions

- **Clone replay:** a clone replays its run once, then stands still where it ended. It does not loop.
- **Pit:** one-way drop. Pressing the clone key is the only way out.
- **Hint:** a brighter, warmer area from a ceiling breach, not a trail of dots.

## Open questions

- Is there any failsafe hint if the player doesn't find the hole after a while?
- Does the story intro in `PROJECT_PLAN.md` (door to the outside slams shut behind the character) play at the start of this level, or before it?