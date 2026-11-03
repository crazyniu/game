# T50 end-of-day WIP handoff

Status: WIP, stopped by lead for end of day. Branch: `mac-ui/t50`. Neither phase is complete; no T50 Unity run, compilation, test suite, screenshot capture or green milestone has occurred.

## Completed
- Saved cancelled T47 pause work as local `mac-ui/map` commit `f9a5720`, not pushed. Started T50 from `origin/t44-hud-art`; T47 code was not carried over.
- Read T50, T48/T49 contracts, approved v7 screen references and all three Phase 2 addenda: intro/full win, random per-bird idle, and popup birds/doors. Latest lead specification changes were merged from `origin/t44-hud-art`.
- Applied an AST member-access migration: 315 `V6Layout.*` references became `V7Layout.*` across 18 source/test files. No V6 compatibility alias was added.
- Merged the first T48 art milestone (`0cb3741`): `coin_pill.png`, `btn_purple.png`, `btn_purple_pressed.png`, `btn_green.png`, `btn_green_pill.png`, `badge_red.png`, with its art-side report/source records. These are six surfaces, not a complete v7 UI.
- Prepared non-overlapping phase-1 slice contracts in ignored local `build/t50-worker-contract.md`; dispatched four Orca workers. Orca emitted needs-input escalations without a concrete question in the transcripts read. Scoped continuation messages were sent, but no completed implementation was returned.
- On the lead's wrap-up instruction, all four dispatches were confirmed `stopped` with `closed_agent_terminal`, then confirmed `released` with archived transcripts. No replacement workers were started.

## Half done: exact changed files
These files contain the mechanical layout-reference migration, not a finished v7 implementation:

- `PerchSort/Assets/Editor/V5/V5Capture.cs`
- `PerchSort/Assets/Scripts/V5/Game/BirdView.cs`
- `PerchSort/Assets/Scripts/V5/Game/BirdhouseView.cs`
- `PerchSort/Assets/Scripts/V5/Game/BoardView.cs`
- `PerchSort/Assets/Scripts/V5/Game/BranchView.cs`
- `PerchSort/Assets/Scripts/V5/Game/HudView.cs`
- `PerchSort/Assets/Scripts/V5/Game/SettingsPanel.cs`
- `PerchSort/Assets/Scripts/V5/Game/TutorialHand.cs`
- `PerchSort/Assets/Scripts/V5/Game/V5Button.cs`
- `PerchSort/Assets/Scripts/V5/Game/V5GameRoot.cs`
- `PerchSort/Assets/Scripts/V5/Game/V5Panels.cs`
- `PerchSort/Assets/Scripts/V5/Game/V5SceneView.cs`
- `PerchSort/Assets/Scripts/V5/Game/V5UI.cs`
- `PerchSort/Assets/Scripts/V5/Game/V6MapModel.cs`
- `PerchSort/Assets/Scripts/V5/Game/V6MapView.cs`
- `PerchSort/Assets/Scripts/V5/Game/WinPanel.cs`
- `PerchSort/Assets/Tests/V5/EditMode/V5LayoutTests.cs`
- `PerchSort/Assets/Tests/V5/PlayMode/V5GameplayTests.cs`

**Known compile blocker:** `V6Layout.cs` still defines `V6Layout`; `V7Layout` has not been defined. Therefore the caller cutover is incomplete and is expected not to compile. This is a static inspection finding, not a claimed compiler result. The task remains explicitly WIP; no stub or alias was created to hide it.

## Not implemented
V7 measured layout; resource resolver/importer; HUD/houses/board/birds conversion; panels/popups/placeholder navigation; baked-path map; adapted geometry tests and v7 captures; all Phase 2 animation additions and timed evidence. Existing animations remain old implementation, not newly accepted v7 behavior. No Core/Meta, level-data or new artwork edits were made by mac-ui.

## Next
1. Complete the V7Layout definition/file cutover and migrate its values from approved images, then the delivered `art/v7/layout_v7.json`.
2. Finish the four phase-1 slices with explicit file ownership; resolve the stopped-worker needs-input condition before dispatching again.
3. Merge further `origin/mac-art/t48` and `origin/mac-birds/t49` deliveries. At the last dependency check, T48 contained only six surfaces and T49 was not published; final layout/glyph/house/panel/map/effect/bird/world resources were unavailable.
4. Only after complete v7 visuals, acquire the Unity lock, run the full suite and inspect all requested `build/screens/v7/` captures; push a genuinely green milestone.
5. Then implement every original Phase 2 requirement and all addenda, including the skippable intro/full staged win, random per-bird idle scheduler, split/animated popup doors and birds, cancellation/input invariants, missing-part log-once fallbacks and timed frame sequences. Shop/Aviary screen-specific animations remain deferred with those future screens.
6. Run completion/cancellation/input tests and record all requested frame sequences; replace this WIP report with final measured evidence.

## Workspace preservation and shutdown
- Pre-existing modified `PerchSort/Assets/Scenes/Game.unity`, `PerchSort/Packages/packages-lock.json`, `PerchSort/ProjectSettings/EditorBuildSettings.asset`, and 359 untracked generated files are not T50 changes and are excluded from the WIP commit. They were not discarded.
- No Unity process was started for T50. Wrap-up `pgrep -fl 'Unity.app/Contents/MacOS/Unity'` returned no matches. `/Users/apple/Documents/projects/.unity-lock` was absent; no lock needed removal.
- Earlier T47 test counts/captures are not T50 evidence.
