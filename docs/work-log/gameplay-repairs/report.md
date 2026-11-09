# Gameplay repairs report

Branch: `codex/gameplay-repairs`.

## Changes

| Reviewed issue | Implementation |
| --- | --- |
| Booster finishes the board without settlement | Moves, undo and boosters finish through `_finish_action()`. A winning booster now reaches `_celebrate()` and the existing reward/progress handler. |
| Undo/restart/purchase inventory diverges | Undo and restart copy the restored full session inventory to the profile and save it. Shop purchases emit their exact item quantity; `grant_booster()` applies it to the current state and every snapshot. |
| Chapter difficulty resets | The same 100 puzzles are ordered globally using initial fragmentation, species, mechanics and existing random-play evidence, capped by known short wins. Chapters no longer force a trivial teaching puzzle at their start. |
| Hints exceed three-star par | Hint caches follow their puzzles, and pars cover their counted hint routes. Future hint export updates the catalog's par together with the routes. |
| Toolbar selects an irrelevant tool by level number | The toolbar includes every unlocked tool with an effect on the current board. Buttons retain their size; gaps narrow when needed to fit the full row. |
| Undo races an old homecoming | Departing birds are tracked and removed, homecoming tweens/effects are cancelled, and suspended continuations are invalidated. Restored goals and resident birds come from the session. Restart also invalidates entrance, action and celebration continuations. |
| Input is discarded during long waits | Ordinary moves retain the next two branch taps (one source/destination pair). Transfers use shorter flight/stagger/landing timings; phase crossfades no longer hold input. Hint searches show a spinner on Hint alone and leave the board usable. |
| Failed background hints repeat forever | Failed search keys are cached. Cancelled work is not classified as failure, and stale requests never consume a Firefly after a move or modal navigation. |
| Navigation rebuilds a map that is immediately discarded | Closing no overlay does nothing. Screen replacement disposes an overlay without refreshing its underlay. Actual dismissal refreshes once. |

The ordinary transfer parameters total `0.06 + 0.28 + 0.10 + (birds - 1) * 0.04`, or 0.44-0.56 seconds for
one to four birds. This is parameter arithmetic, not measured frame time; hatches and winning sequences still
play their own animations. Entrance and homecoming flight durations retain their prior values.

Queued input and pending unpaid hints are cleared when opening settings, goal information, an overlay or a
screen transition. Cancelling a pending hint does not cancel useful background caching or charge inventory.

## Level and save handoff

See [difficulty continuity](../difficulty-continuity/report.md) for the ordering formula, complete ID mapping,
the authored curve and save migration. The existing profile fields and enum orders are unchanged. Per-puzzle
stars, clear counts, best moves and legacy level gifts migrate with the puzzle; unlocked access is retained.
No live save was read or modified during implementation.

| Original review case | Level on this branch |
| --- | --- |
| Level 31, final homecoming through Scarecrow | 8 |
| Level 32, sleepers without crows | 37 |
| Level 52, sleepers and crow without a growing branch | 52 |
| Level 6, hinted route exceeds par | 12: hint 9, par 9 |
| Level 8, hinted route exceeds par | 19: hint 13, par 13 |
| Level 25, hinted route exceeds par | 29: hint 15, par 15 |

Inventory/navigation details are in the [supporting report](../inventory-navigation/report.md).

## Review and acceptance boundary

Implementation and independent source review are complete. No tests were written or run; no lint, build,
Godot/game execution, UI automation, screenshots, solver searches or temporary verification programs were run.
The catalog transformation was an authoring operation using existing source data, not a solver replay or test.

The user owns runtime acceptance, including win payouts, inventory after undo/restart/purchase, animation
cancellation, continuous tapping, hints, migrated saves, tool layout and navigation feel. The score is a heuristic;
human difficulty and neighboring route lengths can still vary. Performance improvements are not measured.

Existing automated test files were left untouched under the user's no-test-work instruction. Their old fixed
level-number migration expectations and early-level branch-count assumption in `tests/run_core.gd` no longer
describe the reordered catalog; those fixtures need updating before that suite can be used for this order.
