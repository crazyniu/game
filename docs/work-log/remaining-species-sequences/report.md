# Remaining species sequence delivery

## Current batch status

| Species | Delivered frames | Status |
| --- | ---: | --- |
| mint | 38 | Offline art review passed; timed runtime acceptance pending |
| cocoa | 38 | Offline art review passed; timed runtime acceptance pending |
| snow | 38 | Offline art review passed; timed runtime acceptance pending |
| ember | 38 | Offline art review passed; timed runtime acceptance pending |
| ink | 38 | Offline art review passed; timed runtime acceptance pending |
| hoot | 38 | Offline art review passed; timed runtime acceptance pending |
| crow | 38 | Offline art review passed; timed runtime acceptance pending |

Each completed species contains flap 6, hop 6, turn_left 4, turn_right 4, cheer 6, preen 5, peek 4, blink 3. Exports are 320x320 RGBA in the canonical Birds resource folder. Import settings are copied from that species' idle metadata, with fresh asset and sprite GUIDs. Existing assets and code are unchanged.

## Method and evidence

Generated with the built-in imagegen tool from the approved species idle reference and the accepted sunny pose tables. Prompts and raw sheets are retained under `art/birds_sequences/prompts/` and `art/birds_sequences/raw/`. The existing pilot slicing tool was executed with the current evidence destination and species-specific metadata source; its source file was not changed. `prepare_remaining.py` applies those two substitutions in memory and selects neutral gray toes for Crow (the pilot orange selector would select its compass); `check_remaining.py` records image and importer checks. Registration uses one scale per sequence and aligns the toe baseline to 300 px. No mirroring, synthetic pose transforms or procedural replacement artwork.

Every completed sequence has a JPEG contact sheet in `evidence/` with idle, light/dark composites and a 64 px grayscale row. Offline APNG previews, inventories and asset checks are under `art/birds_sequences/qa/`. These are offline artwork evidence, not Unity runtime acceptance.

## Rejections and corrections

- Cocoa turn_right: rejected the first four-frame source because an ear tip reached the top canvas edge. Regenerated with square cells and additional padding; complete ear silhouettes verified in the replacement. Rejected source retained as `cocoa_turn_right_rejected_crop.png`.
- Mint hop: rejected the first six-frame sheet because the exported bird read too small and a rectangular haze appeared against the dark background. Regenerated and reviewed the replacement. Rejected source retained as `mint_hop_rejected_scale_haze.png`.
- Mint flap: initial tool preview appeared to have a backdrop. Direct alpha inspection established transparency; a cutout correction was generated and used. The first candidate is retained as `mint_flap_superseded_cutout.png`, not counted as a proven backdrop failure.

- Snow turn_left: first generated source turned right. Retained it as the independently drawn turn_right sequence after review; generated a new left-facing sequence. The original prompt is retained as `snow_turn_right_source_prompt.txt`. No mirror operation was used.

- Ember flap: rejected the first source after export because frame 4 clipped a wing tip. Regenerated with shorter articulated wing extension at unchanged body scale. Replacement passes all export checks; rejected source retained as `ember_flap_rejected_wing_clip.png`.

- Ink blink: rejected the first source because a faint checkerboard was baked into the alpha area and visible on dark composites. Regenerated a clean cutout; rejected source retained as `ink_blink_rejected_checker.png`.

- Hoot flap: rejected the first source for faint rectangular haze in dark-background composites. Regenerated clean alpha with cap, pompom and body scale retained; rejected source retained as `hoot_flap_rejected_haze.png`.

## Verification boundary

- Mint: 38 images verified as 320x320 RGBA with real alpha and species-idle import settings; no clipped exports. All eight contact sheets visually reviewed for identity, pose, scale, light/dark edges and grayscale readability.
- Snow: 38 images and all eight contact sheets reviewed; matching idle import settings, real alpha, no clipped exports or duplicate frame files. Offline APNG previews and checks are saved.
- Ember: 38 images and all eight contact sheets reviewed; matching idle import settings, real alpha, no clipped exports or duplicate frame files.
- Ink: 38 images and all eight contact sheets reviewed; matching idle import settings, real alpha, no clipped exports or duplicate frame files.
- Hoot: 38 images and all eight contact sheets reviewed; matching idle import settings, real alpha, no clipped exports or duplicate frame files.
- Crow: 38 images and all eight contact sheets reviewed; matching idle import settings, real alpha, no clipped exports or duplicate frame files. Gray toe registration preserves the 300 px baseline.
- Naming guard self-test: 8 passed. Naming guard: zero violations before Unity.
- `flock /tmp/unity.lock bash scripts/run_tests.sh`: Unity 6000.6.7f1 compilation and canonical scene build passed; full EditMode **312/312** and full PlayMode **53/53**, zero failed/skipped/inconclusive cases. Retained LevelsData 81 and Layout 41 cases passed. XML and logs remain in `build/`; paths and hashes are recorded in `art/birds_sequences/qa/unity_checks.json`.
- `flock /tmp/unity.lock bash scripts/screenshots.sh`: passed, **34 PNGs at 1080x1920**, under `build/screens/game/`. Reviewed `mech_crow_twig.png`, `level_k7.png` and `popup_new_friend.png` as representative stable captures. Capture inventory and hashes are in `unity_checks.json`.
- Verification waited for the shared lock; no process was killed. Before Unity, recorded 2194 tracked Unity files and backed up scene, metadata, settings and packages. After reviewing generated changes, restored 380 whitespace-only metadata rewrites, seven existing overlay metadata expansions and the rebuilt scene (object/Input System action IDs) from that snapshot. All 2194 tracked Unity files match their pre-run bytes; no scene, runtime code, settings, packages or original art changes are delivered.
- Tests and stable captures used the scene rebuilt by `GameSceneBuilder`. The screenshots use instant motion and synthetic profiles; they do not establish playback of every delivered frame. Timed species animation, real GUI/device and coordinator integration acceptance remain pending. Offline APNG previews are artwork evidence only.

## Missing items

No required species are missing. Mint, cocoa, snow, ember, ink, hoot and crow each have all 38 frames (266 total; 190 added by this worker). Optional mama_bow and chick_fluff blink/cheer sequences are not done.

## Reproduction commands

For each species, run `python art/birds_sequences/qa/prepare_remaining.py <species>` and `python art/birds_sequences/qa/check_remaining.py <species>` in an environment with Pillow. This worker used `/tmp/perch-remaining-species-tools/bin/python`. The source prompts, raw sheets, contact sheets, APNG previews, inventories and checks are retained under `art/birds_sequences/`. All five required species were committed and pushed separately in the requested order.

## Species pushes

| Species | Commit | Frames |
| --- | --- | ---: |
| snow | `d3fecc63` | 38 |
| ember | `122379bd` | 38 |
| ink | `67993414` | 38 |
| hoot | `887f3964` | 38 |
| crow | `201d3514` | 38 |

Final state: all five assigned species delivered and pushed, 40 reviewed JPEG contact sheets, 40 offline APNG previews, no required sequence or frame missing. Optional mama_bow/chick_fluff overlays remain unattempted.
