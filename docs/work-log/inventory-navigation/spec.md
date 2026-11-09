# Inventory and navigation repairs

## Scope

Keep the board's undoable inventory and the persisted profile consistent after tool use, undo, restart, and shop purchases. Remove redundant screen reconstruction when no overlay is being dismissed or when the underlying screen is about to be replaced.

## Contract

- The board synchronizes its restored inventory to the profile on undo and restart.
- The settings restart action calls `BoardView.restart()`, which cancels ongoing board visuals, restores the session, synchronizes inventory, then starts the same level with that profile.
- A successful booster purchase emits its exact booster kind and purchased count. The main controller adds that count to the current session and all of its undo/restart snapshots through `BoardView.granted()` and `GameSession.grant_booster()`.
- A purchase updates the profile once. Synchronizing the session must not apply the purchase to the profile again.
- Closing an actual overlay refreshes the revealed screen. Replacing an overlay or the underlying screen disposes it without refreshing that screen. Calling close with no overlay does nothing.
- Keep the existing UI, persistence keys, and session snapshot semantics.

## Acceptance

The user will verify the branch. Do not run tests, lint, builds, game sessions, UI automation, screenshots, or temporary verification programs.
