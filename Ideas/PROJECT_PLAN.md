# PROJECT_PLAN.md

## Concept

- **One-line pitch:** Isometric game using after images of self to navigate around enemies and solve puzzles across a variety of rooms.
- **How the game uses the theme of time:** Past versions of self used to help complete rooms by defeating/distracting enemies, acquiring information, activating interactables and manipulating objects.
- **Genre and perspective:** True isometric (2:1), HD pixel art (64x32 px floor tiles, 64 pixels per unit)
- **Player goal and win/lose condition:** Solve the puzzles, complete the levels
- **Target platform and engine:** PC, Unity

## Core mechanic

- **Main time mechanic (what the player does with time):** Creates afterimages, each one replays its actions each time you reattempt. Up to 5 clones (Tutorial1 allows 2 and only unlocks cloning once the player is trapped in its pit), each with its own colour; the player always controls one clone (shown in the HUD) and can switch to an earlier clone to re-record its path while the others keep replaying (rules in `gui.md`)
- **Moment-to-moment controls:** WASD/arrows move (screen-relative), E interact, R save the run into the controlled clone and restart, T restart without saving, 1-5 pick the clone to control from the next restart, hold Tab for the chamber map, Esc pause menu. The HUD's R/T buttons and clone icons can also be clicked. Gameplay keys can be rebound in the pause menu's Settings (menus in `gui.md`).
- **What makes it interesting or tricky:** The camera stays close and the character only sees a small pool of light, so the player relies on the hold-Tab chamber map (walls, key features and a live dot per clone) to keep track of their clones. Coordinating the clones/afterimages to work together and solve the puzzle in the fewest clones possible, add a par later on. Maybe add a time limit? As the number of clones increases they get worse due to the dodgy tech?
- **How we will test that it is fun early:** Playtest, keep modular so there's always a working version, later will get others to try

## Scope

- **Must have (playable end to end):** At least one full working level with interesting puzzles, working afterimage mechanics
- **Should have:** Enemies, inside/outside variations
- **Stretch goals:** (Coop? Would this work?), lore, easter eggs, NPCs with text speech

## Game loop and structure

- **Start screen to first play:** title screen (placeholder logo, Continue when a save exists, New Game, Quit), then Tutorial1. Story intro idea: <awake in overgrown chamber (dungeon/cave?..), navigate through puzzle chamber, then onto another chamber (outside?)>
- **Core loop (repeated actions):** Repeat sending out a new clone, all the previous clones replay on a new attempt. Switch to a previous clone (1-5, then R or T) to re-record its path while the rest keep replaying
- **Levels, rounds or progression:** Start by polishing an initial level, then... Tutorial1 ("First Clone", `tutorial1.md`) is built and playable.
- **End states and restart:** Escaping the chambers/map? Can replay a level attempting to use fewer clones or do it quicker

## Art and audio

- **Visual style and palette:** Post-technology/deserted world, overgrown, relic/half broken tech, Portal 2 inspired
- **Asset list (sprites, UI, backgrounds):** UI: tintable clone icon, dead (skull) overlay, selected outline, key badge, R and T buttons (see `gui.md`). Rest TBD
- **Sound effects and music:** TBD
- **Asset sources and licences:** TBD
- **Inspiration (not copying but general style):** Portal 2 (world and puzzle chamber concept), classic isometric room games (view)
- **Placeholder art:** generated in-engine (`placeholder_iso_*` in `Assets/_Project/Art/Placeholders/`) at the final scale, so real art can replace it file by file

## Lore

- What - Rugged female character, escaped lab rat sort of look, arm implant (ark esque)
- Where - Modern/dungeon fusion, evil lab
- Why - Rescuing sibling from deep within the lab. Evil parents owns the evil corp and experiments on you/sibling. Turns out they were just walking into their own cage/pan out at the end with scientists watching you.
- When - post technology world, potentially this is all an illusion created by the lab?

## Story
- Intro - Camera starts from inside, sees open door to outside world (all you can see is bright lights). Text box of somebody yelling "hey, get back here!" or something similar, then character sprints in, and door slams shut behind. Camera then centred on character from now on. **Built in Tutorial1:** entrance door in the back-left wall, line "HEY! GET BACK HERE!" from "???", plays on New Game and Restart Game only, any key skips (details in `tutorial1.md`, step 0).
- 
  
## General level design

- Buttons/pressure plates
- Lasers that take a while to charge up and then fire at some frequency
- Specific door that signals next level

## Detailed character description

- Long black hair, somewhat scruffy, white, shadow of hair obscures facial features
- Dark greyish tank top, baggy almost construction worker like trousers, but in a muted colour.
- Shoulder/upper arm implant, central diamond-ish shaped main part, geometric lines veining out around, if this isn't obvious enough with the low res, just a general glowing bit with some occasional particle effect.


## Tech

- **Engine, language and version:** Unity 6 (6000.6.8f1), C#, URP 2D, Input System package
- **Repo and branching approach:** `main` always runs; short-lived `<area>/<description>` branches merged by PR (details in `CLAUDE.md`)
- **Key systems (input, time system, entities, UI):** one input reader; a shared tick clock that records the player each fixed step and replays clones from it; an actor interface so buttons, pits and exits react to player and clones alike; a level reset interface; uGUI HUD, title screen and Esc pause menu (layout and clone slot rules in `gui.md`, code in `CLAUDE.md`, Key systems)
- **Build and export target:** Windows standalone (settings not confirmed yet)

## Team and tasks

- **Roles:** Programming/Implementation, Asset/Sound design, Planning/Ideas
- **Task split by person:** TBD (likely rotate and work together on different tasks)

## Timeline

- Runs about midday Saturday to midday Sunday
- **Concept locked and repo set up by:** Before the start
- **Core mechanic playable by:** Early afternoon
- **Feature complete by:** TBD
- **Playtest and polish by:** TBD
- **Submission buffer starts:** TBD

## Risks

- **Biggest technical risk and fallback:** Scope creep with interesting features before core mechanics get implemented. Need to ensure a new playable version fairly often in between each new feature / expansion
- **Biggest scope risk and what we cut first:** Extra bits like Coop, detailed lore, etc. come last after polished functioning puzzle levels with cloning mechanic

## Submission checklist

- [ ] Build runs on a clean machine
- [ ] Page and upload set up, with description
- [ ] Screenshots or a short gameplay clip
- [ ] Credits and asset licences listed
