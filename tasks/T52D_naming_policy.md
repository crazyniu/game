# T52D — Current-game naming guard

## Goal and ownership

Maintain one current game by modifying canonical source in place. Numbered implementation identities such as `V5`, `V6`, `V7`, and future equivalents must not return as authored folders, filenames, identifiers, assembly names, scene roots, or resource paths. Temporary task branches and PRs remain normal collaboration, not parallel product identities.

D owns only:

- `ci/check_current_game_names.py` (new dependency-free Python guard).
- `PerchSort/Assets/Scripts/Game/GameLayout.cs` (one provenance comment only).
- `tasks/T52D_naming_policy.md`.
- `tasks/T52D_report.md`.

C owns current documentation, permanent agent instructions, canonical Editor/Tests/Scenes migration, and CI invocation wiring. D may read C's worktree, but may not edit or merge it. The current Codex coordinator accepts deliveries and owns serialized post-integration Mac testing and GUI acceptance. No reports go to retired Claude sessions. No worker Unity/compiler/test-suite execution is authorized; focused static guard fixtures are authorized.

## Guard contract

Run from the checkout with `python3 ci/check_current_game_names.py`. Exit `0` means no naming violations, `1` means naming/conflict/text-policy violations, and `2` means inspection could not complete. Diagnostics identify the tracked path and line (and content column where applicable), with a canonical-name correction instruction. A path-only violation uses line `1`.

Enumerate Git-index tracked paths, including sparse-checkout omissions, under `PerchSort/Assets/{Scripts,Editor,Tests,Scenes,Resources}` and their root `.meta` files. Check every extension's path components for case-insensitive `V` followed by digits, including identifier prefixes, suffixes, and embedded forms. Inspect current working-tree bytes when present; otherwise read the indexed blob with Git. Do not rely on a materialized Unity tree or only a changed-file list.

Inspect UTF-8 C#, assembly definitions/references, scenes, prefabs, serialized assets, JSON, YAML, metadata, text, XML, UXML, and USS. Reject numbered implementation tokens in identifiers, namespaces, serialized names, strings, and comments. Binary image contents, including PNG, are not read; their filenames still receive path checks. Unknown non-text extensions receive path checks only.

Historical docs/tasks/original art are outside these roots. Path segments named `archive`, `archived`, `retired`, `packages`, `vendor`, `vendors`, `thirdparty`, `third-party`, or `third_party` (case-insensitive) are excluded. This exclusion is for genuinely historical/third-party material, not a way to place executable current-game source outside policy. The guard is a naming check, not a proof that excluded source cannot execute.

Ordinary numeric dependency versions and schema/release/save format fields remain intact. A complete quoted numeric version such as `"v3"` or `"v1.2.3"` is allowed only as the value of explicit `schema`, `schemaVersion`, `formatVersion`, `saveVersion`, `releaseVersion`, `dependencyVersion`, or `version` JSON fields, or corresponding `*Version`/`version` assignments. Other numeric fields and dotted versions without a `V` marker do not match. A resource/scene name does not gain a format exemption just because it resembles a release version. No source is rewritten by the guard.

## Exact saved-data compatibility exceptions

Never rename saved-data IDs. These are historical persistence contracts, not implementation identities. Only complete, case-sensitive quoted literals in the exact owners below are exempt; the remainder of each file is still checked. No general prefix exemption or whole-file exemption exists. New versioned storage IDs are not authorized.

All paths below are relative to `PerchSort/Assets/`:

| Owner | Exact exempt literals |
| --- | --- |
| `Scripts/Game/ProgressStore.cs` | `v5.coins`, `v5.muted`, `v5.highestUnlocked`, `v5.bestStars.`, `v5.tutorial.`, `v5.booster.`, `v5.starter.`, `v5.bestMoves.`, `v6.unlockSeen.`, `v7.notifications` |
| `Scripts/Meta/ProfileText.cs` | `v5.coins`, `v5.muted`, `v5.highestUnlocked`, `v5.bestStars.`, `v5.tutorial.`, `v5.booster.`, `v5.starter.` |
| `Tests/Meta/CollectionProgressionTests.cs` | `v5.highestUnlocked`, `v5.starter.0`, `v5.booster.0` |
| `Tests/Meta/ProfileEconomyTests.cs` | `v5.coins`, `v5.muted`, `v5.highestUnlocked`, `v5.bestStars.1`, `v5.bestStars.10`, `v5.bestStars.9`, `v5.bestStars.`, `v5.tutorial.7`, `v5.booster.0`, `v5.booster.1`, `v5.booster.5`, `v5.starter.0` |

The numbered test literals are exact existing examples of the authorized prefix-based stored IDs. Old `Tests/V5/...` paths are deliberately not exempt. These owners/literals were enumerated from current runtime source and C's canonical test migration read-only. C's initially retained `v5.songNotes` and `v5.songPayouts` are outside the authorized list; D reports them rather than expanding exceptions.

## Layout provenance change

Replace only the `GameLayout` source comment referring to a version-numbered original-art path with a neutral approved-forest-layout reference and `docs/CURRENT_GAME.md`. Preserve every layout value and all behavior. C retains exact historical original-art provenance in `CURRENT_GAME.md`; D never edits original art.

## Acceptance and integration

Use disposable Git repositories, not another task's working tree, to exercise the real CLI:

1. Reject numbered directories/files, including case-insensitive prefixes and suffixes.
2. Reject numbered C# classes/namespaces and assembly identities.
3. Reject versioned resource paths and serialized scene roots.
4. Accept canonical names and every exact compatibility literal in its actual owner.
5. Reject a numbered class or resource path even beside an accepted literal in a storage-owner file. Reject unknown/case-altered/misowned storage strings.
6. Preserve legitimate numeric schema/dependency/release format values and excluded historical/vendor material; do not scan PNG payloads.
7. Read modified current working-tree text instead of stale indexed bytes.
8. Detect forbidden content and filenames in real sparse checkout via tracked Git objects, then accept canonical omitted files.

An initial failure on D's accepted-A/B base is expected until C integrates; do not exempt Editor/Tests/Scenes to conceal it. Report observed failures and remaining integration boundaries honestly. Deliver the isolated branch and final SHA to the current coordinator through the verified Mac-to-251 status route. Coordinator acceptance plus Mac testing after integration are required; static D evidence is not Unity/runtime acceptance.
