# T52F — Popup component delivery report

## Baseline and actual failure evidence

Verified `mac-current-policy/t52` was clean with `git status --short --branch`. Local `t52-current-game` resolved to `16b6f2474a466909d3f80e99180af5331141bb77`; `git merge --ff-only t52-current-game` successfully fast-forwarded the task branch from `ab84494` to that integrated baseline. No primary checkout or peer branch was mutated.

Coordinator's third Unity acceptance run on `16b6f24`: compile passed, EditMode 304/304 passed, PlayMode 0/17 passed. Common `OpenLevelCard` setup now fails with `MissingComponentException` for `CanvasGroup` on `Information cards`, at `PopupMotion.Begin:41`. The HUD startup fix is reported working. These are coordinator-provided actual observations, not worker runs.

Source diagnosis: `??` does not use Unity's overloaded object-null comparison. The component lookup can therefore skip `AddComponent` for a Unity fake-null wrapper, leaving the next native component access invalid. The observed exception is consistent with this branch error; this worker does not claim a runtime reproduction or confirmed passing fix.

## Exact source changes

Corrected six Unity-object coalescing expressions in four existing canonical files:

| File / method | Original baseline line | Correction |
| --- | ---: | --- |
| `PopupMotion.Begin` | 40 | Keep cached-group guard; assign `GetComponent<CanvasGroup>()`, then add only if `group == null`. |
| `PopupMotion.Track` | 52 | Assign the rect's existing CanvasGroup, then add only if `canvas == null`. |
| `GameArt.Get` | 28 | Obtain Sprite once; explicitly test `sprite == null` before the same missing-production-sprite exception. |
| `PanelsView.Unlock` | 100 | On `look == null`, preserve the warning and select the same `sunny_idle` fallback before assignment. |
| `PanelsView.DailyGift` | 214 | Obtain `gift_box_base` once and explicitly select the same `icon_basket` fallback only if Unity-null. |
| `BirdView.StartIdlePose` | 171 | Retain the separate quirk-action branch; obtain the ordinary idle Sprite once and select `idleFrame` only if Unity-null. |

No new resource fallback, cache reloading, lifecycle policy, allocation/helper abstraction, broad catch, null-return suppression, or test/assertion change was introduced. Existing normal-live-object behavior and fallback laziness are retained; missing/destroyed Unity objects now take the already-intended fallback/exception branch.

`PopupMotion.Begin` still cancels the previous generation, ensures motion, resets the root group's alpha/raycast/interactable state, and starts the same dim animation. `Track` still captures exactly the same pose fields and original alpha. Existing `Cancel` checks already use Unity-aware `!= null` and retain pose/button restoration, generation invalidation and transient cleanup. No change to close/input/timing behavior was made.

## Audit inventory and static verification

Searched all `??` expressions under `PerchSort/Assets/Scripts/Game`, not only same-line component additions. Initially found 15 expressions: six Unity-object cases above and nine legitimate nullable/managed cases.

Post-change search leaves exactly these nine expressions unchanged:

| Owner | Count | Type / reason |
| --- | ---: | --- |
| `Ambient` | 1 | Nullable `Color` tint. |
| `DevOptions` | 1 | Managed `DevOptions` returned by JSON deserialization. |
| `GameRoot` | 2 | Nullable Boolean notification flag and managed species string. |
| `PanelsView` | 1 | Managed acknowledgement Action. |
| `PopupFlow` | 2 | Managed `GameProfile` and `PlayerProfile` constructor inputs. |
| `ProgressStore` | 2 | Managed `DevOptions` defaults. |

No `GetComponent ... ??` or other Unity-object `??` remains in the assigned folder. This is a source audit result, not a Unity runtime assertion.

Executed only the static naming check:

`python3 ci/check_current_game_names.py`

Observed exit `0`: `Current game naming: 567 tracked text files inspected; 0 violation(s).`

Per worker instruction, **Unity, compiler, EditMode, PlayMode and runtime test execution were NOT_RUN locally**. No regression test was added; new tests have a separate owner. No passing runtime result is inferred from the naming guard or prior baseline compile/EditMode results.

## Delivery and coordinator acceptance

T52F changes only the four source files listed above, `tasks/T52F_popup_component.md`, and this report. Commit/push the isolated task branch and send its final SHA to the current Codex coordinator via the verified Mac-to-251 status route. No invented Task/Dispatch IDs or `worker_done`.

Coordinator must review/integrate and serialize the actual Mac rerun, including existing startup setup and all 17 PlayMode cases, compile/EditMode coverage, and real information-card/popup cancel/reopen/input restoration. The separate regression-test owner handles new coverage. The patch is ready for coordinator runtime acceptance, not claimed runtime-passing.
