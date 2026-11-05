# T54H report — Settings and modal layering

## Delivery

- Branch: `mac-t54h/t54`, created first from the supplied contract tip `ecdab196e4b91ebb2ae519674c486ab9a47ab6dd`.
- Changed runtime files: `PerchSort/Assets/Scripts/Game/SettingsPanel.cs` and `PerchSort/Assets/Scripts/Game/PanelsView.cs` only.
- Added `PerchSort/Assets/Tests/PlayMode/ModalLayerTests.cs` and its Unity metadata, this report, and the requested JPEG evidence under `docs/work-log/popup-layering/evidence/`.
- No gameplay, economy, save identifiers, original artwork, or read-only task-owner source was changed.

## H1 — draw-order investigation and fix

The supplied tip uses one camera-space Game Canvas at sorting order 100. Board bird poses use `UI/Default` on that canvas; there is no separate bird override canvas in `BirdView`. Homebound birds can move into the HUD home-flight layer, so checking only the Board hierarchy would miss that route. Settings previously depended on its design-parent sibling position. `PopupMotion.Begin` also started the scrim at alpha zero and faded it over 0.2 seconds.

The baseline stable capture already measured approximately 56.1% displayed RGB brightness in an unobstructed level strip. A persistent separate-canvas bird overdraw was not established in this checkout; the report does not invent that cause. The definite opening-time problem was an initially transparent scrim, combined with independently delayed controls and a swinging panel.

Both owned modal builders now create an explicitly sorted child Canvas at parent order + 1, currently 101, with its own GraphicRaycaster. Their black scrim is immediately alpha 0.72, not faded in. This covers Board, HUD flight graphics, branches, effects, tutorial graphics, and map content on the underlying canvas without changing GameRoot or BoardView. Modal contents remain later graphics within the modal canvas. The Settings X is constructed last, above the ribbon and other controls.

The accepted timed run logged all 12 level bird poses on canvas 100 and Settings on canvas 101. The permanent PlayMode assertions compare effective canvas sorting order, or the common-parent sibling path when sharing a canvas; they also include still-active birds from the Board bird registry, including reparented birds. Pre-render `Graphic.depth` was deliberately not used: Unity returned -1 before rendering even for graphics that the camera subsequently rendered correctly.

## H2 — one Settings motion, immediate controls

- Removed Settings' PopupMotion swing and `ButtonsLast` calls.
- One centered rig scales from 0.92 to 1 over 0.18 seconds with cubic ease-out. There is no rotation, bobbing, idle motion, or per-control delay.
- Panel wood and ribbon fade from 35% to 100% on that same timeline. Buttons deliberately remain fully opaque throughout opening, including inherited CanvasGroup opacity, to satisfy the explicit first-frame control requirement. They share only the whole-panel scale, not an individual entrance animation.
- Closing fades the panel and scrim and reverses the rig scale over 0.12 seconds with cubic ease-in. Individual controls do not shrink when closing disables interaction.
- All three main controls stay active on both map and level. Restart and Home are non-interactive on the map, where their actions do not apply, but keep their full size and opacity.
- Reopening restores parent interaction before activation; otherwise GameButton could see the previous closing-disabled group and briefly animate an individual control from scale 0.9.
- `GameMotion.Instant`, normal button press feedback, mute handling, and root-owned navigation remain intact.

## H3 — proportions

The Settings artwork canvas is 768 by 420 design pixels, including transparent margins, yielding an approximately 740 by 400 visible wood frame. The previous canvas was 768 by 514. The three controls are identical 180 by 180 squares, with a 206-pixel center pitch and one common vertical center in the cream area. The round X is 120 by 120 at the top-right corner. No artwork was generated or edited.

## H4 — other PanelsView modals

The level card, booster purchase/unlock, goal information, shop/aviary placeholders, and all notification kinds use the same higher modal canvas and immediate dim. Removed per-button delayed entrances and independent portrait/mechanic/reward pop/fade entrances. Main panel entrance is one 0.18-second scale/fade motion with every control present immediately; the level card retains its existing map-origin input contract.

Existing gift, gate, reward-count, and bird narrative actions are not presented as new bird-animation work. The shared PopupMotion/WinPanel animation implementation remains the other owner's surface. New-friend and curious-bird content no longer starts hidden behind an entrance delay. No placeholder screen was promoted into a completed shop or aviary feature.

## Evidence

All files below are actual Unity PlayMode camera renders, not composited game mockups:

- `docs/work-log/popup-layering/evidence/level-settings.jpg` — level 5, 12 board birds, settled Settings, unobstructed X.
- `docs/work-log/popup-layering/evidence/map-settings.jpg` — map Settings with all three controls.
- `docs/work-log/popup-layering/evidence/level-settings-opening.jpg` — 11 frames, 0.00 through 0.50 seconds, 0.05-second intervals.
- `docs/work-log/popup-layering/evidence/map-settings-opening.jpg` — the same interval sequence after closing and reopening on the map.
- `docs/work-log/popup-layering/evidence/settings-opening-before.jpg` — baseline level opening; controls were still absent over this interval.

Contact sheets are 1600 pixels wide; portrait screenshots are 900 by 1600. Capture used a temporary PlayMode coroutine, the real Game scene/root, a 1080 by 1920 RenderTexture and camera render, and Unity's `Time.captureDeltaTime = 0.05`. LitMotion and normal PlayMode updates advanced between frames; no Instant motion was used during opening. The capture harness and its metadata were removed after evidence collection. Fixtures initialized without persistence, so captures did not seed or reset the user's profile.

Recorded scale samples were identical on map and level:

| Elapsed seconds | Rig scale, rounded | Scrim alpha | Main controls |
| --- | --- | --- | --- |
| 0.00 | 0.920 | 0.720 | all active, local scale 1, opacity 1 |
| 0.05 | 0.970 | 0.720 | all active, local scale 1, opacity 1 |
| 0.10 | 0.993 | 0.720 | all active, local scale 1, opacity 1 |
| 0.15 | 1.000 | 0.720 | all active, local scale 1, opacity 1 |
| 0.20–0.50 | 1.000 | 0.720 | all active, local scale 1, opacity 1 |

Level background comparison after opening retained approximately 56.1% displayed RGB brightness in the sampled unobstructed strip. The snapshots and sheets were inspected, including the first-frame opaque controls and X and the final cream-area alignment.

## Validation

Executed in this worktree, serialized with `/tmp/unity.lock`; no Editor was killed.

1. `python3 ci/check_current_game_names.py`
   - PASS: 571 tracked text files inspected, zero violations after staging the owned additions.
2. Graphical targeted PlayMode run:

   ```bash
   flock /tmp/unity.lock env T54H_CAPTURE_DIR=/Users/apple/orca/workspaces/game/t54-settings/build/screens/settings_after /Applications/Unity/Hub/Editor/6000.6.7f1/Unity.app/Contents/MacOS/Unity -batchmode -projectPath /Users/apple/orca/workspaces/game/t54-settings/PerchSort -runTests -testPlatform PlayMode -testFilter T54H -testResults /Users/apple/orca/workspaces/game/t54-settings/build/settings-after.xml -logFile /Users/apple/orca/workspaces/game/t54-settings/build/settings-after.log
   ```

   - PASS: 3/3, zero failures/skips: two retained modal regression tests plus the subsequently removed capture harness.
   - Baseline targeted run: 1/3 passed, two failed; Settings controls were alpha 0.45/scale zero, with Restart/Home inactive on the map. Initial harness compilation issues were corrected before capture. An intermediate test used invalid pre-render depth expectations; the retained test uses the requested sorting-order/sibling comparison instead. Reopening also exposed and fixed the disabled-parent scale transition.
3. Required full validation:

   ```bash
   flock /tmp/unity.lock bash scripts/run_tests.sh
   ```

   - Scene build / compilation: PASS.
   - EditMode: **304/304 passed**, zero failures/skips.
   - PlayMode: **23/23 passed**, zero failures/skips: all 21 retained cases plus the two new modal cases, after removing the capture harness.
   - The final full run also covered the close-time no-individual-shrink assertion and map/level reopen behavior.
   - XML: `build/test-results.xml`, `build/playmode-results.xml`; logs: `build/scene-build.log`, `build/editmode-tests.log`, `build/playmode-tests.log`.

The new tests exercise real Settings open/close/reopen, first-call and first-frame control visibility and opacity, equal sizing/spacing, X raycast priority, blocked board moves, and all PanelsView popup entry paths. No existing test was edited, weakened, ignored, or removed.

## Requested contract change

None required outside the assigned files for the Settings/layering fix. GameRoot, BoardView, BirdView, WinPanel, PopupMotion and PopupFlow remain source-unmodified by this task.

## Boundaries and generated files

- These are Mac Editor PlayMode renders and tests, not device acceptance or manual touch-device calibration. Integrated GUI acceptance remains coordinator-owned.
- No new claim is made about the other owner's drawn bird-frame sequences or audio work.
- Unity import/build dirtied the unowned `Assets/Scenes/Game.unity` and the seven egg/family overlay PNG metadata files (`chick_fluff`, `egg_crack`, `egg_idle`, `egg_shell`, `egg_wobble_l`, `egg_wobble_r`, `mama_bow`). They were left in the worktree, not restored and not staged/committed. The task commit contains only its assigned runtime/test/report/evidence paths.
