# Gameplay repairs

## Scope

Repair the nine issues confirmed by the source review: booster win settlement; inventory after undo, restart and purchases; chapter difficulty resets and route-dependent scoring; hint routes exceeding the three-star allowance; mechanic-based toolbar tools; stale homecoming animations after undo; dropped input and blocking hint requests; repeated failed background hint searches; unnecessary map reconstruction when navigating.

## Contract

- Work on `codex/gameplay-repairs` in the isolated checkout. Keep the primary checkout and its unrelated files untouched.
- Keep one canonical implementation, the 100 fixed puzzles, ten sequential forest worlds, existing save field names and enum values. Preserve player records if levels move.
- Use the same completion path for moves and boosters. Synchronize the restored session inventory to the profile before saving or starting another session.
- Purchases remain outside undo history. Restart refunds this attempt's spent tools without losing purchases.
- Choose toolbar tools from the current board mechanics. Keep the existing icon/button design; add no explanatory UI text or artwork.
- Undo/restart must cancel outgoing homecoming visuals and invalidate their continuations.
- Buffer the next board interaction during a move and shorten the ordinary flight wait. Hint searches must leave gameplay available, discard obsolete requests, and remember a failed board until it changes.
- Difficulty ordering must not force trivial teaching levels at chapter starts or reward a solver's detour. Keep tutorials attached to actual mechanic encounters. Stored hints and three-star thresholds must agree.
- Do not rebuild a screen merely because a nonexistent overlay was closed.

## Ownership

- Coordinator: `game/board_view.gd`, `game/bird_view.gd`, integration and this work log.
- Economy worker: `game/main.gd`, the purchase signal in `game/shop_view.gd`, `core/session.gd` if required, economy work log. Board integration uses `BoardView.sync_inventory()` and `BoardView.restart()`; purchases emit `booster_purchased(kind, count)`.
- Levels worker: `tools/levels`, `levels`, `meta/profile.gd`, `meta/progress.gd`, launcher migration helpers, difficulty work log. Coordinate any board toolbar API changes with the coordinator.

## Acceptance boundary

The user will test and accept the branch. Do not write or run tests, lint, builds, game sessions, UI automation, captures, or temporary verification programs. Source review and reading/editing the authored data are allowed. Report unmeasured timing and subjective difficulty honestly. Commit the completed changes; do not merge, deploy, or release.
