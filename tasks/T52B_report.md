# T52B — Current resource consolidation report

## Result

**DONE — bounded resource relocation and static preservation audits complete.** Branch: `mac-current-resources/t52`; base: `4b42159891619db23811cbef2b0de84ab742e43e`. No other task branch merged. Script/Editor integration and runtime acceptance remain outside this worker's ownership.

[Specification](T52B_current_resources.md) · [machine-readable resource manifest](../docs/team/HANDOFF_2026-11-04_RESOURCE_MANIFEST.json)

## Changes and exact path inventory

The manifest is the exact file inventory, not a sample:

- `current.path_root` + each `current.files` row's `old`/`new` columns gives the asset move. For rows with non-null GUID, append `.meta` to both paths for its sidecar move. `current.folder_metas` lists the three folder-meta moves explicitly.
- Each `archive.relative_files` entry moves from `archive.old_root` + entry to `archive.new_root` + entry. All 584 retired paths are listed; extracted current assets are excluded.
- `metadata_completion.source_root`/`destination_root` plus each completion filename gives the 108 newly copied meta paths, source SHA-256, byte size, and current GUID. These are additional authorized sources, not base moves.
- Documentation added: `tasks/T52B_current_resources.md`, `tasks/T52B_report.md`, `docs/team/HANDOFF_2026-11-04_RESOURCE_MANIFEST.json`.

| Source | Destination | Payloads |
| --- | --- | ---: |
| `PerchSort/Assets/Resources/Art/` | `PerchSort/Assets/Resources/Art/` | 126 PNGs |
| `PerchSort/Assets/Resources/Birds/` | `PerchSort/Assets/Resources/Birds/` | 187 PNGs |
| `PerchSort/Assets/Resources/World/` | `PerchSort/Assets/Resources/World/` | 32 PNGs |
| `PerchSort/Assets/Resources/Art/{dial_sun,dial_moon,hand_pointer,night_overlay,raindrop,swivel_knot}.png` | `PerchSort/Assets/Resources/Art/`, same filenames | 6 PNGs |
| `PerchSort/Assets/Resources/Levels/levels.json` | `PerchSort/Assets/Resources/Levels/levels.json` | 1 JSON |
| `PerchSort/Assets/Resources/V5/dev_options.json` | `PerchSort/Assets/Resources/dev_options.json` | 1 JSON |

All existing associated metas moved with their current assets. Current total: **351 PNGs** (132 Art, 187 Birds, 32 World) and two JSON files; 601 current moved files including existing sidecars and folder metas, plus 108 explicitly authorized primary-source metadata copies.

### Retired inventory

All destinations below are rooted at `archive/retired/PerchSort/Assets/`, outside Unity Assets. Historical resources are not a runnable project.

| Original relative group | Files | PNGs | Metas | Bytes |
| --- | ---: | ---: | ---: | ---: |
| `Resources/Levels` superseded payload + sidecar | 2 | 0 | 1 | 20,439 |
| `Resources/V5` remainder + folder meta | 336 | 167 | 169 | 39,584,525 |
| `Resources/V6` + folder meta | 184 | 91 | 93 | 1,657,552 |
| `Resources/V7` empty wrapper meta | 1 | 0 | 1 | 172 |
| `Art/Resources` + folder meta | 61 | 28 | 33 | 6,257,669 |
| **Total** | **584** | **286** | **297** | **47,520,357** |

## Preservation evidence

Static filesystem SHA-256 and GUID audits, before/after relocation:

- All **1,466 scoped baseline files / 175,349,439 bytes** accounted for bijectively: 601 current moved + 584 retired + 281 unchanged. Zero byte mismatches, missing/unaccounted files, or duplicate destinations.
- All 245 existing current asset metas (243 PNGs + two JSONs) and three current folder metas remain byte-identical, preserving GUIDs, import settings, sprite identifiers, pivots, borders, compression, and other serialized data present in those files. Six V5 PNGs retain their compressed import settings; no image or meta regenerated.
- Baseline and reconstructed-source inventory SHA-256 both equal `0d0720abd59788020f0324e4e1ec2f3d8ca92bf6ba5091c43cf95d03e76f194f`. Manifest defines deterministic digest serialization and records per-file current hashes plus archive inventory/mapping digests.
- Audited every `.meta` under `PerchSort/Assets`: 974 in the base, 677 after relocation, **785 after authorized metadata completion**; zero duplicate GUIDs at every stage. All 297 archived meta GUIDs are absent from active Assets. Serialized active `.unity`/`.asset`/`.prefab`/`.mat`/controller files contain zero references to those retired resource GUIDs.
- All current PNG filenames/routing groups preserved; `gate_open` and `gate_locked` exist in current Art. No runtime V6 resources retained.
- Existing `Resources/Levels.meta` stays byte-identical with GUID `e77116d60f2da4e7887439e39ed76536`. Only the old catalog payload/sidecar were archived before replacement. This preserves the folder container without inventing another GUID; no shared asset-key mapping changed.
- `Resources.meta`, `Art.meta`, and all non-Resources `Assets/Art` remain unchanged. Entire protected inventories match before/after: `art/` (1,766 files), shared `Assets/Audio` including root meta (53), fonts including root meta (95), and shared `Scripts/Audio` including root meta (14). No original artwork changed.

### Baseline metadata gap and authorized completion provenance

At the supplied base, **108 current Birds PNGs had no `.meta`**: all 18 frames each for `cocoa`, `crow`, `ember`, `hoot`, `ink`, and `snow`. These PNGs were moved unchanged. Their baseline manifest GUID/meta hash/size remain null (`MISSING_IN_BASELINE`); this is not meta loss from T52B or a claim that the base contained them. Initial coordinator notice: `msg_04b12cc526de`.

User followup authorized read-only copying the existing Mac primary files from `/Users/apple/Documents/projects/game/PerchSort/Assets/Resources/Birds/<same name>.png.meta` into task `Resources/Birds/`. All **108 metas / 410,311 bytes** copied exactly; full source paths, current destination paths, hashes, sizes and GUIDs are in `metadata_completion`. No source file written/moved/deleted, no GUID regenerated, and source hashes rechecked unchanged after copying. All 108 primary PNG payload hashes also match the relocated task PNGs.

Settings audit: every copied file is a Sprite `TextureImporter` (`textureType: 8`, `spriteMode: 1`). All 108 match the entire importer configuration of existing `Birds/sunny_idle.png.meta` (reference SHA-256 `cf2ccbedfca4f336792ccab68f8e06a9ebd6db7498003d8d18ca1334dd93a7d7`), excluding only per-image generated `internalIDToNameTable` and `spriteSheet.sprites` identity/geometry records. All remaining spriteSheet fields also match. Expected settings include full-rect mesh, custom centered pivot `(0.5, 0.5)`, zero border, 100 pixels/unit, alpha input/transparency, mipmaps disabled, max size 2048 and default-platform uncompressed policy; the existing non-overridden Standalone entry remains compressed, identical to the reference.

**Deviations: zero.** Copied GUIDs are all unique, with no collisions against existing active or retired GUIDs. No current PNG meta remains missing. Final scoped inventory: 1,574 files / 175,759,750 bytes = unchanged 1,466-file base inventory plus these 108 provenance-tracked copies. Every tracked base payload/meta remains byte-identical. **Zero overlay metas copied**: the seven primary-modified overlays were explicitly excluded, and all task overlay PNG/meta bytes remain the tracked base versions; C owns the importer correction. Runtime import behavior is NOT_RUN in this worker; coordinator-owned integrated Mac acceptance remains REQUIRED.

### Current JSON payloads

| Payload | Bytes | SHA-256 | GUID |
| --- | ---: | --- | --- |
| `Levels/levels.json` | 56,698 | `7a887dfd9066e2067e19caf77dcac5d3b0c1e74b5a4e5c47f08ffd155b03fbde` | `32b5d59a03d0440a3ada4f5d9ded6f80` |
| `dev_options.json` | 50 | `afa3f809485a1e06e983c2c976dde4c07bf865ad5afc71db18fd033400eb8680` | `5e8d4902ff6854e66b397bc0860674f5` |

Static JSON inspection confirms 40 authored entries. Original bytes and metas preserved; `unlockAllLevels: true` and `grantTestStock: true` unchanged.

## Static reference audit and integration dependencies

This isolated resource branch intentionally does not modify code, Editor, Scenes, or Tests. Source inspection found these cross-owner cutover obligations:

- Script owner A: current `V5Art.cs` versioned Art/Birds/World load and legacy fallback/crop; `LevelLoader.cs` (`Levels/levels`); `V5DevOptions.cs` (`V5/dev_options`); `V7BirdOverlay.cs` direct `Birds/` load. New keys: `Art/<key>`, `Birds/<key>`, `World/<key>`, `Levels/levels`, `dev_options`; retain existing key routing.
- Editor/Test owner C: current importers still target `Assets/Resources/V5`, `V6`, and `V7`; capture uses `Art/map_bg_w1`; current catalog test uses `Levels/levels`. Importer consolidation must distinguish the six compressed fallback PNGs from the V7 import policy, rather than unconditionally making them uncompressed/full-rect.
- Existing filenames are unchanged within their routing groups; all 345 current V7 filenames and the six explicit fallback keys are present at their mapped destinations. Current gate PNGs exist, so retired gate crop/fallback logic is unnecessary.

No proposed resource-key mapping changes. Container decision: retain existing Levels folder meta; archive the empty V7 wrapper meta as historical metadata. A resource-only branch is not expected to run before A/C integration.

## User-data compatibility

No PlayerPrefs, save files, profile serialization, migration reads, schema versions, enum order, or progress state touched. Persisted `v5.*`, `v6.unlockSeen.*`, and `v7.notifications` identifiers are compatibility IDs, not active parallel implementations; leave them intact to preserve user progress.

## Verification limits and submission

- **Static preservation, inventory, JSON count, filename-routing, global GUID, authorized source-byte and metadata-settings audits: PASS.** The 108-file baseline gap remains recorded separately from its completed primary-source metadata provenance.
- **Worker-local Unity, build/compiler, automated tests, screenshots, playthrough/runtime validation: NOT_RUN.** This records this resource worker's execution boundary, not a stage-wide prohibition. No historical passing results claimed for this work. No generated project artifacts.
- **Current Codex coordinator-owned integrated Mac compile, EditMode, PlayMode, and GUI acceptance: REQUIRED.** The coordinator must accept AND TEST the integrated build, recording actual results, failures, and skips; this worker's static audits do not prove runtime acceptance.
- Commit/push only this task branch and owned paths; submission SHA is reported in the worker terminal/coordinator delivery. Coordinator reviews, integrates, and owns the required Mac acceptance testing; it is not delegated to the user.
- Local coordinator send returned `session_caller_unknown`; the saved paired `root@192.168.1.251` environment successfully delivered the metadata notice to the exact supplied current Codex session. No Dispatch/Task ID invented and no message sent to Claude.
