# T54C report — branch bird idle sequences

## Outcome

Implemented the latest `docs/work-log/integration-contract/spec-animation.md`, including its binding exact branch-action table, in the canonical runtime. Branch: `mac-t54c/t54`. The supplied contract tip was used, and both requested `origin/t54-contract` updates were merged. No peer implementation branch or main was merged or modified.

**Final local verification: EditMode 304/304 passed; PlayMode 26/26 passed; zero failures or skips.** PlayMode includes the 21 retained cases and five new T54C cases. The real twenty-second level-one capture passed its branch-scheduling checks.

**Art boundary:** this checkout contains no `<species>_<anim>_NN.png` sequence resources. Production captures therefore demonstrate the explicitly approved missing-art fallback, not completed drawn animation. Synthetic frame identities are used only in tests and are never exported as artwork or visual evidence.

## Owned changes

- `PerchSort/Assets/Scripts/Game/BirdView.cs`
  - Probes sequence resources through `GameArt.Optional`, starting at `_01` and stopping at the first missing frame. Sequence arrays are shared per GameArt/species with weak-key lifetime; probing/allocation is not repeated per rendered frame. An isolated still is not accepted as a sequence.
  - Uses the exact idle weights: blink 30, look 14, preen 10, flap 8, hop 6, fluff/stretch/yawn 4 each. Missing/incomplete required sequences are removed from the draw; remaining weights are renormalized. Both authored turn directions must exist before enabling look. No immediate action repetition.
  - Starts the next random action 1.0–3.0 seconds after the previous action finishes. Breathing uses independent phase and a 2.4–3.2 second period, y scale 1.000–1.020 and x scale 1.000–0.992. The logical root/Place never participates in idle motion.
  - Implements the table's blink return/double-blink, turn holds/reverse playback, preen hold, double flap loop, hop trajectory and landing squash, and the three still-pose crossfades. Idle rotation, mirrored facing, shiver, shuffle and stretch/fluff scaling have been removed. Only breathing and the specified hop landing squash change idle scale.
  - Selection, flight, sleep and egg guards remain. Switching `GameMotion.Instant` on cancels an active idle pose and resets its visual transform. Selection/cancellation prevents an obsolete idle blend from continuing.
  - `LookAt` uses authored turn directions rather than negative x scale. The existing public invalid-feedback entry now plays blink rather than shaking. Quirk/celebration entry points consume drawn frames when present, with a plain existing-pose fallback. Positional gameplay Hop keeps its destination/callback and uses drawings when available; missing drawings produce a smooth transfer rather than a fake hopping still.
- `PerchSort/Assets/Scripts/Game/BirdIdleGroup.cs`
  - Retains at most three simultaneous idle actions per branch and the same-action neighbour start separation of 0.6 seconds. Removes the obsolete mirror-direction neighbour helper.
- `PerchSort/Assets/Scripts/Game/BirdOverlay.cs`
  - Blends attachment positions with the existing still crossfades. Existing calibrated still poses remain; new same-canvas sequences inherit the species' idle calibration rather than legacy canvas anchors.
- `PerchSort/Assets/Tests/PlayMode/BirdIdleTests.cs` and its Unity metadata
  - Five isolated cases: uninterrupted breathing/logical seats/Instant, missing-sequence draw removal and still behavior, first-gap/lone-frame probing, real timed playback of synthetic frame identities with cadence/holds/landing/cancellation, and twenty seconds of the real level-one branch with branch limits and neighbour separation.
  - No retained test, assembly definition, Core rule, profile/save identifier, artwork, or level data was edited.

`Enter` and `FlyCore` are unchanged by this branch. Preserve T54B's separately approved entry-duration/easing commit `6603e2a` at integration; it has not been cherry-picked here.

## Timed visual evidence

- `docs/work-log/bird-idle/evidence/level_one_idle_twenty_seconds.jpg` — **1600 × 5190**, 81 fixed-schedule samples, 0.00 through 20.00 seconds, spaced by 0.25 seconds. Each tile shows the complete four-bird left branch from level one.
- `docs/work-log/bird-idle/evidence/samples.csv` — 324 per-bird observations, including target/actual elapsed time, frame, action, absolute Unity action-start time, body y scale, rotation and visual position.
- Raw 1080 × 1920 PNGs remain local under ignored `build/timed-idle/idle_000.png` through `idle_080.png`.

These came from real Unity PlayMode, real resources, normal motion and a camera RenderTexture, not edit-mode instant capture or composited fake birds. The profile is nonpersistent and the random seed is fixed for reproducibility. Actual last sample: **20.0005 s**. Maximum sampling lateness: **0.0037 s**. The first-level tutorial hand is visible and partly overlaps the rightmost bird's lower body; it was not hidden or edited for the evidence.

### What moves when

All four birds breathe independently throughout. At the 1.75-second sample, the first yellow bird is entering stretch while the third enters fluff; the second yellow bird enters stretch by 2.50 seconds, and the blue bird enters fluff by 3.25 seconds. Subsequent unsynchronized fluff/stretch/yawn crossfades continue through twenty seconds. Each bird starts six actions in the sampled run. At most three are active simultaneously. Sampled body rotations are all zero and sampled visual perch positions stay fixed; y breathing spans approximately 1.000–1.020.

No blink, turn, preen, flap or hop drawing sequence appears in these production images because those resources have not arrived. The sheet must not be represented as proof of the future sequence art.

## Verification and exact commands

All Unity invocations were serialized with `flock /tmp/unity.lock`, after the user closed their Editor. The primary checkout was not edited and its Editor was not killed.

Full verification, run again after removing unused house-only `peek` probing:

```bash
python3 ci/check_current_game_names.py && flock /tmp/unity.lock bash scripts/run_tests.sh
```

- Naming guard: **PASS, zero violations**.
- Sole-scene build/compilation: **PASS** through `PerchSort.Editor.GameSceneBuilder.Build` in the runner.
- EditMode: **304/304 passed, 0 failed, 0 skipped**.
- PlayMode: **26/26 passed, 0 failed, 0 skipped**.
- Portable result XML: `docs/work-log/bird-idle/evidence/editmode-results.xml` and `docs/work-log/bird-idle/evidence/playmode-results.xml`.
- Local complete runner logs/XML remain under ignored `build/`.

Graphics-enabled focused capture command, executed from this worktree:

```bash
flock /tmp/unity.lock env T54C_EVIDENCE_DIR=/Users/apple/orca/workspaces/game/t54-idle/build/timed-idle /Applications/Unity/Hub/Editor/6000.6.7f1/Unity.app/Contents/MacOS/Unity -batchmode -projectPath /Users/apple/orca/workspaces/game/t54-idle/PerchSort -runTests -testPlatform PlayMode -testFilter PerchSort.PlayModeTests.BirdIdleTests -testResults /Users/apple/orca/workspaces/game/t54-idle/build/idle-focused-results.xml -logFile /Users/apple/orca/workspaces/game/t54-idle/build/idle-focused.log
```

Earlier attempts and corrections:

1. An obsolete queued full-test attempt was cancelled when the exact-table contract arrived; no result is claimed for it.
2. The first focused launch found a compiler ambiguity between the game's SafeArea and Unity UI's SafeArea in the new capture helper. Fully qualifying `PerchSort.Game.SafeArea` fixed it.
3. The next focused graphics run passed **4/5**, including the complete twenty-second capture. The new synthetic playback case failed an exact floating-point visual-body position comparison, printing `(100.00, -190.00)` for both sides. That new assertion now measures visual alignment within 0.001 design pixel; strict logical-seat equality remains. The complete final suites pass all five new cases. No existing assertion or timing threshold was changed.
4. A complete suite run passed 304/304 and 26/26; the subsequent unused-resource-probe cleanup was followed by another complete run with the same passing counts.

The reported initial static/rare idle problem is user-supplied evidence, supported by the old source's 1.5–5 second starts and transformed single stills. No pre-fix Unity reproduction is claimed: the user's Editor was open during initial implementation.

## Limits and integration notes

- **Missing prerequisite:** T54G's real sequence art, for every species in this checkout. The loader and fake-frame runtime tests are complete; phone-size readability, drawn continuity, feet registration and species consistency of undelivered art are unverified. Integrate the art and repeat timed acceptance against actual sequences. T54C did not author, transform, copy into place or commit production art.
- Family bow/fluff calibration for newly drawn head/body positions needs visual review when those resources exist. The current default assumes the promised idle-compatible canvas/baseline, not measured per-frame head landmarks.
- Blink uses five pose states at transition times 0, 1/12, 2/12, 3/12 and 4/12 seconds; the final open state immediately becomes the identical resting pose. This honors the rounded 0.33-second table without adding another full-frame hold. Hop's 0.5 seconds of drawings plus 0.10-second landing squash total 0.60 seconds. Preen uses 5/8 = 0.625 seconds before its 0.4-second hold; flap uses 12/14 seconds for two loops.
- Flap's three-pixel secondary rise is on the visual child and leaves the logical seat fixed. Literal immobile drawn feet plus independently rising torso cannot be established from an unsplit whole-bird sprite; no separately segmented part animation is claimed.
- Verification covers real PlayMode playback/camera capture and retained suites, not manual device/phone or integrated T54B/T54F acceptance.
- Unity scene generation/import left **uncommitted, excluded-from-branch** changes to `Assets/Scenes/Game.unity` and seven resource metadata files: `chick_fluff.png.meta`, `mama_bow.png.meta`, `egg_idle.png.meta`, `egg_wobble_l.png.meta`, `egg_wobble_r.png.meta`, `egg_crack.png.meta`, `egg_shell.png.meta`. They were not staged, reverted blindly or treated as owned edits. A pre-Unity hash manifest for 33 tracked scene/settings/package files is local at `build/pre-unity-manifest.json`; among those, only the generated scene changed.

## Requested contract change

None requested. Missing-art and literal feet/torso-registration boundaries are recorded above rather than hidden behind procedural still transformations. Main and all peer branches remain untouched.
