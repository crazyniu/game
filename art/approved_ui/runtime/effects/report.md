# T48 Effects Delivery

## Scope and provenance

Delivered all 20 assigned effect and animation-particle sprites to `PerchSort/Assets/Resources/Art/`. Each was generated with a separate built-in image-tool call using an approved v7 screen as a direct reference. No CLI or API-key fallback was used. No Unity session, code change, commit or push was performed by this worker.

Exact prompts, reference names and original generated paths are in `generation_records.json`. Original generated RGBA files are preserved in `raw/`. `delivery.json` records dimensions, alpha counts, SHA256 hashes and per-sprite QA paths. `export_effects.py` only resizes, pads and composites generated artwork for QA; it does not draw artwork.

## Files and sizes

- `light_rays.png`: 1024 x 1024.
- `sparkle.png`: 128 x 128.
- `confetti_1.png` through `confetti_4.png`: 64 x 64, gold / pink / lavender / green.
- `glow_soft.png`: 512 x 512.
- `zzz.png`: 192 x 192.
- `leaf_fall_w1.png` through `leaf_fall_w4.png`: 64 x 64, spring leaf / bamboo leaf / maple leaf / snowflake.
- `cloud_soft_1.png` through `cloud_soft_3.png`: 768 x 384.
- `firefly.png`: 48 x 48.
- `leaf_wipe.png`: 1600 x 1600.
- `gift_trail.png`: 512 x 512.
- `house_glow.png`: 512 x 512.
- `gate_light.png`: 1024 x 1024.

## Anchors and compositing

All sprites use center transform pivots `(0.5, 0.5)`. Coordinates here are normalized, with origin at the top left. `house_glow` is centered on the house doorway center. `gate_light` has its luminous aperture/source approximately at `(0.50, 0.25)`; align that point with the gate opening and let the beam extend downward. It has no mechanical hinge. Exact shared-canvas alignment with the separately generated gate must be set by the integrating owner. The raw generated glow is preserved so it can be padded/placed without repainting.

Glows/rays/trails retain faint alpha pixels, with no threshold clipping or hard keying. They are suitable inputs for alpha-aware additive compositing; runtime material and intensity remain unverified. Do not multiply straight-alpha RGB twice. Render the victory rays behind its panel, house glow behind the bird face, and gate light between the next-world backdrop and gate arch/door layers. The static QA overlays deliberately place the effect above the reference screenshot so its complete shape and alpha can be seen; they are not final scene layer-order evidence.

## Verification

All final files are RGBA with genuine transparent pixels and partial alpha. Source alpha minima were zero; source maxima were 252–255. The faintest source border pixels reach alpha 0–3 out of 255; these were preserved rather than cutting glow tails. Only uniform Lanczos resizing and transparent padding were applied. Seasonal particles and confetti were reviewed at their requested 64-pixel canvas size; firefly at 48. No baked black/white matte or checkerboard is visible in the light/dark contact sheet.

Reviewed `qa/effects_contact.jpg` (all sprites over light gray and dark blue) and `qa/overlay_contact.jpg` (all sprites over matching approved screens). Full per-sprite overlays are also saved in `qa/*_overlay.jpg`. The small particles remain distinguishable by silhouette; maple and snowflake are particularly distinct. Cloud shapes vary in height and silhouette. Generated particles match the reference palette and rounded 3D finish, rather than being exact pixel extractions.

## Honest limits

This delivery is static artwork, not completed animation. No Unity import, material, filter-mode, timed movement, actual gate opening, popup transition, clipping, layer ordering or device-resolution acceptance was tested. The gate-light aperture must be bound to the final gate assembly. A single 1600-square leaf cluster does not itself prove full portrait-screen occlusion during a wipe; code must scale/position it for the transition. World 3/4 particle shapes are disclosed seasonal adaptations of the approved map style, since that screenshot does not show maple/snow particles directly.
