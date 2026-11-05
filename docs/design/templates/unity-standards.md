# Unity implementation standards

Applies in stages 4 to 7. Starting code for the patterns below is in [../assets/unity/](../assets/unity/README.md); evidence layout in [evidence.md](evidence.md). The project's existing choices win: do not switch a running project between UGUI and UI Toolkit, Resources and Addressables, or input systems as a side effect of UI work. Propose such a change to the user separately.

## Version awareness

- Read the project's Unity version (`ProjectSettings/ProjectVersion.txt`) before using any engine API.
- Check APIs against the current Unity documentation for that version (documentation tools when available), not memory. Mark anything not verified as unverified.

## Scenes, prefabs and assets

- Never hand-edit `.unity`, `.prefab` or `.asset` YAML while a Unity Editor may have the project open; its next save overwrites the edit. If unsure, assume it is open.
- Prefer building scenes and prefabs from an editor script (a scene builder run in batch mode), so the scene is reproducible and reviewable as code.
- Never invent a GUID, never edit or delete `.meta` files, keep every `fileID` and `guid` reference intact. After any change, open or batch-load the scene to confirm it loads without errors.
- Configure texture import settings per asset type (sprite mode, pixels per unit, compression, max size) from the art direction asset standards; put UI sprites in sprite atlases.

## UI architecture

- One UI system per screen; do not mix UGUI and UI Toolkit on one screen.
- View contract in code: each screen's view exposes methods for the data it shows and C# events for player actions. A fake data source (plain C# class or ScriptableObject with authored states) drives it in stage 4; real logic drives it in stage 6. Views never contain game rules.
- Screen and popup management through one screen stack: push, pop, replace, clear to. Back and close always pop. Modals block input to everything behind them.
- Register events in `OnEnable`, unregister in `OnDisable`. Cache references in `Awake`; no `GetComponent`, `Find` or visual-tree queries per frame.
- No `Find`, `FindObjectOfType` or `SendMessage` in game code; pass references or use events.
- Every tween and coroutine is stopped when its owner is disabled or destroyed.

## UGUI specifics

- Canvas Scaler: Scale With Screen Size with the project's reference resolution; apply the device safe area to a root rect.
- One canvas per layer (HUD, screens, popups, overlays) with explicit sorting order; split frequently changing elements from static ones, because one change rebuilds the whole canvas.
- Fade and hide groups with `CanvasGroup` (alpha, interactable, blocksRaycasts), not per-element alpha.
- Turn off Raycast Target on every non-interactive image and text.
- Prefer anchors over nested layout groups. Pool or virtualize long lists.
- Text with TextMeshPro, not legacy Text.
- Disabled buttons: `interactable = false` plus a dimmed look; never a different size.

## Motion and sound

- All UI motion goes through the project's tween library and the animation table in the pattern library. Provide one global switch that makes motion instant for tests and still screenshots; timed evidence always runs with real motion.
- Frame animation from authored sprite sequences at the specified frame rate; never mirror or scale a still to fake an action.
- Audio through the project's audio service and Audio Mixer groups (music, UI, gameplay); mute and volume settings act on the mixer, so every new sound follows them automatically.
- Start background music per screen from the screen stack, so every screen that needs music has it.

## Development state switcher

- An editor window or a development-only overlay (compiled only under `UNITY_EDITOR` or `DEVELOPMENT_BUILD`) that lists every state of the current screen from the screen spec and applies it through the fake data source.
- It never ships in release builds.

## Verification

- Edit Mode tests for pure logic and layout rules (for example same-role button sizes match in every state). Play Mode tests for screen flow, state reachability and that every popup opens and closes.
- Test names say scenario and expected result: class `<Feature>Tests`, method `<Scenario>_<Expected>`.
- A bug-fix test must be seen failing on the unfixed code before it is trusted.
- Run the project's batch test and screenshot commands on a single Unity process at a time.
- Screenshots: capture every state at the smallest and largest supported resolution with `ScreenCapture` or a capture tool in batch mode.
- Timed evidence: in Play Mode, set `Time.captureFramerate` to a fixed rate, save every frame of the interaction, and log the start and end time of each animation and sound. Measured durations are compared with the screen spec table.
- User review in the Editor: Game view at the target device resolution, or the Device Simulator for safe area and notch.
- Profile before optimizing: Profiler (UI and GC Alloc), Frame Debugger for draw calls. Keep UI work within the frame budget the project sets; if none is set, report it as `NOT ASSESSED`.
- Device check: install a development build on a real phone for touch feel, frame rate, memory and audio.
