# T52H — Sapling acceptance triage report

## Outcome

Fixed the replacement-profile fixture ordering in [GameplayTests.cs](../PerchSort/Assets/Tests/PlayMode/GameplayTests.cs), without changing runtime behavior. No independently demonstrated defect in `GameRoot`, `TutorialHand` or `HudView` warranted a production edit. Runtime confirmation remains NOT_RUN by assignment.

## Baseline and authority

- Current Codex coordinates; no retired Claude session or Dispatch was used.
- Initial `git status --porcelain=v1` was empty on `mac-current-runtime/t52`, HEAD `e21f9b417a2dbab44dc75bcd97cf2b87d98ac9c5`.
- Authorized `git merge --ff-only t52-current-game` succeeded to `d930a6f6616d50d882c4e343d50f0d32fd6529ea` in this existing worktree. No primary-checkout edits.
- Coordinator-provided results, not worker execution: compilation + 304 EditMode PASS, PlayMode 12/17. The three reported Sapling errors were accepted as evidence and not rerun.

## Root cause: source evidence

1. The old `PrepareSaplingIntro` persisted a replacement profile while the setup's L1 root was still alive. It then called `ReloadSavedGame`, whose single-scene load destroyed the outgoing root.
2. [GameRoot.OnDisable](../PerchSort/Assets/Scripts/Game/GameRoot.cs) calls `Save()` before `CancelPresentation()`. `Save()` captures the outgoing session inventory and writes its entire in-memory profile through `ProgressStore.Save`.
3. [ProgressStore.Save](../PerchSort/Assets/Scripts/Game/ProgressStore.cs) writes highest unlocked level, tutorial flags, booster inventory, starter flags, unlock flags and notification receipts. Consequently, the outgoing root can overwrite the fixture's newly seeded L4 progression and stock during reload.
4. `GameRoot.LoadLevel(4)` silently returns when `4 > HighestUnlocked`; initialization creates an L1 session and opens the map. Thus the previous helper never proved that L4 actually loaded. [INFERENCE from lifecycle/source trace, consistent with all three reported failures]: stale L1/map state caused the hidden hand, absence of an active Sapling button and zero stock.
5. [HudView.SetLevel](../PerchSort/Assets/Scripts/Game/HudView.cs) exposes Sapling only at its progression unlock. [TutorialHand.Begin/Refresh](../PerchSort/Assets/Scripts/Game/TutorialHand.cs) requires L4, unseen tutorial, positive stock and unblocked gameplay. These guards are correct for the current contract and were not bypassed.
6. The unchanged authored catalog entry is L4/world 1/noon/capacity 4/par 8, with five ordinary branches and sunny/sky/berry birds. No replacement board or alternate level was introduced.

## Fix and preserved acceptance

- `PrepareSaplingIntro` creates an empty staging scene, makes it active and awaits unloading the old Game scene **before** saving the new profile. This follows the existing teardown's empty-scene/unload pattern. It lets the old root finish its legitimate shutdown save before fixture replacement and leaves no live old root to overwrite the seed.
- Reuses `LegacyGameplayProfile`, existing acknowledged Meta receipts and deterministic fixture clock. Preserves highest unlocked 4, exact supplied stock, Sapling starter-already-granted, all unlock cards seen and the requested tutorial flag. Normal saved-game reload and real `LoadLevel(4)` remain in use; no `LoadFixture`, reflection or runtime notification bypass.
- Adds precondition assertions for progression 4, saved tutorial flag before entry, actual L4, non-map surface, idle/unblocked presentation and exact stock. An incorrect seed or rejected level load now fails at the fixture boundary instead of masquerading as a tutorial defect.
- Retains every original behavior assertion. Additionally requires the branch tap to be accepted; requires actual L4/non-map/exact stock after the persistence reload; and requires the visible Sapling button to be interactable before invoking it.
- Motion methods, common setup/teardown/reload helpers, runtime files, NotificationFlowTests, art, catalog, save keys and resources are unchanged.

## Verification and limits

- Source inspection covered profile capture/persistence, level-load guards, map initialization, Sapling tutorial conditions, HUD visibility/input, notification receipt gates and instant intro/wipe completion. `GameMotion.Instant` already executes the update/completion inline; this fixture adds no sleeps or longer timeouts.
- C# LSP discovery returned `No language servers configured for this project`; no compiler/LSP diagnostics are claimed.
- Non-executing delivery checks PASS: `GameplayTests.cs` outside the Sapling helper/three-test region is byte-identical to `d930a6f`; all 17 UnityTest declarations are retained (not executed); `GameRoot.cs`, `TutorialHand.cs` and `HudView.cs` are byte-identical to that baseline; all seven task/report relative links resolve; touched text has no trailing whitespace. These checks establish source ownership/document links, not runtime success.
- **NOT_RUN:** Unity/import/compilation, EditMode, PlayMode, code tests, runtime smoke, screenshots and visual/motion acceptance. The user expressly prohibits worker execution; current Codex serializes those checks. No historical passing result validates this patch.

## Coordinator integration

Review/cherry-pick the focused task commit from `mac-current-runtime/t52`; do not take the full baseline fast-forward as new T52H implementation. Run the three named Sapling tests in the serialized integrated Mac acceptance pass, including the two preparations in `EmptyStockOrSeenFlagSuppressesSaplingIntro` and the restart/reload persistence path. Keep C's new notification fixture and D's motion delivery independent. The previous PlayMode 12/17 is baseline only, not a post-fix result.
