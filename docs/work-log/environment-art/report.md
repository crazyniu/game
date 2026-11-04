# Environment artwork delivery

## Result

15 of 18 requested resource images are delivered on `mac-environment-art/work`. Three open gates remain rejected after a targeted regeneration pass. No code, original artwork, levels, saves, or gameplay settings were edited. The new art is not wired into the runtime.

| Batch | Requested | Delivered | Missing accepted images |
| --- | ---: | ---: | --- |
| Trunks and junctions | 8 | 8 | None |
| Arch gates | 6 | 3 | All three open states |
| Map extras | 4 | 4 | None |

Missing accepted resources:

- `gate_arch_open_w1.png`
- `gate_arch_open_w2.png`
- `gate_arch_open_w3.png`

Batch pushes were completed in order: trunks/junctions at `6254660`, gates at `387f275`, and map extras at `ff05810`. Later evidence-only changes record verification and cleanup.

## Delivered assets

All product PNGs are in `PerchSort/Assets/Resources/World/`. `art/environment/delivery.json` lists each delivered file, dimensions, mode and SHA-256. New import metadata follows the corresponding existing world sprite conventions with unique GUIDs. Unity imported the new resources successfully during verification.

- Four curved trunks: 380x1920 RGBA.
- Four branch junctions: 240x220 RGBA.
- Three closed gates: 560x600 RGBA; spring blossoms, autumn leaves/berries, frosted vines/icicles.
- Bamboo grove: 1080x900 RGBA.
- Six separate stones on one sheet: 1024x512 RGBA.
- Left and right foreground bushes: 420x520 RGBA each.

Generation used the built-in image tool. Exact prompts and source output paths are recorded in `art/environment/generation.json`; raw results and rejected drafts are retained under `art/environment/`. macOS `sips` performed export sizing and transparent canvas cropping. No artwork was painted procedurally. The right bush was separately generated with matching upper-left lighting.

## Quality checks and integration boundaries

Per-asset JPEG sheets in `evidence/` show light/dark backgrounds and 64 px thumbnails. `map_stones_readability.jpg` additionally shows each individual stone at a 64 px bounding size. Full-resolution offline composites and dimension/alpha measurements are under `art/environment/qa/`.

Opaque lower-trunk width ranges are green forest 154–183 px, bamboo 150–183 px, autumn 163–190 px, and snow 153–173 px. The existing trunk attachment x=73 remains within the opaque top section of each export. Each `board_assembly_w*.jpg` compares an approved board reference with a trunk, five joints, existing branches and existing crown. Crown overlap and fork coverage still need final runtime placement; the offline arrangements do not prove an unchanged runtime crown attachment.

Joints belong in the lower two thirds of the trunk. The offline assembly draws branches, then joints, then the trunk over their vertical overlap, with crown above the fork. Exposed vertical joint ends do not blend seamlessly. Material colors are visually close, but generated grain is not pixel-identical to the existing texture. These are asset-delivery checks, not integrated visual acceptance.

The spec's world labels conflict with source art: existing world 2 is green bamboo and existing world 3 is autumn. New trunks/junctions follow the actual existing materials so they can match the existing branches. This discrepancy was raised with the coordinator; no reply was received before proceeding. Gate decorations follow their explicit written descriptions.

The bamboo layer contains translucent pale mist. Its mean alpha falls from 216.6 at row 720 to 6.1 at row 810 and 0.58 at the bottom; bottom-row maximum is 1/255. It has no opaque rectangular background. `map_assembly.jpg` is an offline composition of the extras over the existing map background, not a runtime screenshot.

## Rejected artwork

Eight rejected raw images are retained:

- Initial green trunk: lower-width drift. Regenerated and replaced.
- Initial autumn junction: white end fades. Regenerated and replaced.
- Initial open gates for all three worlds: stationary-frame registration drift.
- Regenerated open gates for all three worlds: the arch still changed despite localized-edit prompts preserving the closed master.

Open gates are excluded from Resources. JPEG pair/alignment evidence is labelled REJECTED. `art/environment/qa/gate_registration.json` records upper-arch RGB differences after the retry. The complete gate-pair requirement remains unmet; rejected outputs must not be treated as approved cross-fade states.

## Verification

Commands run from this isolated Mac workspace:

- `python3 ci/check_current_game_names.py --self-test`: 8 tests passed.
- `python3 ci/check_current_game_names.py`: zero violations at each batch commit.
- `flock /tmp/unity.lock bash scripts/run_tests.sh`: scene build succeeded; EditMode 312/312 passed; PlayMode 53/53 passed; zero failures or skips. Retained LevelsData 81 cases and Layout 41 cases were discovered.
- `flock /tmp/unity.lock bash scripts/screenshots.sh`: passed; 34 required PNG captures at 1080x1920 in `build/screens/game/`. Map, basic board and dense board captures were opened for inspection.

Test XML/logs remain under ignored `build/`; result summaries and XML hashes are in `art/environment/qa/unity_results.json`. The test counts above are results of this run, not historical baselines. Input scene/settings/package hashes were recorded before Unity; clean committed metadata provides the pre-import metadata hashes in `unity_input_state.json`.

Unity-generated metadata and scene changes were reviewed and preserved in `build/unity-generated-changes.patch`. After capture, 137 reviewed generated files were restored to their pre-import committed bytes; all recorded input hashes match. Stable captures exercise the current game and cannot establish acceptance of these unwired sprites. No timed animation, audio, device or integrated-art acceptance is claimed.
