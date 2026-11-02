# T30-C — v5 level-1 code delivery

Authority: [T30 code task](T30_v5_code.md), [v5 design](../docs/GAME_DESIGN_V5.md). Scope: code delivery on the shared `t30-v5-first-level` branch; no commits or pushes. Unity compilation, scene generation and visual acceptance are delegated to the coordinator's Mac workflow, not claimed here.

## Files

Written in this worker run:

- `PerchSort/Assets/Scripts/V5/Game/PerchSort.V5.Game.asmdef`
- `PerchSort/Assets/Scripts/V5/Game/V5Art.cs`
- `PerchSort/Assets/Scripts/V5/Game/V5UI.cs`
- `PerchSort/Assets/Scripts/V5/Game/LevelLoader.cs`
- `PerchSort/Assets/Scripts/V5/Game/V5Progress.cs`
- `PerchSort/Assets/Scripts/V5/Game/V5GameRoot.cs`
- `PerchSort/Assets/Scripts/V5/Game/BirdView.cs`
- `PerchSort/Assets/Scripts/V5/Game/BranchView.cs`
- `PerchSort/Assets/Scripts/V5/Game/BoardView.cs`
- `PerchSort/Assets/Scripts/V5/Game/HudView.cs`
- `PerchSort/Assets/Scripts/V5/Game/SettingsPanel.cs`
- `PerchSort/Assets/Scripts/V5/Game/WinPanel.cs`
- `PerchSort/Assets/Scripts/V5/Game/TutorialHand.cs`
- `PerchSort/Assets/Editor/V5/V5SceneBuilder.cs`
- `PerchSort/Assets/Editor/V5/V5ArtImporter.cs`
- `PerchSort/Assets/Editor/V5/V5Capture.cs`
- `PerchSort/Assets/Tests/V5/EditMode/PerchSort.V5.EditModeTests.asmdef`
- `PerchSort/Assets/Tests/V5/EditMode/V5RulesTests.cs`
- `PerchSort/Assets/Tests/V5/PlayMode/PerchSort.V5.PlayModeTests.asmdef`
- `PerchSort/Assets/Tests/V5/PlayMode/V5GameplayTests.cs`
- `PerchSort/Packages/manifest.json` — only the required pinned LitMotion dependency added.
- `scripts/run_tests.sh` — only the V5 builder step added after the existing SceneBuilder step.
- `tasks/T30_code_report.md`

Reviewed and retained from the earlier worker, without rewriting:

- `PerchSort/Assets/Scripts/V5/Core/{BoardState,GameSession,Solver}.cs`
- `PerchSort/Assets/Scripts/V5/Core/PerchSort.V5.Core.asmdef`
- `PerchSort/Assets/Scripts/V5/Game/V5Motion.cs`
- `PerchSort/Assets/Resources/V5/levels.json`

`PerchSort/Assets/Scenes/V5.unity` is generated only by `V5SceneBuilder.Build`; it was not hand-written or generated on Linux. No v2 source/tests/scenes, production art, package lock or ProjectSettings files were edited by this worker. Unity will generate metadata and resolve the package lock on the Mac; those generated changes need coordinator review.

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
- **Capture:** `V5Capture.Run` opens the built scene outside Play Mode, initializes the actual root with persistence disabled, uses `V5Motion.Instant`, performs the two authored moves through `V5GameRoot.TryMove`, and renders 1080×1920 PNGs. It restores the previous scene setup and motion flag. This is an implemented edit-mode capture path, not an observed render result.
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

1. Run `bash scripts/run_tests.sh`. The new step runs `V5SceneBuilder.Build` after `SceneBuilder.Build`, builds `V5.unity`, and sets the enabled scene order to V5 then Game. Confirm Unity compilation, LitMotion package resolution and all old/new EditMode/PlayMode results; none were run here.
2. With graphics available, execute `V5Capture.Run` after building the scene. Expected files are `build/screens/v5/level1_start.png`, `level1_after_move1.png`, and `level1_win.png`. Do not use `-nographics` for capture. Camera/URP edit-mode rendering and TMP mesh output have not been exercised here.
3. Play the two moves with `V5Motion.Instant = false`. Verify real wingbeat swaps, transfer/home timing, house/sparkle payoff, input locking, restart/undo cancellation, win stars and coin flight. Static captures do not prove animation.
4. Check 9:19.5 phone and 3:4 tablet safe areas, branch roots/feet, actual pointer hit behavior, generated fallback replacement by production art, and the hand fingertip against the art worker's report. The hand currently uses a top-left-area pivot `(0.22, 0.8)` pending that in-context check.
5. Verify real audio playback/mute, first-play-only tutorial, replay/relaunch persistence and scene transitions back to v2. These rely on Unity runtime and were not simulated with fake Unity APIs.

No claim is made that the slice has passed visual acceptance or Unity semantic compilation. The delivered builders/tests/capture path are ready for the coordinator's Mac verification and compile-error feedback.
