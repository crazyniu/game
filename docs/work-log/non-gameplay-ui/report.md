# Non-gameplay UI integration report

Status: implemented and locally verified. No commit, upload, push, authentication, merge or deployment was performed in this phase. The working tree remains on detached base `5744542795b2065a9c23cf90dc78c20d5686f691`. User runtime acceptance is separate from this implementation review; no approved-runtime baseline was automatically replaced.

## Scope and ownership

Integrated bounded patches for the three Shop categories, item/outfit/nest and simulated-currency purchase dialogs, Draw results, seven dialog families (win, level card, ordinary rewards, new friend, booster unlock, world complete and world unlock), the requested Chest progress/lettering correction, and consistent opt-in Close/Back controls. `inputs.json` records patch hashes. Shared `popups.gd` and `shop_view.gd` were merged by owned functions, not overwritten wholesale.

All Close controls in the scoped non-gameplay surfaces use the original coin-shop image and game-space bounds through `MenuNavigation`. Back retains the accepted matching orange/white-arrow image. Both source PNGs remain byte-identical. Buttons retain callbacks, press tint and fixed dimensions. Settings, shared Modal, tutorial/Help and in-game goal information were deliberately not globally reskinned.

Chest now uses separately generated and reviewed Home-family Open/OK lettering, the original Home Play source and digits, and a green empty track with bright green nine-patch fill inset by 9 pixels. Nineteen retained source images match their pre-change hashes. Original accepted background, chest, platform, button and Home pixels were not redrawn. Independent material and runtime evidence is in `../chest-style/`.

## Issues found and fixed during integration

1. The purchase quantity caret metrics initially requested a nonexistent numeric style. Corrected all five calls using `UiText.number_style(Color.WHITE)` and exercised genuine focus, editing, caret placement, selection and blur in both languages. The final 86 checks pass in headless and graphical runs. Earlier failing pointer diagnostics remain in the component evidence and are explicitly superseded.
2. Preview-only chapter pagination requested a missing brown Next word. Both sibling pagination surfaces now reuse existing painted words and equal product-button geometry. Four checks were added; final navigation count is 107. The initial error log and an intermediate test-parser mistake remain under `build/non-gameplay-integration/`, not represented as passes.
3. Native Map review exposed the current level bird/marker rendering over the level card, even though earlier structural tests passed. Removed the crossing positive z-index and moved the current marker to the last local map sibling instead. Coordinates, bird motion, hit targets and gameplay are unchanged. Added three regression assertions. Fresh graphical evidence and native pointer reopening of level 32 show the card unobscured, with Close restoring the same trail and Back restoring the same selected world. The initial failure screenshot is retained at `evidence/native/native_09.png`; the corrected card is `evidence/map-level-card.png` and `evidence/native-return/native_01.png`.
4. Four copied evidence launchers had executable names rejected by the repository naming guard. Changed their paths to the existing engine alias. The original failed guard logs are retained; the final aggregate guard is clean.

## Final validation

Engine: Godot 4.7.2 standard, GL Compatibility. Headless execution used isolated writable XDG directories. Graphical runs used the authorized cloud desktop, OpenGL/Mesa llvmpipe, and Dummy audio. No user machine was selected.

The final `bash scripts/run_tests.sh` run after the last production change passed the naming self-tests, naming guard, painted-word guard, smoke checks and all 100 fixed-level solution replays. The final naming result was 128 tracked source files and zero violations. Painted-word count is 11 remaining font-rendered call sites, zero problems. Those remaining protected/legacy call sites were not expanded.

Focused merged-tree results:

| Surface | Headless | Graphical |
| --- | --- | --- |
| Menu navigation | 107 checks, 0 failures | 107 checks, 0 failures |
| Chest | 149 passing assertions | 149 passing assertions |
| Existing Draw/purchase contracts | 180 checks, 0 failures | 180 checks, 0 failures |
| Draw result | 353 checks, 0 failures | 353 checks, 0 failures |
| Purchase dialogs including focus/edit | 86 checks, 0 failures | 86 checks, 0 failures |
| Shop cards | 31 passing assertions | 9 input/scroll assertions |
| Family dialogs | 372 checks, 0 failures | 372 checks, 0 failures |
| Map/Aviary including modal ordering | 31 passing assertions | 31 passing assertions |
| Home protection | 97 checks, 0 failures | Unchanged bilingual pixel baseline |

The 100-current-level map clearance check also passes after the ordering fix. The final delivery suite passed again after adding the evidence launchers. Final logs are copied into `evidence/`; exact executable invocations are retained in the QA launcher scripts. A representative focused command is `godot472/godot --headless --path PerchSort --audio-driver Dummy -s res://tests/run_menu_navigation.gd`, with the engine path and isolated XDG environment set as shown in the launchers. Full aggregate invocation used `GODOT=/workspace/scratch/2bd695cff14d/godot472/godot bash scripts/run_tests.sh`.

Final quiet graphical tap-to-playable samples are Home **4369 ms**, Daily **3792 ms**, and Chest **3793 ms**, all within the required 3500–4500 ms. The engine's internal entrance numbers are not the full tap-to-playable metric. These are measured desktop samples, not a guarantee on every device or under load. The preceding phase's cold/load 4609 ms failure remains in that phase's original report, not erased by these samples.

## Visual and motion evidence

`capture-manifest.json` records path, original dimensions and SHA-256 for 245 raw PNGs, including retained diagnostic frames. Final bilingual captures cover the Shop confirmation states, currency preview, the seven dialog families, chapter pagination, Home and Settings. Chest, Draw, Shop and Map/Aviary graphical tests also emit their own state and motion captures. Native engine frames are 456×812. The 1080×1920 images are actual QA render targets created by a test-only viewport wrapper, not enlarged source screenshots or physical-device screenshots. Production viewport settings were not changed.

Copied plain-text logs normalize trailing blank lines for the whitespace guard; original raw logs remain in the ignored build directory. The full bilingual contact sheets `evidence/zh-states.jpg` and `evidence/en-states.jpg` were visually inspected for missing words, collisions, clipped actions and inconsistent geometry. They are labeled review thumbnails; original PNGs retain their dimensions. Component design comparisons and the Chest/Home lettering comparison remain in each component work-log. The corrected Map card was separately inspected at full resolution.

Three six-second canonical animation sequences contain 24 frames each for win celebration, new friend and world unlock. All 24 frames in each sequence differ. Representative poses were inspected in `evidence/motion-contact.jpg`: the friend turns using distinct drawn poses, the win reveal progresses and birds move, and the gate opens. Map/Aviary logs also record continuous scale changes, distinct drawn poses and music/mute state. Draw graphical checks verify animated completion and continued reward-ray rotation. Stills alone were not used as animation proof.

## Native pointer flows

The final integration was operated with actual cloud-desktop pointer and keyboard input, separate from engine-dispatched input tests. Fake isolated profiles were used.

- Shop: changed quantity to 3 with actual focus, Select All and typing; price changed from 30 to 90. Double Buy produced one purchase, increasing inventory by 3 and reducing 1280 to 1190 once. Switched tabs, wheel-scrolled outfits with tabs fixed, cancelled outfit and nest confirmations without charges, and returned Home.
- Map: opened Snow Forest, opened current level 32, confirmed the repaired modal layering, closed to the same trail, returned to the same selected world and Home.
- Draw: double-clicked one remaining attempt, observed one animated +80 result, double-confirmed once, saw the original mystery sprites on the exhausted board, verified the exhausted button ignored input, and returned Home at 1360 from 1280.
- Chest and currency: opened the currency page from Chest, confirmed the ¥30 simulation, and verified no real charge or grant and an unchanged 1360 balance. Currency Close restored the same Chest. Double Open produced one reward (+300 plus three boosters), leaving 1660 coins and two ready chests. Reward confirmation followed the existing route to Home.
- Aviary: opened the collection, entered a bird detail via its name, used Back to the collection and Close to Home. Final Home still showed 1660 coins and the two-chest badge.

`evidence/gui-live.log` and `evidence/gui-live-return.log` pair native captures with timestamp and balance records. Overlay pages can leave the underlying controller `screen` field as Home; raw frames establish the visible page. No real payment service was invoked. Only the QA window was closed afterward; the original game window was preserved.

## Frozen scope and limitations

Six frozen source records (including the exact Settings function) and two navigation PNG hashes pass. Core, Meta, levels, main controller, BoardView, tutorial and shared Modal have no changes against the base. The previous accepted Home frames match pixel-for-pixel in both languages. The Settings static panel crop also matches exactly in both languages; the full screenshot contains randomized live board birds outside that panel, so full-frame equality is not claimed. See `evidence/frozen-pixel-check.json`.

Forty-eight retained Draw/purchase images and nineteen Home/Chest source images pass their hash checks. `source-manifest.json` records final production source/image hashes. No final visual baseline was changed without user approval.

Known diagnostics are disclosed rather than hidden by exit codes. Final graphical navigation and Draw-result tests each report two ObjectDB instances and one resource still in use at shutdown; the existing Draw/purchase harness reports four and two. These occur after zero-failure summaries, without active-flow script errors, and remain unresolved cleanup diagnostics. Headless custom-sampler diagnostics are retained separately. The host cannot change V-Sync and warns accordingly. Final purchase, Chest, family, Shop and Map graphical tests have no script-error diagnostics. Native return logs have no error diagnostics.

Audio player and mute state were checked, but Dummy audio means human listening was not tested. Physical-device touch, GPU performance, real payment processing, release packaging and exhaustive manual play of all levels were not tested. The 100-level automated rules regression passed; gameplay implementation was not modified.

## Local delivery

The complete work remains in the cloud worktree. Required evidence is in this directory and component work-logs; all raw final and diagnostic samples remain under `build/non-gameplay-integration/`. A complete binary diff is backed up separately under `/workspace/shared/game-integration/non-gameplay-ui.patch`; its SHA and file inventory are kept alongside it to avoid a self-referential manifest. The earlier approved-UI patch remains untouched. This is a local backup, not a commit or publication.
