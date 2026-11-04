# T52B — Current resource consolidation

## Authority and scope

- User-authorized T52 identity/organization cleanup, 2026-11-04; base `4b42159891619db23811cbef2b0de84ab742e43e`.
- Work only in the isolated Mac task workspace on branch `mac-current-resources/t52`. Submit this branch to the current Codex coordinator; do not merge other branches. The primary checkout is read-only and only an explicitly authorized metadata source.
- Ownership: `PerchSort/Assets/Resources/**` and its root meta; `PerchSort/Assets/Art/**` and `Art.meta`; their retired counterparts under `archive/retired/PerchSort/Assets/`; this specification, `tasks/T52B_report.md`, and `docs/team/T52_RESOURCE_MANIFEST.json`.
- No Scripts, Editor, Scenes, Tests, runtime logic, gameplay, save format, artwork generation, or original `art/` changes. No additional workers.

## Shared resource contract

1. Move `Resources/V7/{Art,Birds,World}` and their folder metas to `Resources/{Art,Birds,World}`. Preserve every PNG and meta byte: 126 Art + 187 Birds + 32 World = 345 PNGs.
2. Move exactly six still-used `Resources/V5/Art` PNG/meta pairs into current `Resources/Art`: `dial_sun`, `dial_moon`, `hand_pointer`, `night_overlay`, `raindrop`, `swivel_knot`. Preserve their compressed import settings. Total current art: 351 PNGs; no V6 fallback.
3. Archive the superseded `Resources/Levels/levels.json` and its meta before moving current `Resources/V5/levels.json` and its meta into that location. Keep the existing `Resources/Levels.meta` container identity unchanged; no new folder GUID is needed. Move `V5/dev_options.json` and its meta to `Resources/dev_options.json`. Preserve both current payloads byte-for-byte, all 40 authored levels, and both current development flags.
4. Archive remaining `Resources/V5`, `Resources/V6`, their folder metas, the emptied V7 wrapper/meta, and legacy `Assets/Art/Resources` plus its folder meta under `archive/retired/PerchSort/Assets/<original-relative-path>`. Archive paths are outside Unity Assets; empty directories need not be represented in Git.
5. Leave all other `Assets/Art` files, `Art.meta`, `Resources.meta`, all `art/` originals, shared Audio, and fonts unchanged.
6. Shared integration resource keys are `Art/<key>`, `Birds/<key>`, `World/<key>`, `Levels/levels`, and `dev_options`. Script/Editor owners update lookups and importer routing; this worker does not modify those call sites.

### Baseline metadata exception

Static inspection of the authorized base found 108 current bird PNGs without `.meta` sidecars: all 18 frames for each of `cocoa`, `crow`, `ember`, `hoot`, `ink`, and `snow`. Move these PNGs unchanged and retain null baseline GUID/meta hashes as `MISSING_IN_BASELINE`; do not claim absent metadata existed in the base. All 243 existing current PNG metas, both JSON metas, and all moved folder metas must remain byte-identical.

User followup explicitly authorizes copying exactly these 108 existing meta files read-only from `/Users/apple/Documents/projects/game/PerchSort/Assets/Resources/V7/Birds/<same name>.png.meta` to the renamed task `Resources/Birds/` path. Never write/move/delete any primary file. Do not copy the seven primary-modified overlay metas; their importer correction belongs to C. Record completion provenance separately: exact source/destination paths, SHA-256, byte size, GUID, settings comparison to the existing `sunny_idle.png.meta`, and deviations. Audit correct Sprite importer settings, unique GUIDs, unchanged source bytes, and unchanged tracked base metas. No worker-local Unity execution.

## Acceptance and static evidence

- Manifest records every current moved file with old/new path, SHA-256, size, GUID, and preserved meta SHA-256; folder meta moves are explicit.
- Archive inventory summary records counts, bytes, and deterministic path/hash digest per retired group. All scoped baseline files map bijectively to either current or retired destinations: no asset or meta loss and no duplicate destination.
- Byte equality covers PNGs, import settings, GUIDs, JSON payloads, unchanged original art, Audio, fonts, and non-resource Art.
- Audit all `.meta` GUIDs under `PerchSort/Assets`; no duplicate GUID. Retired GUIDs remain only outside Assets. Preserve current resource key names and existing gate assets.
- Report current static reference obligations to the coordinator; stale code references in this resource-only branch are an integration dependency, not silently edited out of ownership.
- Worker-local Unity/build/compiler/automated tests/screenshots/playthrough: **NOT_RUN**; this resource worker performs static file/hash/GUID/reference audits only. This is not a stage-wide testing prohibition. The current Codex coordinator MUST accept and test the integrated build on the Mac: compile, EditMode, PlayMode, and GUI acceptance are **REQUIRED**, with actual results and any failures/skips recorded. Worker static evidence does not satisfy integrated runtime acceptance.

## Delivery

Write the [manifest](../docs/team/T52_RESOURCE_MANIFEST.json) and [report](T52B_report.md), commit and push only the owned paths, then report SHA, exact changed paths, evidence, mapping decisions, and untested boundaries to the current Codex coordinator. Storage compatibility IDs remain untouched; identity cleanup must not reset user progress.
