# T30-C — v5 level-1 code delivery

Authority: [T30 code task](spec-code.md), [v5 design](../../GAME_DESIGN.md). Scope: code delivery on the shared `t30-v5-first-level` branch; no commits or pushes. Unity compilation, scene generation and visual acceptance are delegated to the coordinator's Mac workflow, not claimed here.

## Files

Written in this worker run:

- `PerchSort/Assets/Scripts/Game/PerchSort.Game.asmdef`
- `PerchSort/Assets/Scripts/Game/V5Art.cs`
- `PerchSort/Assets/Scripts/Game/V5UI.cs`
- `PerchSort/Assets/Scripts/Game/LevelLoader.cs`
- `PerchSort/Assets/Scripts/Game/V5Progress.cs`
- `PerchSort/Assets/Scripts/Game/GameRoot.cs`
- `PerchSort/Assets/Scripts/Game/BirdView.cs`
- `PerchSort/Assets/Scripts/Game/BranchView.cs`
- `PerchSort/Assets/Scripts/Game/BoardView.cs`
- `PerchSort/Assets/Scripts/Game/HudView.cs`
- `PerchSort/Assets/Scripts/Game/SettingsPanel.cs`
- `PerchSort/Assets/Scripts/Game/WinPanel.cs`
- `PerchSort/Assets/Scripts/Game/TutorialHand.cs`
- `PerchSort/Assets/Editor/Game/GameSceneBuilder.cs`
- `PerchSort/Assets/Editor/Game/GameArtImporter.cs`
- `PerchSort/Assets/Editor/Game/GameCapture.cs`
- `PerchSort/Assets/Tests/EditMode/PerchSort.EditModeTests.asmdef`
- `PerchSort/Assets/Tests/EditMode/V5RulesTests.cs`
- `PerchSort/Assets/Tests/PlayMode/PerchSort.PlayModeTests.asmdef`
- `PerchSort/Assets/Tests/PlayMode/GameplayTests.cs`
- `PerchSort/Packages/manifest.json` — only the required pinned LitMotion dependency added.
- `scripts/run_tests.sh` — only the V5 builder step added after the existing SceneBuilder step.
- `docs/work-log/game-rebuild/report-code.md`

Reviewed and retained from the earlier worker, without rewriting:

- `PerchSort/Assets/Scripts/Core/{BoardState,GameSession,Solver}.cs`
- `PerchSort/Assets/Scripts/Core/PerchSort.Core.asmdef`
- `PerchSort/Assets/Scripts/Game/V5Motion.cs`
- `PerchSort/Assets/Resources/Levels/levels.json`

`PerchSort/Assets/Scenes/V5.unity` is generated only by `GameSceneBuilder.Build`; it was not hand-written or generated on Linux. No v2 source/tests/scenes, production art, package lock or ProjectSettings files were edited by this worker. Unity will generate metadata and resolve the package lock on the Mac; those generated changes need coordinator review.

## Decisions and behavior

- **Rule authority:** v5 only. The Core keeps home-completed branches reusable, transfers `min(run, free)`, preserves stable bird IDs, emits transfer/home/win events, and restores whole-board snapshots on Undo. Add Branch adds the next unoccupied side/row slot, does not count a move, and is available once in the current board history; undoing its addition restores the allowance. Restart restores the authored layout.
- **JSON boundary:** Unity `JsonUtility` deserializes the field DTOs; Core validates/constructs the board in `GameSession`, with no engine references. There is only one runtime level.
- **Layout:** one Screen Space–Camera uGUI canvas, 1080×1920 reference, match 0.5, full-bleed background and a safe-area design rectangle. The design rectangle uniformly fits the smaller available axis, preserving the authored board/HUD proportions on tall phones and tablets instead of cropping the bottom controls. Wider tablets can have horizontal background gutters; branches are rooted at the design area's edges. This is an explicit responsive-layout adaptation, not a claim of tested device fit.
- **Hit areas:** branch hit rectangles meet at the center without overlapping even though left/right branch art overlaps. Birds and decorative art do not intercept input. Empty background cancels selection. Right branches and bushes mirror the left sprite with negative x scale.
- **Font and controls:** use the existing Fredoka TMP font, serialized by the scene builder. Controls are icon-only with descriptive object names. Settings contains only mute, restart and close. The stuck-state bubble is informational, not a button label.
- **Motion:** retain the pinned LitMotion helper with explicit handle cancellation and `AddTo(gameObject)`. Idle breathing/blinking, select hop/drop, illegal shake, staggered arc flights with alternating wing sprites, landing squash, cheer/home flights, house punches/sparkles, win/stars/coins and button feedback all use LitMotion. Core commits first; input stays locked until the committed presentation finishes. Undo/restart cancel the presentation and reconstruct the restored board. Ordinary move completion retains surviving bird views by ID.
- **Persistence:** `v5.coins`, `v5.bestStars.1`, `v5.tutorialSeen`, `v5.muted`. Wins pay `10 + 5 × stars`; the same attempt cannot pay twice through programmatic undo/re-win. Explicit replay/restart begins a fresh rewarding attempt. Best stars never decrease. The tutorial is marked seen on its first shown attempt and uses hand-only guidance for the two intended moves.
- **Audio:** reuse the existing `AudioManager` for selection, flight/landing, home, win and tool feedback without editing v2 audio. V5 starts no music. A V5-owned audio object is destroyed with the root; a borrowed manager has its prior sound/music state restored when V5 unloads.
- **Missing art:** each missing sprite logs once per root/resource name and gets a generated RGBA fallback cached for that root. Fallback sprites/textures are disposed with the root. These are explicitly fallback shapes, not accepted production artwork. Concurrent art delivery was not modified or visually certified.
- **Solver:** breadth-first search gives shortest move count. L1 has multiple two-move solutions: its tie-broken result is B0→B1, B0→B2; the authored B0→B1, B2→B0 line also wins in two. Search budget exhaustion remains distinct from unsolvability.
- **Capture:** `GameCapture.Run` opens the built scene outside Play Mode, initializes the actual root with persistence disabled, uses `V5Motion.Instant`, performs the two authored moves through `GameRoot.TryMove`, and renders 1080×1920 PNGs. It restores the previous scene setup and motion flag. This is an implemented edit-mode capture path, not an observed render result.
- **Dependencies:** no extra explicit packages added. The pinned LitMotion `package.json` declares Burst 1.6.0, Collections 1.5.1 and Mathematics 1.0.1; Unity's resolver determines compatible installed versions.

## Verification performed on Linux

A temporary .NET SDK 8.0.427 was downloaded outside the repository and its SHA-512 matched Microsoft's release metadata. It compiled the **actual Core source files** with `LangVersion=9.0` and executed a throwaway console smoke, using the authored JSON as input (System.Text.Json only for this standalone harness, not a replacement for Unity JsonUtility).

Observed output:

```text
C#9 syntax: 21 files, zero syntax errors (Unity semantic compilation not performed).
Core smoke PASS: 25 behavioral assertions; actual Core source compiled as C#9.
L1 shortest path: B0->B1, B0->B2
```

The smoke exercised partial transfers, stable IDs, event order, both intended homecomings, win/stars, undo of homes and added branches, illegal/full targets, once-only add-branch, reuse of a home-emptied branch, exact restart, solver non-mutation and budget exhaustion. It is not a Unity/NUnit run.

Additional checks performed:

- `bash -n scripts/run_tests.sh` — passed.
- JSON parsing of the manifest, level data and four V5 asmdefs — six files passed.
- Exact authored L1 layout/capacity/par and exact LitMotion commit pin — matched.
- Confirmed used LitMotion builder/handle method signatures against the pinned upstream source.

Permanent test cases written: **16 EditMode cases** and **3 PlayMode cases**. PlayMode cases cover the public API win/reward, branch pointer select/target handling, and non-instant input locking plus undo cancellation. Tests preserve and restore the V5 PlayerPrefs keys and motion mode. No existing tests were changed.

## Required Mac checks / untested areas

1. Run `bash scripts/run_tests.sh`. The new step runs `GameSceneBuilder.Build` after `SceneBuilder.Build`, builds `V5.unity`, and sets the enabled scene order to V5 then Game. Confirm Unity compilation, LitMotion package resolution and all old/new EditMode/PlayMode results; none were run here.
2. With graphics available, execute `GameCapture.Run` after building the scene. Expected files are `build/screens/v5/level1_start.png`, `level1_after_move1.png`, and `level1_win.png`. Do not use `-nographics` for capture. Camera/URP edit-mode rendering and TMP mesh output have not been exercised here.
3. Play the two moves with `V5Motion.Instant = false`. Verify real wingbeat swaps, transfer/home timing, house/sparkle payoff, input locking, restart/undo cancellation, win stars and coin flight. Static captures do not prove animation.
4. Check 9:19.5 phone and 3:4 tablet safe areas, branch roots/feet, actual pointer hit behavior, generated fallback replacement by production art, and the hand fingertip against the art worker's report. The hand currently uses a top-left-area pivot `(0.22, 0.8)` pending that in-context check.
5. Verify real audio playback/mute, first-play-only tutorial, replay/relaunch persistence and scene transitions back to v2. These rely on Unity runtime and were not simulated with fake Unity APIs.

No claim is made that the slice has passed visual acceptance or Unity semantic compilation. The delivered builders/tests/capture path are ready for the coordinator's Mac verification and compile-error feedback.

## Capture cleanup follow-up

Coordinator-provided Mac results supersede the earlier compilation/test uncertainty: Unity compilation passed, EditMode **179/179**, PlayMode **17 passed / 8 skipped**. Graphics-enabled `GameCapture.Run` produced three distinct real PNGs, but cleanup threw `Invalid SceneManagerSetup: No loaded scene found`; `-nographics` instead produced blank identical PNGs. These reported results were accepted without rerunning the failing baseline.

Changes in `PerchSort/Assets/Editor/Game/GameCapture.cs`:

- Reject `SystemInfo.graphicsDeviceType == GraphicsDeviceType.Null` before opening scenes or writing output, with an explicit instruction to remove `-nographics`.
- Restore the saved scene setup only if at least one saved scene is loaded.
- Reset `V5Motion.Instant` in an inner `finally`, even if scene restoration fails.
- Call `EditorApplication.Exit(0)` only after successful capture and cleanup in batch mode; interactive captures do not close the editor, and failure paths never request a success exit.

Local verification: a temporary C#9 console harness compiled the actual capture source against substituted Unity API seams and passed **16 control-flow assertions**, covering the graphics guard/no output side effects, empty/all-unloaded setups, loaded/mixed setups, motion/render-target cleanup, restoration exceptions, batch success exit and interactive non-exit. This verifies the changed control flow, not real Unity scene restoration, graphics or PNG validity; the harness was not added to the project.

Remaining: coordinator's targeted Mac rerun must confirm graphics-enabled capture exits 0 with the three real images, and `-nographics` fails with the new message before writing images. No existing gameplay code, tests, scenes or art changed in this follow-up.

## T31 — v5.1 layout rework

Authority: [T31 layout task](../layout-design/spec-code.md) and the coordinator-corrected [v5 design section 3](../../GAME_DESIGN.md#3-screen-layout-v51-portrait-design-canvas-1080x1920-origin-top-left-y-downward). The coordinator resolved the initial contradictions before implementation: settings/close are 150px, tracker center is y=663, replay/home centers are x=305/775, and the 40px separation rule applies to independent HUD groups, not decorative overlap or nested pill contents.

### Element changes

| Element | Implemented layout/behavior |
| --- | --- |
| Shared layout | New `V5Layout` owns the section-3 dimensions, positions, derived bounds, row-pitch rule, button styling values and panel geometry; HUD, board, panels, safe-area scaling and capture dimensions consume it. |
| Buttons | One `V5Button` presentation: wood 150px for every secondary control, green 200px only for Next; disabled controls retain their sprites at alpha 0.45/scale 0.9 and reject clicks. Re-enabling restores the normal presentation. Removed the old separate feedback component and round-purple settings use. |
| Coins/settings | Coin pill starts at x=48, y center 130; icon is 64px with 24px padding, and the bounded number area is left-aligned/autosized. Settings is a wood 150px button centered at (957,130). |
| Limb/house | Load `limb_top` at 1080x320 behind the HUD. Missing art uses a generated transparent brown bar with a hook ending at (540,235), not a claimed production asset. The house is 300x340 with a top-center pivot at that hook and a separate four-second ±2-degree LitMotion sway; house punch and sway do not overwrite each other. |
| Home flight | Bird home-flight endpoints now follow the current door position while the house sways/punches, instead of the removed static door coordinate. Existing transfer timing/rules remain unchanged. |
| Flock tracker | One 380x96 pill at (540,663), 40px below the house. It contains a 72px wood level badge/font 44 and one 64px idle icon per initial species, with the specified 28px/40px internal gaps. Unhomed icons use alpha 0.4 plus an outline; homed icons are full color with a small green vector check. Removed the old level pill/loose goal dots. |
| Board | Row count comes from the current board; pitch is `min(300,800/rowCount)` around y=1160. L1 left-row centers are 1010/1310; right branches retain +40. Add Branch re-centers the three-row board, and Undo restores the two-row layout. Bird sizes, slot pitch, first-slot offset and non-overlapping branch hit regions are retained. |
| Bottom bar | Three wood 150px controls at x=300/540/780, y=1760: 90px clear gaps, bottom edge 1835. No labels beneath them. |
| Win panel | 860x900 at (540,1010), scale-in from 0.6; ribbon bottom clears the 150px star row by 40px, stars have 30px gaps at y=870, reward is at y=1060 with a 64px coin/font 56/20px gap. Replay/Next/Home at x=305/540/775 and y=1330 have 60px clear gaps. |
| Settings panel | 700x520 at (540,1010); sound/restart are wood 150px controls at x=400/680, y=1040. The wood 150px close button is inset 24px at top-right; the closest button gap is 41px. |
| Lifecycle | Sway uses the existing linked/cancellable LitMotion helper; Undo/Restart reset and restart the ambient sway while canceling transient feedback. Destruction cancels the linked motions. |

### Files changed

- Added `PerchSort/Assets/Scripts/Game/V5Layout.cs` and `V5Button.cs`.
- Updated `PerchSort/Assets/Scripts/Game/{V5UI,V5Art,GameRoot,HudView,BoardView,BranchView,BirdView,WinPanel,SettingsPanel}.cs`.
- Updated `PerchSort/Assets/Editor/Game/GameCapture.cs` to use the shared canvas dimensions.
- Added `PerchSort/Assets/Tests/EditMode/V5LayoutTests.cs` and `V5ButtonTests.cs`; added the explicit `Unity.ugui` reference to that test asmdef.
- Appended this section to `docs/work-log/game-rebuild/report-code.md`.

No Core rules, level data, scenes, artwork, package files, scripts or files outside ownership were edited. No commit was made.

### Tests and available verification

Added **12 layout EditMode cases** covering all independent HUD pairs at the reference size, tool safe-area containment, two-row centering/right offsets, one/three/four-row pitch behavior, disjoint hit regions, hook/house/tracker attachment, nested pill/text clearance, and win/settings panel gaps. Added **one button EditMode regression** for disabled appearance/click rejection followed by successful re-enable/click. Existing V5 test sources were unchanged; none asserted old positions, so no old assertions were re-pinned.

A temporary, checksum-verified .NET SDK compiled the actual `V5Layout` and Core sources as C#9 and ran a geometry/session smoke, with lightweight substitutes only for Unity's `Vector2`, `Rect` and `Mathf` arithmetic:

```text
C#9 syntax: 25 files, zero syntax errors.
Layout/Core smoke PASS: 84 assertions against actual C#9 source.
HUD min gap=40; settings min gap=41; win gaps=60; tool bottom=1835; row centers=1010/1310.
```

The smoke also exercised the authored L1 win, Add Branch's three-row re-centering, and Undo's restored row count. The updated asmdef JSON parsed successfully. A targeted callsite check found no remaining old `V5UI.DesignSize`, static HUD door/coin points, `V5ButtonFeedback`, or round-purple button use; Next is the sole primary-button call.

**Not verified on Linux:** Unity semantic compilation/NUnit execution, real HUD renders at phone/tablet safe areas, typography/art clearance, the limb sprite/rope visual join, four-second sway, moving-door home flights, real pointer/button transitions, or animation cancellation during runtime. The coordinator must run the existing Mac test/capture workflow and inspect start/after-move/win/settings surfaces with production art; static bounds checks are not screenshots or animation proof.
