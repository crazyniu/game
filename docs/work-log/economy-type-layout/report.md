# Economy type file layout report

## Scope and result

Baseline: `0805f87f50f29f06929d7f281a31a9a9423fe7b7`. Branch/worktree: `economy-type-layout`, `/Users/apple/orca/workspaces/game/economy-type-layout`. CONVENTIONS section 1.4 requires one namespace-level type per matching filename. Moved IClock, SystemClock, WinPayout and PayoutResult from Economy.cs to matching files in the same directory, namespace and PerchSort.Meta assembly. Economy remains in Economy.cs. Only needed System using directives were added to the two clock files.

All five declaration texts compare byte-for-byte with their original declarations, including modifiers, constructors, members, comments and arithmetic. Economy.cs equals the original file minus the four extracted declarations. No callers, tests, storage/schema values, enums, assembly definitions, gameplay, levels, scoring or economy behavior were changed. UI code, resources, layouts, presentation tests, scenes and assets are untouched. Repository gameplay proposals were not treated as authorization.

Actual changed files:

- PerchSort/Assets/Scripts/Meta/Economy.cs
- PerchSort/Assets/Scripts/Meta/IClock.cs and IClock.cs.meta
- PerchSort/Assets/Scripts/Meta/SystemClock.cs and SystemClock.cs.meta
- PerchSort/Assets/Scripts/Meta/WinPayout.cs and WinPayout.cs.meta
- PerchSort/Assets/Scripts/Meta/PayoutResult.cs and PayoutResult.cs.meta
- docs/work-log/economy-type-layout/spec.md and report.md

## Compatibility evidence

- Economy.cs.meta is byte-identical, retaining GUID `651cfe4a2c0324388807fef86bf95429`.
- New GUIDs use the repository's existing two-line Unity metadata template with distinct UUID4 values: IClock `f11d906fcf25452c95ba7dfe80107c5a`, SystemClock `9c965d749fd2480287f778306e104a40`, WinPayout `9875bef230d0463aa59d733e2c2eee48`, PayoutResult `e49c17f9201148438b4752db566b453d`.
- All **1,740** Assets metadata GUIDs are valid and unique. No existing metadata file changed.
- PerchSort.Meta.asmdef is unchanged, has noEngineReferences enabled, and directly owns the extracted files in the same folder. No nested asmdef/asmref or explicit project inclusion requires changes. Namespaces and public/internal identities remain unchanged. These are pure interface/class/struct declarations, not Unity components or serialized script classes.
- Tracked Unity scene/prefab/asset/template/inputactions/JSON/meta scanning found **zero** references to the original Economy script GUID outside its own meta. Existing type references in Aviary, GameRoot, PopupFlow, FakeClock and retained tests resolve the same qualified names; callers were read-only.
- The five files each contain exactly one matching namespace-level type. Per-declaration SHA-256 fingerprints and comparisons are preserved in `build/economy-type-layout/compatibility.log` alongside the check script.

## Completed checks and deferred acceptance

After implementation:

- `python3 build/economy-type-layout/check-compatibility.py`: **5/5 declaration comparisons**, original metadata equality, assembly inclusion, serialized-reference and GUID checks passed.
- `python3 ci/check_current_game_names.py --self-test`: **14/14 passed**, no skips/errors.
- `python3 ci/check_current_game_names.py`: **220 tracked text files**, zero violations; all four new sources were staged before inspection.
- `EVENT_NAME=workflow_dispatch GITHUB_OUTPUT=/tmp/economy-type-layout-ci-output.txt python3 ci/check_changes.py`: **28** current configuration/script files validated. This all-file static invocation selects Core/workflow jobs; it does not execute them locally.
- `git diff --cached --check`: passed.

Static logs, script and a **1,772-file** scene/meta/settings/package SHA-256 snapshot are under ignored `build/economy-type-layout/`. No Unity command or screenshot was started by this worker, so there are no generated import/scene changes from this batch. Local dotnet is not on PATH. Exact-head PR CI is checked and linked in the handoff; its current path selection does not compile Meta or select pure-Core/workflow lint jobs for this source-only Meta change. A green naming/configuration gate is not compilation or runtime acceptance.

Under the user's latest unified-verification instruction, **integrated Unity compilation and full EditMode/PlayMode/Meta compatibility suites remain pending with the coordinator** after all authorized edits are integrated. Earlier 318/122 counts are not results for this change. Actual new metadata import remains pending. Screenshots are inapplicable to this non-UI file organization change; no visual/device/listening acceptance is claimed. No additional bug was confirmed in this bounded extraction audit; broader naming findings and design suggestions were not repaired.

## Concurrency and preserved evidence

Live GitHub refs matched the baseline and there were no open PRs at initial inspection. Despite the handoff's completed-task summary, Orca showed active input-lock-fixes and aviary-flock-counts workspaces. Read-only checks found no changes to Economy.cs or the four new target paths in either workspace. Their dirty files and processes were left untouched. At preflight, another worker's AviaryFlockSmoke Unity process (PID 67789; flock PID 67788) held /tmp/unity.lock in the aviary-flock-counts worktree; this did not block static work and no new Unity run was attempted. Process/lock state must be checked again before future integrated verification.

Before closing the merged profile-type-layout worktree, its complete static/integrated evidence and spec/report were preserved in `/tmp/game-profile-type-layout-evidence/complete-acceptance.tar.gz` (28 entries), SHA-256 `8c3e0df45a4fb7f4e5ad006364252685e3ca65278ee6d936aec30e32ee40b16b`. Orca confirmed removal of only that completed worktree. The primary checkout and all other worker checkouts were never modified.

The new PR stays draft. The worker does not merge, mark ready, deploy or force-push. No implementation blocker was encountered; engine acceptance is deliberately pending, not passed or denied.


## Completed unified verification

After the batch was complete, the coordinator explicitly authorized one unified run, superseding the initial engine-test deferral above. No valid Unity evidence existed for this exact implementation; the earlier profile-layout run was not reused.

Read-only preflight found main `0805f87f50f29f06929d7f281a31a9a9423fe7b7`, PR #119 head `c5922460af3757ecafb32494b5f93a1728e875b4`, no active Unity process, and an available `/tmp/unity.lock` (`flock -n /tmp/unity.lock true` succeeded). The aviary-flock-counts worktree/process from the earlier inspection no longer existed. The active input-lock-fixes branch `mac-input-lock-fixes/work` was at `dda24fc4c51db8746114c0159a27c39cbda63432`, with uncommitted BirdhouseView.cs, BoardView.cs, GameRoot.cs and InputLockTests.cs changes: its documented scope and diff concern presentation playback/house animation callbacks and input handling, without Core/Meta changes. It was excluded under the no-UI boundary and left untouched. UI-only PR #121 was also excluded. Closed duplicate #120 was not incorporated. No new pending in-scope non-UI dependency was found; this is not a claim that every repository issue is resolved.

Fetched GitHub merge-preview **`9354635a3233fad82c257d1df90515fac43d5761`**, verified its exact parents are the main and PR-head commits above, and verified its tree **`6a9a9d606d4522ab1e0d1ef65694f0f211693b0a`** equals the PR head's tree. Tested that preview detached in this same isolated worktree; no local merge or main update was performed. Recorded 1,772 scene/meta/settings/package SHA-256 hashes from the clean tree before Unity.

Exact verification commands and results:

- `python3 ci/check_current_game_names.py --self-test`: **14/14 passed**, no skips/errors.
- `python3 ci/check_current_game_names.py`: **220 tracked text files**, zero violations.
- `flock /tmp/unity.lock bash scripts/run_tests.sh`: **one run only**, Unity **6000.6.7f1**, exit **0**. Scene build/compilation succeeded. EditMode **318/318 passed**, zero failed/skipped/inconclusive, **2026-11-05 10:48:55–10:49:09 UTC**. PlayMode **122/122 passed**, zero failed/skipped/inconclusive, **10:49:15–10:54:58 UTC**. No filters, test edits, weakening or repeat run. EditMode breakdown: game 251, Meta **53**, Audio 14; LevelsData **81** and Layout **41** retained.

This supplies real Unity compilation and full retained Meta compatibility coverage for the extraction, replacing the initial pending status for this source tree. It is not screenshot, UI, device, audio-listening or release acceptance. No screenshot command was run.

After the run, queried/fetched remote main again: it remained **`0805f87f50f29f06929d7f281a31a9a9423fe7b7`**. The exact main diff from the original base was empty, recorded in `main-diff.log`; no later code was silently treated as tested.

Evidence is under ignored `build/economy-type-layout/integrated/`: `scene-build.log`, `editmode-results.xml`, `editmode-tests.log`, `playmode-results.xml`, `playmode-tests.log`, `unity-acceptance.log`, `results-summary.json`, naming logs, `pre-unity-state.json`, `post-run-remote-refs.log`, `main-diff.log`, `generated-changes.json`, `generated-unity.patch`, `generated-review.log`, `scene-reconciliation.json`, `overlay-review.log`, `restoration.log`, and `reconcile-generated.py`. A separate preserved archive is `/tmp/game-economy-type-layout-integrated-evidence.tar.gz`, SHA-256 **`8647fdfaa053b183a7c9fd0a5e261940a91f3f00061d29e0dce6440f4be5a1e6`**.

Unity generated **908** tracked-file differences: **894** whitespace-only meta changes, seven egg/family meta serialization completions, six known world gate-door pivot resets, and scene reserialization. Every GUID stayed unchanged; the seven overlays retained their critical pivot/mesh/size/compression/physics settings. Scene document payloads match after ignoring numeric object/input-action file IDs and document order. The known gate hinge pivots were centered by the existing importer; no importer, UI or gate fix was authorized or made.

After the test process exited, preserved/classified the complete binary patch, verified each current generated hash and each original blob against the recorded baseline, then restored only those proven generated files. All **1,772** original hashes match again; the worktree was clean. No generated resource/meta/scene changes enter this PR. The primary checkout and other workers' dirty files/processes were never modified or stopped. Passing normal build/import/tests does not prove unchanged gate visuals or validate unimported serialized scene bytes; that remains an existing visual/importer boundary.

Returned to economy-type-layout and reran the declaration/GUID/static naming checks successfully. The subsequent follow-up changes only this report; all implementation content equals the tested preview. No second Unity run is required or performed for the report-only commit. The initial head's CI succeeded at https://github.com/crazyniu/game/actions/runs/42012853232 (14 naming tests, 220 files zero violations; Core/workflow jobs not selected). Final report-head CI is verified separately in the handoff. PR #119 stays draft; no merge, readiness change, deployment or force push was performed. No execution blocker remains for this validation.


### Main advanced after completed validation

During report publication, remote main advanced to **`2cbc1a456580fa33d74b7b45127444f6641587f0`**, the external merge of UI-only PR #121 at **2026-11-05 10:56:29 UTC**. This was after PlayMode ended at 10:54:58 and after the immediate post-run main check still showed 0805f87f. The exact later diff is https://github.com/crazyniu/game/compare/0805f87f50f29f06929d7f281a31a9a9423fe7b7...2cbc1a456580fa33d74b7b45127444f6641587f0 and is also saved as `build/economy-type-layout/integrated/later-main-diff.log` (separate from the earlier runtime evidence archive). It changes glyph metrics, GameCapture, BirdhouseView, layout/UI/HUD/map/settings/shop/hint presentation, presentation tests, glyph tooling and evidence/docs; **Core and Meta have no changes in that diff**.

The worker did not merge, rebase onto, modify or run Unity against that UI commit. The completed 318/122 results cover only preview 9354635a and its exact tree reported above, not the newer main/UI combination. Meta extraction implementation is unchanged, but whole-game acceptance of a merge with that newer main is not established by this run. The latest no-UI boundary prevents silently expanding this task to validate PR #121. The coordinator owns any decision about acceptance of that later base; no automatic repeat Unity run was started. Final-head CI is reported for its actual SHA and is not substituted for newer-base Unity acceptance.
