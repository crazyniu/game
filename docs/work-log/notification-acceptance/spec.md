# T52G — genuine notification-flow PlayMode acceptance

## Authority and baseline

User-assigned tests-only coverage task. The clean `mac-current-entry/t52` branch was fast-forwarded to local `t52-current-game` at **16b6f2474a466909d3f80e99180af5331141bb77** before changes. Current Codex coordinator reviews/integrates and serially runs Unity acceptance on the Mac. The worker does not run Unity, compilation or code tests and does not touch the primary checkout.

Existing 17 PlayMode tests seed daily/friend/world receipts to isolate earlier gameplay. They do not establish genuine startup, acknowledgement, persistence or finale notification acceptance. The reported PopupMotion fake-null failure is independently owned and must not be worked around here.

## Exclusive ownership

- `PerchSort/Assets/Tests/PlayMode/NotificationFlowTests.cs`
- Its paired `.meta`
- `docs/work-log/notification-acceptance/spec.md`
- `docs/work-log/notification-acceptance/report.md`

No runtime, resource, scene, existing test/assertion, asmdef, CI, guideline or peer-task changes. Do not add skips, expected-error suppression, new versioned persistence literals, fake roots, mock payouts, alternate scenes or broad harnesses.

## Contract and coverage

Use the real Game scene, serialized GameRoot and normal Start initialization with notifications enabled. Set a deterministic Meta.IClock in sceneLoaded before Start. Keep `DevOptions.OverrideForTests = new DevOptions()` so normal progression/economy applies. Read/write the real ProgressStore and invoke real acknowledgement controls/request APIs. Do not use LoadFixture, OpenPopup or direct ledger acknowledgement to fabricate notification delivery.

1. **Daily gift and persistence:** fresh actual profile, no preclaimed daily receipt. Before acknowledgement no coins or receipt; cancellation creates a new request and rejects the stale one. The visible acknowledgement commits exactly +50 once. Interrupt actual timed coin collection after the durable claim; disabling/re-enabling, duplicate/stale acknowledgement and scene reload must not repay it. Verify stored coins, mute and best-moves values through existing ProgressStore helpers and actual root reload.
2. **L1 new friends:** seed only the unrelated daily receipt. Observe the actual level intro blocking input and notification delivery. After intro, Sunny then Skye each require acknowledgement and pay +20 once; receipts commit individually, stale Sunny acknowledgement cannot consume Skye, and replay after scene reload does not pay either again.
3. **L10 finale:** use real authored level loading, intro and Solver actions through root TryMove/TapSwivel; no direct session win or fixture. Ordinary win payout excludes +200. Next produces WorldComplete(1), acknowledgement pays +200 once, then the actual timed map gate opens before WorldUnlock(2) can be acknowledged. No early L11. WorldUnlock acknowledgement releases deferred L11. Reload and a genuine L10 replay must not repeat the world payout.
4. **L40 finale:** solve the real authored last level through root moves. Ordinary win excludes the world payout; WorldComplete(4) acknowledgement grants +200 once and returns to the map. No fifth-world popup, deferred level or L41 node; receipt, balance and best moves survive scene reload.

Use Meta.Economy claim previews/receipt queries to verify actual once-only eligibility and paid state; retain real claim APIs for seeding unrelated receipts only. Finale-focused profiles may meet unrelated species, mark unrelated booster cards seen, and preclaim prior worlds, but never preclaim the notification under test. Solver budget remains the current 200000 states / 3000 ms; Unknown fails, never skips. State waits have realtime deadlines; no unbounded coroutine loops.

## Isolation and acceptance

- Snapshot every integer key enumerated by ProgressStore.Keys and the notification JSON string, preserving missing keys as missing. Never DeleteAll.
- Unload any outgoing Game scene before seeding so OnDisable saves cannot overwrite the test profile.
- UnityTearDown unloads the test root before restoring preferences; finally restores every snapshot even after an assertion/unload failure, unsubscribes sceneLoaded and restores DevOptions, GameMotion.Instant and Time.timeScale.
- Keep this fixture nonparallel because it owns real shared PlayerPrefs during each case. No new production isolation seam.
- At least one reward transition/cancellation uses real timed motion. Daily and L1 are timed; L10 additionally observes real GateView.IsOpening before unlock acknowledgement. Stable-state helpers may use instant motion for bounded authored solves.
- Four new UnityTests, no Ignore/Explicit or failure suppression. Existing 17 remain unchanged: expected integrated PlayMode discovery becomes **21**, subject to actual coordinator discovery/execution. Static source counts are not a passing test result.
- Static review checks API signatures, notification ordering/receipt boundaries, snapshot restoration, asset GUID uniqueness, naming policy and ownership. Worker runtime validation is NOT_RUN; coordinator must integrate the separate runtime correction and serially compile/run all tests/acceptance.

Commit/push only this task branch and report exact SHA, paths, static evidence and untested limits to current Codex through the supplied Orca route. No main merge, additional workers or primary-checkout mutation.
