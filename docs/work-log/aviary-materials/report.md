# Aviary presentation repair delivery

Implemented in the existing working tree for user review; runtime acceptance is pending.

## Changes

- Replaced each large species-card face with a transparent 480 x 432 hit region, a live bird
  on a leafy wooden perch and a painted name. Unknown species use the existing question-bird
  artwork and a small padlock without revealing their names.
- Replaced the detail hero rectangle with a translucent warm glow and a separate leafy branch.
  Bird fitting now registers the existing `BirdView` foot origin to that branch. A dedicated
  display layer keeps the hero above its perch when an outfit is equipped.
- Kept the ability sentence in one compact cream strip. Moved the detail title clear of the
  unchanged balance counter and spaced the outfit section below the hero composition.
- Replaced the six outfit-card faces with equal transparent positions and small perches.
  Equipped outfits retain the check and gain a small green foot marker; locked outfits keep
  dimmed live birds, padlocks, compact gold prices and the existing purchase confirmation.

The existing list drag/fling calculations, scroll restoration, live idle/showcase/cheer paths,
purchase and equip calls, affordability, profile signals, balance refresh, locale rebuilding,
shop Back destination and `preview()` states remain. No gameplay, economy or save changes.

## Ownership and materials

Edited only `PerchSort/game/aviary_view.gd`, the Aviary section of
`docs/design/home-and-aviary.md`, and this subject's work-log spec/report. The document edit
acquired `build/ui-page-locks/home-and-aviary.lock`, reread the file and replaced only the
assigned section; the lock was released in `finally`.

Reused complete production assets: `island_branch`, `island_unknown_bird`, `glow_soft`,
`island_pill_green`, existing check/padlock art, painted labels and shared compact controls.
No assets generated, edited, sliced or replaced. No shared-helper or locale-manifest edits.
No workers or Git state mutations. Existing uncommitted work was retained.

## Inspection and acceptance boundary

Read-only source/document inspection used `cat`, `sed`, `rg`, `git status --short` and
`git diff -- PerchSort/game/aviary_view.gd`; existing assets were opened with `view_image`.
These reads informed implementation and do not establish runtime acceptance.

No tests, naming guards, lint, builds, exports, game launches, UI automation, screenshots,
motion captures or temporary validation programs were written or run. No baseline changed.
Rendering, word readability in both locales, bird contact during all poses, input, scroll return,
purchase/equip behavior, motion, audio and performance remain unassessed by execution. The user
can review the current Aviary and shop-owned detail paths, including unknown, locked, equipped,
crow, maximum-balance and insufficient-funds states, through the retained preview contract.
