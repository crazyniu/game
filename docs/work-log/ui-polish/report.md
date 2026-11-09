# Report

- New art: `art/map_world_w1..w4.png` (local Codex, prompt `tools/map/sketch/world.txt`; ten node centres measured
  from the painted switchbacks), `art/kit/card_gold.png` (`tools/kit/sketch/gold.txt`). `map_bg_continuous_w*` removed.
- Tone pass: `tools/kit/tone.py` once over the kit (see tools/kit/sketch/README.md).
- Verification: `bash scripts/run_tests.sh` rc=0 (core checks 0 failures); `bash scripts/screenshots.sh` 76 captures,
  failed=0, reviewed as contact sheets; naming guard 0 violations. Stills only; motion not re-recorded.
- Open: the How to Play popup over the map still shows the world sign dimmed above its ribbon.
