# T40 built-in image-generation prompts

## Map navigation atlas

Use case: ui-mockup
Asset type: isolated Perch Sort MAP UI artwork atlas on TRUE RGBA TRANSPARENCY.
ONE1536x1152 image with FOUR columns and THREE rows, 12 evenly spaced cells384x384. Each individual sprite fits280x280 maximum, wide transparent gutters, no contact. No captions, letters or numbers. Clean restrained soft dimensional colored pictograms with thin warm dark-brown outline, cream highlights, muted leaf greens. Same calm cream-glass forest game style. All isolated shapes, no full background.
Exact row-major order:
Row1 col1 SHOP: a simple pale woven shopping BASKET with green cloth and a single gold coin visible, clear handle.
Row1 col2 AVIARY: a tiny cute yellow bird portrait INSIDE a regular cream picture FRAME with a small green leaf at its upper corner. Clearly a framed bird, no cage bars.
Row1 col3 PLAY: one large muted-green right-facing TRIANGLE, rounded corners, brown contour.
Row1 col4 LOCK: one simple CLOSED warm amber padlock with cream keyhole.
Row2 col1: a blank regular round green DISC, thin warm brown outline, muted leaf green fill, soft minimal highlight.
Row2 col2: a blank regular round CREAM DISC, thin warm brown outline, pale ivory fill, soft minimal highlight.
Row2 col3: a blank regular round GREY DISC, thin warm brown outline, warm gray fill, subtle highlight, no icon or number.
Row2 col4: a single thin outlined EMPTY five-point STAR, dark warm brown line and transparent center, for unearned rating.
Row3 col1: small CLOSED forest WORLD GATE: two short bamboo-and-wood posts with a simple leafy arch, two little closed wooden doors, a tiny CLOSED padlock centered on doors, light moss detail. Compact symmetrical front view; no fence extending beyond cell.
Row3 col2: SAME forest WORLD GATE with both doors OPEN and NO lock, same posts and leafy arch, clear passage, same compact dimensions.
Row3 col3: one small pair of brown FOOTPRINTS for a moves-count icon, clear simple silhouette.
Row3 col4: one soft oval ground SHADOW only, dark warm brown, translucent blurred edge, no object.
Nothing outside individual sprites, no smoky atmosphere or colored backdrop. True transparent pixels.

## Selected direction

Coordinator message msg_4fdcfdeaa3c5 confirms the user chose A. No map B is produced. Final A uses 260 px K3 chip-center spacing and 44 px gift badges. The earlier K3 B is retained as a superseded comparison only.

## Map background (coordinator addendum msg_0edc00b3a988)

Use case: ui-mockup
Asset type: background art ONLY for a portrait forest puzzle world map, no UI.
Create one1080x1920 portrait image, 9:16. Match the attached existing Perch Sort forest art direction: softly dimensional rounded lush foliage, cozy warm illumination, charming polished 2.5D casual game, detailed but calm. This is a world-map illustration seen from high above at a gentle isometric angle, not an eye-level sky scene.
A wide continuous pale cream sandy PATH winds in a gentle S from bottom toward top, surrounded by layered little rounded trees, grass and bushes. The path has plenty of broad open clearings for level nodes. The lower TWO THIRDS depict spring oak woods with sunny yellow-green round canopies; the upper THIRD transitions into a cooler misty BAMBOO GROVE with elegant tall thin bamboo clumps and scattered tiny drifting leaves. All forest, no town, desert, mountain kingdom, sea or snow. Keep the middle path luminous and low texture.
Approximate path centerline in1080x1920 coordinates with top-left origin: enters bottom at(520,1900), broad clearing at(350,1510), bends to(360,1260), bends right to(650,1060), right clearing(780,840), curves back left through(620,640), then bamboo clearing(410,420), then(350,240), exits top at(500,0). Wide path180-210px. Leave the crossing at(620,640) free for a gate to be composited later. Leave top corners and bottom y1680..1860 quiet for HUD buttons. Scenic foliage at side margins, no leaves obscuring path.
DO NOT draw any level buttons, node discs, stones shaped like buttons, numbers, words, stars, UI, houses, gates, locks, birds, ropes, ribbons, labels, watermarks or frame. Just one beautiful forest landscape and continuous S path.

## Goal-house atlas

Use case: ui-mockup
Asset type: tiny goal-house atlas for a modern calm forest puzzle game.
One1536x1536 transparent PNG. THREE columns by THREE rows, exactly NINE isolated house icons, all IDENTICAL size and geometry. Each in its own512x512 cell, actual icon about320x350 with wide transparent gutters. No labels, no text, no birds, no foliage, no shadows outside icon.
Each is a SMALL clean cream birdhouse: simple triangular colored roof, pale ivory body, thin warm-brown outline, one LARGE DARK oval/rounded-arch doorway in the lower center. The doorway should be spacious, about half the house width and half the house height, EMPTY dark brown inside so a bird portrait can be composited later. Plain little straight base ledge. Quiet matte cream, subtle top highlight, no gloss, no wood grain, no giant bevels. No perches sticking outside, no signs, no ropes, no progress dots or gift badges. Consistent single facade, not 3D perspective. This must read clearly at88px.
Roof colors in row-major order:
row1: sunflower yellow, raspberry pink, mint leaf green.
row2: sky blue, cocoa brown, ivory lavender white.
row3: ember orange, midnight ink violet, soft tawny owl brown.
True RGBA transparency, not a scene.

## Utility atlas and updated Berry gift

Use case: ui-mockup
Asset type: Perch Sort isolated utility pictogram atlas.
Generate a true transparent PNG square with a FOUR by FOUR even grid, 16 isolated pictograms, each icon inside its own cell with wide fully transparent gutters. No text EXCEPT the tiny counter '+1' within the explicitly named crossed-out counter symbol. Clean COLORED rounded dimensional pictograms with thin dark warm-brown contour, muted leaf greens, pale cream and warm gold. All simple and readable at64px, small version readable at34px. No button bases, no scene, no atmosphere, no shadows outside icons. Flat transparent background.
EXACT row-major order:
Row1: 1 golden COIN with simple leaf embossed; 2 golden five-point STAR; 3 golden TROPHY cup with small green leaf emblem; 4 green CHECK MARK.
Row2: 1 green circular REPLAY ARROW; 2 tiny green-roof HOME; 3 muted green SPEAKER with TWO sound waves (unmuted); 4 small brown CLOSE X.
Row3: 1 green PLUS sign; 2 golden EIGHTH NOTE (decorative gift reference only, not meter); 3 green FOOTPRINT with a pale feather and a tiny brown '+1' counter crossed out by a diagonal line, this is 'next move does not count', NOT a music/encore icon; 4 green forward CHEVRON.
Row4: 1 a small warm-brown OUTLINED EMPTY CIRCLE, transparent center, for progress dot; 2 same small circle FILLED GREEN, for progress dot; 3 an amber padlock with open shackle, unlock symbol; 4 a red count-badge disc, plain empty soft red circle with thin brown outline.
No captions. Exactly16 sprites separated on true transparent background.

## Coordinator update (2026-11-03)

Message msg_3d4138e0075b supersedes the original task: remove every Song meter; allow board band y=420..1600; show an x2 chain ribbon near the K7 tray; replace Encore with next-move-does-not-count gift. Earlier prompts and raw drafts below are provenance only, not final requirements.

## Surface transparency correction

Use case: background-extraction
Edit target: attached UI surface atlas. Keep the SIX existing surface designs, colors, layout, shapes and proportions exactly the same. Remove the entire smoky brown/green background and ALL broad ambient glows. Every pixel outside the six sprite silhouettes must be completely transparent alpha=0. Sprite interiors must be fully opaque. Keep at most a tiny narrow contact shadow immediately below each silhouette. True isolated sprites on RGBA transparency, no colored backdrop, no checkerboard, no atmosphere. Preserve blank cream square, blank green square, blank pale bark/moss square, blank leaf badge, blank green ribbon, blank cream round medallion. Do not add or change any artwork.

## Nine gift glyphs

Use case: ui-mockup
One transparent sprite atlas, exact 1536x1536 square, THREE by THREE regular grid of NINE small gift pictograms. Each cell512x512, icon centered and about280x280. No text, numbers, labels, panels, button bases, enclosing circles, scenery or background. Absolutely empty transparent space around each icon. Clean simple colored symbols with dark warm-brown outline, softly dimensional yet restrained, pale ivory highlights. Readable at34px. Minimize tiny details. Warm leaf green, amber, teal, soft violet. Each has an OBVIOUSLY DIFFERENT silhouette.
Exact row-major order:
1 Wake song: ONE small golden rising SUN with ONE green music NOTE attached on its right.
2 Breeze: THREE turquoise WIND curls sweeping horizontally, plus ONE tiny leaf.
3 Encore: TWO violet MUSIC NOTES joined with ONE circular return arrow, no sun.
4 Gardener: ONE green WATERING CAN in profile with a short spout, no plant.
5 Stare: TWO LARGE round owl EYES side by side, cream and teal, no head or egg.
6 Light step: ONE cream FEATHER at a diagonal with THREE small footprint dots below.
7 Warm glow: ONE golden hanging LANTERN, clear handle and rectangular glowing window.
8 Night call: ONE dark violet OWL HEAD with two ear tufts and ONE small sound-wave arc on its right, no moon.
9 Moonbeam: ONE pale crescent MOON with a single long diagonal golden LIGHT BEAM ending in a four-point sparkle.
True RGBA transparency; do not draw a colored backdrop or checkerboard.

## UI surface atlas

Use case: ui-mockup
Asset type: Perch Sort UI surface atlas, true transparent background.
One image, 1536x1024, THREE columns by TWO rows, six isolated sprites in equal512x512 cells. Transparent gutters. No text, no icons inside, no letters, no numbers, no decorative scene. Keep each shape strictly inside its own cell, artwork about340px wide.
row1 col1: a BLANK cream-glass regular rounded square, radius82 at this340 size (28 at116), matte pale cream #FFF5DC, warm brown outline9px at this size (3 at116), exceptionally soft18px shadow below (6 at116), restrained top highlight, FLAT center fill, no heavy bevel, no orange.
row1 col2: same blank regular rounded square but muted leaf green primary face #86B745, same warm brown thin border, flat interior and gentle highlight.
row1 col3: same square silhouette, dimensions and corners but a light natural BARK AND MOSS skin: muted pale tan-gray wood, sparse fine grain, small moss green strip inside upper edge, NOT glossy orange wood, NOT an irregular outline. Quiet clean believable forest material.
row2 col1: a single plump green leaf-shaped LEVEL BADGE with entirely empty center, clean dark brown contour, minimal veins at edge, no text.
row2 col2: a horizontal LEAF-GREEN RIBBON header, width410 height100, simple small folded ends, thin brown outline, blank center, no text, no trophy.
row2 col3: a tiny round CREAM BADGE medallion, empty pale cream flat center, warm brown outline, small soft shadow, no contents.
Elegant modern casual game, clean colored dimensional illustration, minimal surface detail. Actual RGBA transparency, never draw checkerboard.

## Tool pictogram atlas

Use case: ui-mockup
Asset type: transparent game pictogram sprite atlas, one image containing exactly EIGHT isolated individual tool icons.
Generate an EXACT 2048x1024 transparent RGBA canvas with FOUR columns and TWO rows, each cell512x512. Place each icon centered in its cell, actual artwork280x280, wide completely transparent gutters, no button background, no text, no numbers, no grid, no labels, no shadows outside each icon. No contact between icons.
New Perch Sort UI: softly dimensional illustrated COLORED pictograms, clean warm dark-brown 3px-at-64px contour, cream highlights, leaf green positive, amber yellow, tiny red negative accent. Calm tasteful modern casual game. Clean readable silhouettes that still work at64px, low detail, no glossy orange wooden squares, no white silhouettes.
EXACT row-major order:
row1 col1 Undo: a single chunky green curved arrow returning LEFT, clearly one arrowhead.
row1 col2 Hint: a literal golden electric LIGHT BULB, silver screw base, exactly three small glow rays, not a bee.
row1 col3 Add branch: a small horizontal natural brown branch with one small leaf, large GREEN PLUS attached at lower right.
row1 col4 Peek-eggs / Owl Eye: one cream EGG, one unmistakable almond-shaped teal EYE floating above it, two distinct stacked shapes.
row2 col1 Wake / Morning Bell: a golden handless BELL with orange sunrise rays above, visible bell clapper.
row2 col2 Scarecrow: a charcoal CROW profile with beak and folded wing and one RED X at lower right. This means shoo crow, not a farm scarecrow.
row2 col3 Spring rain: a blue gray RAIN CLOUD, three blue rain drops, one GREEN two-leaf SPROUT below.
row2 col4 Settings: a small warm brown GEAR with six teeth and a cream center hole.
True transparent background; no checkerboard drawn.

These are concept-generation instructions, not executable game specifications. All generation used the built-in image tool; original images were read-only references.

## Cream-glass K3

Use case: ui-mockup
Create one beautiful shippable-quality portrait game screen concept for Perch Sort. EXACT 1080x1920 portrait canvas (9:16), app content only, no phone frame, status bar, watermark or annotations. Coordinates below use top-left origin in design pixels.
Input images: image 1 is the existing approved forest/branch/bird world reference ONLY, NOT a UI layout to copy. Image 2 is the actual forest background. Image 3 is the actual Sunny bird design. Image 4 is the actual branch design. Preserve this soft detailed 2.5D bird/forest art language: plush feathered expressive yellow Sunny, pink Berry, mint-green Mint, fine natural warm branches, bushes at roots, open sky and distant forest. Do not replace birds with flat stickers or generic spheres.
NEW UI visual language: restrained cream glass, pale ivory #FFF5DC rounded squares radius28 with thin warm dark-brown #725237 3px line, soft6px drop shadow, delicate top highlight. Colored literal pictograms with brown outline, no white silhouettes, no glossy orange wooden UI or thick bevel. Fredoka Bold style dark brown numerals. Plenty of open space; HUD must be SMALL.
Top: coin pill bounds(40,64,190,72), small48px golden coin plus "240"; centered leaf-green level badge72x72 at(540,100) with only "5"; gear in80x80 cream rounded square centered(1000,100). No other top controls.
Goal tray: ONLY ONE slim cream plank920x150 centered(540,250), short ropes at x180 and900 from top canopy. Exactly THREE small104x120 goal chips, centers x270,540,810 y250. Each is a tiny cream birdhouse with species-colored roof, face peeking through door, four small dots under house. Yellow: four empty dots; pink: two filled and two empty; mint: four green dots and small green check. Each has a tiny34px round gift glyph attached at upper-right. No large dangling houses or extra gift signs. Tray ends at y325.
Board y420..1520: EXACTLY FOUR branches, two left and two right, all existing-art warm slender branches with bushes only at outer-edge roots, only two tiny leaves at inner tips. Left tips end x510, right tips begin x570 (60px clear gap). Left branch foot lines at y710 and1180, right at y770 and1240, giving60px side offset. Eight birds total: left upper yellow+pink, right upper mint+yellow, left lower pink+mint, right lower yellow+pink. Birds approximately136px tall, feet rest on wood. Clear airy playable area, no collisions.
Song pill360x48 centered(540,1590): exactly five music notes, first THREE filled gold and last TWO empty brown outlines. No text or other objects in this row.
Bottom: exactly three116x116 cream square buttons at centers(225,1745),(435,1745),(645,1745); fourth slot empty. 64px pictograms: green curved BACK arrow for Undo; literal gold LIGHT BULB for Hint; small brown BRANCH with green plus for Add branch. Tiny40px count circles attached top-right: "3","2",green "+". No labels under buttons. Safe bottom forest remains visible.
No headings, no promotional text, no new mechanics, no large icons. This is a static concept for approval, not an animation.
