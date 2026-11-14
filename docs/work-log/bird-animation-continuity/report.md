# Bird animation continuity report

Baseline: main `94a581bb24957bc0824831e0fab1165d606b3dee`, Godot 4.7.2, isolated cloud worktree and XDG profile.

## Confirmed and fixed
1. `cheer()` previously launched even when another action owned the bird. The win popup hero starts its cheer timer at 1.6 seconds, but flight delay, takeoff, travel and landing span about 1.67 seconds. Competing tweens overwrite painted frames, release `acting` early and emit multiple `finished` signals. Decorative cheers now skip busy/selected/showcasing birds; the repeating timer can retry later. Normal idle cheers remain unchanged.
2. `nudge_asleep()` previously kept its tween only in a local variable. `wake()` reset `_nudge`, but the old tween wrote it again on the next frame and continued mixing sleepy poses into wake/flight. The reaction is now owned and cancelled when another action takes over. Wake retains the current painted frame for its existing blend rather than snapping to a new pose.

No asset, layout, gameplay, level, save or economy changes. Other reported issues #233–236 are outside this patch. Open issues were checked; no duplicate animation issue was found.

## Verification
- `godot --headless --path PerchSort res://tests/run_bird_animation.tscn`: four original assertions failed on unchanged production BirdView; all eight assertions pass after the fix. Checks cover flight ownership/completion count, wake interruption, normal cheer, flight interruption and normal sleep reaction completion. This regression is included in `scripts/run_tests.sh`.
- `GODOT=<Godot 4.7.2> bash scripts/run_tests.sh`: naming self-tests, naming guard, painted-word guard, smoke and all 210 saved solutions passed. The additional focused scene also passed independently.
- `git diff --check`: passed.
- Graphical OpenGL run launched through the cloud desktop, `--level=4 --frames=<local>/entrance --step=0.1 --wait=6 --bird-log` and `--preview=popups:win_three_stars --frames=<local>/win --step=0.1 --wait=6 --bird-log`. Sixty frames per sequence. Inspected progression from flying birds to landed birds and settled win popup. Post-fix entrance log reports 4283 ms, within the documented 3.5–4.5 second interval; capture overhead makes this illustrative, not a performance benchmark.
- Local evidence is in sibling `game-animation-evidence/`: `regression-before.log`, `regression-after.log`, `suite.log`, `entrance/`, `win/`, logs and frame detail sheet. No evidence images are committed.

## Boundaries
This is a focused animation fix, not acceptance of every animation in all 210 levels or every mobile device. Frame sequences are real graphical runtime captures, not proof of smooth real-device frame pacing. The entrance capture reports two ObjectDB instances / one resource at process exit; this is not claimed fixed. Read-only broader QA and difficulty findings remain separate. No merge or deployment was performed.
