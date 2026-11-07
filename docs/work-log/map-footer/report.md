# Map footer report

## Changes

- Extended `PerchSort/game/map_view.gd`'s trail viewport by 260 design pixels to end at y=1920, removing the horizontal foreground cutoff above navigation.
- Removed the bottom gradient and its state, leaving the painted trail visible through the bottom region.
- Changed map Settings from the cream style to the existing orange style, which supplies a white icon. All three navigation buttons retain their positions and sizes above the trail.
- Updated `docs/design/ui-system.md` with the current footer contract. Existing header clipping, scroll-end clearance and unrelated workspace edits are preserved.

## Acceptance boundary

No tests, lint, builds, game launches, UI automation or screenshots were run. Runtime appearance and input acceptance remain with the user. Relaunch the game to load the source changes.
