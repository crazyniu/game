# Animated outfit art delivery

Delivered 27/27 outfits, 54/54 actions, 243/243 PNG frames and matching metadata. No code or retained artwork changes.

## Method

Only Codex built-in image generation was used. The requested Sunny straw-hat GIF pilot returned a still six-pose sheet rather than an animated file. Switched to the established complete-action sheet method, using the exact bare cheer/blink frames and canonical outfit still as references. The first call exceeded the five-reference limit; the six bare frames were assembled into one reference sheet and the retry succeeded. No PixelLab or external paid generation service was called in this worker run.

Each action is generated together in a 3-column sheet. Slicing reuses the existing `art/birds_sequences/qa/prepare_sequences.py` toe selector and background compositor without changing that file. A single median toe-span scale per action registers the sheet to corresponding bare frames, followed by per-frame toe-center/baseline translation. No mirroring or synthetic poses. PNGs use 320 x 320 RGBA, centered pivots, and species-idle importer settings with fresh asset/sprite GUIDs. Exact source scales, offsets, alpha ranges, hashes and reference anchors are recorded in registration JSON files.

## Evidence

- `art/cosmetics/raw/animated-outfits/`: built-in generated source sheets.
- `art/cosmetics/prompts/animated-outfits/`: generation prompts.
- `art/cosmetics/qa/animated-outfits/`: bare reference sheets, registration data, light/dark and 64 px color/grayscale comparisons.
- `docs/work-log/animated-outfits/evidence/`: per-outfit action contact sheets, bare row above dressed row on both backgrounds.
- `art/cosmetics/gif/`: 100 ms/frame offline light/dark review GIFs assembled from exported frames. These are previews, not native generator animation or Unity runtime evidence.

Built-in RGB previews can show color outside the bird, but exported alpha and dark/light composites determine transparency. Each delivered contact sheet was visually reviewed for accessory identity, pose, face, proportions, edges and small-size readability. Rejected one Cocoa tier 1 blink source for an opaque background; retained under `raw/animated-outfits/cocoa_outfit_1_blink_rejected_background.png` and regenerated using the built-in tool. Also rejected Hoot tier 2 cheer for missing reading glasses; retained as `hoot_outfit_2_cheer_rejected_missing_glasses.png` and regenerated.

Rejected Ember tier 3 cheer for a closed beak in pose 5; retained as `ember_outfit_3_cheer_rejected_beak.png` and regenerated.

## Exact completion list

Complete: sunny_outfit_1, sky_outfit_1, berry_outfit_1, mint_outfit_1, cocoa_outfit_1, snow_outfit_1, ember_outfit_1, ink_outfit_1, hoot_outfit_1, sunny_outfit_2, sky_outfit_2, berry_outfit_2, mint_outfit_2, cocoa_outfit_2, snow_outfit_2, ember_outfit_2, ink_outfit_2, hoot_outfit_2, sunny_outfit_3, sky_outfit_3, berry_outfit_3, mint_outfit_3, cocoa_outfit_3, snow_outfit_3, ember_outfit_3, ink_outfit_3, hoot_outfit_3.

Missing: none.

## Verification boundary

Asset audit: 243/243 frames passed 320 x 320 RGBA, transparent/opaque alpha range, non-clipped visible bounds, centered pivot and species-idle importer comparison. All 243 PNG hashes and asset GUIDs are distinct. All 54 offline GIFs contain the expected six or three frames at 100 ms/frame. See `art/cosmetics/qa/animated-outfits/asset-audit.json` and the updated `inventory.json` (zero missing frames).

Source/evidence counts: 54 accepted action sheets, three rejected sheets and one retained prior-worker input image (58 raw PNG files); 57 generation prompt records; 54 registration JSON files; 54 contact sheets in this evidence directory; 54 offline review GIFs. Twenty-seven species/tier batches were committed and pushed in tier order.

Commands completed:

- `uv run --with pillow python /tmp/audit_animated_outfits.py`: 243 frames, 54 actions, zero failures.
- `python3 ci/check_current_game_names.py --self-test`: nine tests passed.
- `python3 ci/check_current_game_names.py`: 178 tracked text files, zero violations.
- `git diff --cached --check`: passed for every delivery batch.

`flock /tmp/unity.lock bash scripts/run_tests.sh` completed successfully on Unity 6000.6.7f1: scene build succeeded; **317/317 EditMode** and **87/87 PlayMode**, zero failures or skips. The retained LevelsData 81 and Layout 41 cases were discovered and passed. Results: `build/test-results.xml`, `build/playmode-results.xml`; build/test logs: `build/scene-build.log`, `build/editmode-tests.log`, `build/playmode-tests.log`. Committed result summary: `art/cosmetics/qa/animated-outfits/unity-verification.json`.

`flock /tmp/unity.lock bash scripts/screenshots.sh` is queued for stable-state captures. Before Unity, 1,709 tracked scene, metadata, settings and package files were snapshotted under ignored `build/animated-outfits-before-unity/`. Generated normalization and scene changes will be restored from those exact pre-run bytes after capture. Offline artwork comparisons and GIFs do not establish integrated runtime motion acceptance. No timed in-game outfit playback or device acceptance is claimed; this delivery changes assets only.
