# Current map marker verification

Base main c1332d9 after #308. No open PR touched this surface at preflight. Runtime change is removal of four decoration lines only; the current-level bird and all interaction code remain unchanged. The former warning-only map test now fails if a Glow node returns.

Actual Godot4.7.2 cloud X11/OpenGL Compatibility screenshots of world1/current8 were inspected before and after. The soft yellow area behind8 is removed; layout, title, current bird, tile numbers, stars, locks and world arrows remain in place. Existing natural background lighting is unchanged. No claim is made about mobile-device acceptance.

## Validation

- run_map_nodes: 3858 checks, zero failures, 5.52s; includes strict no-glow assertions and timed bird texture observations.
- check_map_hit_regions: 101430 checks, zero failures across all210 current levels, 12.29s.
- run_map_card_clip: passes, 2.08s.
- scripts/run_tests.sh: exit0, including all210 stored solutions and94 goal checks. Expected invalid-fixture diagnostics retained.
- git diff --check passes. No artwork, gameplay, save or audio code changes.

## Ignored evidence

`build/heading/map-glow.png` is the original rendering; `map-no-glow.png` is corrected. Both use canonical `--preview=map_view:world_page --capture=<path> --wait=1` through the actual desktop engine, not a mockup. `build/heading/no-glow-motion/f000.png` onward is the unchanged bird animation sampled with `--frames=<dir> --step=0.25 --wait=2.5`. Focused logs/results are in `build/heading/no-glow-tests`; aggregate output is `build/heading/no-glow-aggregate.log`.

Cloud capture logs include unsupported V-Sync configuration and ALSA device absence followed by Dummy fallback; audio is not tested or changed. GUI regression runs explicitly use Dummy audio. No full56-test sweep or closure of unrelated #254 acceptance is claimed. The older user-local catalog/tool work remains untouched.
