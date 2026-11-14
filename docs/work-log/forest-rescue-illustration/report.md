# Forest rescue illustration report

## Change

Replace only the rejected large blue undo symbol with a small leafy perch and the game's existing sunny/sky birds. Use the original animated BirdView implementation and aviary branch, not a generated imitation or a still presented as animation. Keep the accepted `Free Rewind` / `免费回退` images, 540 × 162 button, compact shell, close control and `p.invoke(on_rescue)` unchanged. No gameplay, economy, save, other tool illustration or other modal changes.

The illustration is 320 × 180 design pixels. The frame is 12 px shorter solely because the prior icon's 192 px row is now 180 px. Native bird pose sequences/breathing remain active in live offers; instant mode stays deterministic. A `motion` preview permits observing the actual shared BirdView over time without changing gameplay.

## Validation

- `run_single_card_actions.gd`: 468 checks, 0 failures. Both languages; actual close/rescue pointer edges and exactly-once callback tests preserved, plus compact native-perch structure/quiet-mode/input checks.
- `GODOT=... XDG_DATA_HOME=/tmp/game-single-user bash scripts/run_tests.sh`: exit 0; naming/painted-word guards, launcher recovery, smoke, core, save recovery UI, bird animation and goal checks passed. Goal tests deliberately emit invalid-species diagnostics for rejected fixtures.
- `git diff --check`: clean.
- `run_stuck_rescue.gd`: 40 checks, 0 failures. Actual free rewind and existing rescue searches unchanged.
- Actual cloud desktop OpenGL images: `build/forest-rescue/normal-zh.png`, `normal-en.png`. Inspected both: birds land on the branch, content stays below the title, X is clear, accepted lettering/buttons preserved.
- Actual 4-second, 0.25-second sampled frame sequence: `build/forest-rescue/motion/f000.png` through `f015.png`. Retains native motion only; no new custom rescue flight is claimed.
- UI runner still reports 2 ObjectDB instances / 1 resource in use at exit. This report does not claim leak-free shutdown.
- Initial parallel runs were interrupted to remove memory contention with graphical capture; reported tests are completed serial reruns.
- No new art was needed; existing clean game assets were reused. Captures/logs remain ignored local evidence, not source assets.

Visual acceptance remains with the owner. No merge/deploy performed.
