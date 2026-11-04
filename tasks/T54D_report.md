# T54D — Uniform buttons

## Delivery status

Source implementation and three new regression cases are ready for coordinator review. Unity compilation, EditMode, PlayMode, GUI acceptance and image evidence are **NOT_RUN / NOT_PRODUCED**. This is not a runtime acceptance claim.

The user explicitly selected **push now and transfer Unity validation to the coordinator** after being informed that the primary-checkout Editor was open and project instructions require coordinator-owned Unity runs. This supersedes this worker's original requirement to capture evidence and run the full suites before pushing; it does not waive coordinator acceptance.

- Branch: `mac-t54d/t54`.
- Assigned contract tip: `cb438e87e67e6d66c97bdf00b77ef356ca161290`.
- Implementation commit: `f1b31dd` (`Keep disabled buttons full size and align tool geometry`).
- No merge, main modification, artwork generation, save-key change or existing-test change.

## Problem P4 and changes

### Shared `GameButton`

`PerchSort/Assets/Scripts/Game/GameButton.cs` no longer reads `GameLayout.DisabledScale`. Disabled states use unit scale regardless of their dimming policy. Alpha dimming and Unity's existing non-interactive behavior remain. Only an enabled pressed state targets the existing press-feedback scale; disabling during a press immediately restores unit scale and cancels the pending transition.

Existing press/release timing, sprites and easing are unchanged. `GameLayout.cs` remains untouched; its now-unused disabled-scale constant is outside this worker's ownership.

### HUD geometry and badges

`PerchSort/Assets/Scripts/Game/HudView.cs` centers every tool on its existing authored center and uses one common size per toolbar: the undo rectangle for that layout. The three-tool layout remains 108 by 108 design units. The four-tool layout uses 128 by 132 for every tool, removing the fourth tool's former 132 by 132 exception. The fresh tutorial retains its minimal 64-unit control.

All inventory badges now take their vertical reference from the same authored hint badge. Their common button width also gives them an identical local horizontal offset. Existing 46-unit / 50-unit badge sizes remain per layout. Undo continues to have no inventory badge because it is history, not consumable stock.

### Other shared-button consumers inspected

Read-only inspection confirmed that settings, map bottom-bar controls and popup actions use `GameUI.Button`, which constructs `GameButton`. The shared disabled-scale fix therefore applies to these consumers without edits to their owners. This is source-path evidence only, not GUI proof. The HUD gear's existing temporary input-lock dimming exception is preserved.

## New regression coverage

`PerchSort/Assets/Tests/PlayMode/T54DButtonTests.cs`, with its Unity metadata, adds:

1. `ThreeToolsKeepEqualGeometryWithUndoDisabledAndEnabled`: authored level 5, actual legal move 0 to 3, disabled/enabled undo, equal tool rects/local and rendered scales, equal badge geometry, then timed disable/re-enable transitions.
2. `FourToolsKeepEqualGeometryWithUndoDisabledAndEnabled`: authored level 40, move 3 to 6, the same geometry/state checks for all four active tools.
3. `DisablingAPressedToolRestoresItsSizeAndRejectsTheClick`: real tool press feedback, disable mid-press, reject the undo click without consuming history, remain full-size after the old animation window, and settle after a subsequent press/release.

The fixture initializes the real Game scene with persistence and notifications disabled, supplies explicit stock through the existing `LoadFixture` API, and restores global motion/time/dev-option state. It does not seed or alter PlayerPrefs. No existing assertions or tests were weakened or skipped. These three cases have **not been compiled or executed locally**.

## Verification and exact boundaries

| Check | Command / action | Observed result |
| --- | --- | --- |
| Branch creation | `git switch -c mac-t54d/t54` | Created assigned branch from the provided contract tip |
| Naming policy | `python3 ci/check_current_game_names.py` | PASS; zero violations |
| Unity process preflight | Process inspection | User Editor PID 93788 was open on `/Users/apple/Documents/projects/game/PerchSort`; it was not killed or modified |
| Full suites | `flock /tmp/unity.lock bash scripts/run_tests.sh` | **NOT_RUN**, transferred to coordinator by explicit user choice; no EditMode or PlayMode counts available |
| Timed animation evidence | Real Unity frame sequence / contact-sheet JPEG | **NOT_PRODUCED** |
| Bottom-bar crops | Undo disabled and enabled, three- and four-tool levels | **NOT_PRODUCED** |
| GUI acceptance | Settings, map, popup and HUD buttons | **NOT_RUN** |

The baseline 304 EditMode / 21 PlayMode figures in the task contract are expectations, not results from this branch. No Unity invocation was made, including no competing process or unprotected run. No substitute static or synthetic artwork is supplied as runtime evidence.

## Coordinator communication

- Initial local delivery to the documented Codex session failed because that session is not on the local host; no message was sent there.
- The blocker was successfully delivered to the same documented Codex session through the configured remote Orca host: receipt `msg_34dafd883c1f`.
- The user's explicit validation-transfer decision was delivered with receipt `msg_39d82dc2b26a`.

## Requested contract change and remaining acceptance

No additional production-file ownership is requested. The only approved delivery change is transferring runtime validation and evidence to the coordinator.

After the user closes their Editor, the coordinator must serialize Unity with `flock /tmp/unity.lock`, run the full test script, report exact discovery/pass/failure counts, and inspect the real animated UI. Capture fixed-interval press/release and disable/re-enable frames, make a contact-sheet JPEG no wider than 1600 pixels under `tasks/evidence/T54D/`, and supply bottom-bar crops with undo disabled and enabled for both toolbar counts. No screenshot or animated-behavior acceptance should be inferred from this report.
