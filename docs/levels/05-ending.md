# Ending ("Reunion")

Built as `Assets/_Project/Scenes/Ending.unity`. It is the last scene in Build Settings and comes after Chamber 4.

## Purpose

This is the bad-ending plot twist from [the story](../story.md). All game long she has chased her sister through the lab. The last room is a trap: the sister was a projection, a cage drops on her, and the camera pulls back to show scientists who have been watching the whole experiment. There are no puzzles and no clones. It is a short, mostly scripted scene.

## Room

- A long, narrow, clean chamber, 7 cells wide and about 31 long (the least overgrown room in the game: it is the lab's real test room). The door end and the hologram end are far apart, so crossing takes a few seconds.
- The entrance door is in the back-left wall at the far end (she arrives from Chamber 4). There is no exit.
- A floor projector stands at the other end of the room, with a cyan hologram of the sister standing on it.
- A strip of 8 warm overhead pools of light runs from the door to the hologram, like the ceiling-breach pool over Tutorial1's pit.
- A two-way mirror is set into the back-right wall (two blocks high there) a few cells back from the hologram end, behind the cage in the pull-back view. From inside the room it looks like plain dark glass and the silhouettes are not visible at all. When the twist comes a pale lit-room layer fades in inside the pane and the silhouettes (clipped to the pane by a sprite mask) show dark against it. Behind it is a small observation room with three lab-coat silhouettes and its own lights, off at first.
- A few dim lamps light the entrance end.

## Flow

1. **Arrival** (cutscene, skippable with any key): the door is open and she sprints in from Chamber 4. The shutter slams behind her.
2. **First sight** (cutscene, same skip): the camera zooms out and glides along the room. The guide lights come on one after another from the door towards the flickering hologram. She says "Sis?". The camera returns to her and the zoom closes in.
3. **Walk** (the player has control): she can only walk, a few seconds to the hologram. The clone controls stay locked and hidden. The title card reads "FINAL CHAMBER / REUNION".
4. **Trap** (control ends for good once she reaches the projector): she is drawn to the hologram's centre as it glitches out, then about 0.4 s of silence. Every lamp and the guide strip cut out with a heavy clunk and a shake. A harsh white spotlight bangs on over her and the large cage slams down over her with a big shake.
5. **Confusion:** a speech bubble above her head shows "...?".
6. **Pull-back:** the camera slowly zooms out and drifts towards the two-way mirror. Dim lights come on behind the glass, the glass brightens and the silhouettes fade in, watching her.
7. **End:** the screen fades to black and the lines type out: "SUBJECT 07 CONTAINED.", "RESETTING TRIAL...", then "THE END". The credits scroll and then the game returns to the title. Any key skips the credits. Finishing the game clears the Continue save.

## Credits

- Developed by Ollie, Freddie, Aman
- Art made with PixelLab
- University of Bristol Game Jam 2026, theme: Time

## Art

PixelLab, in the cheap modes:
- a large iso cage (one sprite)
- the two-way mirror panel
- lab-coat scientist figures, shown as black silhouettes
- the floor projector

The sister reuses the main character's sprite, tinted cyan as a hologram.

## Notes

- The sister has no character art of her own; she is the player sprite tinted as a hologram.
- Sound: a lamp clunk, the cage slam and the silence before the trap (see [audio](../audio.md)).
