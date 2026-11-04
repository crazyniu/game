# mac-ui — HOLD for Codex coordinator ownership split

## Checkpoints
- Branch remains `mac-ui/t50`. Do not switch checkout or merge until the current Codex coordinator provides the exact checkpoint/ownership plan.
- **Last review-ready checkpoint: `1abcee5`.** This contains the integrated v7 worlds/coloured boosters and interleaved-board fix. Its locked suite passed **453/453 EditMode; 31 PlayMode passed, 0 failed, 8 skipped**. Graphics captures are in [build/screens/v7/](https://github.com/crazyniu/game/blob/cb438e87e67e6d66c97bdf00b77ef356ca161290/docs/team/status/mac-ui.md#L5), including K3/K7, coloured Owl Eye and [v7_world_w1.png](https://github.com/crazyniu/game/blob/cb438e87e67e6d66c97bdf00b77ef356ca161290/docs/team/status/mac-ui.md#L5) through [v7_world_w4.png](https://github.com/crazyniu/game/blob/cb438e87e67e6d66c97bdf00b77ef356ca161290/docs/team/status/mac-ui.md#L5). These numbers/captures apply to `1abcee5`, not the WIP below.
- **Isolated code WIP: `9f84a95`**, `wip: preserve popup queue and intro integration for parallel ownership split`.
- **Do not merge `9f84a95` into main.** It is mid-integration, not compile/test/review-ready. All current source edits and both existing new `.meta` files are preserved in that commit; nothing was reset/deleted.
- No tests, Unity runs, all-level playthrough, merges, checkout changes or worker dispatch were performed after the latest pause instruction. No sub-workers were started during this resumption. The last owned Unity lock was released after the previous green run.

## Exactly what the unverified WIP contains
- New [V7PopupFlow](https://github.com/crazyniu/game/blob/cb438e87e67e6d66c97bdf00b77ef356ca161290/docs/team/status/mac-ui.md#L11): ordered notification state, immutable Meta new-species preview, acknowledgement-time met/+20 and daily/+50 claims, first-finale pending bits, world-unlock continuation, deferred Next and a serialized reward-ledger/balance/continuation snapshot.
- [V5Progress](https://github.com/crazyniu/game/blob/cb438e87e67e6d66c97bdf00b77ef356ca161290/docs/team/status/mac-ui.md#L12): owns that state and saves it as string key `v7.notifications`, included in `Keys()`; first-win recording occurs before stars are updated.
- `Meta/Economy`: adds `ClaimWorldComplete`, sharing the existing one-time world receipt and 200-coin constant with `OnWin`. Existing `OnWin` semantics were not intentionally changed. The Game queue is responsible for qualifying the committed first finale.
- `GameRoot`: routes map/level/win exits through one notification pump; acknowledgement delegates capture request identity to reject stale double clicks; input/modal guards cover intro and notification transitions; cross-world Next is deferred until the sequence finishes. Clock is injectable. Nonpersistent capture/fixture notification control is explicit.
- New [V7LevelEntrance](https://github.com/crazyniu/game/blob/cb438e87e67e6d66c97bdf00b77ef356ca161290/docs/team/status/mac-ui.md#L15): approximately 1.6s skippable entrance orchestration (house drop, row growth, bird/egg entry, chrome fade); completion gates new-friend/booster delivery. Integration methods were added to HUD/scene/branch/bird/board views.
- [V5Panels](https://github.com/crazyniu/game/blob/cb438e87e67e6d66c97bdf00b77ef356ca161290/docs/team/status/mac-ui.md#L16): acknowledgement callback and mandatory-notification dismissal guard; Rook gets 'Never goes home.' instead of a gift; world-unlock sign selects the actual world.
- New [V7GateView](https://github.com/crazyniu/game/blob/cb438e87e67e6d66c97bdf00b77ef356ca161290/docs/team/status/mac-ui.md#L17): aligned T48 leaves/hinges/light/padlock, shake/fall/swing motion. [V6MapView](https://github.com/crazyniu/game/blob/cb438e87e67e6d66c97bdf00b77ef356ca161290/docs/team/status/mac-ui.md#L17) uses these gates, focus-then-unlock completion and coins-to-pill/counting motion.
- These changes are **not verified** and are not claimed as complete Phase 2 animation work.

## Known unfinished work / immediate integration risks
1. No new EditMode/PlayMode coverage has been written or run for this WIP. Compilation has not been checked.
2. Existing `GameplayTests`/[V5DevOptionsTests](https://github.com/crazyniu/game/blob/cb438e87e67e6d66c97bdf00b77ef356ca161290/docs/team/status/mac-ui.md#L22) preference guards assume every [V5Progress.Keys()](https://github.com/crazyniu/game/blob/cb438e87e67e6d66c97bdf00b77ef356ca161290/docs/team/status/mac-ui.md#L22) entry is an integer. They must preserve/delete/restore the new string key correctly before running suites; otherwise tests can damage the saved notification snapshot.
3. Existing gameplay fixtures assume no automatic daily/new-friend modal and fixed starting coins. Seed acknowledged Meta receipts deliberately for unrelated legacy regressions, and add independent fresh-profile tests for the actual new popup flow; do not bypass the feature globally.
4. EditMode/PlayMode test asmdefs currently do not directly reference `PerchSort.Meta`; add that reference when new tests use Meta clocks/profiles.
5. Intro, stale-callback cancellation, acknowledgements across reload, duplicate-reward prevention, popup ordering/no stacking, first/next-day/rollback daily rules and deferred cross-world Next still need actual verification and smoke captures.
6. [V7GateView.cs.meta](https://github.com/crazyniu/game/blob/cb438e87e67e6d66c97bdf00b77ef356ca161290/docs/team/status/mac-ui.md#L26) did not exist at the WIP inventory. Do not run Unity during this hold; include it when a later authorized import generates it. [V7LevelEntrance.cs.meta](https://github.com/crazyniu/game/blob/cb438e87e67e6d66c97bdf00b77ef356ca161290/docs/team/status/mac-ui.md#L26) and [V7PopupFlow.cs.meta](https://github.com/crazyniu/game/blob/cb438e87e67e6d66c97bdf00b77ef356ca161290/docs/team/status/mac-ui.md#L26) are already preserved.
7. Full win/popup bird choreography, random per-bird idle scheduling and the remaining Phase 2 timed sequences are still pending. Remaining T49 bird/egg/overlay deliveries are also not complete at the last integration fetch.

## Changed/new files in `9f84a95`
Paths below are relative to [PerchSort/Assets/Scripts/V5/](https://github.com/crazyniu/game/blob/cb438e87e67e6d66c97bdf00b77ef356ca161290/docs/team/status/mac-ui.md#L30).

### Modified (10)
- `Game/BirdView.cs`
- `Game/BoardView.cs`
- `Game/BranchView.cs`
- `Game/HudView.cs`
- `Game/GameRoot.cs`
- [Game/V5Panels.cs](https://github.com/crazyniu/game/blob/cb438e87e67e6d66c97bdf00b77ef356ca161290/docs/team/status/mac-ui.md#L38)
- [Game/V5Progress.cs](https://github.com/crazyniu/game/blob/cb438e87e67e6d66c97bdf00b77ef356ca161290/docs/team/status/mac-ui.md#L39)
- [Game/V5SceneView.cs](https://github.com/crazyniu/game/blob/cb438e87e67e6d66c97bdf00b77ef356ca161290/docs/team/status/mac-ui.md#L40)
- [Game/V6MapView.cs](https://github.com/crazyniu/game/blob/cb438e87e67e6d66c97bdf00b77ef356ca161290/docs/team/status/mac-ui.md#L41)
- `Meta/Economy.cs`

### New (5)
- [Game/V7GateView.cs](https://github.com/crazyniu/game/blob/cb438e87e67e6d66c97bdf00b77ef356ca161290/docs/team/status/mac-ui.md#L45)
- [Game/V7LevelEntrance.cs](https://github.com/crazyniu/game/blob/cb438e87e67e6d66c97bdf00b77ef356ca161290/docs/team/status/mac-ui.md#L46)
- [Game/V7LevelEntrance.cs.meta](https://github.com/crazyniu/game/blob/cb438e87e67e6d66c97bdf00b77ef356ca161290/docs/team/status/mac-ui.md#L47)
- [Game/V7PopupFlow.cs](https://github.com/crazyniu/game/blob/cb438e87e67e6d66c97bdf00b77ef356ca161290/docs/team/status/mac-ui.md#L48)
- [Game/V7PopupFlow.cs.meta](https://github.com/crazyniu/game/blob/cb438e87e67e6d66c97bdf00b77ef356ca161290/docs/team/status/mac-ui.md#L49)

## Interfaces to preserve while the coordinator assigns ownership
- A: `BirdView.Enter(start, target, delay, airborne, landed, arrived)`, `BoardView.Enter(bird, start, target, delay, branch)`, `BranchView.Entrance(progress)` are used by the intro orchestrator.
- B: [V5Panels.IsNotification](https://github.com/crazyniu/game/blob/cb438e87e67e6d66c97bdf00b77ef356ca161290/docs/team/status/mac-ui.md#L53) and `Popup(kind, species, reward, acknowledge, world)` are used by the notification pump. Acknowledgement must remain the only claim boundary; preview popups grant nothing.
- C: [V5SceneView.Entrance(alpha)](https://github.com/crazyniu/game/blob/cb438e87e67e6d66c97bdf00b77ef356ca161290/docs/team/status/mac-ui.md#L54), [V6MapView.UnlockGate(world, done)](https://github.com/crazyniu/game/blob/cb438e87e67e6d66c97bdf00b77ef356ca161290/docs/team/status/mac-ui.md#L54) and `CollectCoins(from, to, done)` completion/cancellation boundaries are used by root input/queue integration.
- Shared assignment still needed: [V7GateView.cs](https://github.com/crazyniu/game/blob/cb438e87e67e6d66c97bdf00b77ef356ca161290/docs/team/status/mac-ui.md#L55) is currently consumed by the map and is a candidate shared door widget for popup presentation. Do not let B/C edit it concurrently.
- Retained integration scope requested by coordinator: popup queue/profile/root/input/intro, including `HudView` entrance hooks and the Meta claim bridge as assigned in the exact plan. No ownership assumptions will be acted on before that plan.

HOLD: all implementation edits paused. Report/questions go to the current Codex conversation through this terminal/status, not the former Claude channel.
