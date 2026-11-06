# Aviary list and detail repair

The captured list now clips scrolling cards inside the cream opening, with a soft vertical edge and the board frame above the content. The detail screen now has three large visible outfit cards, contained ability and homecoming rows, and access to the fourth outfit by horizontal scrolling. Only the aviary list, detail and their capture fixtures were changed.

The geometry repair is ready for coordinator review. The supplied runtime artwork still differs from the approved mockups; the differences below prevent a claim that the entire screen matches within 20 pixels. User acceptance remains pending.

## What was wrong

- The list board was a single image behind an oversized rectangular scroll area. Cards could cover the top arch and bottom frame, and the viewport ended in a hard line.
- The scrollbar occupied the frame edge. Bird portraits were too small inside their rings, and the card footer spacing did not match the reference.
- Detail squeezed four small outfit cards into one row. Their portraits, selected state, ability row and homecoming pill did not match the mockup proportions.
- Number bounds were applied after the initial glyph layout, allowing maximum values to overflow in an immediate state capture.

## Changes

`AviaryView.cs` defines the cream viewport at x=130..930, y=300..1788 on the 1080×1920 design canvas. Its `RectMask2D.softness` is `(0, 24)`. Portraits use stencil masks so the parent viewport's soft clipping still reaches the birds. The scrollbar track is x=934..956, y=416..1724; its 44×118 thumb stays inside the cream opening.

`AviaryBoardGraphic.cs` reuses the unchanged board texture through geometric patches. The background fills the page; a second mesh draws the arch, sides and bottom bar after the content and scrollbar. Its opening follows the painted arch. No new board artwork was generated or edited.

List rings are 344 pixels across, centered at x=330 and 730, with a 480-pixel row pitch. The first ring center is y=494; name ribbons overlap the ring bottoms at y=633. Homecoming pills and gold ability badges sit below them. The reference image measures roughly 340 pixels across the rings, so the brief's “about 170 px” was interpreted as radius rather than diameter. Per-species portrait offsets account for the transparent margins of the existing front sprites. Title and close-disc positions were aligned to the reference.

The detail panel is `(110, 1040, 860, 820)`. Three outfit cards of 236×260 are visible in a 720×300 clipped row, with the fourth canonical choice reachable by scrolling. The equipped choice has a green edge and check; locked choices retain readable silhouettes and padlocks and do not accept equip clicks. Berry's ability badge and painted caption share a row inside the cream panel. Longer captions use a centered badge above a full-width plate to preserve the lettering's aspect ratio. The homecoming pill is 440×168, centered at `(540, 1690)`.

Numbers receive their bounds before `SetValue`, so 99999 fits inside the coin and homecoming pills. All visible words remain painted label sprites, and all numbers remain painted glyphs. The existing front-facing breathing/blink implementation and collection/equip persistence were retained.

`AviaryFakeData.cs` adds the mid-scroll and last-outfit states. `GameCapture.RunAviary` applies the requested `IScreenStates` fixtures and saves PNGs, with `AudioListener.volume = 0` and `AudioListener.pause = true` before scene initialization or any state application. It does not change the saved mute setting.

## Capture evidence

References: `origin/ui-design-review`, commit `95423b6fd1d93e63ce3f6c248923176e76b069ee`, `art/ui_design/aviary_list.png` and `aviary_detail.png`. Both source images were opened and inspected.

Six serialized silent capture iterations were used to correct geometry. The final Unity 6000.6.7f1 batch exited with code 0 and produced all ten states. Every final side-by-side was visually inspected. Delivered captures are JPEG at 1080×1920; comparisons are 2160×1920, with the mockup on the left and the capture on the right at the same scale. The comparisons were assembled directly from the original capture PNGs, with no changes to their contents. [capture.log](capture.log) records each state with listener volume zero and pause true.

| State | Capture | Side-by-side |
| --- | --- | --- |
| List / Reference | [JPEG](reference.jpg) | [Comparison](reference-comparison.jpg) |
| List / Last row | [JPEG](last-row.jpg) | [Comparison](last-row-comparison.jpg) |
| List / Maximum coins and homecomings | [JPEG](maximum.jpg) | [Comparison](maximum-comparison.jpg) |
| List / Mid-scroll | [JPEG](mid-scroll.jpg) | [Comparison](mid-scroll-comparison.jpg) |
| Detail / Reference Berry | [JPEG](detail-reference.jpg) | [Comparison](detail-reference-comparison.jpg) |
| Detail / Equipped outfit | [JPEG](detail-equipped.jpg) | [Comparison](detail-equipped-comparison.jpg) |
| Detail / All outfits locked | [JPEG](detail-locked.jpg) | [Comparison](detail-locked-comparison.jpg) |
| Detail / Maximum homecomings | [JPEG](detail-maximum.jpg) | [Comparison](detail-maximum-comparison.jpg) |
| Detail / Longest name Flurry | [JPEG](detail-flurry.jpg) | [Comparison](detail-flurry-comparison.jpg) |
| Detail / Last outfit | [JPEG](detail-last-outfit.jpg) | [Comparison](detail-last-outfit-comparison.jpg) |

Final capture command, from the worker checkout:

```sh
python3 ci/check_current_game_names.py &&
PERCH_CAPTURE_DIRECTORY="$PWD/build/screens/aviary-scroll" \
PERCH_CAPTURE_STATES='List / Reference=reference|List / Last row=last-row|List / Maximum coins and homecomings=maximum|List / Mid-scroll=mid-scroll|Detail / Reference Berry=detail-reference|Detail / Equipped outfit=detail-equipped|Detail / All outfits locked=detail-locked|Detail / Maximum homecomings=detail-maximum|Detail / Longest name Flurry=detail-flurry|Detail / Last outfit=detail-last-outfit' \
flock /tmp/unity.lock \
/Applications/Unity/Hub/Editor/6000.6.7f1/Unity.app/Contents/MacOS/Unity \
-batchmode -disable-audio -projectPath "$PWD/PerchSort" \
-executeMethod PerchSort.Editor.GameCapture.RunAviary \
-screen-width 1080 -screen-height 1920 \
-logFile "$PWD/build/aviary-scroll-capture.log"
```

The required naming preflight reported 14 self-checks passing at the start and zero naming violations before the final capture. No EditMode or PlayMode suites were run or added. There were no C# compilation failures or capture exceptions in the final run. Raw PNGs and the full machine-specific Unity log remain under ignored `build/`; the committed log contains only the relevant capture evidence. Unrelated Unity import metadata was restored to the pre-run snapshot. Only the new graphic's `.cs.meta` is included.

## Remaining differences and acceptance boundary

- The current forest background, board arch, vines, leaves and cream/frame textures differ from the composed mockups. The detail panel still has its supplied corner leaves; the art lead owns their replacement.
- The existing gold ability badges have line-style symbols rather than the mockup's colored illustrations. Name ribbons, caption plates, padlocks and house icons also use the supplied kit's differing shapes and textures.
- Berry's retained front portrait has folded wings. Its visible vertical extent is approximately y=449..1059, close to the reference, but its roughly 484-pixel opaque width is narrower than the reference's roughly 746-pixel spread-wing pose. No still mockup bird was substituted for the animated runtime bird.
- The fourth outfit remains part of the game. Horizontal scrolling exposes it; the reference depicts only three choices. Long ability captions use the stacked arrangement shown in the Flurry capture rather than shrinking their painted lettering into Berry's shorter caption slot.
- Mid-scroll and last-row captures compare against the same list mockup because separate approved images for those states were not supplied. Their purpose is to show clipping, frame layering and scrollbar containment.
- These are static captures with instant motion and nonpersistent preview data. They do not establish timed animation, live input, audio playback, device behavior or final user acceptance. Audio was intentionally disabled throughout capture. No other reassigned screens or the game's sound behavior were changed.
