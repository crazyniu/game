# T52H — Sapling acceptance triage

## Authority and ownership

Current Codex is the coordinator. Work only in the existing `t52-current-runtime` Mac worktree, starting with a clean-tree check and fast-forward to local `t52-current-game` at `d930a6f6616d50d882c4e343d50f0d32fd6529ea`. Do not edit the primary checkout.

Owned code: the Sapling preparation helper and three Sapling tests in [GameplayTests.cs](../PerchSort/Assets/Tests/PlayMode/GameplayTests.cs). `GameRoot.cs`, `TutorialHand.cs` and `HudView.cs` are conditionally authorized only for a demonstrated runtime defect. Motion methods are not owned. C owns new NotificationFlowTests; D owns motion runtime. No Unity, compilation or test runs: the coordinator serializes execution.

## Reported acceptance failures

Coordinator-supplied baseline: compilation and 304 EditMode cases pass; PlayMode is 12/17. This worker must not rerun those checks to confirm them.

- `SaplingIntroPersistsOnShowAndBranchTapDismissesWithoutSpending`: inactive hand at old line 311.
- `SaplingButtonDismissesIntroAndUndoDoesNotRepeatIt`: no active Sapling button at old line 326.
- `EmptyStockOrSeenFlagSuppressesSaplingIntro`: expected stock 1, actual 0 at old line 343.

## Requirements

1. Trace profile seeding, outgoing-root persistence, reload, progression and actual authored L4 selection. Distinguish stale fixture state from a runtime defect.
2. Correct the root cause without ignoring tests, suppressing notifications globally, substituting `LoadFixture`, adding stock grants or weakening assertions.
3. Preserve gameplay, art, all 40 authored levels, save keys and current canonical identities. Do not create numbered or alternate implementations.
4. Keep changes focused, commit/push the task branch and send an English evidence report directly to the current Codex mailbox specified in the assignment. No Dispatch exists; do not invoke `worker_done`.

## Acceptance

- Outgoing scene shutdown completes before replacement Sapling profile persistence; an old root cannot overwrite the seed during reload.
- Each preparation verifies unlocked progression 4, the requested pre-entry tutorial flag, actual L4 gameplay rather than the map/stale L1, settled unblocked presentation and exact requested inventory.
- Existing behavior checks remain: hand position, durable once-on-show flag, branch dismissal without spending, restart/reload suppression, clickable Sapling growth, consumption/Undo refund, no repeated prompt, and suppression for zero inventory/already-seen flags.
- Only owned Sapling code and these task/report files change. Runtime remains untouched unless evidence demonstrates a separate defect.
- Report distinguishes static inspection from execution. Compilation, Unity lifecycle, PlayMode and visual acceptance remain coordinator-owned and unverified by this worker.

## Delivery

See [T52H report](T52H_report.md). The coordinator must rerun the three named cases in the serialized integrated Mac acceptance run; this task's static delivery is not a passing-runtime claim.
