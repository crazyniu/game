# Task tutorial: teach the game

Nobody can tell how to play: there is no introduction. Add one.
- A short illustrated how-to-play of 4 steps, shown automatically the first time the player opens level 1 (use a
  profile tutorial flag through the existing `has_tutorial` / `set_tutorial`, no new save fields), and always
  reachable from a help button (icon `info`, a secondary cream button) on the map's bottom row next to the others (keep
  the existing three buttons' positions sensible; all secondary buttons the same size).
- The steps, each with a small live scene built from the real art (the world 1 painting crop or plain cream card, branch
  layers, live `BirdView` birds) and an animated pointing hand (`hand_pointer`), one sentence of painted text per
  step: 1 "Tap a branch to pick up its birds" (a group of the same bird on top lifts); 2 "Tap another branch with room
  to move them: same bird on top, or an empty branch"; 3 "Fill a branch with one kind of bird and they fly home to the
  house" (show birds flying into the house); 4 "Send every kind home to win. Fewer moves, more stars" (three stars).
  Steps advance by a Next button (green primary) and can be skipped (red close); the last step ends with Play.
- Each step animates by itself in a loop (hand moves, birds hop with their painted frames, never a flipped still).
- Text goes through the locale catalog (see spec.md). Keep the sentences short.
- Preview states `tutorial_1` .. `tutorial_4` in your script and add them to `scripts/screenshots.sh`.
Evidence: captures of all four steps, a frame sequence of each step showing its loop, and the first-run trigger working
(first open of level 1 shows it once, then not again).
