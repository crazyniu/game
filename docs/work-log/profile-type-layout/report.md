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
