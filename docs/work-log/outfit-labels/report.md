# Outfit name ribbon delivery

Added 23 transparent painted outfit-name PNGs to `PerchSort/Assets/Resources/Art/labels/`, with the exact requested filenames and wording. Added all 23 outfit-ID mappings in the requested order to [the text inventory](../plush-ui/text-inventory.md#outfit-ribbon-additions).

[Contact sheet: all 27 ribbons](../plush-ui/outfit-labels.jpg), 1500 × 1188 JPEG. Read left to right, top to bottom: Straw Hat, Cozy Scarf, Round Glasses, Flower, then the 23 new names in inventory order. All sprites use the same display scale in the sheet, exposing the occupied-height differences already present in the four originals.

## Production and visual review

Used 23 built-in `image_gen` calls, with true transparent output enabled. Each call painted the complete wording together with its ribbon using Straw Hat as the edit reference. The [specification](spec.md#generation-prompt-set) records the prompt set. No fonts, font baking, procedural text, or copied letter fragments were used.

Final canvases are 1368 × 294 RGBA. The generated ribbon was cropped to its substantive alpha bounds (bounds measured at alpha >= 16; the image alpha itself was not thresholded), downscaled to 1344 × 156 and placed at (12, 69), matching Straw Hat's occupied rectangle. Pillow performed geometry and contact-sheet placement only. The four reference PNGs remain byte-identical to Git.

Inspected all 27 labels at 300%, including every letter, repeated letters, word breaks and each lowercase i-dot. All 23 new spellings passed; no spelling regeneration was necessary. Local review crops are in ignored `build/outfit-labels/review/`. The contact sheet was also visually reviewed for family consistency, leaf clusters, transparency edges and text placement. This is offline artwork review, not runtime acceptance or a claim of pixel-identical regenerated leaf details.

Coordinator correction: no `.meta` files were created. The requested template does not exist in the checkout; Unity generates metadata and `GameArtImporter` applies label import settings.

## Verification

| Command/check | Result |
|---|---|
| `python3 ci/check_current_game_names.py --self-test` | 14 passed |
| `python3 ci/check_current_game_names.py` | 259 tracked text files inspected; 0 violations |
| `python3 build/outfit-labels/verify.py` | 23 PNGs verified: format, exact canvas, real alpha, transparent margins, exact ordered ID/text/file mappings, no metadata; four originals byte-identical |
| Protected-state SHA-256 comparison | Scene, scene metadata, project settings, packages, AGENTS.md and CLAUDE.md unchanged; agent files identical |
| `git diff --check` | Passed after removing one trailing blank line |
| `flock /tmp/unity.lock bash scripts/run_tests.sh` | Failed before Unity launched: configured Mac executable missing |
| `flock /tmp/unity.lock bash scripts/screenshots.sh` | Failed before Unity launched: configured Mac executable missing |

No Unity process was present and the lock was available before both attempts. This assigned workspace runs Linux. The exact missing executable is `/Applications/Unity/Hub/Editor/6000.6.7f1/Unity.app/Contents/MacOS/Unity`. EditMode/PlayMode: zero cases executed, no result XML, no passing or skipped test counts asserted. No runtime screenshots were produced. Mac import, compilation, full suites and runtime visual acceptance remain required during integration.

## Resolution boundary

The prompts requested 2736 × 588 for Blue Bow Tie and 4096 × 1024 for the remaining labels. Every built-in result was actually 2172 × 724; substantive ribbon widths were about 2130 px before downscaling. This is approximately 1.6× the delivered ribbon width, **not the required native 2×**. No synthetic upscaling is counted as oversampling. Accepting that source-resolution exception remains the coordinator's decision; the saved assets and contact sheet are available for review.

No runtime code, rules, levels, economy, saves, original art, scenes or settings were changed. Only the assigned branch is committed and pushed.
