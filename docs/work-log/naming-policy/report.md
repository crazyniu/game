# T52D — Delivery report

## Delivery and scope

Implemented the dependency-free [current-game naming guard](../../../ci/check_current_game_names.py) and the [T52D contract](spec.md). The guard checks path names across all Git-index tracked `PerchSort/Assets`, including new parallel roots outside the five content roots. It inspects authored text only under Scripts/Editor/Tests/Scenes/Resources, uses working-tree bytes when available, and reads indexed Git blobs for omitted sparse-checkout files. It reports actionable path/line diagnostics and exits nonzero on violations.

Changed only the authorized four files:

- `ci/check_current_game_names.py` — new guard.
- `PerchSort/Assets/Scripts/Game/GameLayout.cs` — one provenance comment now references the approved forest layout and `docs/CURRENT_GAME.md`. No layout values or executable statements changed.
- `docs/work-log/naming-policy/spec.md` — scope, exact exception inventory, acceptance criteria, and ownership.
- `docs/work-log/naming-policy/report.md` — this report.

D did not modify C-owned instructions/docs/CI wiring, Editor/Tests/Scenes, original artwork, compatibility IDs, or another task's worktree. C's worktree was inspected read-only. No workers were launched. Temporary branches/PRs remain normal collaboration; development continues in canonical source rather than numbered game forks.

## Exact compatibility exceptions

Exceptions mask only the complete, case-sensitive quoted literal in its exact owning path. They do not skip a line or file. All paths below are relative to `PerchSort/Assets/`:

| Owner | Exact exempt literals |
| --- | --- |
| `Scripts/Game/ProgressStore.cs` | `v5.coins`, `v5.muted`, `v5.highestUnlocked`, `v5.bestStars.`, `v5.tutorial.`, `v5.booster.`, `v5.starter.`, `v5.bestMoves.`, `v6.unlockSeen.`, `v7.notifications` |
| `Scripts/Meta/ProfileText.cs` | `v5.coins`, `v5.muted`, `v5.highestUnlocked`, `v5.bestStars.`, `v5.tutorial.`, `v5.booster.`, `v5.starter.` |
| `Tests/Meta/CollectionProgressionTests.cs` | `v5.highestUnlocked`, `v5.starter.0`, `v5.booster.0` |
| `Tests/Meta/ProfileEconomyTests.cs` | `v5.coins`, `v5.muted`, `v5.highestUnlocked`, `v5.bestStars.1`, `v5.bestStars.10`, `v5.bestStars.9`, `v5.bestStars.`, `v5.tutorial.7`, `v5.booster.0`, `v5.booster.1`, `v5.booster.5`, `v5.starter.0`, `v5.songNotes`, `v5.songPayouts` |

The runtime sets were enumerated from `ProgressStore.cs` and `ProfileText.cs`; test literals were enumerated from C's canonical `Tests/Meta` worktree paths. Coordinator decision `msg_0f617bd7f741` confirms the two Song IDs are actual historical save identifiers, not arbitrary keys or current implementation names. They prove migration ignores obsolete Song data. Only canonical `Tests/Meta/ProfileEconomyTests.cs` may retain those exact literals; runtime and other files remain disallowed. `LegacyMigrationUsesActualKeysAndIgnoresObsoleteSongKeys` is unchanged. There is no allowance for obsolete `Tests/V5` paths or newly versioned storage keys.

## Focused verification performed

Executed the actual `python3 ci/check_current_game_names.py` CLI in disposable Git repositories using Python's standard library, with assertions on exit codes and actionable diagnostics. No permanent test framework or fixture files were added. All temporary repositories were removed.

**48 fixture scenarios passed against the final corrected guard:** reran the original 29-scenario matrix with external-history fixtures outside active Assets and the Song runtime-rejection correction, plus 12 focused Song-boundary scenarios and seven path-selection review cases.

| Coverage | Observed behavior |
| --- | --- |
| Canonical identities and all exact owner/literal exceptions | Exit `0`; canonical namespaces, assembly names, scenes, resource paths and all 34 owner/literal pairs accepted. |
| Ordinary formats, external history, binary payloads | Explicit numeric schema/dependency/release version values accepted; historical docs/tasks/original art and archive/retired/vendor/Packages trees outside active Assets ignored. Canonical PNG and external-to-content-root font payloads are not parsed; their paths remain checked. |
| Numbered paths | Rejected a lowercase numbered directory, prefix filename, suffix filename, and numbered PNG resource directory. |
| C#/assembly identities | Rejected prefix/suffix classes, namespace, and assembly name. |
| Resource/scene identities | Rejected a C# resource root, C# scene path, serialized scene root, serialized resource name, and dotted-numeric scene/resource names that were not format fields. |
| Compatibility-owner isolation | Rejected a `V8` class beside `"v5.coins"` in `ProgressStore.cs`, a `V9/Art/bird` path beside that key on the same line, a numbered namespace in `ProfileText.cs`, and a numbered class in the compatibility test owner. |
| Exactness | Rejected `v8.coins`, `v5.newSetting`, case-altered `V5.coins`, correctly spelled IDs in an unrelated file, an allowed-looking prefix extended with `/V9`, and historical Song IDs in runtime. |
| Current working bytes | Rejected an unstaged numbered class despite a canonical indexed blob; accepted canonical working bytes despite a numbered indexed blob. |
| Real sparse checkout | With only `ci` materialized, rejected an omitted C# namespace via its indexed Git blob, an omitted numbered PNG path, and a fresh omitted `Assets/V8/Canonical.cs` root. After canonicalizing the disposable repository, omitted source passed. |
| Historical Song IDs, positive boundary | Each of `v5.songNotes` and `v5.songPayouts` individually accepted only in canonical `Tests/Meta/ProfileEconomyTests.cs`. |
| Historical Song IDs, negative boundary | Each literal rejected in `Scripts/Game/ProgressStore.cs`, `Scripts/Meta/ProfileText.cs`, `Tests/Meta/CollectionProgressionTests.cs`, and an unrelated test file (eight cases). |
| Historical Song IDs, no file/line exemption | A `V8` class beside `v5.songNotes` and a `V9/Art/bird` path beside `v5.songPayouts` in the canonical test owner still rejected (two cases). |
| Requested bypass regressions | Rejected `PerchSort/Assets/Scripts/retired/V8Game.cs` and `PerchSort/Assets/V8/Canonical.cs`. |
| Nested directory names do not exempt content | Rejected a `V8Game` class in canonically named files under active `Scripts/archive`, `Scripts/vendor`, `Scripts/thirdparty`, and `Scripts/Packages`. |
| Font names versus payloads | Rejected a numbered font asset path outside the five content roots without parsing its invalid-UTF-8 payload; canonical font paths with the same binary payload passed. |

The first sparse fixture exposed a blob-ID string/bytes mismatch in the fallback. Corrected that line, then reran the sparse negative and positive cases successfully. The other completed fixture paths did not depend on that fallback.

## Historical pre-C worker-base result

On the accepted-A/B worker base `50b2bce`, before the comment cleanup: exit `1`, **620 tracked text files inspected, 585 violations**.

After the comment cleanup and guard correction: exit `1`, **620 tracked text files inspected, 583 violations**:

| Remaining owning root | Violations |
| --- | ---: |
| `Editor` | 95 |
| `Scenes` | 7 |
| `Tests` | 481 |

No remaining diagnostic came from `GameLayout.cs`, `Scripts/Game/ProgressStore.cs`, or `Scripts/Meta/ProfileText.cs`. The two removed diagnostics were the original-art version markers in the replaced comment. These observations predate C integration and are not a result for the current integrated tree. They were not excused by new folder or file exemptions.

## Integration boundaries and required acceptance

1. The coordinator reports C is integrated and a separate SafeArea compile ambiguity has been fixed. D has not independently run or repeated Unity compilation. Guard invocation remains `python3 ci/check_current_game_names.py`; it supports sparse CI when Unity assets are not materialized.
2. The initial interpretation of the two Song literals was incorrect and is superseded by coordinator decision `msg_0f617bd7f741`. They are test-only historical-storage compatibility exceptions. The correction changes only the guard/spec/report, not C's regression test or any runtime storage ID.
3. C owns the integrated current documentation, permanent instructions, CI wiring, and exact historical original-art provenance. `docs/CURRENT_GAME.md` was absent only from D's original accepted-A/B base; it is not reported as a pending-C blocker.
4. The final integrated tree has not been evaluated by D. The coordinator must run the updated guard on that tree, review/accept this correction, and complete required serialized Mac integration testing plus GUI acceptance. Static fixture success is not runtime acceptance.
5. Per worker instruction, **no Unity, compiler, EditMode/PlayMode, or code test-suite runs were performed**. No historical test result is claimed for this change.

Review identified a real bypass in the initial guard: excluding any `retired`/`archive`/`vendor`/`thirdparty` path component could hide C# that Unity still compiles. Removed the component exclusions entirely; no vendor allowlist is introduced. All tracked Assets paths now receive naming checks, so a fresh `Assets/V8` root cannot evade selection. Content remains restricted to the five documented authored roots and text suffixes; untracked files, binary/font payloads, and external historical/package trees are not parsed. This correction changes only the guard/spec/report, not gameplay or actual tests. Naming verification is not Unity compile or runtime acceptance.

## Submission

Submit only the four authorized files on isolated branch `mac-current-policy/t52`. Commit/push and final SHA are reported directly to the current Codex coordinator through the user-provided verified Mac route; no Task/Dispatch IDs or `worker_done` are used. Coordinator acceptance and post-integration Mac testing remain required before declaring the integrated game accepted.
