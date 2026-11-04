# T52E — HUD startup delivery report

## Failure evidence supplied by the coordinator

Integrated commit `475131a`: Unity compile passed; EditMode 304/304 passed; all 17 PlayMode cases failed in common startup `SetUp` (0/17 passed).

The reported startup error was `Can't add component CanvasGroup to Settings because already added`, followed by a null-reference exception in `HudView.Entrance:210`, through `LevelEntrance.Cancel` / `GameRoot.OpenMap`. The failing chained assignment dereferenced `gearFade`, which came from an unsuccessful duplicate `AddComponent<CanvasGroup>()` on the HUD gear button. `Settings` here is not the settings modal.

These are actual coordinator-provided observations. This worker did not rerun them. D's previously accepted naming guard result (567 files, zero violations) is also coordinator-reported and was not re-executed for T52E.

## Changes

Only `PerchSort/Assets/Scripts/Game/HudView.cs` and the two T52E task documents are changed by this follow-up:

1. In `Configure`, replace the gear button's duplicate component creation with `settings.GetComponent<CanvasGroup>()`.
2. In `Entrance`, keep coin/tool/text alpha driven by the existing `controls` factor, and drive gear alpha with `controls * (settings.IsInteractable() ? 1 : GameLayout.DisabledAlpha)`.

The second adjustment is necessary because the reused group also owns `GameButton`'s state opacity. It preserves the existing disabled-alpha rule instead of letting entrance/cancellation overwrite a disabled gear with full opacity. It follows the same `IsInteractable()` decision as `GameButton.DoStateTransition`. Enabled entrance opacity and all position, rotation, tool settling, scale, and cancellation inputs are unchanged.

No broad catch, fallback group, additional null guard, assertion removal, test/setup change, settings-modal change, or gameplay change was introduced. Original D guard files are untouched. No other CanvasGroup sites were edited; the independent reviewer owns their audit.

## Source-level initialization/cancellation review

Read the owned HUD initialization, `Entrance`, `Refresh`, `Cancel`, and `OnDisable` paths, plus the existing button-construction and button-state implementation read-only:

- `GameUI.Button` adds `GameButton` before returning. `GameButton` requires CanvasGroup; retrieving it after button creation respects that ownership and does not allocate a duplicate group.
- The coin plate and bottom-bar group remain owned by their separate, non-button containers.
- `Configure` initializes the gear group, other alpha targets, houses layer, tools, effects, and chorus synchronously. Normal post-configuration startup cancellation can use them even before any houses have been added.
- Before configuration, the existing `motion == null` cancellation guard remains unchanged. This patch does not suppress failures in incomplete configuration.
- `Cancel` still increments the coin revision, cancels both motions, stops coin counting, restores coin text, cancels each house, calls `Entrance(1, 1)`, and removes transient presentation.
- With controls restored to `1`, enabled gear opacity is `1` and disabled gear opacity is `GameLayout.DisabledAlpha`. At controls `0`, gear opacity is `0`; between those endpoints, the same entrance factor modulates the appropriate state opacity.
- `Refresh` retains ownership of settings interactability. Button scale/press state code is untouched.

This is source inspection and reasoning, not runtime verification. The edit tool applied the exact bounded source changes; no Unity behavior or test pass is inferred from that result.

## Verification limits and required coordinator run

Per explicit worker instruction, **no Unity, compiler, or test execution was performed**. No new suite or mock runtime was added. The existing 17 startup tests are the regression coverage and must be rerun by the coordinator after integration, together with the serialized compile/EditMode workflow and actual map/entrance/cancel/disabled-gear surface checks.

Acceptance remains pending coordinator runtime results: no duplicate CanvasGroup error, no startup `HudView.Entrance` exception, all existing startup tests passing, and preserved entrance/cancellation plus disabled presentation. Independent review of other CanvasGroup sites may produce separate work; this patch does not claim to resolve findings outside `HudView.cs`.

## Submission

Use isolated branch `mac-current-policy/t52`, without merging C/main or modifying the primary worktree. Commit/push the owned source and two T52E documents only. Deliver the final SHA to the current Codex coordinator using the verified Mac-to-251 status route; no Task/Dispatch identifiers or `worker_done` are used.
