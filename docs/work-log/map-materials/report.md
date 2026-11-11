# Map material repair delivery

## Implemented

- Replaced the carousel and world-trail cream star-count chips with the existing
  painted star progress treatment: gold-rimmed green track, collected-star fill,
  gold star and white painted count. Both use the same page-local fitting helper.
- Kept each group's former bounds. The shared progress component is constructed
  at 112 px height and uniformly scaled so its nine-patch margins and overhanging
  star fit the 104 px carousel slot and 80 px world-trail slot.
- Retained the useful landscape cards, painted forest scenes, wooden names, world
  badges, locks, completion checks and restrained page dots. No enclosing surface,
  replacement art, new wording or locale manifest entries were added.
- Preserved all navigation, snapping, preview states, current-node bird, level
  interactions and Play enablement/dimensions. Existing uncommitted layout and
  shared-balance refinements were retained, not reimplemented by this task.

Owned changes are `PerchSort/game/map_view.gd`, `docs/design/map.md`, and this
folder's spec/report. No shared-file edits or locks were required. No shared-helper
change is requested. Core, meta, levels, saves, audio and other page scripts were
not edited by this task.

## Inspection and delivery boundary

Read-only source inspection used `cat`, `sed`, `rg`, `git status --short` and
`git diff -- PerchSort/game/map_view.gd`. Existing production paintings, name plank
and progress/card textures were inspected with the image viewer. No design pixels
were extracted and no asset was generated or edited.

No tests, naming guards, lint, builds, exports, game launches, UI automation,
screenshots, timed captures or temporary validation programs were run. No runtime
evidence was created. Visual fit, digit readability, input, first/middle/last forest
accessibility, long localized headings, animation, audio, other resolutions and
performance remain NOT ASSESSED; the user owns acceptance. The source review is
not a runtime pass. No Git commit, branch operation, pull, merge or release occurred.

User review states and the retained element boxes are in `docs/design/map.md`.
