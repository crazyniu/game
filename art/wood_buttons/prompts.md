# Wooden button generation

Tool: built-in image_gen, transparent background enabled.

## Resting button

Use case: precise-object-edit. Asset: Perch Sort secondary round UI button, blank icon backing, transparent RGBA.
Image 1 is the edit target btn_purple: preserve its frontal circular silhouette, centred alignment, thin rounded gold rim, near-full square canvas occupation, subtle bottom shadow. Replace ONLY the purple material with honey-brown plank wood.
Image 2 node_wood is the exact wood palette and grain reference. Image 3 ui_settings is the wooden panel and soft glossy plush game style reference.
Match the node's warm honey-orange brown wood, gently curved visible horizontal grain and understated plank joins. Keep a thin luminous gold bevel like the target, soft upper-left highlight, darker lower-right bevel and tiny bottom shadow. Centre 65 percent should be calm, slightly darker/flatter low-contrast wood, leaving white outlined glyphs readable. No giant knots, nails, icons, text, leaves, sparkles, extra objects, perspective tilt, thick cylindrical base, checkerboard, or background.
One single round blank button, straight-on, square composition with transparent margin proportion like image 1. Real alpha transparency with clean antialiasing, no pale matte or fringe. Intended final canvas 160x160. Glossy crafted wood, not purple, not yellow plastic, not photorealistic.

Inputs: `PerchSort/Assets/Resources/Art/btn_purple.png` (edit target), `PerchSort/Assets/Resources/Art/node_wood.png` (wood reference), `art/approved_ui/ui_settings.png` (panel/style reference).

Output: `wood_raw.png`. Production canvas normalization and review compositions are recorded in the report.

## Pressed button

Use case: precise-object-edit. Create the pressed state of this exact blank honey-wood game button. Preserve exactly the wood grain and plank joins, thin golden rim, circular silhouette, full canvas, alignment, shadow and transparent background. Only darken the face slightly (about 8 percent) and reduce the upper-left white inner highlight footprint to 97 percent of its current width and height around its own centre. The outer rim and total button size must stay identical. No text, icon, background, extra marks, or redesign. Output one square RGBA sprite on real transparency.

Input: `wood_raw.png`. Output: `wood_pressed_raw.png`.

## Pressed refinement

The first pressed result darkened the centre by about 24 percent and was rejected. The selected output replaces it in place; no discarded production copy is retained.

Use case: precise-object-edit. This is the RESTING wood button and you must make a very subtly pressed state, NOT a dark brown variant. Keep the exact grain, seams, rim, outer shadow, silhouette, square framing, all margins and transparency. Face RGB must be only 8% darker: reference orange-brown central pixels around RGB (188,93,32) should become approximately (173,86,29), NOT (143,68,27). The gold rim stays identical. The existing main white upper-left face highlight must be precisely scaled to 97% width and 97% height about its own centre; the tiny second highlight also scales to 97%. Preserve highlight brightness. No additional changes. No icons, no text, no background, real RGBA transparent cutout. This is a minimal state edit of the exact supplied sprite.
