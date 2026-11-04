# T52A — Current runtime identity

## Authority and scope

Implement the T52 shared contract against main `4b42159891619db23811cbef2b0de84ab742e43e`. The current Codex coordinator owns integration. This task owns only `PerchSort/Assets/Scripts/**` (including folder metas), `archive/retired/PerchSort/Assets/Scripts/**`, this specification, and `tasks/T52A_report.md`. No additional workers. Commit and push this isolated branch; do not merge other branches or modify the primary checkout.

## Required changes

1. Archive legacy `Scripts/Core`, `Scripts/UI`, and `Scripts/View`, including their folder metas, outside Unity under `archive/retired/PerchSort/Assets/Scripts/` before installing current Core. Shared Audio stays byte-identical.
2. Move current `Scripts/V5/{Core,Meta,Game}` to `Scripts/{Core,Meta,Game}` with all source/assembly/folder meta bytes preserved. Retire the obsolete enclosing V5 folder and archive its meta. Rename namespaces and assemblies from `PerchSort.V5.*` to `PerchSort.*`.
3. Apply shared type/file mappings: GameRoot, GameArt, GameButton, DevOptions, GameFixtures, GameMotion, PanelsView, LongPress, GameProfile, ProgressStore, SceneView, GameUI, EmptyClick, SafeArea, ViewModel, MapModel, MapView, LevelNodeState, GameLayout; strip the V7 prefix from the other current presentation types. Preserve existing multi-type file organization and serialized fields. Core friend assembly becomes `PerchSort.EditModeTests`.
4. Use resource keys `Art/`, `Birds/`, `World/`, `Levels/levels`, and `dev_options`, including BirdOverlay's direct load. Confirm current resources statically before removing the retired-art fallback and gate cropping. Resource payload movement belongs to T52B.
5. Update active-version comments, log/error text, and object names without renaming historical source-art provenance or storage compatibility IDs.

## Invariants

No gameplay, transaction, animation timing/cancellation, method behavior, save-key, serialized field, or enum-order changes. In particular, T50 `transactionGeneration` remains distinct from presentation/hint `revision`. PlayerPrefs versioned IDs, ProfileMigrator reads, ProfileText DataMember names, popup SaveData fields, and schema versions remain unchanged. No persistence reset or migration redesign.

## Acceptance evidence

- Archived legacy scripts and shared Audio match baseline hashes byte-for-byte.
- Every retained current `.meta` matches baseline bytes at its mapped path, including GameRoot GUID `07203ec62b1604449b634c729c1303a3`.
- Static mapping/reference audit finds no active V5/V6/V7 implementation symbols, namespaces, assemblies, or resource prefixes except documented provenance/storage compatibility strings.
- Static source comparison limits changes to mapped identities, resource keys, authorized retired fallback removal, and explanatory comments/log/object text. Core behavior and T50 transaction ownership remain unchanged.
- Report exact changed paths, mappings, preservation evidence, cross-task integration dependencies, and limitations.

## Validation ownership (updated user directive)

Worker-local runtime validation is **NOT_RUN**: do not independently launch code, compiler, build, Unity, automated tests, screenshots, or playthroughs. Static source/file/hash/GUID/reference inspection is authorized.

Stage testing is **REQUIRED**, owned by the current Codex coordinator after integrating T52A/B/C. The coordinator serializes Mac compilation, EditMode, PlayMode, and GUI acceptance. Preserve legitimate tests and assertions; do not disable CI because of the superseded no-tests instruction. T52C/coordinator own test migration and replacement of obsolete legacy-only minimum counts with justified current-coverage safeguards. Report integration/test migration risks; static checks do not establish runtime acceptance.
