# Remaining species sequence delivery

## Current batch status

| Species | Delivered frames | Status |
| --- | ---: | --- |
| mint | 38 | Offline art review passed; Unity acceptance pending |
| cocoa | 38 | Offline art review passed; Unity acceptance pending |
| snow | 38 | Offline art review passed; Unity acceptance pending |
| ember | 38 | Offline art review passed; Unity acceptance pending |
| ink | 38 | Offline art review passed; Unity acceptance pending |
| hoot | 38 | Offline art review passed; Unity acceptance pending |
| crow | 0 | Pending |

Each completed species contains flap 6, hop 6, turn_left 4, turn_right 4, cheer 6, preen 5, peek 4, blink 3. Exports are 320x320 RGBA in the canonical Birds resource folder. Import settings are copied from that species' idle metadata, with fresh asset and sprite GUIDs. Existing assets and code are unchanged.

## Method and evidence

Generated with the built-in imagegen tool from the approved species idle reference and the accepted sunny pose tables. Prompts and raw sheets are retained under `art/birds_sequences/prompts/` and `art/birds_sequences/raw/`. The existing pilot slicing tool was executed with the current evidence destination and species-specific metadata source; its source file was not changed. `prepare_remaining.py` applies those two substitutions in memory; `check_remaining.py` records image and importer checks. Registration uses one scale per sequence and aligns the toe baseline to 300 px. No mirroring, synthetic pose transforms or procedural replacement artwork.

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
- Naming guard self-test: 8 passed. Naming guard: zero violations before Unity.
- Unity compilation, full EditMode/PlayMode suites, stable screenshots and timed runtime evidence are pending. The shared Unity lock was occupied by other authorized batch workers when inspected. No process was killed.

## Missing items

Crow: all 38 frames are pending. Optional mama_bow and chick_fluff blink/cheer sequences are not done.
