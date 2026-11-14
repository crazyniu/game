# Catalog measurement and sibling repair

## Scope and method

Issue #247, based on main `906ae680d221a505fbc93f34a9e714d3437125b9`. The cloud worktree is independent of the owner's local work. This change does not reorder levels, alter IDs/worlds, change save or economy rules, add mechanics, or change UI.

`fragility_before.csv` and `fragility.csv` contain the baseline and repaired 210-level measurements. `world_summary.csv` gives per-world route ranges/medians, random win shares, early traps, unknown searches and sibling outliers. `changed_levels.csv` lists every replacement and its original, before/after route length and par.

A route length here is the length of the **stored winning route**, not a proven shortest solution or a direct measure of human difficulty. Random play is deterministic (seed = level ID), 40 trials per level, with immediate reversal suppressed. It measures recoverability of this policy, not real-user success. A budget-exceeded search is **unknown**, never a proven win or loss. Raw measurement uses 6,000 nodes / 250 ms; repaired-level early-alternative acceptance uses 60,000 / 2,000 ms.

## Repairs

Exactly 15 sibling levels exceeded the specified stored-route ratio: 2, 6, 14, 16, 18, 20, 21, 23, 31, 44, 62, 63, 65, 100 and 111. These are route-ratio outliers, not 15 claims of independently measured human difficulty bugs. Their layouts were regenerated with the generator's light mode, preserving branch geometry and mechanics. All other 195 level records and hint entries are semantically unchanged.

The generator previously rejected full sleeping flocks as though they could already go home. It now uses `Rules.can_home`, preserving valid teaching structures. It no longer emits an out-of-band "best" candidate when no acceptable candidate exists. Every first-three-step alternative must now have a proven winning continuation; budget exhaustion rejects the candidate. One-action originals cannot silently produce two-action siblings.

The hint exporter accepts selected IDs and updates only those entries/pars. If any selected solution cannot be proven, it exits unsuccessfully without writing the catalog or hints. The 15 replacements have stored, legal, booster-free wins, and their initial three steps have no unresolved alternative at the stricter acceptance budget.

## Verification

Engine: Godot **4.7.2.stable.official.ed1daf0bf**, standard Linux build. Profiles are isolated under ignored `build/catalog/`. No device testing or human difficulty validation is claimed.

- `GODOT=<target-engine> bash scripts/run_tests.sh`: exit 0. Naming/painted-word guards, five mocked launcher recovery cases, smoke, all 210 stored-route/core checks, save recovery UI, bird animation assertions and 94 goal checks passed. Expected negative-fixture validator messages are present in the log, not test failures.
- `<target-engine> --headless --path PerchSort -s res://tests/run_catalog_measure.gd`: **472 checks, 0 failures**. Checks all 110 sibling ratios and the repaired routes/state keys/early alternatives.
- Semantic comparison against the base verifies the exact 15 changes; 195 unrelated levels/routes, all IDs, worlds, variant metadata, and branch geometry stay unchanged.
- Both complete target-engine measurement processes exited 0: **210 rows and 8,400 seeded random trials per snapshot**. Neither contains a proven trap beside steps 0–2. Sibling ratio outliers: **15 before, 0 after**.
- Random wins: **5,255/8,400 (62.56%) before; 5,351/8,400 (63.70%) after**. Proven later-route trap alternatives: **292 before, 283 after**. Unknown alternative searches: **186 before, 182 after**. These are state/action counts, not counts of defective levels. Do not interpret unknowns as safe.
- The raw scan's time limit can change an unknown count even in an untouched world (world 8: 9 before, 10 after). The catalog is identical there; this is a measurement-budget boundary, not a claimed regression or improvement. Random-policy success also decreases in worlds 3 and 5; this repair enforces the sibling contract, not uniform human difficulty.

Commands (from the repository root):

```sh
export XDG_DATA_HOME="$PWD/build/catalog/final-user"
export XDG_CONFIG_HOME="$PWD/build/catalog/config"
export GODOT=/path/to/Godot_v4.7.2-stable_linux.x86_64
bash scripts/run_tests.sh
"$GODOT" --headless --path PerchSort -s res://tests/run_catalog_measure.gd
"$GODOT" --headless --path PerchSort -s ../tools/levels/measure_fragility.gd -- 1 210
```

The baseline measurement uses the exact same tool and engine with the base's `levels.json` and `hints.json` in an ignored snapshot. Do not regenerate baseline data from the repaired catalog.

## Visible runtime evidence

All 15 repaired initial boards were captured and inspected in the cloud desktop with the target engine, GL Compatibility and dummy audio. Evidence remains local under ignored `build/catalog/screens/level_<id>.png`; it is not committed. An isolated synthetic profile marks species/tutorials known so first-encounter overlays do not hide the boards. The unchanged presentation shows the expected per-world scenery, branch geometry, birds and mechanism markers; this is not a redesign or a phone-device acceptance pass.

Level 23 was also played through the real board input by the existing `--autoplay` runner, with `--frames=<ignored-dir> --step=0.25 --wait=40`. Final log: entrance **3699 ms**, won **true**, **2 moves / par 3**, total **4422 ms**. The 21 recorded frames show entrance/turn/transfer progression. The runner exits one second after the session wins, before the full result-popup sequence, so complete victory animation is not claimed. Dummy audio means audible output was not verified.

The GUI process reported **5 leaked ObjectDB instances and 2 resources still in use on exit**. These remain an explicitly unverified teardown boundary, not a silent pass; no unrelated lifecycle fix is included. No script/parse errors were observed in the capture log.

## Limits and downstream work

This does not claim globally monotonic difficulty. Ordering/progression belongs to #239/#250 and must follow the catalog's dependency chain. Later-route traps and unknown searches are explicitly retained in the data for follow-up, not counted as passing recoverability. The owner may still have independent uncommitted catalog work locally; this cloud branch neither reads nor overwrites it. Reconcile that work before merging. No deployment or release is performed.
