# Partial-goal trial route repair

Related issue: [#260](https://github.com/crazyniu/game/issues/260); original implementation: #275. Base: merged Cocos main `da2f3e5e` (#314).

## Change

The Cocos migration retained the three trial levels, goal rules and board presentation, but dropped `--trial` from the launchers and the `open_trial` navigation path. The restored route passes the authored definition to the canonical `BoardView` with a copied profile, skips catalog settlement, displays zero win rewards/drops, and connects Next/Replay to trial numbers. Trial 1 requests its opening hint after the entrance and does not consume a Firefly. Invalid trial numbers return home.

Normal navigation clears the active trial number. The settings replay path also recognizes a trial, so IDs 9001–9003 cannot enter `Levels.get_level` or normal settlement. Trial profile callbacks refresh only its HUD; tool consumption and tutorial flags stay in the copy. The normal real-profile save lock remains in place because existing explicitly confirmed tool/undo purchases still spend real feathers, as documented in #260. Trial wins do not alter the resume level, stars, clears, inventory, coins, collection or unlocks.

## How to open

- macOS/Linux: `./run.sh --trial=1`; use 2 or 3 to start directly. `./run.sh serve --trial=2` uses an existing build.
- Windows: `.\run.ps1 --trial=1` or `.\run.ps1 -Trial 1`; use `-Command serve -Trial 2` for an existing build.
- Existing localhost web build: `http://127.0.0.1:8765/?trial=1`.

Build the changed source before using `serve`. Trial launch takes precedence when both `trial` and `level` URL parameters are supplied. A trial holds the same real-profile browser lock as normal play; another tab with that save must close before this tab loads it.

## Verification and remaining acceptance

No local test, lint, build, screenshot, UI automation or temporary validation program was run for this follow-up. Source review covered launch parsing, trial navigation, the separate settlement callbacks, copied-profile ownership and the real-profile purchase path. CI status is recorded in the pull request, not treated as runtime acceptance.

The 210 catalog levels, stored solutions, trial definitions, core rules and painted assets are unchanged. The user has not yet played this restored Cocos route or accepted the partial-goal idea. Keep #260 open; the order goal and the number of partial-goal levels per world still await that decision.
