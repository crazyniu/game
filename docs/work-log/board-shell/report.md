# Board shell report

## Stage / gate

- Stage: UI shell; screen: board.
- Gate delivered for Editor review: open `PerchSort/Assets/Scenes/Game.unity`, press Play, then open `Tools > UI States` and choose a board state. Choosing a state opens the board from the map automatically.
- Implementation and the single allowed Unity compile are complete. Visual, interaction, timed animation, audio and persistence acceptance are **not verified**; the user stopped automated suites and capture runs.
- Branch: `mac-board-shell/work`. Implementation checkpoint: `36d2a150`; integration commit: `70aa3f4a`. This report and Unity-generated script metadata are committed afterward on the same branch; final delivery SHA is returned in chat.
- Fetched and merged `origin/main` at `269a5e3f`, including the input-lock repair and delivered board HUD artwork. No PR is created.

## Changed

- `IBoardView` is the presentation boundary. Detached snapshots provide birds/seats, goals, HUD values, permissions, selection, hints, bubbles, tutorial targets, entrance, lifecycle and resolution transactions. Canonical views do not inspect Session/profile/rules; `GameRoot`, `BoardSessionAdapter` and `BoardTutorial` own those policies.
- Real Session events and development-only `BoardFakeData` drive the same `BoardPresentation`, `HudView`, `BoardView`, `BranchView`, `BirdView`, `BirdhouseView`, `SceneView`, `LevelEntrance` and `TutorialHand`. No second scene or copied board views.
- One rope-hung goal house with species-specific face/gift/check badges, delivered house/rope/door/badge sprites, move/star pill, coin pill, dial, fixed-size Settings/tool discs, zero-stock plus hit targets and history-driven Undo visibility. Footprint and gift coins are referenced by stable asset name.
- **Current trees are retained by coordinator decision.** Existing trunk/junction/limb sprites and rendering remain. No `board_tree_*` loading, tree placeholders or tree-art removal.
- Feet lines follow the supplied row table; birds use front-facing rest art, continuous breathing and available drawn actions. Eggs/sleepers retain seated entrance. Counters, press/release, badge and door transitions use the existing motion infrastructure. Existing board-action audio cues remain where the draft mapping is unresolved.
- House transitions retain a revision owner across opening, arrival and closing. The single manual playback clock retains the upstream five-second budget, minimum 0.4 duration scale, idempotent fast-forward and tap consumption. Independent events overlap; homecoming flocks serialize because there is one physical doorway. Cancellation does not reset active manual motion handles.
- `Tools > UI States` supplies 305 named fixtures, a filter, an Instant motion toggle and Return to real game. Fake state classes are guarded for Editor/development builds; the Editor window is outside runtime assemblies. Fake navigation uses synthetic profile/session context with persistence disabled, then restores the original attempt/profile references.
- Existing Layout, cosmetics, gameplay, input-lock, popup-bird, sleep, stuck and tutorial consumers were migrated to the one-house/contract behavior. Their sources compile; no suite was executed.
- Core, Meta, catalog, economy, save/storage contracts and destination popup/map/shop/aviary view files were not changed by this task. Destination fixtures call existing owners rather than redesigning them.

### Changed task files

Paths below are relative to the repository root; incoming main artwork is an integration dependency, not worker-authored art. New C# files also retain their actual Unity-generated `.meta` companions.

- `PerchSort/Assets/Editor/Game/UiStateSwitcherWindow.cs`
- `PerchSort/Assets/Scripts/Game/BirdView.cs`
- `PerchSort/Assets/Scripts/Game/BirdhouseView.cs`
- `PerchSort/Assets/Scripts/Game/BoardAction.cs`
- `PerchSort/Assets/Scripts/Game/BoardAssetKeys.cs`
- `PerchSort/Assets/Scripts/Game/BoardControlFeedback.cs`
- `PerchSort/Assets/Scripts/Game/BoardFakeData.cs`
- `PerchSort/Assets/Scripts/Game/BoardMotionMode.cs`
- `PerchSort/Assets/Scripts/Game/BoardNumber.cs`
- `PerchSort/Assets/Scripts/Game/BoardPresentation.cs`
- `PerchSort/Assets/Scripts/Game/BoardResolutionEvent.cs`
- `PerchSort/Assets/Scripts/Game/BoardSessionAdapter.cs`
- `PerchSort/Assets/Scripts/Game/BoardSnapshot.cs`
- `PerchSort/Assets/Scripts/Game/BoardTutorial.cs`
- `PerchSort/Assets/Scripts/Game/BoardView.cs`
- `PerchSort/Assets/Scripts/Game/BranchView.cs`
- `PerchSort/Assets/Scripts/Game/BubblePresentation.cs`
- `PerchSort/Assets/Scripts/Game/EntrancePresentation.cs`
- `PerchSort/Assets/Scripts/Game/GameLayout.cs`
- `PerchSort/Assets/Scripts/Game/GameRoot.cs`
- `PerchSort/Assets/Scripts/Game/HintPresentation.cs`
- `PerchSort/Assets/Scripts/Game/HudSnapshot.cs`
- `PerchSort/Assets/Scripts/Game/HudView.cs`
- `PerchSort/Assets/Scripts/Game/IBoardView.cs`
- `PerchSort/Assets/Scripts/Game/IScreenStates.cs`
- `PerchSort/Assets/Scripts/Game/InputPresentation.cs`
- `PerchSort/Assets/Scripts/Game/LevelEntrance.cs`
- `PerchSort/Assets/Scripts/Game/OverlayPresentation.cs`
- `PerchSort/Assets/Scripts/Game/SceneView.cs`
- `PerchSort/Assets/Scripts/Game/SelectionPresentation.cs`
- `PerchSort/Assets/Scripts/Game/SleepBubble.cs`
- `PerchSort/Assets/Scripts/Game/StuckHint.cs`
- `PerchSort/Assets/Scripts/Game/TutorialHand.cs`
- `PerchSort/Assets/Scripts/Game/TutorialPresentation.cs`
- `PerchSort/Assets/Tests/EditMode/LayoutTests.cs`
- `PerchSort/Assets/Tests/PlayMode/CosmeticsTests.cs`
- `PerchSort/Assets/Tests/PlayMode/GameplayTests.cs`
- `PerchSort/Assets/Tests/PlayMode/InputLockTests.cs`
- `PerchSort/Assets/Tests/PlayMode/PopupBirdCutTests.cs`
- `PerchSort/Assets/Tests/PlayMode/PopupBirdTests.cs`
- `PerchSort/Assets/Tests/PlayMode/SleepFeedbackTests.cs`
- `PerchSort/Assets/Tests/PlayMode/StuckHintTests.cs`
- `PerchSort/Assets/Tests/PlayMode/TutorialHandTests.cs`
- `docs/work-log/board-shell/spec.md`
- `docs/work-log/board-shell/report.md`

## Verified

| Check | Result / boundary |
|---|---|
| `python3 ci/check_current_game_names.py --self-test` | Passed: 14 guard self-tests. These are Python naming-policy checks, not Unity suites. |
| `python3 ci/check_current_game_names.py` | Passed: 259 tracked text files, zero violations before Unity. |
| External Roslyn compile of runtime/editor and retained test sources | Passed; existing CS0649 warnings for `GlyphMetrics.Document.glyphs` and `AudioManagerTests.testMusic`. Compilation only; no test execution. |
| External Roslyn compile with Editor/development symbols absent | Passed. This is a source conditional-compilation check, not a player build or stripping proof. |
| Detached fixture-construction smoke | 305 states, 351 snapshots, 52 resolution events, zero errors. Checked branch capacity, stable bird-ID uniqueness, goal coverage, edge clearance, and actual background/gift-coin asset paths. Recording view and host stubs were used; this does not verify Unity rendering, navigation, animation or persistence. |
| Single serialized Unity batch compile | Passed; return code 0, script build succeeded, successful batch shutdown. Exactly one Unity invocation. |
| EditMode / PlayMode suites | NOT RUN by user instruction. |
| Runtime screenshots / GUI / timed audio and animation | NOT RUN by user instruction; user reviews in Editor. |

### Exact Unity command

```sh
flock /tmp/unity.lock /Applications/Unity/Hub/Editor/6000.6.7f1/Unity.app/Contents/MacOS/Unity -batchmode -nographics -projectPath /Users/apple/orca/workspaces/game/board-shell/PerchSort -quit -logFile /Users/apple/orca/workspaces/game/board-shell/build/board-shell-compile.log
```

Observed: `*** Tundra build success`, `Exiting batchmode successfully now!`, `Application will exit with return code 0`. Wall time: 72.72 seconds. No executeMethod, tests or screenshots were requested from Unity. Unity reports UAC0009 deprecation warnings for the repository's `DEVELOPMENT_BUILD` guard convention, including new guarded sources; guards are retained rather than weakening release exclusion. Licensing access-token and analytics connection diagnostics did not prevent entitlement resolution, compilation or successful exit.

## Evidence / Editor review

Local ignored evidence:

- `build/board-shell-compile.log` — sole Unity compilation/import/shutdown log.
- `build/board-shell-fixture-construction.log` — all click paths and detached construction result.
- `build/board-shell-before-unity.json` and `.tar` — recorded 1,793 scene/metadata/settings/package files before Unity; tracked workspace was clean.
- `build/board-shell-unity-import-changes.json`, `build/board-shell-unity-import-output.tar` — recorded/preserved all 925 tracked metadata-only import changes before restoring those exact paths to their clean tracked baseline. No scene/settings/package changes occurred.
- `build/board-shell-unrelated-generated-metadata.tar` — preserved unrelated newly generated resource metadata before removing that import-only output. New script metadata stays with the implementation. No primary-checkout file was modified.

Open the worker project in Unity, open `Assets/Scenes/Game.unity`, press Play, choose `Tools > UI States`, leave Filter empty to see all fixtures, then select any path below. Instant motion defaults to the existing shared setting; turn it off to review transitions and idle motion. Select a state again to replay it. Return to real game restores the original attempt. Buttons listed below are exact names, not submenu registrations.

Scripted interaction: in Typical, select branch 0 then branch 3 for the authored legal transfer; choose Illegal move for refusal, Undo after the scripted move, zero-stock badge for the existing Shop boundary, and each face badge for its own Goal info. Recovery world-ending/final Win fixtures expose existing World complete and map-return owners; the world-ending path then exposes World unlock and Daily gift. These are synthetic UI outcomes, not economy-rule changes.

### Complete state click paths

- `Tools > UI States > Data / Empty start - no snapshot`
- `Tools > UI States > Data / Typical`
- `Tools > UI States > Data / Empty branch`
- `Tools > UI States > Data / Empty doorway - zero deliveries`
- `Tools > UI States > Numbers / Coins 0`
- `Tools > UI States > Numbers / Coins 240`
- `Tools > UI States > Numbers / Coins 99999`
- `Tools > UI States > Numbers / Coins 2147483647`
- `Tools > UI States > Data / 2 species`
- `Tools > UI States > Data / 7 species`
- `Tools > UI States > Data / 2 rows`
- `Tools > UI States > Data / 3 rows`
- `Tools > UI States > Data / 4 rows`
- `Tools > UI States > Data / 5 rows`
- `Tools > UI States > Data / Odd branch count`
- `Tools > UI States > Data / All authored seats occupied`
- `Tools > UI States > Data / Added branch`
- `Tools > UI States > Data / Eleventh branch - six rows`
- `Tools > UI States > Data / Zero species - unavailable`
- `Tools > UI States > Data / All home with crow remaining`
- `Tools > UI States > Numbers / Counted moves 0`
- `Tools > UI States > Numbers / Counted moves 7`
- `Tools > UI States > Numbers / Counted moves 8`
- `Tools > UI States > Numbers / Counted moves 9`
- `Tools > UI States > Numbers / Counted moves 11`
- `Tools > UI States > Numbers / Counted moves 12`
- `Tools > UI States > Numbers / Counted moves 999`
- `Tools > UI States > Numbers / Counted moves 2147483647`
- `Tools > UI States > Numbers / Star limits 19 and 22`
- `Tools > UI States > Numbers / Coin one to zero`
- `Tools > UI States > Numbers / Coin gain`
- `Tools > UI States > Numbers / Phase countdown 1`
- `Tools > UI States > Numbers / Phase countdown 2`
- `Tools > UI States > Numbers / Phase countdown 3`
- `Tools > UI States > Numbers / Chain 0`
- `Tools > UI States > Numbers / Chain 1`
- `Tools > UI States > Numbers / Chain 2`
- `Tools > UI States > Numbers / Chain 3`
- `Tools > UI States > Numbers / Chain 7`
- `Tools > UI States > Numbers / Inventory 0`
- `Tools > UI States > Numbers / Inventory 1`
- `Tools > UI States > Numbers / Inventory 99999`
- `Tools > UI States > Numbers / Inventory 2147483647`
- `Tools > UI States > Numbers / Inventory one to zero`
- `Tools > UI States > Numbers / Inventory gain`
- `Tools > UI States > Numbers / Longest name Flurry`
- `Tools > UI States > Numbers / Expanded localization boundary`
- `Tools > UI States > Numbers / Missing baked label`
- `Tools > UI States > Access / First visit before tools unlock`
- `Tools > UI States > Access / Later visit`
- `Tools > UI States > Access / Undo hidden at start`
- `Tools > UI States > Access / Undo shown after move`
- `Tools > UI States > Access / Undo hidden after final undo`
- `Tools > UI States > Access / Three slots with Undo hidden`
- `Tools > UI States > Access / Four slots with Undo hidden`
- `Tools > UI States > Access / Sapling already used`
- `Tools > UI States > Access / Context absent`
- `Tools > UI States > Access / Context present`
- `Tools > UI States > Tools / Firefly locked`
- `Tools > UI States > Tools / Firefly owned`
- `Tools > UI States > Tools / Firefly zero plus`
- `Tools > UI States > Tools / Firefly no effect disabled`
- `Tools > UI States > Tools / Firefly applied`
- `Tools > UI States > Tools / Sapling locked`
- `Tools > UI States > Tools / Sapling owned`
- `Tools > UI States > Tools / Sapling zero plus`
- `Tools > UI States > Tools / Sapling no effect disabled`
- `Tools > UI States > Tools / Sapling applied`
- `Tools > UI States > Tools / OwlEye locked`
- `Tools > UI States > Tools / OwlEye owned`
- `Tools > UI States > Tools / OwlEye zero plus`
- `Tools > UI States > Tools / OwlEye no effect disabled`
- `Tools > UI States > Tools / OwlEye applied`
- `Tools > UI States > Tools / MorningBell locked`
- `Tools > UI States > Tools / MorningBell owned`
- `Tools > UI States > Tools / MorningBell zero plus`
- `Tools > UI States > Tools / MorningBell no effect disabled`
- `Tools > UI States > Tools / MorningBell applied`
- `Tools > UI States > Tools / Scarecrow locked`
- `Tools > UI States > Tools / Scarecrow owned`
- `Tools > UI States > Tools / Scarecrow zero plus`
- `Tools > UI States > Tools / Scarecrow no effect disabled`
- `Tools > UI States > Tools / Scarecrow applied`
- `Tools > UI States > Tools / SpringRain locked`
- `Tools > UI States > Tools / SpringRain owned`
- `Tools > UI States > Tools / SpringRain zero plus`
- `Tools > UI States > Tools / SpringRain no effect disabled`
- `Tools > UI States > Tools / SpringRain applied`
- `Tools > UI States > Hints / Pending`
- `Tools > UI States > Hints / No solution`
- `Tools > UI States > Hints / Timeout unknown`
- `Tools > UI States > Hints / Failed request`
- `Tools > UI States > Hints / Successful transfer hint`
- `Tools > UI States > Hints / Swivel hint`
- `Tools > UI States > Hints / Stuck no legal move`
- `Tools > UI States > Hints / Stuck bubble dismissed`
- `Tools > UI States > Selection / Idle`
- `Tools > UI States > Selection / Selected exposed run`
- `Tools > UI States > Selection / Cancel same branch`
- `Tools > UI States > Selection / Cancel empty space`
- `Tools > UI States > Selection / Empty tip`
- `Tools > UI States > Selection / Egg tip`
- `Tools > UI States > Selection / Sleeping bird tapped with bubble`
- `Tools > UI States > Selection / Legal transfer`
- `Tools > UI States > Selection / Legal partial transfer`
- `Tools > UI States > Selection / Legal full transfer`
- `Tools > UI States > Selection / Illegal wrong species`
- `Tools > UI States > Selection / Illegal full target`
- `Tools > UI States > Selection / Illegal egg blocked target`
- `Tools > UI States > Selection / Rapid repeated taps`
- `Tools > UI States > Selection / In flight blocked input`
- `Tools > UI States > Selection / One queued landing tap`
- `Tools > UI States > Selection / Rejected stale queued action`
- `Tools > UI States > Selection / Swivel drawn hop`
- `Tools > UI States > Resolution / Takeoff flight landing`
- `Tools > UI States > Resolution / Flock home badge lighting`
- `Tools > UI States > Resolution / Multiple flocks cascade`
- `Tools > UI States > Resolution / All home`
- `Tools > UI States > Resolution / Undo last delivery empty house`
- `Tools > UI States > Resolution / Undo delivery another species home`
- `Tools > UI States > Gifts / sunny`
- `Tools > UI States > Gifts / sky`
- `Tools > UI States > Gifts / berry`
- `Tools > UI States > Gifts / mint`
- `Tools > UI States > Gifts / cocoa`
- `Tools > UI States > Gifts / snow`
- `Tools > UI States > Gifts / ember`
- `Tools > UI States > Gifts / ink`
- `Tools > UI States > Gifts / hoot`
- `Tools > UI States > Gifts / No target`
- `Tools > UI States > Gifts / Light step grant`
- `Tools > UI States > Gifts / Light step consume`
- `Tools > UI States > Gifts / Light step undo`
- `Tools > UI States > Gifts / Berry free transfer`
- `Tools > UI States > Gifts / Berry free swivel`
- `Tools > UI States > Mechanics / Mystery egg`
- `Tools > UI States > Mechanics / Revealed egg`
- `Tools > UI States > Mechanics / Egg wobble`
- `Tools > UI States > Mechanics / Egg hatch`
- `Tools > UI States > Mechanics / Authored sleeper`
- `Tools > UI States > Mechanics / Nocturnal day sleeper`
- `Tools > UI States > Mechanics / Both sleep causes`
- `Tools > UI States > Mechanics / Wake still nocturnally asleep`
- `Tools > UI States > Mechanics / Mama following chicks`
- `Tools > UI States > Mechanics / Family target capacity reached`
- `Tools > UI States > Mechanics / Crow parking`
- `Tools > UI States > Mechanics / Crow departure`
- `Tools > UI States > Mechanics / Twig`
- `Tools > UI States > Mechanics / Partial growing branch`
- `Tools > UI States > Mechanics / Full growing branch`
- `Tools > UI States > Mechanics / Upgraded twig`
- `Tools > UI States > Mechanics / Swivel`
- `Tools > UI States > Phase / Disabled`
- `Tools > UI States > Phase / Day`
- `Tools > UI States > Phase / Night`
- `Tools > UI States > Phase / One move until flip`
- `Tools > UI States > Phase / Clock flip reset`
- `Tools > UI States > Phase / Gift triggered flip`
- `Tools > UI States > Phase / Moonbeam expiry`
- `Tools > UI States > Phase / Moonbeam active`
- `Tools > UI States > Phase / Undo restoring Day`
- `Tools > UI States > Phase / Undo restoring Night`
- `Tools > UI States > Phase / Undo restoring Moonbeam`
- `Tools > UI States > Phase / Undo restoring Light step`
- `Tools > UI States > Entrance / Prepared covered`
- `Tools > UI States > Entrance / Skip Prepared covered`
- `Tools > UI States > Entrance / Cancel Prepared covered`
- `Tools > UI States > Entrance / Focus loss Prepared covered`
- `Tools > UI States > Entrance / House`
- `Tools > UI States > Entrance / Skip House`
- `Tools > UI States > Entrance / Cancel House`
- `Tools > UI States > Entrance / Focus loss House`
- `Tools > UI States > Entrance / Branches`
- `Tools > UI States > Entrance / Skip Branches`
- `Tools > UI States > Entrance / Cancel Branches`
- `Tools > UI States > Entrance / Focus loss Branches`
- `Tools > UI States > Entrance / Birds`
- `Tools > UI States > Entrance / Skip Birds`
- `Tools > UI States > Entrance / Cancel Birds`
- `Tools > UI States > Entrance / Focus loss Birds`
- `Tools > UI States > Entrance / Settle controls`
- `Tools > UI States > Entrance / Skip Settle controls`
- `Tools > UI States > Entrance / Cancel Settle controls`
- `Tools > UI States > Entrance / Focus loss Settle controls`
- `Tools > UI States > Entrance / Ready`
- `Tools > UI States > Entrance / Skip Ready`
- `Tools > UI States > Entrance / Cancel Ready`
- `Tools > UI States > Entrance / Focus loss Ready`
- `Tools > UI States > Entrance / First play`
- `Tools > UI States > Entrance / Skip First play`
- `Tools > UI States > Entrance / Cancel First play`
- `Tools > UI States > Entrance / Focus loss First play`
- `Tools > UI States > Entrance / Replay`
- `Tools > UI States > Entrance / Skip Replay`
- `Tools > UI States > Entrance / Cancel Replay`
- `Tools > UI States > Entrance / Focus loss Replay`
- `Tools > UI States > Entrance / Repeated skip`
- `Tools > UI States > Entrance / Instant motion`
- `Tools > UI States > Entrance / Reduced motion`
- `Tools > UI States > Tutorial / Level 1 source`
- `Tools > UI States > Tutorial / Level 1 target`
- `Tools > UI States > Tutorial / Level 2 source`
- `Tools > UI States > Tutorial / Level 2 target`
- `Tools > UI States > Tutorial / Level 3 source`
- `Tools > UI States > Tutorial / Level 3 target`
- `Tools > UI States > Tutorial / Level 6 source`
- `Tools > UI States > Tutorial / Level 6 target`
- `Tools > UI States > Tutorial / Level 11 source`
- `Tools > UI States > Tutorial / Level 11 target`
- `Tools > UI States > Tutorial / Level 15 source`
- `Tools > UI States > Tutorial / Level 15 target`
- `Tools > UI States > Tutorial / Level 21 source`
- `Tools > UI States > Tutorial / Level 21 target`
- `Tools > UI States > Tutorial / Level 24 source`
- `Tools > UI States > Tutorial / Level 24 target`
- `Tools > UI States > Tutorial / Level 31 source`
- `Tools > UI States > Tutorial / Level 31 target`
- `Tools > UI States > Tutorial / Level 33 source`
- `Tools > UI States > Tutorial / Level 33 target`
- `Tools > UI States > Tutorial / Level 1 second source`
- `Tools > UI States > Tutorial / Level 1 second target`
- `Tools > UI States > Tutorial / Level 2 hint prompt`
- `Tools > UI States > Tutorial / Level 4 owned Sapling prompt`
- `Tools > UI States > Tutorial / Expected action`
- `Tools > UI States > Tutorial / Off script action`
- `Tools > UI States > Tutorial / Undo`
- `Tools > UI States > Tutorial / Restart`
- `Tools > UI States > Tutorial / Completion`
- `Tools > UI States > Tutorial / Modal suppression`
- `Tools > UI States > Recovery / Stuck is not loss`
- `Tools > UI States > Recovery / 1 star clear`
- `Tools > UI States > Recovery / 2 star clear`
- `Tools > UI States > Recovery / 3 star clear`
- `Tools > UI States > Recovery / Same star replay`
- `Tools > UI States > Recovery / Upgraded star top up`
- `Tools > UI States > Recovery / World ending win`
- `Tools > UI States > Recovery / Final level return`
- `Tools > UI States > Surfaces / Settings`
- `Tools > UI States > Surfaces / Goal info sunny pending`
- `Tools > UI States > Surfaces / Goal info sunny home`
- `Tools > UI States > Surfaces / Goal info sky pending`
- `Tools > UI States > Surfaces / Goal info sky home`
- `Tools > UI States > Surfaces / Goal info berry pending`
- `Tools > UI States > Surfaces / Goal info berry home`
- `Tools > UI States > Surfaces / Goal info mint pending`
- `Tools > UI States > Surfaces / Goal info mint home`
- `Tools > UI States > Surfaces / Goal info cocoa pending`
- `Tools > UI States > Surfaces / Goal info cocoa home`
- `Tools > UI States > Surfaces / Goal info snow pending`
- `Tools > UI States > Surfaces / Goal info snow home`
- `Tools > UI States > Surfaces / Goal info ember pending`
- `Tools > UI States > Surfaces / Goal info ember home`
- `Tools > UI States > Surfaces / Goal info ink pending`
- `Tools > UI States > Surfaces / Goal info ink home`
- `Tools > UI States > Surfaces / Goal info hoot pending`
- `Tools > UI States > Surfaces / Goal info hoot home`
- `Tools > UI States > Surfaces / Shop focus Firefly`
- `Tools > UI States > Surfaces / Booster unlock Firefly`
- `Tools > UI States > Surfaces / Shop focus Sapling`
- `Tools > UI States > Surfaces / Booster unlock Sapling`
- `Tools > UI States > Surfaces / Shop focus OwlEye`
- `Tools > UI States > Surfaces / Booster unlock OwlEye`
- `Tools > UI States > Surfaces / Shop focus MorningBell`
- `Tools > UI States > Surfaces / Booster unlock MorningBell`
- `Tools > UI States > Surfaces / Shop focus Scarecrow`
- `Tools > UI States > Surfaces / Booster unlock Scarecrow`
- `Tools > UI States > Surfaces / Shop focus SpringRain`
- `Tools > UI States > Surfaces / Booster unlock SpringRain`
- `Tools > UI States > Surfaces / Shop confirmation Affordable`
- `Tools > UI States > Surfaces / Shop confirmation Unaffordable`
- `Tools > UI States > Surfaces / Shop confirmation Cancel`
- `Tools > UI States > Surfaces / Shop confirmation Success`
- `Tools > UI States > Surfaces / New friend single`
- `Tools > UI States > Surfaces / Notifications multiple after entrance`
- `Tools > UI States > Surfaces / World complete`
- `Tools > UI States > Surfaces / World unlock`
- `Tools > UI States > Surfaces / Map return`
- `Tools > UI States > Surfaces / Daily gift map return`
- `Tools > UI States > Environment / World 1 morning`
- `Tools > UI States > Environment / World 1 noon`
- `Tools > UI States > Environment / World 1 golden`
- `Tools > UI States > Environment / World 1 rain`
- `Tools > UI States > Environment / World 2 noon`
- `Tools > UI States > Environment / World 2 mist`
- `Tools > UI States > Environment / World 2 lanterns`
- `Tools > UI States > Environment / World 2 drizzle`
- `Tools > UI States > Environment / World 3 morning`
- `Tools > UI States > Environment / World 3 noon`
- `Tools > UI States > Environment / World 3 sunset`
- `Tools > UI States > Environment / World 3 windy`
- `Tools > UI States > Environment / World 4 morning`
- `Tools > UI States > Environment / World 4 noon`
- `Tools > UI States > Environment / World 4 bluehour`
- `Tools > UI States > Environment / World 4 aurora`
- `Tools > UI States > Environment / Normal audio`
- `Tools > UI States > Environment / Muted`
- `Tools > UI States > Environment / Focus loss`
- `Tools > UI States > Environment / Focus resume`
- `Tools > UI States > Environment / Background modal`
- `Tools > UI States > Environment / Foreground modal`
- `Tools > UI States > Environment / Entrance interrupted restored`
- `Tools > UI States > Environment / Local loading`
- `Tools > UI States > Environment / Missing required asset`
- `Tools > UI States > Environment / Missing required data`
- `Tools > UI States > Environment / Offline local data available`

## Placeholder / missing input / not assessed

- **Trees:** not placeholders. Current trees intentionally stay, including their existing pieced construction; the old draft requirement for continuous painted tree images is superseded for this assignment.
- Native supplied HUD art is used. The zero-stock plus and one-star any-clear linked-ring mark are procedural icon geometry because no dedicated baked marks were supplied; they contain no live fixed words.
- Expanded-localization baked name/effect artwork was not supplied. The named expanded-localization boundary opens existing Goal info with the longest available current baked name; it cannot prove expanded localized art fits. Destination views were not edited, and no new art was fabricated.
- Outfit-specific action-frame sets were not supplied. Retained outfit rest art is used where available; drawn actions use existing base-species frames. Outfit continuity during actions needs art/Editor review.
- The draft still has unapproved numeric subrects/overflow, badge geometry, small white-mark normalization, six-row Sapling layout and shared sound decisions. Conservative choices are recorded in `spec.md`: required 99999 coins and 999 moves render in full; larger values use numeric compact notation with exact accessible names; seven-plus badges use smaller faces inside unchanged 88-square hit targets; six rows use the explicit warned extension. These choices are not visual approval.
- The expanded-localization, native focus/audio recovery, both safe-area aspect ratios, modal layering, continuous idle/frame cadence, 3.5–4.5-second entrance including wipe/cards, and real save restoration have not been exercised in Play mode. Synthetic focus fixtures drive visual lifecycle state; actual native focus/audio requires an Editor/device event.
- Same-star and upgraded-star Win rewards are authored fake presentation values (5 and 10), not an economy implementation. Real reward policy is unchanged.
- No new audio or art was generated by the worker. No screenshots or timed evidence are claimed. No release/player build was run.

## Coordinator questions

No blocking questions. Keep the current-tree override. The user can review the delivered shell in the Editor; unresolved draft geometry, missing expanded-localization/outfit frames and timed/device acceptance require their respective owners, not silent rule or destination-screen changes.
