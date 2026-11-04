# T52F — Unity-aware popup component lookup

## Scope and prerequisites

Own `PerchSort/Assets/Scripts/Game/PopupMotion.cs`, other concrete unsafe Unity-object `??` expressions under `Scripts/Game`, this specification, and `tasks/T52F_report.md`. Do not edit gameplay, actual tests/assertions, scene/data/art, or the primary checkout. New regression tests have a separate owner. The current Codex coordinator is the sole Unity runner and performs serialized post-integration acceptance.

First verify the task branch is clean, then fast-forward to local `t52-current-game` at `16b6f2474a466909d3f80e99180af5331141bb77`. Keep the task branch isolated. No worker Unity/compiler/test runs; a static naming guard is permitted.

## Actual runtime failure supplied by coordinator

Third acceptance run on `16b6f24`: compile passed, EditMode 304/304 passed, PlayMode 0/17 passed because common `OpenLevelCard` setup failed. The reported exception is `MissingComponentException` for `CanvasGroup` on `Information cards`, at `PopupMotion.Begin` line 41. The previous HUD startup issue is reported fixed.

Do not rerun these failures to confirm them. They are coordinator observations; source inspection diagnoses the likely Unity fake-null branch error, not a separately reproduced worker result.

## Required change

C# `??` tests managed-reference nullness and does not invoke `UnityEngine.Object`'s native-object validity comparison. `GetComponent<CanvasGroup>() ?? AddComponent<CanvasGroup>()` can therefore retain a missing/destroyed Unity wrapper instead of creating the required component.

- In `PopupMotion.Begin`, preserve the existing cached-group check, retrieve the group, then use explicit `group == null` before `AddComponent<CanvasGroup>()`. Keep the alpha/raycast/interactable reset and dim animation unchanged.
- In `PopupMotion.Track`, retrieve the rect's group and add it only after explicit `canvas == null`; preserve the existing pose capture and cancellation restoration.
- Audit every `??` expression under `Scripts/Game`. Replace concrete Unity-object left operands with explicit Unity-aware checks, keeping the same fallback values, exception messages and laziness. Leave ordinary managed-reference and nullable-value defaults unchanged.
- Do not introduce helper frameworks, new fallback resources, catch-and-continue paths, null-return suppression, cache reload policies, or weakened test setup.

## Exact audit findings

Six Unity-object expressions require correction across four source files:

| File / method | Left operand and existing fallback |
| --- | --- |
| `PopupMotion.Begin` | `GetComponent<CanvasGroup>()` → add CanvasGroup |
| `PopupMotion.Track` | `rect.GetComponent<CanvasGroup>()` → add CanvasGroup |
| `GameArt.Get` | `Optional(name)` Sprite → existing missing-production-sprite exception |
| `PanelsView.Unlock` | optional curious-bird Sprite → existing `sunny_idle` |
| `PanelsView.DailyGift` | optional gift-box-base Sprite → existing `icon_basket` |
| `BirdView.StartIdlePose` | `idlePoses[action]` Sprite → existing `idleFrame` |

Nine expressions remain deliberately unchanged: nullable `Color` in Ambient; managed DevOptions parse/defaults (three total across DevOptions/ProgressStore); nullable Boolean and managed species string in GameRoot; managed acknowledgement Action in PanelsView; and managed GameProfile/PlayerProfile constructor defaults in PopupFlow (two).

## Acceptance

Static audit must leave no Unity-object `??` expression in the assigned folder and retain all nine legitimate managed/value cases. `python3 ci/check_current_game_names.py` must pass. Report exact edited files and no worker runtime pass.

Coordinator must integrate/review, rerun compile and the existing 304 EditMode / 17 PlayMode coverage, and exercise actual information-card startup, popup tracking with existing/missing groups, cancel/reopen and button restoration. Separately owned regression tests may cover missing/destroyed Unity objects; this worker does not add or modify them. Runtime acceptance is not established by source review or the naming guard.
