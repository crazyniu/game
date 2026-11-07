# Home page and polish follow-ups: report

## Home page
- New canonical screen `PerchSort/game/home_view.gd` (`HomeView`), opened at launch instead of the map. One Codex
  painting (`tools/home/home_raw.png`, cropped to `art/home_bg.png`) with two painted branches; perch lines measured with
  `tools/scene/cut_branches.py` `cut_slot` (left 1042, right 1243). Painted logo `art/home_logo.png`.
- Layout: coins top left, settings top right, logo, five live birds (met kinds, equipped outfits), green Play with a
  chip showing the map picture and the level Play opens, then map, shop, aviary and help picture buttons.
- Motion: logo drops in and breathes; birds fly in one after another; every 6 s one bird flits to a free seat.
- Navigation: Play starts the next level (first unlocked level without stars) directly; the map gets a back button
  to home (settings left the map row, it is on the home page); win and settings Home buttons return home.
- Preview states `home_view:home`, `home_view:home_new` (in `scripts/screenshots.sh`).

## Follow-ups
- Outfits never sit off the head: an outfitted bird only uses frames whose anchors were measured; turns are cut to
  their reliable leading frames, flap/hop hold the nearest reliable frame, unreliable poses and the preen are unused.
- Hints are instant from the first frame: `tools/levels/export_hints.gd` writes each level's start route to
  `levels/hints.json`; `hint_timing_17/30/34/1` log press_to_hand_ms=0.0.

## Checks
`bash scripts/run_tests.sh` (naming, smoke, core), home captures and a 9 s frame sequence, berry outfit frames over
12 s at 0.05 s. The user tests on the Mac.
