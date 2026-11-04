# T52G — notification acceptance test delivery

## Delivery and scope

DONE for the bounded tests-only implementation on `mac-current-entry/t52`, based on **16b6f2474a466909d3f80e99180af5331141bb77**. The branch was clean before the user-requested fast-forward from local `t52-current-game`. The commit SHA is supplied in the coordinator/terminal submission.

Exactly four new paths:

1. `PerchSort/Assets/Tests/PlayMode/NotificationFlowTests.cs`
2. `PerchSort/Assets/Tests/PlayMode/NotificationFlowTests.cs.meta`
3. `docs/work-log/notification-acceptance/spec.md`
4. `docs/work-log/notification-acceptance/report.md`

No runtime, existing test/assertion, scene, resource, asmdef, CI, H-owned sapling region, peer fix, art original or primary-checkout changes. No additional worker was started. No production Save was disabled or bypassed.

**Worker Unity/compilation/EditMode/PlayMode/GUI execution: NOT_RUN.** The coordinator owns integrated serial execution. Existing reported PopupMotion fake-null failures and the separate sapling/motion corrections are not reinterpreted as passing results or worked around in these tests. The user reported F fixed startup for 12/17 existing PlayMode cases, with 3 sapling-fixture and 2 motion cases separately owned, and a d930a6f capture produced 34 stable PNGs; none validates these new tests.

## Added permanent acceptance cases

| UnityTest | Genuine behavior and failure boundary |
| --- | --- |
| `DailyPaysOnlyOnAcknowledgementAndSurvivesTimedCancellationAndReload` | Fresh persisted profile and actual map daily notification. No +50 or receipt before acknowledgement. Disable/re-enable before payment replaces the request; stale request cannot claim its replacement. Real visible-button acknowledgement commits +50, duplicate acknowledgement during timed collection cannot repay. Interrupt the live collection after durable payment, re-enable and reload; stale requests and same-day eligibility remain unpaid. Mute changes, best-moves compatibility key, balance and receipt survive actual root saves/reload. |
| `FirstLevelIntroThenSunnyAndSkyeEachPayOnceAndPersist` | Only unrelated daily receipt is preclaimed. Real L1 wipe/intro blocks moves and notification delivery. Sunny then Skye are delivered after intro and pay +20 each only on their actual acknowledgement controls. Persisted receipts commit separately. Stale Sunny request cannot consume/pay Skye. Actual scene reload and L1 re-entry do not pay either again. |
| `FirstFinalePaysOnceThenGateAndUnlockAcknowledgementReleaseDeferredLevel` | Genuine authored L10, normal intro, Solver transfer/swivel actions through GameRoot, ordinary win persisted without +200. Next produces WorldComplete(1); acknowledgement pays +200 once. Real timed map GateView.IsOpening must occur before WorldUnlock(2) acknowledgement becomes available; early acknowledgement cannot release L11. Unlock acknowledgement resumes deferred L11, consumes its continuation, and grants no world-2 reward. Reload plus real L10 replay must not repeat world-1 payout. |
| `LastFinaleAcknowledgementReturnsToMapWithoutADeferredLevelFortyOne` | Genuine authored L40 solved through root moves. Ordinary win does not prepay world reward. WorldComplete(4) acknowledgement pays +200 then returns to map, without fifth-world notification, L41 node or deferred next level. Duplicate request/reload cannot repay; last-level best moves, balance and world-4 receipt persist. |

The fixture adds **4 UnityTests** to the unchanged existing **17**, giving a source-derived expected integrated PlayMode count of **21**. No Ignore/Explicit, expected-error suppression or conditional omission. This is not an executed discovery/pass count. The existing script's earlier minimum does not itself establish discovery of the four new cases; coordinator must verify all four named cases and total 21 in actual results rather than infer coverage from a minimum check.

## Real integration and bounded setup

- Loads the actual enabled **Game** scene with normal serialized GameRoot/Start initialization and default notification-enabled persistence. No LoadFixture, OpenPopup, Initialize(false), reflection or mock root/ledger acknowledgement path.
- `SceneManager.sceneLoaded` injects a deterministic **Meta.IClock** before Start (2032-06-15); actual startup computes eligibility using that clock. `DevOptions.OverrideForTests = new DevOptions()` disables synthetic progression/stock flags.
- Uses real ProgressStore for seeds, persisted assertions and reloads; Meta.Economy claim previews and RewardLedger receipt queries establish paid/unpaid eligibility. Target receipts are never preclaimed. Finale-only profiles seed unrelated daily/species/prior-world receipts with real Meta APIs and mark unrelated booster cards seen; their balance is explicitly set to 100 for payout isolation. L1 friends and daily each begin unclaimed in their focused cases.
- Actual acknowledgement buttons drive the positive claims. The root request API is used only for duplicate/stale/early-ack rejection checks. No presentation-only popup is opened to pretend a notification was scheduled.
- Authored finale search retains **200000 states / 3000 ms**; Unknown fails rather than skips. Actions go through root.TryMove/root.TapSwivel, wait for committed playback and assert real Win, state and persisted best moves. No handcrafted winning board or direct GameSession action bypass.
- Daily popup/collection/cancellation and L1 intro/friend rewards use real timed motion. L10's post-reward map/gate/unlock/intro sequence is also timed. Authored solving and irrelevant stable setup use instant motion to bound the tests. State waits use 15-second realtime deadlines; fixed 0.2-second intervals intentionally exercise cancellation during live presentation, not screenshot acceptance.

## PlayerPrefs and lifecycle safety

The fixture is NonParallelizable because it temporarily owns the real ProgressStore keys. It snapshots every integer key from **ProgressStore.Keys()**, separately snapshots the notification JSON string, and records absence so missing keys remain absent after restoration. It introduces no new versioned key literals or storage exception.

The user relayed H's finding that seeding prefs while an outgoing root is alive lets **OnDisable → Save** overwrite the replacement seed. This fixture explicitly follows the corrected lifecycle:

1. Capture the original preferences before any mutation.
2. Unload outgoing Game into a temporary empty cleanup scene before clearing/seeding. **Boot itself also awaits UnloadGame before ProgressStore.Save(profile)**, so seed safety does not depend only on SetUp.
3. Real Reload does **not** reseed or suppress Save; LoadSceneAsync replaces the old scene normally, exercising genuine OnDisable persistence.
4. UnityTearDown unloads the active test Game before restoring keys. Its finally block restores every integer/string snapshot, saves preferences, unsubscribes sceneLoaded and restores DevOptions.OverrideForTests, GameMotion.Instant and Time.timeScale, including failure cleanup. No DeleteAll.

Mute is exercised through root.SetMuted and the existing MutedKey; best moves use the existing BestMovesKey helper and actual solver wins; receipts are inspected only through real loaded profile APIs. The temporary cleanup scene is not a new asset/build entry or alternative implementation.

## Observed static evidence

- Reviewed actual GameRoot initialization, notification pumping/acknowledgement, finale/deferred navigation, disable/enable/save lifecycle; ProgressStore key inventory; PopupFlow receipt/continuation policy; Meta Economy/Aviary; current gate/intro/button timing; existing PlayMode asmdef and metadata convention.
- Four `[UnityTest]` attributes; unchanged existing source contains 17. Static source scan found no LoadFixture/OpenPopup/Ignore/Explicit/DeleteAll/log-error-suppression or version-numbered identities in the new fixture.
- Lexical C# comment/string/delimiter sanity passed. Boot unload-before-save ordering and real reload's lack of reseeding were statically inspected. This is not C# parsing/compilation; no C# parser/compiler was available/used.
- New metadata GUID **0205587b81f5470baef085826fa51b61**; no collision in active Assets metadata.
- Ran only the permitted static naming audit after tracking the new test/meta: `python3 ci/check_current_game_names.py` → **569 tracked text files inspected; 0 violations**. It inspects source/paths and does not compile or execute Unity/tests.
- New test source SHA-256: `38dd4a27c7daaa1ff223568ecedf6c382e05c9646fe2310fffb1d7a6360d70ed`. Best-moves assertions retain the minimum across real replay attempts, rather than incorrectly requiring a later attempt to overwrite an earlier better record.

## Pending coordinator verification

Compile/import and run the integrated suite after incorporating the separately owned runtime/fixture fixes. Confirm all **21 PlayMode** cases are discovered, including the four names above, with no skips or ignored failures. Confirm the current 304 EditMode baseline remains unaffected. These new tests have **not** been executed, and no pass is claimed.

Runtime-only risks include actual sceneLoaded/Start scheduling, popup input readiness and stale callback teardown after the independent fake-null fix, gate-opening observability and timed cancellation, deterministic authored solver completion under the existing budget, and teardown restoration on an actual runner failure. Those are the behavior these tests are intended to catch, not reasons to bypass production flows or weaken assertions. GUI/timed visual acceptance remains distinct from coroutine result acceptance and static PNG output.
