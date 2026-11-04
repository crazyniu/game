# T48 Gate Assembly Delivery

Nine final RGBA sprites are delivered in `PerchSort/Assets/Resources/V7/Art/`: `gate_arch`, `gate_door_left`, `gate_door_right`, `gate_padlock`, `gate_backdrop_w2`, `gate_backdrop_w3`, `gate_backdrop_w4`, `gate_locked` and `gate_open` (all `.png`). Every file uses the same 1024 x 1024 canvas.

## Sources and preparation

The arch reuses the previously generated approved-popup-derived asset from `assemblies_wip/raw/gate_arch.png`. The padlock reuses the approved-derived `padlock.png`; its small original is suitable for the small lock shown in this assembly, but should not be enlarged into a hero prop. `reused_sources.json` preserves their existing provenance. A single built-in image-tool call generated a matching closed paired door object; its center seam was cut into two independent transparent leaves. Three further built-in calls generated bamboo, maple and snowy-pine scenery plates. All four calls used `ui7_popup_world_unlock.png` as an explicit reference; the door call also referenced the existing isolated arch. Each succeeded in one attempt. Exact prompts and source paths are in `generation_records.json`, original results in `raw/`.

Pillow operations were limited to alpha cutouts, cropping, resizing, padding, mirroring and compositing. Scenery alpha follows the existing arch inner silhouette, with a small overlap behind the wood. No gate artwork was drawn programmatically. `assemble.py` reproduces the nine final outputs and metadata.

## Alignment contract

`delivery.json` is authoritative for integration values. Coordinates in pixel fields have top-left origin. The shared opening bounds are **xyxy**, `[240, 250, 768, 854]`: left, top, right-exclusive, bottom-exclusive. Its bounding rectangle is therefore 528 x 604 pixels; the actual top is arched and the silhouette includes the natural leaf/rock edge shapes.

The center door seam is x=512. Outer hinge points are left `(246, 610)` and right `(778, 610)`. Unity bottom-left normalized pivots are left `(0.240234375, 0.404296875)` and right `(0.759765625, 0.404296875)`. Door entries explicitly carry these hinge pivots, not center pivots. All other sprites use center pivots. The padlock is centered on `(512, 610)`.

The existing `gate_light.png` was not modified. Its normalized top-left aperture anchor `(0.5, 0.25)` aligns to assembly `(512, 375)` when drawn in rectangle `[192, 215, 640, 640]` (xywh). Its hash is recorded in the delivery metadata. The light is separate, never baked into locked/open fallbacks.

Layer order is recorded in metadata. Locked fallback: bamboo backdrop, closed left/right doors, arch, padlock. Open fallback: bamboo backdrop, arch, outward projected left/right doors. Animated assembly: world backdrop, optional gate light, moving doors, arch, padlock while locked. Hide the padlock when unlocking. Individual scenes can swap backdrop worlds without changing any geometry.

The static open fallback uses horizontally mirrored and narrowed crops of the generated closed leaves. Exact crop/resize/offset operations are recorded under `open_fallback_transforms`. This is an approximate static projection of fully opened leaves, not a 3D rotation sample or evidence of animation. Runtime motion must use the recorded outer hinges and be reviewed separately.

## QA and limitations

All nine outputs were checked for RGBA, genuine zero-alpha pixels and antialiased partial alpha. Sizes, hashes and alpha counts are in `delivery.json`. Reviewed `qa/gates_contact.jpg`: the closed doors cover the opening, open fallback exposes the bamboo path, seasonal plates fit the same aperture, and the arch opening is empty when viewed alone. Reviewed `qa/world_light_composites.jpg`: existing light aligns inside all three seasonal openings without changing the source effect.

Per-sprite approved-screen overlays are in `qa/*_overlay.jpg`; these are alpha/style inspection composites. The original screenshot's gate remains visible around the replacement, so they are not reconstructed runtime screens or proof of exact final placement. No white or black matte is visible in the inspected contact composites.

No Unity session, import, timed door swing, world-transition gameplay, additive material intensity or device-resolution acceptance was performed. Snow and maple scenery are explicit seasonal adaptations of the approved bamboo gate scene. No commits or pushes were performed by this worker; final integration/submission belongs to the parent.
