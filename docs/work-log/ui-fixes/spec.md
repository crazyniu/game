# UI fixes on the existing UI

User direction (2026-11-08): keep the existing UI and fix its problems; the glossy candy kit and a full relayout were
both rejected (kept in `git stash`, not in the tree).

- Fewer, smaller words: button words are capped at about a third of the button height; popup titles and shop card names
  are smaller; the settings popup drops the privacy, terms, contact and credits entries and the icons inside its
  Restart and Home buttons.
- Home: no floating icons or captions. Daily and check-in sit between the logo and the branches, the four places
  (map, shop, aviary, chest) along the bottom, all as one tile size (`UI.tile`: picture over one short word). The
  level button reads "Level N". The map footer uses the same tiles.
- Bigger buttons: `UI.PLAY_SIZE` 400x144 (at most 1.25x the 128 secondary buttons), shop price slot 300x112.
- Greens: `tools/kit/brighten_greens.py` brightens the green kit pieces (ribbon, green pill and disc, toggle, card rim)
  from their committed versions toward the map's foliage green.
- Board: perch spacing never exceeds 150 px and the block is centred (`PERCH_STEP_MAX`); every flock takes
  `FLOCK_FLIGHT_SECONDS` (0.9 s) to reach the house; the last flock of one kind goes home by itself
  (`Rules._auto_home`, no move counted).
