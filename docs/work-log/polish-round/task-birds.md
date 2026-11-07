# Task birds: continuous, natural bird motion

The user sees bird motion as not continuous, in the shop and on the board. Known causes (review in
`docs/work-log/ui-motion-review/report.md`): outfit hops repeat the same standing texture six times and only
move it vertically; flight sequences always use the plain species frames so an outfit disappears in flight and
returns on landing; each bird schedules idle actions independently so neighbours twitch together or in
sequence; frame changes pop at 12 to 14 fps without easing. Find and fix what actually looks wrong:
- Inspect the real sequences (`PerchSort/birds`: turn, preen, flap, hop, cheer, flight_*, front blink, poses) and play
  them in a capture; describe what pops or jumps.
- Make each action read as one fluid motion: ease the vertical rise and landing squash, cross-fade between consecutive
  painted frames where the frame rate is low (never mirror or flip a still), blend into and out of the rest frame.
- Outfits: outfit frames exist only for front, blink and cheer. Make every action keep the outfit visible. Preferred:
  an overlay of the outfit art following the head through the frames using measured head anchors per frame; if that is
  not feasible, ask the coordinator to have the missing frames painted and list exactly which (species, outfit,
  action, frame count). Do not hide the outfit during actions and do not leave it frozen.
- Coordinate idles on a branch: at most one bird of a branch acts at a time; spread starts so the board always has a
  little motion somewhere but never twitches in unison.
- Flights keep the outfit.
Evidence: for the board (level 12) and for a shop outfit card: frame sequences at 0.05 s over 6 s before and after,
a contact sheet of each action, a log of action starts per bird proving the coordination, and an honest list of what
still pops.
