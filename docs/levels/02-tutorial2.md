# Tutorial 2: "Taking the Hit"

**Purpose:** Reinforce the afterimage mechanic from Tutorial 1, and introduce afterimages as physical objects that can absorb danger, including one afterimage protecting another.

**Mechanics taught:**
- Afterimages can block a laser for you
- An afterimage is destroyed when it takes a hit
- Chaining afterimages: one afterimage protects the next, which protects you
- Using more than one afterimage in a single chamber (2 required)

## Room layout

- **Entrance:** Leads in from the Tutorial 1 door.
- **Corridor 1:** Thin corridor reached by a right turn after the entrance.
- **Laser 1:** At the end of Corridor 1, aimed back down it. Timed so the player can't make it down the corridor in one pass without an afterimage.
- **Left turn:** At the end of Corridor 1, by Laser 1. The only way onward.
- **Corridor 2:** Long, thin corridor after the left turn.
- **Laser 2:** At the end of Corridor 2, aimed back down it, with the same timing as Laser 1.
- **Exit door:** On the left at the end of Corridor 2. Opens automatically when the player reaches it.

## Step-by-step flow

1. **Enter.** The player walks in from Tutorial 1.
2. **Turn right.** The player enters Corridor 1.
3. **Hit the first laser.** Walking down the corridor, the player meets Laser 1 and can't get past it in one pass.
4. **Use an afterimage.** The player's afterimage walks the corridor first and takes the shot, letting the player get through and take the left turn.
5. **Hit the second laser.** Walking down Corridor 2, the player meets Laser 2 and can't get past it either.
6. **Use a second afterimage.** The first afterimage takes Laser 1 for the second afterimage, and the second afterimage takes Laser 2 for the player.
7. **Exit.** The player reaches the end of Corridor 2, turns left, and the door opens automatically. Chamber complete.

## Afterimage breakdown

| Run | Laser 1 | Laser 2 | Result |
|---|---|---|---|
| Run 1 (player) | Hit | — | Becomes Afterimage 1 |
| Run 2 (player) | Afterimage 1 takes the hit | Hit | Becomes Afterimage 2 |
| Run 3 (player) | Afterimage 1 takes the hit | Afterimage 2 takes the hit | Player reaches the door |

**Total afterimages required:** 2

## Teaching beats

| Moment | What the player learns |
|---|---|
| Laser 1 can't be passed in one run | Some obstacles need an afterimage, not just good timing |
| Afterimage takes the shot | Afterimages are physical and can shield you |
| Afterimage is destroyed | Afterimages can be sacrificed |
| Laser 2 after the left turn | The same idea applies again, and one afterimage isn't enough |
| Afterimage 1 protecting Afterimage 2 | Afterimages can help other afterimages, not just you |
| Door opens automatically | Clear payoff, chamber complete |

## Art and feedback notes

- The laser's timing should be readable so the player understands that one pass isn't enough.
- An afterimage taking the hit and being destroyed needs to be clearly visible, since it carries the main lesson.

## Decisions

- **Laser hit on the player:** a death. The run is saved as a clone only if it was marked with F; otherwise it is thrown away like T.
- **Restart point:** every attempt starts at this chamber's entrance.
- **Laser timing:** a fixed tick cycle from each attempt's tick 0 (charge 80, beam 10, cooldown 30 ticks, at 50 ticks per second). Hits never change the schedule.
- **Laser 2** stays dark until the left turn.
