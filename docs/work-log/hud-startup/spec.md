# T52E — HUD settings-button startup ownership

## Scope and authority

Own only `PerchSort/Assets/Scripts/Game/HudView.cs`, this specification, and `tasks/T52E_report.md`. Implement the bounded runtime acceptance fix on the existing isolated D branch; do not merge C/main or edit the primary worktree. Do not change gameplay, actual tests, assertions, startup setup, or the settings modal. The current Codex coordinator accepts the delivery and serializes Unity/compiler/test execution on Mac. Another independent reviewer owns the audit of other CanvasGroup sites; do not broaden this patch into that audit.

No Unity, compiler, or test execution is authorized in this worker. Existing 17 startup tests supply regression coverage; do not add a mirroring suite. Write English task documentation and commit messages, commit/push only the three owned paths, and report the final SHA through the verified coordinator route.

## Actual failure evidence

Coordinator runtime acceptance on integrated commit `475131a`:

- Unity compile passed.
- EditMode: 304/304 passed.
- PlayMode: 0/17 passed; all failed in common `SetUp`.
- Startup error: `Can't add component CanvasGroup to Settings because already added`.
- Subsequent null-reference failure: `HudView.Entrance:210`, at `coinFade.alpha = gearFade.alpha = toolFade.alpha = coins.alpha = controls`, through `LevelEntrance.Cancel` / `GameRoot.OpenMap` startup.

These are coordinator-observed results, not worker executions. D's naming guard is separately reported accepted/integrated with 567 files and zero violations; do not rerun that accepted work here.

## Cause and minimal correction

`GameUI.Button` creates `GameButton`, which has `[RequireComponent(typeof(CanvasGroup))]`. The HUD `Settings` object is the gear button, not `SettingsPanel`. `HudView.Configure` must obtain its already-required CanvasGroup with `GetComponent<CanvasGroup>()`, not add another component and retain the failed addition's null return.

The same group is used by button state transitions, which apply `GameLayout.DisabledAlpha`. In `HudView.Entrance`, preserve this state by setting gear alpha to `controls * (settings.IsInteractable() ? 1 : GameLayout.DisabledAlpha)`. Keep the other existing alpha assignments, geometry, timing inputs, button scale behavior, and cancellation sequence unchanged. Do not introduce catches, fallback component creation, or new null-return suppression.

## Initialization and cancellation invariants

- `Configure` creates the settings button before obtaining `gearFade`; the required CanvasGroup already exists when `GameUI.Button` returns.
- `coinFade` belongs to the coin plate and `toolFade` to the bottom-bar container; neither container is a `GameButton`, so their existing component creation remains unchanged.
- All objects used by `Entrance` are initialized synchronously during successful `Configure`, before normal startup cancellation. Do not change the pre-configuration `Cancel` guard or hide partial configuration failures.
- `Cancel` still invalidates coin callbacks, cancels both motions, resets coin display, cancels houses, calls `Entrance(1, 1)`, and clears transient presentation.
- `Entrance(1, 1)` restores final geometry and alpha: enabled gear alpha is `1`; disabled gear alpha is `GameLayout.DisabledAlpha`. An empty house dictionary at map startup is valid.
- An entrance control factor of `0` hides the gear; intermediate factors retain the existing fade envelope while respecting its enabled/disabled opacity.

## Coordinator acceptance

After integration, rerun the actual Unity compile, existing EditMode coverage, and all 17 PlayMode startup tests in the coordinator's serialized Mac workflow. Require no duplicate-CanvasGroup startup error and no `HudView.Entrance` null reference. Confirm map startup, level entrance, interruption/cancellation, and gear enabled/disabled presentation on the actual game surface. Source review is not a runtime pass, and historical passing results do not validate this patch.
