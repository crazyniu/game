# Verification report

Base: a14e30a04a25aee2d51cdb1c6df26a08d56f7053. Only nine test scripts and this work-log change; no product logic, artwork, balance, levels or persisted data changes.

## Results
Godot 4.7.2 standard, GL Compatibility on isolated Xvfb, 1080x1920 for synthetic pointer coordinates, isolated XDG data/config. Commands follow `godot --path PerchSort --audio-driver Dummy --rendering-driver opengl3 --resolution 1080x1920 -s res://tests/run_<name>.gd`.

- aviary_actions:1203 checks,0 failures; price/equip actual bottom-edge input retained.
- chest:0 failures; reward shape assertions occur after temporary sparkle cleanup, with exact amount/deduplication checks retained.
- dialog_skin:192 checks,0 failures.
- level_heading:7861 checks,0 failures, including both locales and all210 ordinals.
- purchase_close:1676 checks,0 failures, including unchanged pointer/callback checks.
- map_card_clip:103 checks,0 failures; every world unlocked from Levels.COUNT.
- home:0 failures; eight real entry clicks, four drawn birds, breathing and localization covered.
- map_nodes:3858 checks,0 failures in representative graphical cases; additional headless `--all-levels` tests both locales for all210 current levels (see result below).
- draw_purchase:228 checks,0 failures. Every pack across four display balances; opening/cancel preserves profile, confirming credits exactly once despite duplicated signal, other fields remain unchanged. The current PaymentService is a documented placeholder, not real billing.
- `bash scripts/run_tests.sh`: naming self-test, naming guard, painted-word check, smoke and210 core solution replays pass.
- `git diff --check`: pass.

## Verification limits and diagnostics
Intermediate failures identified stale assumptions, then updates were re-run: direct static PurchaseDialog reference caused autoload compilation order problems in a test and was changed to runtime loading; fixed0.2-second modal-exit waiting was replaced with bounded lifetime waiting. No product change was needed.

The final draw/purchase run reports an ObjectDB/resource cleanup warning at process exit; assertions complete with0 failures, but this warning is not claimed resolved. No physical device, real billing/ad SDK, audio-listening or complete visual approval is claimed. Evidence and cache stay local, not committed.

`run_purchase_surface` requires historical geometry captured before the original surface change. That artifact is unavailable: it was not regenerated from current output to manufacture a passing baseline. The separate legacy `check_trail_clearance` still targets retired route geometry and is not repaired by these nine-suite changes; current node geometry gets actual rendered-node checks instead.

No auto-merge or deployment. Review should confirm these contracts reflect the accepted current product before adopting the tests.

All-level node run: `map node checks: 127680 checks, 0 failures`.
