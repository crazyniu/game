# T30-C v5 level-1 vertical slice (code)

Branch [t30-v5-first-level](https://github.com/crazyniu/game/blob/cb438e87e67e6d66c97bdf00b77ef356ca161290/tasks/T30_code.md#L3), shared worktree `/home/developer/Projects/game`. Unity project `PerchSort/` (Unity 6000.6.7f1, URP, uGUI, new Input System). **This machine has no Unity Editor**: you cannot compile or run tests here; write carefully, keep C# simple and conservative (C# 9, no newer syntax), and a coordinator will compile/test on a Mac and send you any errors.

Authority: `docs/GAME_DESIGN.md` (rules, level 1, layout, animation). The user explicitly authorized implementing v5 level 1 now. The old v2 code and `docs/spec/*` are history; do not follow RULES v4.

## Ownership
You may create/edit only:
- [PerchSort/Assets/Scripts/V5/**](https://github.com/crazyniu/game/blob/cb438e87e67e6d66c97bdf00b77ef356ca161290/tasks/T30_code.md#L9) (new code)
- `PerchSort/Assets/Editor/Game/**` (new editor code)
- [PerchSort/Assets/Tests/V5/**](https://github.com/crazyniu/game/blob/cb438e87e67e6d66c97bdf00b77ef356ca161290/tasks/T30_code.md#L11) (new tests)
- `PerchSort/Assets/Resources/Levels/levels.json`
- [PerchSort/Assets/Scenes/V5.unity](https://github.com/crazyniu/game/blob/cb438e87e67e6d66c97bdf00b77ef356ca161290/tasks/T30_code.md#L13) only via the scene builder (do not hand-write scene YAML)
- `PerchSort/Packages/manifest.json`: add exactly `"com.annulusgames.lit-motion": "https://github.com/annulusgames/LitMotion.git?path=src/LitMotion/Assets/LitMotion#0b4c588ee75a07198841d92aab653e6b39445089"` (LitMotion's Burst/Collections/Mathematics dependencies resolve via its package.json; if a dependency must be listed explicitly, add it and say why)
- `scripts/run_tests.sh`: add a step that runs `GameSceneBuilder.Build` after `SceneBuilder.Build`; keep everything else.
- `tasks/T30_code_report.md`

Do not modify v2 code (`Assets/Scripts/{Core,UI,View,Audio}`), existing tests, existing scenes, or `art/`. Another worker is producing art into `PerchSort/Assets/Resources/Art/` concurrently — do not write there. Do not commit or push.

## Architecture
- `Scripts/Core` — asmdef `PerchSort.Core`, `noEngineReferences: true`. Pure C#: `Species` (string id), `BranchState`, `BoardState` (immutable-ish snapshot or cloneable), `GameSession` (Select/TryMove/Undo/Restart/AddBranch, move count, stars, events list per move: Transferred(from,to,count,birdIds), FlockHome(branch,species,birdIds), Won), `LevelDef` + parser for `levels.json` (no Unity JsonUtility in Core — write a tiny parser or accept a pre-parsed DTO from the game layer; simplest: the game layer uses JsonUtility into DTOs and passes them to Core), `Solver` (BFS/DFS over states, for Hint and for tests). Birds have stable int ids so views can follow them.
- `Scripts/Game` — asmdef `PerchSort.Game`, references Core, LitMotion, Unity.InputSystem (or use uGUI EventSystem pointer clicks — prefer uGUI `IPointerClickHandler` on branch hit areas), UnityEngine.UI, TextMeshPro if the project already uses it (check; otherwise legacy `Text` with the Fredoka font in `Assets/Fonts`).
  - Single uGUI Canvas, Screen Space - Camera, CanvasScaler 1080×1920 match 0.5 (or match width — choose so the board fits on 9:19.5 phones and 3:4 tablets; document the choice); safe-area container.
  - `GameRoot` bootstraps: loads level 1, builds the board view, HUD, panels, tutorial.
  - `BoardView` / `BranchView` / `BirdView`, `HudView` (coin pill, settings, level pill, birdhouse, bottom bar), `WinPanel`, `SettingsPanel`, `TutorialHand`.
  - Sprites load from `Resources/Art/<name>` per the file list in `tasks/T30_art.md`. **If a sprite is missing, fall back to a generated placeholder** (rounded rect / circle texture tinted per species: sunny #FFD23F, sky #5BC0EB) and log one warning, so the slice runs before art lands. Right-side branches/bushes mirror the left sprite via negative x scale.
  - All motion via LitMotion per design §5, including wing-frame swapping during flight. Input locked during animations; state is committed in Core first, then the view plays the events. Cancel/complete motions on restart/undo/destroy so nothing leaks (`AddTo(gameObject)` plus explicit handles).
  - Persistence via PlayerPrefs keys prefixed [v5.](https://github.com/crazyniu/game/blob/cb438e87e67e6d66c97bdf00b77ef356ca161290/tasks/T30_code.md#L28): coins, best stars per level, tutorial seen, muted.
  - Audio: if `Assets/Scripts/Audio/AudioManager` is trivially reusable without touching it, use it for tap/land/home/win; otherwise skip audio but keep the mute toggle persisted.
- `Editor/Game/GameSceneBuilder.cs` — `public static void Build()` (batchmode-callable): creates [Assets/Scenes/V5.unity](https://github.com/crazyniu/game/blob/cb438e87e67e6d66c97bdf00b77ef356ca161290/tasks/T30_v5_code.md#L30) with camera, EventSystem (InputSystemUIInputModule), and a `GameRoot` object; builds at runtime from code (keep the scene minimal); sets EditorBuildSettings scenes to [\[V5.unity, Game.unity\]](https://github.com/crazyniu/game/blob/cb438e87e67e6d66c97bdf00b77ef356ca161290/tasks/T30_v5_code.md#L30) so the player starts in V5 while v2 tests still find `Game.unity`. Also ensure sprite import settings for `Resources/Art/*.png` are Sprite (Single), alpha is transparency, no mipmaps, max size 2048 — do this via an `AssetPostprocessor` in `Editor/Game`, scoped to that folder only.
- Also add `Editor/Game/GameCapture.cs` with a batchmode method `GameCapture.Run` that enters play mode-free capture: instantiate the scene, render the camera to a 1080×1920 PNG at [build/screens/v5/level1_start.png](https://github.com/crazyniu/game/blob/cb438e87e67e6d66c97bdf00b77ef356ca161290/tasks/T30_code.md#L31), then apply move B0→B1 through the session API, wait for animations (or skip them via a static [V5Motion.Instant](https://github.com/crazyniu/game/blob/cb438e87e67e6d66c97bdf00b77ef356ca161290/tasks/T30_code.md#L31) flag), capture `level1_after_move1.png`, finish the level and capture `level1_win.png`. If a play-mode-free render is not feasible, implement it as a PlayMode test that writes these files instead and say so.

## Tests (must compile on Unity 6000.6.7f1)
- `Tests/EditMode` (asmdef referencing Core, `UNITY_INCLUDE_TESTS`): rules — partial transfer by capacity, illegal target rejected without state/move change, flock home empties branch and keeps it usable, win detection, undo restores everything including move count and homed flocks, add-branch once only, stars thresholds, level 1 parses and is solvable in exactly par=2 by the solver, the intended line wins.
- `Tests/PlayMode`: load [V5](https://github.com/crazyniu/game/blob/cb438e87e67e6d66c97bdf00b77ef356ca161290/tasks/T30_code.md#L35) scene, wait for root, perform the two level-1 moves via the public API with instant motion, assert the win panel is active and coins increased by the reward; second test: tap simulation through `BranchView` click handlers (select + target) moves birds.
- Existing tests must keep passing (baseline EditMode 163, PlayMode 14 passed / 8 skipped). Do not edit them.

## Report
`tasks/T30_code_report.md`: file list, design decisions, what is untested because there is no Unity here, and any deviation from the design doc with reason.

Done = all files written per ownership, report written, no edits outside ownership. Compile errors found later on the Mac will be sent to you.
