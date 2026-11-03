# mac-ui — end-of-day T50 WIP

## Branch and disposition
- Branch: `mac-ui/t50`.
- Code WIP commit: `e8be358` (`wip: save partial T50 layout caller cutover`).
- Stopped by lead for end of day. No new work, Unity runs or tests are being started.
- Cancelled T47 remains on `mac-ui/map`; local pause WIP `f9a5720` was not pushed. T47 code was not carried onto T50.
- Detailed handoff: `tasks/T50_report.md`.

## Done
1. Read T50, T48/T49 contracts and approved v7 screens. Merged latest lead specifications, including intro/full win, random per-bird idle and popup birds/doors additions.
2. Applied 315 AST member-access replacements from `V6Layout.*` to `V7Layout.*` across the 18 files below.
3. Merged first T48 milestone `0cb3741`: `coin_pill`, `btn_purple`, `btn_purple_pressed`, `btn_green`, `btn_green_pill`, `badge_red`. These six delivered surfaces are not the complete UI.
4. Created a shared slice contract in local ignored `build/t50-worker-contract.md` and dispatched four non-overlapping phase-1 workers. No completed implementation was returned.
5. On wrap-up, all four workers were confirmed stopped (`closed_agent_terminal`) and then released with transcripts archived:
   - LayoutAssetsHUD: `ctx_d2dbf709c7d0`
   - BoardBird: `ctx_dc5a2ac5efb2`
   - PanelsRoot: `ctx_8482b8738058`
   - MapEvidence: `ctx_706d8b3d8a7d`

## Half done — exact files
Only the mechanical layout-reference migration is present in these files; their v7 appearance/behavior has not been implemented:
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

## Blockers and verification limits
- `PerchSort/Assets/Scripts/V5/Game/V6Layout.cs` still defines `V6Layout`; `V7Layout` does not yet exist. [INFERENCE] The incomplete caller migration will fail compilation until that definition/file cutover is completed. No compiler was run for T50.
- The four Orca workers repeatedly escalated `needs-input`; the transcripts inspected contained no concrete approval question. Continuation guidance requested the exact prompt. Their liveness was unverifiable before the user-authorized stop; no autonomous retry/replacement was attempted.
- Last asset check: T48 had only six HUD surfaces; final layout JSON, glyphs, houses, panels, map/effects and T49 bird/world deliveries were missing. `origin/mac-birds/t49` was not published at that check. No final asset completeness is claimed.
- No T50 EditMode/PlayMode suite, runtime capture or green milestone exists. Old T47 results do not verify T50.

## Next
1. Complete `V6Layout.cs` -> `V7Layout.cs` and measured v7 geometry; migrate from approved images, then `art/v7/layout_v7.json` when delivered.
2. Finish phase-1 resource/importer, HUD/houses, board/birds/worlds, panels/popups/placeholders and baked-path map. Adapt layout/behavior tests and required static v7 captures. Resolve the worker prompt issue before dispatching again.
3. Merge subsequent T48/T49 deliveries, acquire the Unity lock, run full suites and inspect actual v7 screens. Commit/push only a genuinely green visual milestone.
4. Only then implement all Phase 2 animations and addenda: intro, full win/popup sequences, per-bird idle scheduler, animated/masked doors/gates/birds, mechanics/ambient/transition effects, cancellation/input invariants and all timed frame sequences. Shop/Aviary screen-specific animation remains deferred with those future screens.

## Shutdown and preserved local work
- No Unity process was started for T50. Final process check returned no Unity editor matches.
- `/Users/apple/Documents/projects/.unity-lock` was absent; no owned lock needed removal.
- Pre-existing modified `Game.unity`, `packages-lock.json`, `EditorBuildSettings.asset` and 359 untracked generated files were preserved and excluded from T50 WIP staging. No unrelated edits were discarded.

PAUSED by lead: end-of-day WIP saved; continue tomorrow. T50 is incomplete and not green.
