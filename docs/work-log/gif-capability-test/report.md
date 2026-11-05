# Built-in GIF capability evaluation

The built-in generator available in this session did **not** produce an animated GIF. An explicit request for a 24-frame, 20-fps flight animation returned **one static RGBA PNG containing four simultaneous bird poses**. This interface did not provide the requested GIF capability in the observed test. Generation stopped after this first result as instructed; no substitute animation was assembled.

## Scope and provenance

- Date: 2026-11-05, Mac workspace `gif-capability-test`.
- Branch: `mac-gif-capability-test/work`; starting commit `81dbe776070f7d3ae4ac74d278540fabfd2fba9d`.
- Exactly one call to Codex built-in `image_gen.imagegen`; tool-call wall time **36.242 seconds**. No external generation service, paid API, PixelLab, Pixelorama, CLI generator or video encoder was used.
- The exposed tool accepts a prompt, reference paths and transparent-background flag, but no explicit output-format, temporal-frame-count or FPS arguments. The prompt requested these properties, including an actual GIF rather than a still or sprite sheet.
- Exact prompt, parameters and original output location: [generation_request.json](../../../art/gif_tests/generation_request.json).
- Input identities: `sunny_idle.png`, `sunny_flap_01.png`; supporting style reference: `art/approved_ui/ui_win.png`. All were visually inspected before generation.
- Original bytes preserved as [sunny_flight_output.png](../../../art/gif_tests/sunny_flight_output.png); SHA-256 `184560c09c9b656964ca4b279bcae035addbc73f93e59e83dac89ad76d0e9e5d`.

## Requested cases versus actual output

| Case | Requested | Actual format / resolution | Temporal frames / FPS | Bytes | Transparency | Result |
| --- | --- | --- | --- | --- | --- | --- |
| A: Sunny flight and landing | 24 frames, 20 fps, 1.2 s; probe requested 512 x 512 | PNG RGBA / 1254 x 1254 | 1 / unavailable; no duration | 548,464 | 256 alpha values, 8-bit multilevel | Failed animated-GIF delivery |
| B: Straw-hat happy cheer | 6-8 frame loop | No output | Not measured | N/A | Not measured | NOT_RUN after stop condition |
| C: Sparkle/confetti burst | 12 frames | No output | Not measured | N/A | Not measured | NOT_RUN after stop condition |

The file starts with PNG signature `89504e470d0a1a0a`. Its chunks are `IHDR`, `caBX`, `IDAT`, `IEND`; there is no APNG animation-control chunk. Pillow reports `n_frames=1` and no duration metadata. FPS is **undefined**, not 20 or zero. Four drawings within one raster are not four temporal frames. The sole real frame was extracted losslessly to [sunny_flight_frame_01.png](../../../art/gif_tests/sunny_flight_frame_01.png). No GIF exists to copy under the 2 MB limit.

## Measurements

Colour counts are distinct RGB triplets among pixels with alpha greater than zero, measured on the original PNGs before compositing or JPEG encoding. Hidden RGB in fully transparent pixels is excluded. Different canvas sizes and subject area mean colour count is not a comparative quality score.

| Image / actual frame | Resolution | Visible RGB colours | Alpha levels |
| --- | --- | ---: | ---: |
| Generated sole frame | 1254 x 1254 | 85,400 | 256 |
| Existing sunny_idle | 320 x 320 | 19,770 | 255 |
| Existing sunny_flap_01 | 320 x 320 | 19,589 | 255 |
| Existing sunny_flap_02 | 320 x 320 | 21,249 | 256 |
| Existing sunny_flap_03 | 320 x 320 | 21,925 | 255 |
| Existing sunny_flap_04 | 320 x 320 | 21,754 | 256 |
| Existing sunny_flap_05 | 320 x 320 | 22,157 | 255 |
| Existing sunny_flap_06 | 320 x 320 | 20,857 | 255 |
| Existing sunny_outfit_1 | 320 x 320 | 23,273 | 253 |

Generated output: **111,511 visible RGBA tuples**, **1,313,194 fully transparent pixels**, **259,309 partially transparent pixels**, and **13 fully opaque pixels**. All alpha values 0-255 occur, so transparency is genuinely multilevel, not a binary cutout or painted checkerboard. Visible-pixel alpha median is 253/255; the existing flap and hat references also have median 253. The high partially-transparent count therefore does not mean the whole bird is visibly translucent. Full statistics, hashes and source sizes: [measurements.json](../../../art/gif_tests/measurements.json).

## Visual findings

- **Colour / texture:** At native 100%, the PNG retains yellow shading, pink cheeks and feather texture. Reviewed face and wing crops show no obvious palette banding or ordered dithering at 100% or nearest-neighbour 400%. At 400%, pixel steps, soft feather edges and a pale wing rim are visible. These are observations of a truecolour PNG; GIF palette, dithering and GIF alpha quality were not tested.
- **Alpha edges:** Dark and green-leaf composites reveal a soft pale/yellow rim around the generated wing tip. It is less conspicuous on the light background. The existing flap has a tighter, sharper feather edge in the selected crop; the hat reference also retains a light rim in places. This is a localized visual comparison, not a measured matte-contamination score or a clean-alpha acceptance. No dehalo, thresholding or alpha repair was applied.
- **Identity:** Curled crest, brown eyes, orange beak/feet, yellow feathers and pink cheeks remain recognizable. The spatial poses vary in wing posture, face placement and body silhouette. The 50% overlays show doubled eyes, beak and crest against the idle reference. Manual crop normalization includes scale/aspect changes and pose differences, so overlay mismatch cannot be attributed solely to identity drift. Some crops contain neighbouring-pose fragments because all poses share one canvas.
- **Accessories:** The output has no hat, as requested for A. The existing hat is included as a comparison only; no generated accessory consistency claim is possible because B was not attempted.
- **Motion:** There are **zero temporal transitions**. Entry trajectory, flap cadence, touchdown squash, landing timing, smoothness and loop seam are unmeasurable. The still lacks the requested flat landing surface, and the top-left bird is already partially visible. It cannot demonstrate a start completely outside the frame or a one-shot landing.

## Evidence

Open sheets at actual image size; a chat or browser fit-to-window preview may reduce them. All sheets are at most 1534 pixels wide. JPEGs use quality 98 with no chroma subsampling; numeric measurements use lossless PNGs, not these lossy sheets.

- [Native comparison](evidence/native_comparison.jpg): complete output, all six existing flap PNGs, idle and hat at 100% (one source pixel per sheet pixel).
- [Texture comparison](evidence/texture_comparison.jpg): labelled face and feather crops at 100% and nearest-neighbour 400%, generated / existing flap / existing hat side by side.
- [Alpha comparison](evidence/alpha_comparison.jpg): dark `(30,33,40)`, light `(245,245,240)` and actual green leaves from the approved win reference, with 100% crops and 400% detail. Smaller context previews are explicitly labelled.
- [Actual-frame overlay](evidence/frame_overlay.jpg): the sole actual frame blended 50/50 with a reference canvas registered to the landing pose, at native canvas resolution.
- [Static identity overlays](evidence/identity_overlay.jpg): four spatial crops with normalized idle overlays, explicitly not temporal frames. Registration and crop coordinates are recorded in [measure.py](measure.py).

## Fitness verdict

**Good for:** an offline static pose/concept discussion in the recognizable plush style, with rich PNG colour and real multilevel alpha. This does not approve the output for production art.

**Fails for:** direct fixed one-shot GIF delivery (1 frame instead of 24), specified canvas size (1254 instead of 512), FPS/duration control (no timing), isolated animation frames, demonstrated landing/squash and verified temporal consistency. Hat cheer and confetti suitability remain unknown, not independently failed tests. No evidence from this run supports replacing existing hand-sliced PNG animation assets.

This is a bounded result for the tool exposed in this session, not a claim about every Codex deployment or future capability. Generating a pose sheet and assembling it with code would be a different workflow and was deliberately not performed under the user's stop condition.

## Verification and boundaries

Commands run:

```sh
git switch -c mac-gif-capability-test/work
uv run --with pillow --with numpy python docs/work-log/gif-capability-test/measure.py
python3 ci/check_current_game_names.py --self-test
python3 ci/check_current_game_names.py
git diff --cached --check
```

Measurement completed; all five evidence sheets were visually reviewed. Naming self-test: **9 passed, 0 failures**. Naming guard and staged whitespace check: **passed**. Original generator bytes and extracted frame pixels were checked for preservation. Changed paths are restricted to this work log and `art/gif_tests/`.

Unity compilation, EditMode, PlayMode, GUI screenshots and device checks: **NOT_RUN**, not claimed passing. This assignment stops at documenting an unsupported generation result and introduces no game implementation, Resources changes or runtime animation. No existing tests were changed, weakened or skipped within a test run. No production acceptance, release, merge or primary-checkout mutation is claimed.
