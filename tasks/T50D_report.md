# T50D — Notification, intro and root integration delivery

## Submission

**DONE — D source accepted for integration after coordinator static re-review of bf643f4. All A/B/C/D/E and final assets are integrated in source; no runtime acceptance.** Current final state and main reconciliation are owned by [final T50 report](T50_report.md) and the Mac-only handoff; historical review boundaries below remain available.

- Source commit: **`52cad38e2742263b4469ead8ef5a90d80f564b73`**.
- Branch: `mac-notifications/t50`; workspace: `/Users/apple/orca/workspaces/game/t50-notifications`.
- Baseline: handoff `0b44966`, preserved source WIP `9f84a95`.
- Isolated D review boundary: **`08b32a733ab952f79ca66f3424696f498a2f6720`**, including fixture correction **`e39e0ddc5f330d35ff416a609f0eefbb20e7fcad`**. Approved A/C integration is separately recorded below; the final terminal block identifies the pushed delivery tip.
- Integration lifecycle correction: **`f24c11f`**, settling/canceling any pending Win HUD count before entering the mandatory finale popup sequence. It changes only D-owned root code; the isolated D review boundary above remains available.
- Coordinator: current native Codex conversation on 251, `orca_session_id:codex_0551b7e6_4ea7_4c29_936b_a50a25d9c1ec`. No former Claude recipient, Dispatch ID or sub-worker. Separate D commits were retained before integrating only approved A `a012e70`, C `fb90bb7` and B `4256de63412041e65ddf2554cb14323d79f6d888`.
- Specification: [T50D](T50D_notifications.md). Current-stage directives and four real workspaces: [Codex handoff](../docs/team/HANDOFF_2026-11-04_CODEX.md). Durable worker status: [mac-notifications](../docs/team/status/mac-notifications.md).

## Coordinator blocker follow-up

The coordinator's completed parallel static review of `3a05763` confirmed one blocker: busy input called InputTap before Defer, incrementing the same revision captured by PlayCommitted. An ignored airborne tap or queued landing/effect tap therefore discarded the committed Board completion and could leave IsBusy set indefinitely. This finding is accepted as ground truth; no reproduction/test was run.

Bounded correction: V5GameRoot now gives each PlayCommitted run its own transactionGeneration. InputTap still changes revision, preserving hint-result and delayed hint-presentation invalidation, but does not own the committed Board completion. New committed runs and CancelPresentation advance transactionGeneration; destroy also invalidates it. The completion still checks root lifecycle, session identity and its existing finished one-shot before calling FinishAction.

Static source trace only:
- Airborne ignored input advances only revision; the committed transaction token remains current.
- Landing/effect input retains the unchanged one-slot queuedTap rule. FinishAction clears IsBusy and consumes that slot once; it is no longer skipped by unrelated input revision changes.
- Undo/Restart/navigation/root disable call CancelPresentation and invalidate both generations, including same-session cancellation. Session replacement and destruction remain separately guarded.
- Solver result and post-playback hint callbacks retain their original revision/session checks; stale hints stay rejected.

No public API, input/raycast policy, reward/receipt/economy, queue cardinality, peer implementation or test source changed in this follow-up. Correction **`bf643f4b513e60e7a24bc241e5907562a7087ecd`** was committed/pushed before the separately approved asset merges and passed final coordinator static re-review. Its changed files are only V5GameRoot and report/status/integration handoff. Execution/compilation/Unity/tests remain NOT_RUN.

## Separately approved offline asset integration

- Bird assets `fd1446948f58d5d8c4fe9f5b201b75eb15d5edb9` merged conflict-free as `5c22b064b7cab8d6c702d4a6b9321963cd474b2d`.
- Egg/overlay assets `f79dc3875ca7b6138e9c304059c2d3432a5c2775` (asset `34eddeb`) merged conflict-free as `b9d019c5aa6d9fe204fa8d6dd0d7d807a5013db1`.
- Both exact commit objects were verified locally. These are explicit coordinator-approved asset revisions, not generation/edit work by D. D's blocker-correction boundary is preserved; no BirdView or other code was changed for these merges.
- Coordinator approval covers the offline 170 distinct 320×320 RGBA bird frames/ten portraits and the seven egg/overlay assets, their metadata/style/contracts/sheets. Import, in-context rendering and animated/runtime behavior remain NOT_RUN; no historical/asset-sheet evidence is promoted to runtime evidence.
- Coordinator-approved E **`2b2592560e8a7128e35e874e046bfa7dfd97f00f`** is integrated at **`83a4d93db4b811598a923f7926fb73389ec81a4b`**: BirdView plus internal helper/report, authored Sunny/Skye family markers and normalized overlay anchor/scale/pose routing. Primary and independent static review passed; old offsets are no longer used for the reviewed V7 poses.
- All bounded code/assets are accepted for integration, not runtime. No remaining code-feature TODO; main PR/merge and user-test checkout update belong to the coordinator after final tip review. No Unity/test execution or unrelated code scope.

## Changed files

Source under `PerchSort/Assets/Scripts/V5/Game/`:

- `V5GameRoot.cs`: notification/navigation lifecycle, committed-win persistence, exact popup order, deferred Next, last-world map return, full-canvas leaf ownership, node-origin card integration and callback-timed HUD counting.
- `V7PopupFlow.cs`: persistent contextual friend/booster requests, map-return continuation, receipt/order handling and suspension of level-only requests on the map.
- `V7LevelEntrance.cs`: skippable entrance cancellation, generation guards, settled board/chrome poses, tutorial/input boundary and allocation-free indexed branch updates.
- `HudView.cs`: independent cancel-safe coin-count channel, completion overload, intro chrome/staggered bottom-button motion and disable cleanup.

Documentation:

- `tasks/T50D_notifications.md`
- `tasks/T50D_report.md`
- `docs/team/status/mac-notifications.md`
- `docs/team/HANDOFF_2026-11-04_CODEX.md`

Coordinator-authorized fixture compatibility corrections (no execution, no new test cases), commit **`e39e0dd`**:

- `PerchSort/Assets/Tests/V5/PlayMode/V5GameplayTests.cs`
- `PerchSort/Assets/Tests/V5/EditMode/V5DevOptionsTests.cs`
- `PerchSort/Assets/Tests/V5/PlayMode/PerchSort.V5.PlayModeTests.asmdef`
- `PerchSort/Assets/Tests/V5/EditMode/PerchSort.V5.EditModeTests.asmdef`

Existing `V5Progress.cs` and `Meta/Economy.cs` are intentionally unchanged: their WIP `v7.notifications` string snapshot and `ClaimWorldComplete` receipt bridge already supply the required integration. No new source asset was created by D; existing `.meta` files remain unchanged. D changed no Core, authored levels, economy amounts/eligibility, libraries, artwork, scenes, project settings, shared layout/art/motion utilities or original user-test checkout. Only explicitly authorized existing fixtures/asmdefs changed.

## Implemented requirements

| Requirement | Source behavior |
| --- | --- |
| Intro before friends/cards | Every normal notification-enabled level start/restart runs the 1.6-second entrance; skip settles board/branches/houses/chrome and completes once. Tutorial/input is held until intro then mandatory notifications finish. Bird/egg flight/landing uses the existing T50A-owned Enter contract. |
| New friends | Meta Aviary preview supplies canonical debut order, including Rook. Only matching acknowledgement commits met/+20. Preview presentation does not grant rewards. Rook's no-home copy remains in the popup-owned view. |
| Booster cards | Follow all new friends. Existing unlock-seen-on-presentation policy and public APIs remain. T50B's generation-guarded Once protects button callbacks; no extra booster API is required. |
| Daily | Eligibility/claim uses the injected Meta calendar clock. Map-only delivery, acknowledgement-time +50/save, then one Map.CollectCoins collection/count operation. Same-day, rollback and duplicate-claim behavior remains Meta's unchanged receipt rule. |
| First finales | A committed winning action records the existing ordinary win reward, stars/best moves and first-finale pending bit before choreography. This prevents teardown during the final flight from losing an earned finale notification. The +200 itself still commits only at world-complete acknowledgement through the shared one-time Meta receipt. |
| Finale exit order | Leaving Win via Next/Home releases world-complete → map gate unlock → world-unlock → map daily. A Replay does not prematurely release its pending finale receipt. World 4 has no unlock popup and returns to the map after completion/counting. |
| Deferred Next | Persisted while the world/map sequence runs. Consumption occurs only when the destination level is actually built, with the resulting profile save; cancellation before the covered rebuild leaves the continuation available. |
| One popup | The root pump respects modal/busy/transition boundaries, holds one request identity and ignores mismatched acknowledgements. Each paid acknowledgement is saved before coin presentation; stale/canceled callbacks cannot grant again. |
| Persistence/reload | Reward ledger/balance, finale/unlock bits, deferred Next, map-return intent and contextual friend/booster queues share the notification snapshot. Transient Current/intro/tween state is not revived after reload. Map presentation suspends level requests; starting a level derives its own species/card queue from durable receipts. |
| Teardown/rebuild | Root invalidates callbacks, cancels leaf/map/board/HUD/modal/scene/intro presentation and clears transient input locks. Re-enable rebuilds the committed board or map, resumes pending receipts, or restores Win without another grant. Intro cancellation restores poses and suppresses its old completion. |
| Leaf transition | One independently configured full-Canvas layer covers map↔level changes, including finale map entry. Old presentation cancellation happens before Play. The covered callback calls BuildLevel/map-surface setup, not LoadDefinition/CancelPresentation, so it neither cancels nor increments away its own wipe token. Completed starts intro/pumping. External cancellation invalidates the run. |
| Level card origin | Root always queries the displayed map-node origin and passes its out value to the consumed-once popup setter before LevelCard, including the `(540,960)` failure fallback. |
| Win count timing | Root invokes the existing HUD numeric count only from approved Win.Show coinFlightStarted, capturing before/after totals. The HUD retains the old balance until that point. The callback has revision/once guards and performs no grant or save. Views own visual flights; root owns reward/persistence. |

No owned implementation requirement is intentionally omitted. Integration with reviewed peer commits and runtime/visual acceptance remain outstanding stage work, not completed evidence.

## Exact APIs

### Added by T50D

Namespace: `PerchSort.V5.Game`.

```csharp
public void HudView.CountCoins(int from, int to, Action done);
public bool V7PopupFlow.NeedsMap { get; }
```

The two-argument `HudView.CountCoins(int from, int to)` remains available and delegates to the new overload. A successful count restores its final value and invokes done once. Superseding/canceling/disable invalidates the old run, restores the current committed balance and discards its continuation. Root cancellation clears the corresponding root lock; canceled callbacks must not resume stale navigation.

No existing public root, profile, queue, entrance or HUD signature was removed/changed. All navigation logic remains root-owned.

### Peer APIs consumed through reviewed integration

```csharp
// T50C
public void V7LeafTransition.Configure(V5Art art);
public void V7LeafTransition.Play(Action covered, Action completed);
public void V7LeafTransition.Cancel();
public bool V7LeafTransition.IsPlaying { get; }
public bool V6MapView.TryGetLevelCardOrigin(int id, out Vector2 point);
// Existing C contracts
public void V6MapView.UnlockGate(int world, Action done);
public void V6MapView.CollectCoins(int from, int to, Action done);
public void V5SceneView.Entrance(float alpha);

// T50B, coordinator-approved additions
public void V5Panels.SetLevelCardOrigin(Vector2 point);
public void WinPanel.Show(int earnedStars, int reward, int chorusCoins, Action coinFlightStarted);
```

Existing `V5Panels.Popup(V7PopupKind, string, int, Action, int)`, `Unlock(BoosterKind)`, `Hide()`, three-argument Win.Show, and gate `Configure(V5Art,int,bool)` / `Open(Action)` / `IsOpening` remain unchanged. T50A's Board.Enter/Cancel/Settle and branch Entrance hooks remain unchanged.

The isolated D review boundary intentionally contained no copied peer implementation. Explicitly approved A `a012e70` merged as `d8d00f73c98de48088238fa9f1c76406ccad1af6`, C `fb90bb7` as `8dd09021b9524cd08af97bea7bd034adda2a18ef`, then B `4256de63412041e65ddf2554cb14323d79f6d888` as **`3a057636325fa017aecbbcb074766eeb79e01b46`**; all merges were conflict-free. B's exact local commit object was verified before merge; no peer workspace/branch was modified. All root-facing peer symbols are now present and their signatures were statically checked. Compilation and runtime remain NOT_RUN; no reflection, placeholder or fake fallback was introduced.

Approved B removes the pre-ack daily currency flight. Map.CollectCoins after root acknowledgement is the only daily collection spectacle. Win.Show's callback supplies the actual flight start; root does not copy its 1.55-second implementation constant or create a second flight.

After A/C integration, a root-only static correction in `f24c11f` clears the deferred Win count flag and cancels/settles the HUD count when leaving Win for a finale. This covers programmatic early Next/Home as well as ordinary button exits: a canceled Win coin-flight callback cannot leave the HUD holding its old amount beneath the world-complete popup. No reward or receipt changes occur in that cancellation.

## Static compile-risk review performed

- Read the owning T50 task/handoff/design, approved level/new-friend/daily/world-complete/world-unlock images, relevant root/profile/queue/HUD/intro/Meta/view/motion sections, T50C delivery report and T50B callback implementation read-only.
- Checked public callers in Game, editor captures and existing V5 tests; preserved their existing public signatures. New overload uses System.Action and existing UnityEngine.Vector2, within the existing Game namespace/assembly.
- Checked Game asmdef: Core, Meta, Audio, LitMotion, UGUI, TextMeshPro and InputSystem references already exist. No runtime assembly/package reference changes are required. Both existing test asmdefs now explicitly reference Meta for the V5Profile receipt-aware constructor and PlayMode's deliberate Meta seed operations.
- Checked notification bit ranges/order, immutable Meta receipt use, shared balance/receipt serialization, current-request identity, root/intro/count generation invalidation, completion detachment and the separation between covered rebuild and external cancellation.
- Checked ordinary win/Chorus amounts still come from existing V5Profile.Reward; new-friend/daily/world amounts still come from existing Meta operations. Only the ordinary reward commit is moved before visual playback for durable interruption handling; +20/+50/+200 remain acknowledgement-only.
- Checked existing V7PopupFlow/V7LevelEntrance metadata is present; no source file or GUID was regenerated.
- C# language-server status reports no configured server. No LSP diagnostics, compiler or runtime success is claimed. Static review cannot prove Unity lifecycle ordering, serialized compatibility under actual import or actual timing/rendering.

## Explicit NOT_RUN / untested limits

Per the latest user instruction: **Unity/import/compilation, EditMode, PlayMode, code tests, runtime smoke, all-level playthrough, screenshots, timed animation sequences, profiler/device/safe-area checks are NOT_RUN.** No new tests were authored. Historical `1abcee5` results are not evidence for this code. PR35 CI validated configuration/scripts only and did not trigger Core tests.

Future authorized verification must exercise reload/disable during intro, friend acknowledgement, world-complete count, gate animation, daily collection and deferred navigation; repeated skip/Undo/Restart; first/replay finales including level 40; midnight/date rollback; synthetic test-stock coin display; real popup/coin timing and full-screen leaf coverage. These are untested limits, not claimed passes.

The coordinator authorized the existing fixture correction rather than leaving the known string-key hazard. Both fixtures now preserve/restore `v7.notifications` separately as a string, including absent versus present-empty values; all integer keys retain their prior handling. PlayMode's unrelated gameplay cases use explicit already-met species and acknowledged daily/world receipts created through Meta APIs, then reset the seed balance to the existing expected coins. A fixture-only injected clock at `DateTime.MaxValue.Date` makes the already-acknowledged daily state independent of the host date, including the initial scene load; notification scheduling is not globally disabled. The Sapling fixture's replacement profile uses the same receipt seed. Gameplay/progression assertions remain unchanged. These corrections are statically reviewed only; execution remains NOT_RUN.

## Integration and pause

Final source authority: the coordinator accepted D `bf643f4` and E `2b25925`, alongside A/B/C and final assets. Approved pre-main runtime/art tree is `83a4d93db4b811598a923f7926fb73389ec81a4b`; incoming main is `3e82c64734f3e7291a332d1c0e90263954ce40cd`. This worker reconciles main into the isolated task branch only, preserving accepted source and current Codex directives, and finalizes T50 report/handoff. No main/original checkout update is claimed. After publication HOLD for coordinator final tree/PR review; later feature work is parallelized, OMP preferred/Codex fallback, Mac-only. No tests/Unity/runtime acceptance or automatic T51/new feature.
