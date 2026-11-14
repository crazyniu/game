# Information confirmation report

## Completed
- Credits uses a fixed green painted OK outside the frame. Its scroll viewport is bounded separately, so dragging long text does not move or obscure the exit.
- Draw completion no longer presents a redundant corner X. Existing bottom OK still closes through the shared lifecycle and restores the original draw exactly once.
- Inventory found other single-acknowledgement reward, new-bird, tool-unlock and world-result dialogs already use bottom actions. Settings, purchase decisions, tutorial skip, ongoing draws, chapters and chest navigation remain untouched.
- Only Credits document assets exist; no nonexistent terms/privacy/contact page was added.

## Verification
Godot 4.7.2 standard GL Compatibility, isolated fake profiles.
- `GODOT=... bash scripts/run_tests.sh`: all repository guards/core/smoke/recovery/animation/goal suites passed.
- `run_aux_dialogs.gd`: 20 checks, zero failures (both languages; fixed action, scroll clamps, frame clearance, no duplicate X).
- `run_draw_result.gd`: 353 checks, zero failures (actual production fake-profile reward grants, repeated confirmation, exactly-once restoration, owner teardown).
- `run_menu_navigation.gd`: 95 checks, zero failures (new confirmation navigation and unchanged neighboring Close controls).
- `git diff --check`: passed.
- Real cloud desktop GL runtime captures in Chinese/English for Credits and draw result; narrow portrait requested at 360x780, respecting the project's configured minimum window size. The action remains visible and centered, and does not overlap the document viewport. Captures are ignored under `build/screens/`, not committed.

Focused test processes emit existing exit-time ObjectDB/resource lifetime diagnostics; assertions passed but this is not a clean resource-leak certification. No new animation was introduced or global system-back behavior changed; existing shared dismissal/return callbacks remain in place. User visual review of these particular two screens is pending. No automatic merge or deployment requested.
