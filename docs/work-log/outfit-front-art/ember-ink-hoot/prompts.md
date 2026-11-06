# Ember, Ink and Hoot generation prompts

Built-in `image_gen` only. Each main edit used the species front sprite upscaled with Lanczos to 1280×1280 as edit target and the corresponding upscaled side outfit as accessory reference. The tool returned 1254×1254 PNGs; extraction normalized these to the 1280×1280 work canvas before final 320×320 downsampling. No API/CLI fallback was used.

## outfit.ember.1

Use case: precise-object-edit. Edit target image 1 is a 1280x1280 upscaled front-facing Ember bird game sprite. Image 2 is accessory reference only: red triangular neck bandana tied on viewer left. Add exactly this red velvet bandana to image 1, fitted snugly around the neck below the beak, triangle centered on chest and knot at viewer left. Keep the original front bird pose, face, eyes, feathers, crest, feet, silhouette, exact scale and position pixel-aligned and otherwise unchanged. Same plush short-fur style, soft upper-left light. Only accessory pixels should change. Return one 1280x1280 transparent PNG of the front bird wearing this bandana. No background, no checkerboard, no text, no mirroring, no repositioning, no new costume features.

## outfit.ember.2

Use case: precise-object-edit. Image 1 is the edit target: 1280x1280 front-facing Ember bird. Image 2 is only the accessory reference, a red headband decorated with yellow-orange flames and a knot and two red tails at viewer left. Add that exact flame headband to the FRONT-facing bird in image 1. Fit it around forehead below crest and above eyebrows, centered straight front perspective, tied at viewer left. Same plush velvet style and upper-left light. Lock image 1 position, scale, front pose, both eyes, face, wings, crest and feet; only the headband area changes. Keep crest visible. Do not turn or mirror the bird. Return one 1280x1280 transparent PNG, no background or ground shadow, no text.

## outfit.ember.3

Use case: precise-object-edit. Image 1 is edit target, the 1280x1280 frontal Ember sprite. Image 2 is accessory reference only, silver knight open-face helmet with raised slotted visor, round side hinges and cheek side pieces. Fit exactly that silver helmet to image 1 in centered front perspective. Preserve orange flame crest sticking up above helmet. Visor stays raised across forehead ABOVE eyes, face and beak fully visible. Same softly painted plush game style, warm upper-left light; soft silver rather than harsh chrome. Keep bird's original frontal pose, scale, position, face, eyes, wings and feet pixel aligned unchanged; change only the helmet region. No mirror or turn, no text. Return one 1280x1280 transparent PNG.

## outfit.ink.1

Use case: precise-object-edit. Edit target image 1: 1280x1280 front-facing Ink blue bird. Image 2 accessory reference only: drooping deep purple sleeping cap with cream stars, thick lavender-white plush rim, white pompom on viewer left; blue crest peeks above rim. Add same sleeping cap snugly to image 1 in front perspective. Keep face, both eyes, beak, white chest star, body, wings, feet, exact scale and position unchanged. Purple cap surrounds top/back of crest and has rim above eyebrows; preserve blue crest. Warm upper-left lighting, soft velvet plush texture, no knit/stitching, no mirrored bird. Return single 1280x1280 transparent PNG. No background, no text.

## outfit.ink.2

Use case: precise-object-edit. Image 1 is exact edit target: 1280x1280 front-facing blue Ink bird. Image 2 is accessory reference only: ornate round silver spectacles, silver moon crescent detail at top outer rim, small side arms. Paint the same silver round glasses onto image 1, straight frontal view, one round frame centered over each eye and bridge resting above beak. Thin clear barely tinted lenses, eyes completely visible, fine pale reflection only, not opaque lens discs. Keep both eyes, face, beak, crest, white chest star, body and silhouette identical to image 1, same position and scale. Warm upper-left light, soft painted plush game style. Do not mirror or turn. Return one 1280x1280 transparent PNG, no background or text.

## outfit.ink.3

Use case: precise-object-edit. Image 1 is 1280x1280 front-facing blue Ink bird, exact edit target. Image 2 accessory reference only: very SMALL purple pointed wizard hat with pale cream stars, tiny brim and crooked tip, perched beside crest at viewer right. Add the same tiny wizard hat to image 1 snugly resting on upper-right forehead beside blue crest. Hat must remain tiny, about 120 pixels wide on this 1280 canvas, same relative size as reference. Plush velvet, upper-left warm light. Keep bird's original front-facing pose, eyes, face, body, white chest star, crest, feet, exact position and scale unchanged. No turning or mirroring, no background, no text. Return one 1280x1280 transparent PNG.

## outfit.hoot.1

Use case: precise-object-edit. Image 1 is edit target, exact 1280x1280 FRONT facing small brown Hoot owl at its existing scale and position. Image 2 is outfit reference: dark purple charcoal academic mortarboard with gold edging and gold tassel hanging on viewer left. Fit that exact graduation cap onto image 1, front perspective. It must fully cover the existing purple star nightcap including its white pompom, using only the graduation cap and its hanging gold tassel; no remaining nightcap or pompom visible. A broad snug purple graduation cap base may cover old nightcap silhouette, tassel covers old pompom. Keep bird eyes, beak, face, body, wings, feet, exact size and position unchanged. Same plush velvet softness, warm upper-left light, no new other accessories, no mirrored or turned bird. Return one 1280x1280 transparent PNG, no background, no text.

## outfit.hoot.2

Use case: precise-object-edit. Image 1 exact edit target is 1280x1280 FRONT-facing Hoot owl with purple star sleeping cap. Image 2 accessory reference only: thin round brown-gold wire spectacles with small bridge and side arms. Add same thin round spectacles to image 1, front-facing, one round lens centered over each original eye, bridge above beak. Preserve nightcap, stars, pompom, eyes, face, wings, body, feet, exact original scale and pixel position. Glass must be nearly clear with very subtle reflection, eyes fully visible through lenses. Same soft painted plush style and warm upper-left light. No mirroring or turning, no text, no background. Return one 1280x1280 transparent PNG.

## outfit.hoot.3

Use case: precise-object-edit. Image 1 exact edit target: 1280x1280 frontal brown Hoot owl with purple star nightcap. Image 2 accessory reference only: dark royal-blue cape sprinkled with gold stars, dark blue collar cord and round gold star clasp centered under beak. Paint same cape onto image 1, in front perspective, hanging on both sides behind wings with visible gold star pattern and front collar clasp centered on chest below face. Fit snugly to shoulders, not floating. Preserve existing nightcap, pompom, face, both eyes, beak, feet, wings, body size, scale and pixel position. Same velvet plush softness and upper-left warm light. No turning or mirroring, no background, no text. Return one 1280x1280 transparent PNG.

## Ink spectacles extraction repair

Use case: background-extraction. Extract ONLY the silver moon spectacles accessory from this 1280x1280 image. Keep its exact pixel position, size, straight-on perspective, two circular silver ornate rims, crescent moons, bridge and side arms unchanged. DELETE the entire blue bird, all feathers, both eyes including pupils/irises/white eyeballs, beak, chest star and feet. Transparent empty background everywhere. The interiors of BOTH lenses must also be transparent empty space with only a very faint semi-transparent pale blue glass sheen / small white reflection, NO eyes and no shapes from the bird baked into them. Do NOT fill glass with opaque color. Result should be ONLY a small pair of silver spectacles around x350..920,y560..745 in the same otherwise empty 1280x1280 canvas. No cropping, no resizing, no recentering. Preserve clean antialiased edges and genuine transparency.

## Hoot spectacles extraction repair

Use case: background-extraction. Extract ONLY the thin brown-gold round wire spectacles from this 1280x1280 image. Preserve exact position and scale and geometry: round rims, bridge, side arms at x380..900,y690..865. DELETE the owl completely, including hat, fur, BOTH eyes, pupils, irises, white eyeballs, beak and feet. Every space inside both lenses is transparent empty space with only an extremely faint semi-transparent glass sheen, no baked-in eyes or other bird detail. Do not fill lenses opaque. Return only the two round brown-gold frames and bridge and side arms, same unchanged pixel position on an otherwise empty 1280x1280 transparent canvas. No crop, no recenter, no resize, no background, no text. Clean antialiased alpha.

## Hoot graduation-cap coverage repair

Use case: precise-object-edit. Image 1 is FRONT-facing Hoot, exact 1280x1280 edit target. Image 2 is graduation outfit reference. Replace purple star sleeping cap with the SAME dark charcoal-purple graduation mortarboard, gold trim and left gold tassel. CRITICAL this will be extracted as an opaque accessory overlay and put ON TOP OF IMAGE 1 unchanged: the new cap+tassel MUST COVER EVERY PIXEL of the old purple cap and white pompom silhouette. Therefore make a broader mortarboard and fitted dark-purple skullcap extending all the way down the viewer-left side, with a FULL BROAD GOLD TASSEL placed exactly over the old white pompom. Old pompom spans approx x255..390,y710..852 on 1280 canvas, gold tassel must completely cover that whole footprint, at least 150px wide. The new dark cap must cover the entire old purple area including its top-left bulge around x350..550,y510..700. No purple nightcap, white pompom, or stars may peek out. This remains a single elegant graduation mortarboard, not two hats. Lock all owl facial features, eyes, front pose, body, wings, feet, position and scale; same original face untouched. Warm upper-left light, soft velvet plush. Return one transparent 1280x1280 PNG. No mirror, no background, no text.
