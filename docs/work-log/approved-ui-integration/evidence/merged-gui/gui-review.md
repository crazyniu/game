# Integrated graphical review

Reviewed on 2026-11-11 in the merged cloud worktree `/workspace/scratch/2bd695cff14d/game-approved-ui-integration`, based on commit `5744542795b2065a9c23cf90dc78c20d5686f691`. This report is engineering verification, not a claim of user visual acceptance or permission to merge/release.

## Result

The corrected merged UI passes the graphical feature checks and exercised navigation loops. Final graphical suites: Draw/Purchase **180/180**, Chest **72/72**, Map/Aviary **28/28**. The initial map chick obstruction and Chest full-progress spill were corrected by their owners and visually rechecked. The Draw suite needed a test-wait correction, not a production behavior change.

A fresh, quiet-process tap-to-playable run passed all three paths, but the initial cold Home sample under concurrent work was 4609 ms and missed the 4500 ms ceiling. Both results remain in evidence. Test teardown resource diagnostics also remain; this is not a clean-resource-lifecycle claim.

No production files were edited, no commits or pushes made by this reviewer, and no real payment or personal save was used. The integration owner separately reports full repository/headless checks, including Home 97 and all 100 trail positions. Those are not reclassified as native GUI checks here.

## Environment and capture truth

- Godot `/workspace/scratch/2bd695cff14d/godot472/Godot_v4.7.2-stable_linux.x86_64`, official `ed1daf0bf`.
- Real cloud desktop, OpenGL Compatibility, Mesa llvmpipe. The shell's Xvfb socket limitation was not bypassed. Launches used the supported desktop terminal.
- Production settings unchanged. Native portrait viewport captures are **456×812**. The desktop is 1364×1024; the actual application window includes horizontal surrounding space.
- `render-target.gd` is a QA-only launcher of the canonical `main.tscn`. It sets the root viewport content scale to 1080×1920, VIEWPORT/KEEP, and runs existing previews. Its PNGs are genuine **1080×1920 render-target output**, not enlarged 456×812 images and not a 1080p physical-device/window test. The Map/Aviary suite uses the same rendering-target approach.
- Timing used the ordinary default-window launcher, not this high-resolution capture mode.
- Fake profile and isolated XDG data/config/cache directories are under this evidence directory. Manual live fixture started with 1280 coins, 93 stars, current level 32 and all species met. It never accessed a real save.
- Original unrelated game window 41943043 remained intact. The review's game was closed and terminal 29393412 returned to its prompt.

## Reproduction

All paths below are relative to `build/approved-ui-integration/gui/`. Launch these scripts from the cloud desktop terminal; each includes the absolute engine/worktree paths, isolated data locations, and complete arguments.

1. `bash run-graphical-tests.sh`: initial complete entry timing, Chest, Draw/Purchase and Map/Aviary suites. Preserved original failures.
2. `bash run-captures.sh`: 28 Chinese/English native captures, three 24-frame motion sequences, and additional 1080×1920 render-target captures.
3. `bash run-entry-repeat.sh`: fresh process after the integration owner's concurrent Godot work ended.
4. `bash run-live.sh`: canonical main scene with the fake profile for native-pointer navigation.
5. `bash run-corrections.sh`: corrected map suite, representative trail positions, map recaptures; also the intermediate Draw wait fix.
6. `bash run-draw-verified.sh`: final WeakRef-based wait test, 180 passing assertions without the intermediate freed-lambda diagnostic.
7. `bash run-chest-layout.sh`: final Chest 72 assertions; refreshed Chinese/English half, ready, multiple and reward states; refreshed high-resolution Chest images.
8. `python3 assemble-comparisons.py`: eight labeled side-by-side derived comparisons, with unchanged original images retained separately.

Canonical graphical command pattern:

`Godot_v4.7.2-stable_linux.x86_64 --path <worktree>/PerchSort --audio-driver Dummy --rendering-driver opengl3 --resolution 456x812 -s res://tests/run_draw_purchase.gd`

Unexecuted diagnostic drafts `check-draw-return.gd` and `run-supplement.sh` are not verification evidence.

## Entry timing: complete contract, not internal timer

`check_entry_timing.gd` measures from an engine-dispatched input event to interactive BoardView, completed leaf wipe and no blocking popup. It does not measure native OS pointer-delivery latency. The internal `ENTRANCE` timer omits the first leaf-wipe phase and scene creation and must not independently be judged against the 3.5–4.5 second total contract.

| Run | Home Play | Daily | Chest Play | Result |
| --- | ---: | ---: | ---: | --- |
| Initial OpenGL, concurrent integration work | 4609 ms | 3781 ms | 3789 ms | Home failed ceiling by 109 ms |
| Fresh OpenGL after other Godot work ended | 4413 ms | 3810 ms | 3818 ms | All pass |

Raw logs: `entry-timing.log`, `entry-timing-repeat.log`. No entrance animation code was changed. These samples establish a passing quiet run and a load-sensitive first sample, not universal performance across devices or loads.

## Native-pointer cross-page checks

Observed through the supported desktop mouse interface, not inferred solely from signals:

- **Draw:** Home gift → available draw. Double-click produced one animation and one +400 result. Confirm restored exhausted draw with all mystery icons present. Clicking disabled Draw did nothing; close returned to unobscured Home. Balance moved 1280 → 1680 once.
- **Chest / Purchase:** Home chest → 3 ready chests and 30/30. Coin plus → all six packages. Selecting ¥30 displayed an explicit simulated-purchase modal for +3600; confirm did not add money or charge. Closing purchase returned to the same Chest with the same balance and ready count. Double-clicking Open produced one reward; confirm returned Home, ready count 3 → 2 and balance 1680 → 1980 once. This full native-pointer loop preceded the final cosmetic progress-inset/painted-label adjustment. The final Chest suite and captures revalidated the adjusted code; a second native-pointer purchase loop was not repeated after that cosmetic change.
- **Settings / language:** retained Music, Sound, Vibration and Language rows plus Help. Native Chinese → English switch updated controls and Home Play without missing art; close removed the overlay. Chinese/English static screenshots cover the other updated pages.
- **Shop:** Home backpack → existing booster shop → close → Home. No manual purchase.
- **Aviary:** Home nest → list → Berry detail with six choices → Back to list → scroll to final birds → close Home. Last rows remained accessible and complete; no manual outfit purchase. Graphical suite separately covers a fake-profile outfit purchase/equip and insufficient-funds state.
- **Daily:** Home trophy → interactive board → Settings → Home. No level completion was attempted in this manual loop.
- **Map:** fresh live process after the map fix → Home map → selected Snow Forest → fixed Play → current-level-32 trail with the bird fully visible below its center node → Back to the same selected world → Back Home.

Thus all six Home entry categories were exercised in the merged tree. No stuck dimmer or duplicated reward was observed. This is not exhaustive multi-touch, long-press, phone, or every-page focus testing. Earlier component-level Home press/release tests are separate evidence and are not relabeled as merged-tree tests.

## Visual findings and fixes

### Home

Both languages preserve the bright approved jungle, title, branch and all four complete birds. Six independent backings separate the icons from the grass; badges remain above their frames. Their centers are low-saturation translucent dark green, **not transparent hollow centers**. The original icon artwork and original click regions remain unchanged.

`home-pixel-scope.json` compares the merged default captures with the previous bright-jungle foreground captures: **zero changed pixels outside the six backing rectangles in both languages**. The comparison therefore supports unchanged foreground geometry/background rather than implying the whole previous capture was a newly approved baseline.

### Map and Aviary

Initial level-8 trail capture exposed the chick's head behind the next lock despite passing structural checks. The map owner moved the existing bird into free space and corrected current-node draw order. Final `map-aviary-corrected.log` has 28 passes. `trail-clearance/level_8.png`, `level_12.png`, and `level_25.png` show the complete bird in right, below-center and left space respectively, without covering lock discs, numbers or stars. The corrected default Chinese/English world-page captures and native level-32 loop agree.

Aviary list/details preserve an open two-column/perch layout, mystery silhouettes, original species, six equal outfit choices and visible prices. Hero poses and outfit birds animate. List clipping at the viewport boundary is harder than the reference's decorative foliage fade; it did not prevent scrolling to or using the last rows. This is a remaining polish difference, not hidden by the pass count.

### Draw and Purchase

Nine-tile draw composition, five attempt stars, center chest, painted values and main button match the accepted design structure. Purchase shows six complete packages and equal price actions: 600/¥6, 2000/¥18, 3600/¥30, 9000/¥68, 20000/¥128, 33000/¥198. Runtime uses the approved forest direction in place of the old beach reference background. These are simulations, not a connected store or a real payment acceptance test.

Initial Draw 179-check run failed two assertions immediately after closing reward because a fixed 0.2-second wait completed before the graphical tween callback. The owner changed only the test to a bounded actual-node-exit wait and added an exit assertion. The intermediate wait emitted a freed-node lambda diagnostic; final typed WeakRef test removes that issue. Final `draw-purchase-verified.log`: 180 checks, 0 failures. Original and intermediate logs remain.

### Chest

Use the verified current two-panel reference `chest-reference/star_chest_and_rewards_review.png`, SHA-256 `b28d9054f02c7a3df58667aafdd24f064a958bcc69977ee3339f29f2942b13e6`, not the superseded enclosed English card.

The final owner change keeps the original green art and shared track but gives the Chest fill its own inset and rounded alpha clip. Full 30-star and 92-star cases now retain a continuous visible gold rim. Original Chinese/English action art was enlarged inside the unchanged buttons; both remain centered and unclipped, including disabled Open. Ready badge, title, balance, chest and two actions are complete. `chest-layout.log`: 72 passes; final raw states in `chest-layout/`.

The rewards remain card-free objects over the open chest with painted amounts and one confirmation action. The supplied reward preview has +300, firefly ×2 and bell ×1, while the reference illustrates three different boosters. This is fixture-dependent reward content; actual two-/three-/zero-booster layouts are separately captured and tested, not forced to match the mockup's reward selection.

## Design comparisons

`comparisons/` contains Home contrast, Map, Aviary list, Aviary detail, Draw, Purchase, Chest progress and Chest reward. `sources.json` identifies both original inputs and any reference half-crop. Every comparison is clearly labeled **derived, uniformly rescaled**; raw captured PNGs are separate. The standard comparison script's Xvfb route was unavailable, so equivalent side-by-side composition was used without changing runtime pixels or reference assets.

The comparisons show matched structure and style, while keeping the game's approved existing bird/art identities. Differences in drawing details, painted type, fake progress/rewards, the forest substitution and the Aviary clipping boundary are not pixel-equivalence claims. No `art/approved_runtime` acceptance baseline was written.

## Animation and audio evidence

Home birds, Berry detail and Draw each have 24 frames requested at 0.25-second intervals over 6 seconds; all 24 PNGs are distinct. `motion-analysis.json` records dimensions/counts. `motion-contact-sheet.jpg` is a derived sample from frames 0, 10 and 20. Home and Aviary show genuinely different drawn poses, including eyelids, wings and turns, rather than a mirrored still. Native observation also showed continuous motion. Draw sequence includes highlight movement and result transition; final result return was checked manually and by the corrected suite.

Map/Aviary graphical tests verify live breathing, distinct pose textures, menu-music player activation and mute/unmute. Chest also verifies mute stops music. All launches use the Dummy audio driver: **audio service state was tested, audible sound quality was not heard or verified**. Still images alone do not establish animation or music acceptance.

## Diagnostics and boundaries

- Driver reports unsupported VSync change. No production graphics setting was altered to hide it.
- Final Draw and Chest tests each report 4 ObjectDB instances and 2 resources still in use on process exit. The representative-trail helper reports 2 ObjectDB instances and 1 resource on exit. These remain open teardown diagnostics, not a claim that cleanup is error-free.
- Initial timing, original map overlap and original/intermediate Draw failures are retained; corrected evidence is clearly named separately.
- Tests are llvmpipe cloud-desktop checks, not mobile/GPU qualification, store integration, frame-rate profiling, or an exhaustive every-state/every-language/manual-input matrix.
- The final source hash manifest distinguishes reviewed corrected sources from the initial `source-hashes.txt` snapshot.

## Delivered captures

Nine unchanged 1080×1920 render-target PNGs were saved successfully for the parent to attach: Home Chinese/English, Map cards, Aviary list/detail, Chest ready/reward, Draw and Purchase. `library-upload-result.json` retains exact paths and IDs with per-item success; `capture-manifest.json` records original PNG dimensions and SHA-256 values. Native 456×812 Chinese/English PNGs remain alongside the report for window-size review. No failed or pre-fix image was substituted as the final deliverable.
