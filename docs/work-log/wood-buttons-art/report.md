# Wooden secondary button delivery

## Delivered assets

- `PerchSort/Assets/Resources/Art/btn_wood.png`
- `PerchSort/Assets/Resources/Art/btn_wood_pressed.png`
- Corresponding `.meta` files, copied byte-for-byte from the matching purple asset except for fresh asset GUIDs. This deliberately retains the original internal sprite names/IDs, centered pivot, PPU, mesh, filter, compression, alpha settings and stored sprite rectangles.
- Selected generation sources and all generation prompts in `art/wood_buttons/`.

Both PNGs are 160 x 160 RGBA. Their alpha channels are identical, so pressing does not shrink the disc. Alpha bounds are `(4, 3, 156, 156)` with 7,213 fully transparent pixels in each canvas. There is no disabled-state sprite.

The task's explicit wood direction supersedes the purple-button material described in CONVENTIONS. Existing art, approved references, runtime code, scene, settings, packages and persistence were not changed. Resource selection remains the integration owner's work.

## Visual review

The face uses honey-brown horizontal planks, restrained curved grain, a thin rounded golden rim, upper-left polish and a bottom shadow. Its hue belongs to the same material family as `node_wood.png` and the approved settings panel. The flatter frontal face is appropriate for controls; it does not copy the map nodes' deep cylindrical perspective.

Reviewed all requested assets: `icon_gear`, `icon_undo`, `icon_home`, `icon_close`, `icon_sound_on`, `icon_basket`, `icon_aviary`, `tool_hint`, `tool_branch`, `tool_bell`. Source glyphs are unchanged, including their existing near-white tint. Each glyph is fitted into a 90 px box on the 160 px disc, with a 3 px `#5A2F0E` outline. The entire composition is then downsampled to 64 px, including its outline.

All ten remain recognizable at 64 px. The branch and gold bell have the smallest color separation from wood; the outline preserves their silhouettes. Fine basket slots and the aviary bird lose detail at that size, but the main symbols remain readable. Wood grain is visible at native size and sufficiently subdued behind glyphs at phone size. The new row blends with the honey-wood nodes and green ribbon substantially better than purple.

The first pressed generation was rejected because it darkened the central sample by roughly 24 percent. The refined selected state is subtler, with an approximately 13.3 percent encoded-luma reduction in the central 50 x 50 sample. The prompt specifies a 97 percent inner-highlight footprint. This is a generated raster treatment, not a mathematically certified 3 percent transform; at 160 px that difference is approximately one pixel. The outer silhouette is verified pixel-identical between states.

No light matte, dark matte or colored fringe is visible in the reviewed native and 64 px composites on white, dark blue-black and green backgrounds. This is visual asset review, not a GPU import or device claim.

## Evidence

- `evidence/before_after.jpg`: old purple versus new wood in matched level bottom-bar and shop/aviary tab-row mocks, plus the approved wooden settings panel. The map crop retains authored nodes for direct material comparison.
- `evidence/icon_readability.jpg`: all ten glyphs at native 160 px and 64 px, rest against the approved level screenshot and pressed against approved map nodes.
- `evidence/alpha_states.jpg`: both states at native and reduced size against light, dark and green backgrounds.

These are explicitly labeled offline compositions of the approved screenshots, not Unity screenshots. No runtime visual acceptance is claimed.

## Preparation and checks

Built-in `image_gen` generated the resting source and edited its pressed state. The selected RGBA sources are preserved as `wood_raw.png` and `wood_pressed_raw.png`. Pillow was used for canvas normalization, metadata copying and the requested deterministic review compositions. Preparation removed alpha values at or below 10 to discard isolated nearly transparent generation specks, cropped both sources to `(47, 18, 1182, 1200)`, resized to 152 x 154 with Lanczos, and placed them at `(4, 3)` in a transparent 160 x 160 canvas. Pressed uses resting alpha exactly. RGB under fully transparent pixels is zeroed. No background color was keyed out.

- `python3 ci/check_current_game_names.py --self-test`: PASS, 9 tests.
- `python3 ci/check_current_game_names.py`: PASS, 0 violations; repeated after staging the new files.
- Direct Pillow/NumPy asset inspection: PASS, both canvases RGBA 160 x 160, identical state alpha, and metadata identical to the corresponding source except GUID.
- `git diff --cached --check`: reports 16 inherited trailing-space lines in the two copied Unity metadata files. Preserved intentionally to keep the requested exact copies apart from GUIDs. The same check excluding `*.meta` passes.
- `flock -n /tmp/unity.lock true`: PASS; the file lock was available.
- `flock /tmp/unity.lock bash scripts/run_tests.sh`: NOT_RUN. The user's Unity Editor remained open on the primary checkout (PID 29814), with import workers. Requested closure while continuing art work. No Editor was killed and no competing Unity process was launched. EditMode and PlayMode counts: unavailable, not historical counts.
- `flock /tmp/unity.lock bash scripts/screenshots.sh`: NOT_RUN for the same live-Editor boundary. Existing code still selects the purple resources, so ordinary captures would not establish acceptance of this unintegrated asset pair.

## Integration boundaries

The coordinator must wire the new resource names and requested glyph outline, review actual runtime screenshots, and run the serialized Unity suites when the user's Editor is closed. No code changes were authorized for this worker. The exact subpixel 3 percent highlight reduction remains a visual approximation in the generated source; silhouette, canvas and importer compatibility are measured.
