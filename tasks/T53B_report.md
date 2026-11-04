# T53B — top birdhouses, HUD and boosters

## Delivery and scope

- Branch: `mac-ui-hud/t53`, created from the supplied `t53-ui-fidelity` tip `5ba155e820b37a157b1f8a494af9c607b0f05d9e` (runtime base `63d4782`). No merge or main-branch changes.
- Production changes: `PerchSort/Assets/Scripts/Game/BirdhouseView.cs`, `HudView.cs`, `GameButton.cs` only. `GameUI.cs` and `ViewModel.cs` did not require changes.
- Existing handlers, accessible object names, booster unlocks/context policy, authored levels, rules, inventory/save semantics and animation durations remain intact. No original artwork or resource assets changed.
- Status: reachable HUD/layout changes implemented and visually exercised. **Not an exact approved-image match**: asset/content/undo-contract limits below remain for coordinator acceptance.

## D3 — houses

- One to three goals now hang from individual substantial ropes with leaf sprigs; no full-width beam. A single goal uses the measured master house region `(400,152,246,232)`; two/three goals retain all authored species, enlarged to that size below the header. Three suspension points clear both the coin pill and gear.
- Four or more goals retain the world-specific beam and short suspension ropes. House shared-canvas bounds fit the measured visible alpha region, avoiding the previous overlapping seven-house roofs. Rope dimensions compensate for transparent padding instead of displaying a 4-pixel thread.
- Body, roof, doorway mask, hinged door leaf, portrait, arrival/gift coordinates and open-house part share the same rectangular canvas. Seven-house portraits fill their doorway rather than appearing as tiny faces.
- Completion is a green disc with a white check, composed from existing button/check sprites. Existing open/close, peek, arrival and check-pop motions retain their durations.

## D4 — HUD and boosters

- Few-house coin/gear placement now uses the master screen's measured header, rather than the many-house header. Coin art compensates for alpha padding; amount text uses the measured text box, a larger dark-red numeral and existing auto-sizing.
- Gear stays full-color during temporary non-modal input blocking; actual modal disable still fades it. Existing input blocking is not bypassed.
- Red inventory badges display the actual numeric stock, including `0`; empty stock no longer displays `+`. Three-/four-control positions and badge sizes use the approved measured rows.
- Fresh level 1 has only a small faded undo control (64-pixel canvas). Explicit unlocked/test-stock progression still exposes its usable boosters: this preserves the retained developer-stock acceptance contract.
- The fourth context slot retains existing mechanic selection. A crow fixture shows Scarecrow; ordinary levels do not acquire a crow button merely to resemble the concept image.

## Visual evidence

Four serialized capture passes were made: baseline, first implementation, refinement, and final. Actual scene/UI rendering was inspected beside the approved images, including house overlap, coin/rope intersections, numeric badges, first-level controls and home-check state.

Portable review JPEGs are committed under **`tasks/evidence/T53B/`**, quality 80, maximum height 1080 pixels (full sheets 1195×1080; header sheets 1440×368; booster strip 1440×207):

| Evidence | Purpose |
| --- | --- |
| `final_level_k3_full.jpg`, `final_level_k3_header.jpg` | Approved master versus actual authored level 5: enlarged independent rope-hung goals, no beam, readable header. |
| `final_level_k7_full.jpg`, `final_level_k7_header.jpg` | Approved seven-house screen versus actual authored level 40: spaced houses/portraits, short ropes, retained beam. |
| `final_level1_start_full.jpg` | Fresh authored level 1, minimal undo-only row. Reference is the supplied master; there is no separate approved L1 image in this comparison. |
| `single_goal_stock3_full.jpg`, `single_goal_stock3_header.jpg` | Explicit nonpersistent single-species fixture, 240 coins and stock 3: proves the single centered large house path. Not an authored level replacement. |
| `crow_stock3_full.jpg`, `crow_stock3_boosters.jpg` | Existing crow/twig fixture with explicit stock 3: proves real numeric 3 badges and mechanic-gated fourth slot. |

Full-resolution captures remain in `build/screens/game/`. Full-resolution side-by-sides/baseline/iteration evidence remain in `build/t53b/`, including `before_*`, `iteration1_*`, `final_*`, `single_goal_stock3_*` and `crow_stock3_*`. Additional smoke frames: `build/t53b/single_goal_complete.png` and `crow_stock2.png` (actual Scarecrow consumption from 3 to 2).

The comparison script was outside the repository at `/tmp/t53b/compare.py`. A disposable `T53BSmoke.cs` under the permitted EditMode task prefix exercised a real nonpersistent fixture and reused `GameCapture` rendering; that source and its generated metadata were removed after capture. No permanent tests were added or existing tests edited.

## Verification — exact results

All Unity invocations were serialized with **`flock /tmp/unity.lock`**. No running Unity process was found before the first invocation; no user Editor was killed.

1. `python3 ci/check_current_game_names.py`
   - **PASS:** 567 tracked text files inspected; 0 violations.
2. `flock /tmp/unity.lock bash scripts/screenshots.sh` — four passes.
   - **PASS:** all 34 expected 1080×1920 frames produced on each successful pass. The initial attempt could not start because `flock` was unavailable; `brew install flock` resolved the prerequisite before Unity ran.
3. `flock /tmp/unity.lock bash scripts/run_tests.sh`
   - Scene build/compilation succeeded.
   - **EditMode: 304/304 passed, 0 failed, 0 skipped.** XML: `build/test-results.xml`.
   - **Initial PlayMode: 14/17 passed, 3 failed, 0 skipped.** XML: `build/playmode-results.xml`.
   - `TestModeShowsFortyClearedLevelsWithoutSavingSyntheticProgressOrStock`: initial L1 hiding conflicted with explicit test-stock accessibility. Corrected the HUD policy, without changing the test.
   - `AnimatedTransferLocksAirborneInputAndUndoCancelsCallbacks`: bird 6 idle displacement `2.925354` versus `<0.1`.
   - `TimedFlockArrivalsReachTheirGoalDoor`: nearest sampled distance `56.0198212` versus `<40`.
   - **Coordinator confirmed the latter two are pre-existing base failures fixed by `c074716`, which is not in this worker's base.** BirdView, idle/flight behavior and thresholds were not touched; that commit was not merged into this worker branch.
4. Corrected L1 developer-stock scenario recheck:

   ```bash
   flock /tmp/unity.lock /Applications/Unity/Hub/Editor/6000.6.7f1/Unity.app/Contents/MacOS/Unity -batchmode -nographics -projectPath /Users/apple/orca/workspaces/game/t53-hud/PerchSort -runTests -testPlatform PlayMode -testFilter PerchSort.PlayModeTests.GameplayTests.TestModeShowsFortyClearedLevelsWithoutSavingSyntheticProgressOrStock -testResults /Users/apple/orca/workspaces/game/t53-hud/build/t53b-stock-results.xml -logFile /Users/apple/orca/workspaces/game/t53-hud/build/t53b-stock-tests.log
   ```

   - **PASS: 1/1, 0 failed, 0 skipped.** Full suite was not rerun after this correction; do not present an inferred 15/17 as a measured full-run result.
5. Disposable single-goal/stocked-crow runtime smoke:

   ```bash
   flock /tmp/unity.lock /Applications/Unity/Hub/Editor/6000.6.7f1/Unity.app/Contents/MacOS/Unity -batchmode -projectPath /Users/apple/orca/workspaces/game/t53-hud/PerchSort -runTests -testPlatform EditMode -testFilter PerchSort.EditModeTests.T53BSmoke -testResults /Users/apple/orca/workspaces/game/t53-hud/build/t53b-smoke-results.xml -logFile /Users/apple/orca/workspaces/game/t53-hud/build/t53b-smoke.log -screen-width 1080 -screen-height 1920
   ```

   - **PASS: 1/1, 0 failed, 0 skipped.** Rendered single-goal/three-stock states, completed the single-species flock/check, used Scarecrow, and observed real remaining stock 2.

Scene-builder object-ID rewrites and seven automatic bird-overlay metadata expansions were compared with the pre-run byte snapshots and restored exactly; none is included in delivery. Settings/packages retain their pre-run state. Generated output stays in ignored `build/`.

Static captures and the instant-motion smoke are not timed visual animation/device acceptance. Required integrated Mac/GUI acceptance remains coordinator-owned; two pre-existing timed tests are not represented as passes.

## Remaining differences and requested contract change

1. **Authored L5 versus single-house concept:** the screenshot named `level_k3` loads authored level 5, which has sunny, sky and berry goals. The approved master depicts only one sunny house and a bird in flight. Hiding two real goals or changing level data is not authorized. The three enlarged rope-hung houses are a disclosed count adaptation, not an exact concept match. The extra single-goal fixture demonstrates the supported centered layout without changing production levels.
2. **Authored L40 versus approved K7 content:** level 40 has different seven species/order and growing/egg/sleep/day-night mechanics, not the approved crow arrangement. Its fourth booster is SpringRain under the retained policy; a separate real crow fixture proves Scarecrow presence. Homed check states depend on actual progress rather than being painted onto two concept positions. Matching those content details requires an approved capture fixture/content contract, not a HUD disguise.
3. **Wrong supplied colored-icon assets:** `PerchSort/Assets/Resources/Art/icon_branch_plus.png` is a white branch/sprout silhouette, not the brown branch/white plus in the approved bottom row (`ui_level_k3.png`, approximately x666–774/y1748–1856). `icon_scarecrow.png` is a white crow/red-X silhouette, not the dark crow/red-disc-X in the approved K7 fourth control (approximately x742–874/y1726–1858). These items remain asset-blocked: originals/resources were not recolored, replaced or generated. Request correctly colored production icons from the asset owner.
4. **House/portrait artwork differs:** supplied `house_body.png`, colored roofs and `Birds/*_face.png` do not reproduce the approved illustrated roof/body shape and peeking face/feet exactly. Geometry now fits measured bounds; it cannot recreate missing painted detail. Leaf sprigs reuse `leaf_badge`; the check uses an existing green disc with a gold rim rather than the concept's white-rimmed badge. Request matching art if pixel-level acceptance requires these differences removed.
5. **Undo stock semantics:** current Core undo is history-based, not a consumable booster and has no stock of 3. It remains correctly disabled without history and has no fabricated red stock badge. An approved undo-inventory/economy contract would be required to reproduce the concept's red undo `3`; this task did not change rules or invent counts.
6. **L1 test-mode exception:** fresh progression is undo-only; existing explicit unlocked/test-stock mode keeps boosters accessible, as required by the retained PlayMode test. Making all L1 entries hide those boosters would require coordinator resolution of that existing contract.
7. **Unowned board/scene differences:** branches, tree composition, level bird arrangements, backgrounds and popup art remain visibly different in the full sheets; these are not changed by T53B. No broad animation claim is made from a still frame.

No BirdView changes, weakened assertions, renamed persisted fields, alternate runtime identities, scene/resource edits, peer merges or main mutations are included.
