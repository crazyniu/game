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
