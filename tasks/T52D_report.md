# T52D — Delivery report

## Delivery and scope

Implemented the dependency-free [current-game naming guard](../ci/check_current_game_names.py) and the [T52D contract](T52D_naming_policy.md). The guard enumerates Git-index tracked authored Unity paths, checks working-tree bytes when available, and reads indexed Git blobs for omitted sparse-checkout files. It reports actionable path/line diagnostics and exits nonzero on violations.

Changed only the authorized four files:

- `ci/check_current_game_names.py` — new guard.
- `PerchSort/Assets/Scripts/Game/GameLayout.cs` — one provenance comment now references the approved forest layout and `docs/CURRENT_GAME.md`. No layout values or executable statements changed.
- `tasks/T52D_naming_policy.md` — scope, exact exception inventory, acceptance criteria, and ownership.
- `tasks/T52D_report.md` — this report.

D did not modify C-owned instructions/docs/CI wiring, Editor/Tests/Scenes, original artwork, compatibility IDs, or another task's worktree. C's worktree was inspected read-only. No workers were launched. Temporary branches/PRs remain normal collaboration; development continues in canonical source rather than numbered game forks.

## Exact compatibility exceptions

Exceptions mask only the complete, case-sensitive quoted literal in its exact owning path. They do not skip a line or file. All paths below are relative to `PerchSort/Assets/`:

| Owner | Exact exempt literals |
| --- | --- |
| `Scripts/Game/ProgressStore.cs` | `v5.coins`, `v5.muted`, `v5.highestUnlocked`, `v5.bestStars.`, `v5.tutorial.`, `v5.booster.`, `v5.starter.`, `v5.bestMoves.`, `v6.unlockSeen.`, `v7.notifications` |
| `Scripts/Meta/ProfileText.cs` | `v5.coins`, `v5.muted`, `v5.highestUnlocked`, `v5.bestStars.`, `v5.tutorial.`, `v5.booster.`, `v5.starter.` |
| `Tests/Meta/CollectionProgressionTests.cs` | `v5.highestUnlocked`, `v5.starter.0`, `v5.booster.0` |
| `Tests/Meta/ProfileEconomyTests.cs` | `v5.coins`, `v5.muted`, `v5.highestUnlocked`, `v5.bestStars.1`, `v5.bestStars.10`, `v5.bestStars.9`, `v5.bestStars.`, `v5.tutorial.7`, `v5.booster.0`, `v5.booster.1`, `v5.booster.5`, `v5.starter.0` |

The runtime sets were enumerated from `ProgressStore.cs` and `ProfileText.cs`; test literals were enumerated from C's canonical `Tests/Meta` worktree paths. There is no allowance for obsolete `Tests/V5` paths or newly versioned storage keys.

## Focused verification performed

Executed the actual `python3 ci/check_current_game_names.py` CLI in disposable Git repositories using Python's standard library, with assertions on exit codes and actionable diagnostics. No permanent test framework or fixture files were added. All temporary repositories were removed.

**29 fixture scenarios passed:**

| Coverage | Observed behavior |
| --- | --- |
| Canonical identities and all exact owner/literal exceptions | Exit `0`; canonical namespaces, assembly names, scenes, resource paths and all 32 owner/literal pairs accepted. |
| Ordinary formats, exclusions, PNG payload | Explicit numeric schema/dependency/release version values accepted; historical docs/tasks/original art and archive/retired/vendor/package subtrees excluded. Canonical PNG with non-UTF-8 bytes and a forbidden-looking token inside its payload accepted without reading image content. |
| Numbered paths | Rejected a lowercase numbered directory, prefix filename, suffix filename, and numbered PNG resource directory. |
| C#/assembly identities | Rejected prefix/suffix classes, namespace, and assembly name. |
| Resource/scene identities | Rejected a C# resource root, C# scene path, serialized scene root, serialized resource name, and dotted-numeric scene/resource names that were not format fields. |
| Compatibility-owner isolation | Rejected a `V8` class beside `"v5.coins"` in `ProgressStore.cs`, a `V9/Art/bird` path beside that key on the same line, a numbered namespace in `ProfileText.cs`, and a numbered class in the compatibility test owner. |
| Exactness | Rejected `v8.coins`, `v5.newSetting`, case-altered `V5.coins`, correctly spelled IDs in an unrelated file, an allowed-looking prefix extended with `/V9`, and unauthorized obsolete Song test keys. |
| Current working bytes | Rejected an unstaged numbered class despite a canonical indexed blob; accepted canonical working bytes despite a numbered indexed blob. |
| Real sparse checkout | With only `ci` materialized, rejected an omitted C# namespace via its indexed Git blob and an omitted numbered PNG path via the index. After canonicalizing the disposable repository, the omitted source passed. |

The first sparse fixture exposed a blob-ID string/bytes mismatch in the fallback. Corrected that line, then reran the sparse negative and positive cases successfully. The other completed fixture paths did not depend on that fallback.

## Observed worker-base result

On the accepted-A/B worker base `50b2bce`, before the comment cleanup: exit `1`, **620 tracked text files inspected, 585 violations**.

After the comment cleanup and guard correction: exit `1`, **620 tracked text files inspected, 583 violations**:

| Remaining owning root | Violations |
| --- | ---: |
| `Editor` | 95 |
| `Scenes` | 7 |
| `Tests` | 481 |

No remaining diagnostic came from `GameLayout.cs`, `Scripts/Game/ProgressStore.cs`, or `Scripts/Meta/ProfileText.cs`. The two removed diagnostics were the original-art version markers in the replaced comment. Remaining failures are expected until the coordinator integrates C's canonical Editor/Tests/Scenes work. They are not excused by new folder or file exemptions.

## Integration boundaries and required acceptance

1. C must finish/integrate its owned migration and CI call wiring. Invocation is `python3 ci/check_current_game_names.py`; the guard must run in sparse CI even when Unity assets are not materialized.
2. Initial read-only C inspection found `Tests/Meta/ProfileEconomyTests.cs:102` using `v5.songNotes` and `v5.songPayouts`. They are outside the explicitly authorized ten saved-data IDs. D notified the current coordinator through the verified Mac-to-251 status route and deliberately rejects these strings rather than silently widening the exception list. C owns neutralizing those deliberately ignored arbitrary test keys; their post-inspection status is not asserted here.
3. C owns `docs/CURRENT_GAME.md`, permanent instructions, and exact historical original-art provenance. That document is not present in D's accepted-A/B base; the new comment's target depends on C integration.
4. The final integrated tree has not been evaluated by D. The coordinator must rerun the guard after integrating C, review/accept the delivery, and perform serialized Mac integration testing plus GUI acceptance. Static fixture success is not runtime acceptance.
5. Per worker instruction, **no Unity, compiler, EditMode/PlayMode, or code test-suite runs were performed**. No historical test result is claimed for this change.

The guard intentionally operates on tracked paths and the documented textual suffixes, not arbitrary untracked files, binary payloads, or historical/vendor trees. It checks naming, not Unity compile correctness, asset loading, gameplay behavior, or exclusion-tree execution isolation.

## Submission

Submit only the four authorized files on isolated branch `mac-current-policy/t52`. Commit/push and final SHA are reported directly to the current Codex coordinator through the user-provided verified Mac route; no Task/Dispatch IDs or `worker_done` are used. Coordinator acceptance and post-integration Mac testing remain required before declaring the integrated game accepted.
