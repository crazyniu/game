# Warm game and toy shop dialog integration

## Scope

Isolated `warm-dialog-redesign` worktree based on preserved integration commit `1466792`. No commit or push performed. No pricing, gameplay, profile schema, probability or payout-table changes.

- Game dialog family: independent warm-gold wood rim, opaque face, existing localized painted titles and original content/action geometry. Applies to level details, win, new bird, world rewards/unlock, daily reward result, tool unlock, tutorial/lesson, undo/stuck, save recovery and Credits.
- Shop confirmation family: independent coral/turquoise toy rim matching the selected reference, original item content and exterior price action. Close uses the original artwork/hit size, placed inside its new upper-right corner. Booster quantities, outfit/nest purchases and recharge callbacks are unchanged.
- Existing ordinary pages, aviary and full-screen chest/draw art were not redrawn.
- Reward motion patch: clearer target selection and quicker draw outcome, claimed-day lift and sequential win stars. Confirmed daily-close race fixed by rejecting a departing modal before opening its delayed result. Reward settlement is still before decoration.

## Asset gate

No reference screenshots were cut into runtime textures. Both independent generated rim assets were reviewed at 640×440, 640×760 and 640×1400 in Godot. Warm image center was unsuitable and is never drawn; only the clean outer border is sampled with an opaque native face. First technical attempt had a rectangular seam, which was rejected. Revised rendering has no visible seam/transparency and consistent corners. Coordinator reviewed both three-size checks and actual bilingual win/Firefly screens before family-wide integration.

## Unified validation

`build/warm-assets/core.log`: naming/self-test, painted-word guard, launcher recovery, smoke, all 210 stored routes/core, corrupt-save preservation/exit and bird-animation regression passed. New untracked source paths additionally scanned through the same naming predicates: no violations. `git diff --check` passed.

Actual X11/OpenGL graphical suites on final code, 3,473 checks and zero failures:

| Suite | Checks |
|---|---:|
| Game dialog content/actions/visible glyph centering |381|
| World reward content/amounts/callbacks |113|
| Guides/lessons, six shop-matched item images/input |330|
| Tool guide logic |56|
| Board HUD |216|
| Purchases, quantities/payment callbacks |118|
| Purchase disabled/enabled hits |128|
| Purchase Close geometry and real center/edge double-pointer clicks, both languages |1676|
| Auxiliary/Credits scroll |10|
| Menu navigation |107|
| Modal motion, 46 canonical states plus extended entries |228|
| Draw/claim/win strengthened motion, repeat presses and interruption |110|

Draw result appeared at 2,083/2,144/2,244/2,321/2,401/2,501/2,608/2,701 ms for targets 0–7. Current-balance reads include intervening credit, rewards/callbacks remain exact and single-dispatch; Next remains actionable during win decoration.

Extra actual daily flow: 4 checks passed, last coin arrival 1,278 ms and result opening 1,728 ms, so result did not interrupt flight.

## Explicit limitations and diagnostics

An early run while heavy frame capture was concurrent recorded one old-shop Close timing failure: modal was already closing, but its completion signal had not fired at the unchanged 0.20-second assertion. Evidence retained in `build/warm-assets/close-before-shop.log`. Final new-shop serial full test passed all 1,676 checks without relaxing thresholds. Extra serial repeat is recorded separately.

Auxiliary/menu tests retain existing ObjectDB/resource-in-use warnings at process exit. The software driver reports unsupported V-Sync; these are not counted as clean engine exit. Dormant chapters preview retains its previously known missing white-action word and was not redesigned. No physical iOS/Android safe-area or real-money transaction validation is claimed.

## Evidence

- `build/warm-assets/frames.png`, `toy-frames.png`: actual asset-size checks.
- `build/warm-assets/win_{zh,en}.png`, `shop_{zh,en}.png`: single-page approval captures.
- `build/warm-assets/screens/`: actual bilingual family captures; newest shop confirmation captures replace the earlier old-green-frame shots.
- `build/warm-assets/live-motion/`: actual triggered draw, claim and win sequences, not static design mockups.
- Logs at `build/warm-assets/run_*.log`; extra repeats use `_repeat.log`.

## Final supplementary results

Second independent serial Close rerun: 1,676 checks, zero failures, same 0.20-second completion assertion and same hit probes. Daily flight rerun: four checks, zero failures. Thus final targeted graphical checks including these repeats total 5,153 passed, zero failed.

Actual trigger captures completed: 50 frames each for `lucky_draw_spin`, `daily_reward_claim` and `win_three_stars` (150 frames). Reviewed `live-motion/review.jpg`: highlight reaches selected draw tile before result; claimed-day flight finishes before reward card; win birds land above the border, stars appear sequentially, and reward numerals reach exact final values. Capture slows execution and is not used for latency measurements. Bilingual static family/shop captures (34 PNGs) were reviewed for title placement, border continuity, button outside placement and word centering. Final shop contact sheet is `contact-shop.jpg`; game Chinese sheet is `contact-zh.jpg`.

All changes remain local/uncommitted for coordinator review. Prior integrated commit is preserved in its original worktree.
