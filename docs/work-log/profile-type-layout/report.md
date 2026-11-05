# Profile type file layout report

## Change and compatibility

Baseline: `821242aa738bc16f369cd3eb55b6217248246795`. Branch/worktree: `profile-type-layout`, `/Users/apple/orca/workspaces/game/profile-type-layout`. Current refs and workspaces were checked before editing; no open PR or conflicting owner of these files was found. The collection and glyph workspaces were left untouched.

CONVENTIONS section 1.4 requires one namespace-level type per matching file. This batch fixes two containing files by moving three declarations:

- `Scripts/Meta/ProfileText.cs` -> `Scripts/Meta/ProfileMigrator.cs`: public ProfileMigrator, still in PerchSort.Meta. ProfileText and its nested Data contract remain in the original file.
- `Tests/Meta/ProfileEconomyTests.cs` -> `Tests/Meta/Profiles.cs`: internal Profiles helper, still in PerchSort.Meta.EditModeTests.
- The same test file -> `Tests/Meta/FakeClock.cs`: internal FakeClock helper, same namespace and visibility.

All five retained/extracted declaration texts compare exactly with the baseline; the two original files equal their baseline text minus the extracted declarations (apart from the final separating blank line). No test declaration, assertion, serialized field, DataMember name, migrator literal, enum, class identity or behavior changed. New files have only their needed using directives and namespace wrappers. Existing references from CollectionProgressionTests and GameRoot resolve the same type names; neither caller was edited.

The existing PerchSort.Meta.asmdef and PerchSort.Meta.EditModeTests.asmdef are unchanged. Unity includes the new .cs files in those same directories/assemblies automatically; no nested asmdef/asmref intervenes. No explicit project file enumerates the moved declarations. The pure-Core CI project does not compile or run Meta fixtures, so its passing result is not Meta compilation proof.

Original metadata is byte-identical:

- ProfileText.cs.meta: `2ba3a6a002e014ebf87ee2e8665d5502`.
- ProfileEconomyTests.cs.meta: `9d48d83aa13da4c9ab6d45d997e83f0e`.

New metadata uses the repository's existing `fileFormatVersion: 2` / 32-hex GUID template with distinct UUID4 identifiers: ProfileMigrator `9cbe9ca05f194bf1bfd3af8ae4667cea`, Profiles `d8e283ae97f64d96afdbbf65d655781c`, FakeClock `8a8d845f8f9343afae19cba8804fcd9d`. All 1,736 Assets metadata files have valid unique GUIDs. Actual Unity import remains part of deferred integrated acceptance.

The naming guard transfers exactly its existing seven-literal allowance from ProfileText.cs to ProfileMigrator.cs. The new regression checks exact set equality, absence of the former owner's exception, acceptance at the new owner, rejection at the old owner, and rejection of extended keys. Other allowances are unchanged.

## Validation and limits

Completed in the new worktree:

- `python3 /tmp/check-profile-type-layout.py`: five exact declaration comparisons passed; two original metas byte-identical; five files each have one matching namespace-level type; two assembly definitions unchanged; 1,736 valid unique asset GUIDs.
- `python3 ci/check_current_game_names.py --self-test`: **14/14 passed**, no skips/errors.
- `python3 ci/check_current_game_names.py`: **216 tracked text files**, zero violations (new files staged so they are included).
- `EVENT_NAME=workflow_dispatch GITHUB_OUTPUT=/tmp/profile-type-layout-ci-output.txt python3 ci/check_changes.py`: **28** configuration/script files validated; Core and workflow jobs selected.
- `git diff --cached --check`: passed.

The compatibility script and logs are preserved in ignored `build/profile-type-layout/` (`check-compatibility.py`, `compatibility.log`, `naming-self-tests.log`, `naming-check.log`). `state.json` records SHA-256 hashes for 1,768 scene/meta/settings/package files, including the three new metas. No Unity process or test run was started for this batch, and no generated scene/import differences exist. No UI code, assets, resources, presentation tests, GameRoot, PopupFlow, importer or gate metadata was changed.

The coordinator's later instruction supersedes per-batch engine testing: finish authorized fixes first, then perform one integrated verification. **Full Unity compilation, Meta compatibility tests, complete EditMode/PlayMode suites and their discovery counts remain PENDING with the coordinator.** Earlier 318/122 results are baseline history, not results for this change. Screenshots are not applicable to this non-UI file organization change and were not run. No visual, device or audio acceptance is claimed. Exact-head GitHub CI is required for the draft PR; its final run and commit are returned in the handoff. The draft must not be marked ready or merged by this worker.

## Previous evidence and remaining scope

Before closing the completed naming-audit worktree, its logs/XML/hash snapshots/generated patch and audit documents were preserved as `/tmp/game-naming-audit-evidence/acceptance.tar.gz`, SHA-256 `5876bc39fb92dd235b646208b55d6528614fcf7a0b7bca03b68e495ca49302b9`. Orca confirmed that old worktree was removed; the retained branch was not rewritten. The user's primary checkout was never modified.

This bounded batch resolves the two named multi-type files only. Other inventory items remain separate audit work. The latest no-UI authorization excludes UI/resource renaming and gate importer repairs; no broader naming or functional cleanup was attempted. Storage compatibility and existing published history remain intact.


## Authorized current-game documentation correction

Issue #114 finding C06 identified two stale CURRENT_GAME passages. Read-only inspection confirmed ShopView's catalog tabs, purchase/confirmation and cosmetic actions, AviaryView's species cards/details and equipment actions, and DevOptions.Load's three compile-time branches. Updated only those two passages in `docs/CURRENT_GAME.md`, with links to canonical sources. The resource JSON has both flags false; EditorPrefs defaults are false in Editor, development builds read the resource JSON, and release uses ForBuild(false, null) to return new all-false options. Editor/development tests can override loading. Reset-progress and persistence-safety guidance is unchanged. No UI/runtime/settings/resource data was edited and no fresh visual/device acceptance is claimed.


## Completed unified verification of the pending merge

The coordinator clarified that the integrated version means the PR's pending merge, not an already merged main. The later instruction authorized one full run after this batch was complete, superseding the initial deferral above. No prior full Unity run existed for this batch.

Verified GitHub merge-preview commit: `1f97a2c49e933b201d950a7ba9078a9aea6109ef`. Its parents are exactly main `821242aa738bc16f369cd3eb55b6217248246795` and PR head `3dc81d04e00dba00d178c4b794aac09d68d073db`. Its tree, `28934880be2d82928b7b93e2434a51b64d49d846`, is identical to that PR head's tree; no local merge was performed. No other open PR was present at preflight; this is not a claim that every historical issue or remaining naming finding is resolved.

The existing isolated worktree was clean and checked out detached at that merge-preview commit. No Unity process was active, and `flock -n /tmp/unity.lock true` succeeded. Recorded SHA-256 state for 1,768 scene/meta/settings/package files, then ran:

1. `python3 ci/check_current_game_names.py --self-test`: **14/14 passed**, no skips/errors.
2. `python3 ci/check_current_game_names.py`: **216 files**, zero violations.
3. `flock /tmp/unity.lock bash scripts/run_tests.sh`: **one run only**, Unity **6000.6.7f1**, exit **0**. Scene build/compilation succeeded. EditMode **318/318 passed**, failed/skipped/inconclusive **0** (09:28:19–09:28:33 UTC, 2026-11-05). PlayMode **122/122 passed**, failed/skipped/inconclusive **0** (09:28:39–09:34:11 UTC). No test filter or test/source changes. EditMode breakdown: game 251, Meta 53, Audio 14; LevelsData **81** and Layout **41** retained.

Thus the moved Meta and test declarations compiled and the full retained Meta compatibility suite executed in Unity. This replaces the earlier pending engine status for this exact source tree. No screenshot, UI, artwork, device or listening acceptance is claimed; screenshots were not applicable to this non-UI file organization/documentation batch.

Evidence is preserved in ignored `build/profile-type-layout/integrated/`: `scene-build.log`, `editmode-results.xml`, `editmode-tests.log`, `playmode-results.xml`, `playmode-tests.log`, `unity-acceptance.log`, `results-summary.json`, naming logs, `pre-unity-state.json`, `generated-changes.json`, `generated-unity.patch`, `generated-review.log`, `scene-reconciliation.json` and `restoration.log`. A separate compressed copy is `/tmp/game-profile-type-layout-integrated-evidence.tar.gz`; its SHA-256 is returned in the handoff.

Unity generated 908 tracked-file changes: 894 whitespace-only metas, seven egg/family meta serialization completions, six known world gate-door hinge-pivot resets, and scene reserialization. Every meta GUID was preserved; the seven overlays' critical pivot/mesh/size/compression/physics settings stayed unchanged. The scene has the same document payloads after ignoring numeric object/input-action file IDs and document order. Those are generated build/import differences, not authorized UI changes. No importer or gate fix was made.

After the run exited, preserved the complete binary patch and classified differences; checked each current file hash against the captured generated state and each original blob against the pre-run hash before restoring only those 908 generated files. All 1,768 original recorded hashes then matched and the worktree was clean. No user process was killed, no unrelated changes were removed, and no generated scene/meta/resource changes enter this PR. The existing gate pivot reset remains a separate importer/visual boundary; normal build/import/test success does not certify unchanged gate visuals or the unimported serialized scene bytes.

Returned to the assigned profile-type-layout branch. Subsequent changes are this report only; implementation content is identical to the tested merge-preview tree. No second Unity run is needed or performed for this documentation-only follow-through. The original PR head's CI succeeded at https://github.com/crazyniu/game/actions/runs/42002040586 (14 naming tests, 216 files zero violations, 63/63 pure-Core tests with zero skips; workflow lint was not selected). The final report commit's exact-head CI is checked separately and returned in the handoff. The worker keeps the PR draft and does not merge, mark ready or deploy.
