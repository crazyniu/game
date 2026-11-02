"""Record exact image-tool prompts and output contracts for T30 art."""
import json
from pathlib import Path

ROOT = Path(__file__).resolve().parents[1]
COMMON = "Use case: stylized-concept. Original production game art for Perch Sort, a bright warm forest bird sorting game. Plush 3D-rendered cartoon style, appealing rounded forms, clean soft thick edges, warm upper-left studio light and gentle rim light, polished casual mobile game finish. No text, letters, logos, watermark, decorative border, checkerboard, or unrelated objects. "
SPRITE = "One isolated asset on genuinely transparent RGBA background, no ground plane or detached cast shadow. Large centered object with a little transparent padding, entirely visible. "
BIRD = "Single original plush bird, three-quarter view turned slightly toward screen right while looking toward the viewer, orange beak and orange feet, soft short fur with clean silhouette. Square canvas, body centered at x=50%, body center y=55%, top of tuft at y=10%, standing feet end near y=95%; preserve these anchors across frames. Leave enough room at both sides for later open wings. "
SUNNY = "Character Sunny: lemon-yellow round chick with one distinctive curled head tuft, round chubby ball-shaped body, rosy peach cheeks, big glossy dark eyes and cheerful open smiling orange beak. Short folded wings. "
SKY = "Character Sky: sky-blue tall egg-shaped bird, broad white belly, one small round blue top-knot bun on a tiny stalk, calm half-closed glossy dark eyes, little closed orange beak, distinctly narrower and taller silhouette than Sunny. Short folded wings. "
items = []
def add(name, size, brief, transparent=True, reference=None):
    items.append(dict(name=name, size=size, transparent=transparent, reference=reference,
                      prompt=COMMON + (SPRITE if transparent else "") + brief))

add("bg_forest_day", [1080,1920], "Opaque full-bleed portrait 9:16 background only. Sky-blue at the top blending to pale warm cream near the bottom, soft fluffy clouds mostly near the top, soft hazy distant forest hills only very low in the frame, lush rounded saturated green leafy canopy framing the two top corners. The central playing band from 29% to 78% image height must be calm very low-contrast mostly open sky with no foliage intrusions. No branches, no birds, no UI, no houses, no paths, no objects in the playing band.", False)
add("branch_left", [640,120], "A single nearly horizontal natural warm wooden branch, very wide 16:3 canvas. The thick root begins cut cleanly at the LEFT canvas edge x=0 and extends fully to that edge; no margin on the left. The branch tapers toward a slender twig tip at the right with only two small green leaves close to the tip. Broad slightly organic but nearly flat walkable top surface at one-third image height, y about 40 of 120, across almost the entire branch. Soft wood grain with warm highlights. No fork upwards, no standing creatures, no support, no floor.")
add("bush_edge", [360,300], "Single dense rounded leafy bush cluster, lush bright leaf greens with darker lower leaves, large soft oval leaf shapes. Cluster extends rightward from the LEFT canvas edge with the far-left foliage cut by that edge. This is a screen-edge cover for a branch root, opaque foliage through its center, no visible wood or pot. Wide low dome profile.")
add("birdhouse", [300,340], "One cute cozy chunky warm wooden birdhouse front view, centered. Moss-green thick rounded sloped roof, NEVER red, round dark door hole exactly in the center of house facade, small cylindrical perch peg directly below the door, short natural hanging rope/vine at top. Simple softly bevelled planks, soft cream wood highlights. Entire short rope and base visible. No bird, lettering, flowers, dangling ornaments, or scenery.")
add("bird_sunny_idle", [256,256], BIRD+SUNNY+"Standing relaxed idle pose. Both feet fully visible at the common baseline. Feather wings folded against the body.")
add("bird_sky_idle", [256,256], BIRD+SKY+"Standing relaxed idle pose. Both feet fully visible at the common baseline. Feather wings folded against the body.")
for species in ("sunny", "sky"):
    for pose in ("blink", "fly_up", "fly_down"):
        ref=f"bird_{species}_idle"
        change={"blink":"Change ONLY the eyelids to gently closed curved lids for a blink; preserve every other feature, feet, silhouette, canvas, position, body size, beak, colors, highlights and fur exactly.",
                "fly_up":"Raise BOTH wings upward and outward in a clear flight upstroke, broad rounded feather fans pointing toward upper corners. Tuck both orange feet against the belly. Preserve the identical character, head tuft, eyes, face, body size, body center, colors, proportions and camera of the reference; do not tilt or translate the torso. Keep all wings within the square canvas with transparent margins.",
                "fly_down":"Lower BOTH wings downward and outward in a clear flight downstroke, broad rounded feather fans pointing toward lower corners. Tuck both orange feet against the belly. Preserve the identical character, head tuft, eyes, face, body size, body center, colors, proportions and camera of the reference; do not tilt or translate the torso. Keep all wings within the square canvas with transparent margins."}[pose]
        add(f"bird_{species}_{pose}",[256,256], "Input image is the edit target and exact identity reference. "+change, reference=ref)
for name,desc in {
    "btn_square_green":"Glossy leaf-green rounded-square button base, front facing, chunky evenly rounded corners radius about 60 in a 256 square, dark forest-green thick soft outline, lighter upper-left highlight and gentle bevel. Perfectly empty face, no icon.",
    "btn_square_wood":"Warm honey-wood rounded-square button base, front facing, chunky evenly rounded corners radius about 60 in a 256 square, dark warm brown thick soft outline, subtle wood grain, lighter upper-left highlight and gentle bevel. Perfectly empty face, no icon.",
    "btn_round_purple":"Circular glossy plum-purple button base, front facing perfect round silhouette, dark plum thick soft outline, lighter lilac upper-left highlight and gentle bevel. Perfectly empty face, no icon."}.items():
    add(name,[256,256],desc)
icons={
 "undo":"A single bold bent return arrow sweeping left: short horizontal tail enters from right, bends smoothly upward then left, ending in a broad LEFT-facing arrowhead. Clearly a back/undo gesture, not a circular refresh.",
 "hint":"A light bulb with a rounded bulb dome, small thick screw base and three short separated rounded light rays around its top.",
 "add_branch":"A simple sturdy horizontal tree branch with one small twig and an unmistakable large PLUS sign at its upper right, white branch and white plus.",
 "settings":"One symmetric chunky six-tooth rounded gear with a genuinely transparent circular hole through its center.",
 "restart":"One near-complete clockwise circular arrow with a clear gap and a single broad arrowhead at the upper-right end; transparent center.",
 "sound_on":"One side-view speaker with two curved sound-wave strokes on its right; transparent gaps between strokes.",
 "sound_off":"One side-view speaker with a bold small X at its right, no sound waves; transparent gaps.",
 "close":"One symmetric diagonal X made of two broad rounded bars.",
 "next":"One large right-pointing rounded triangular play arrow.",
 "home":"One simple house silhouette with a pitched roof and central rectangular open doorway; transparent doorway."}
for name,desc in icons.items():
    add("icon_"+name,[192,192],"Single centered WHITE icon with a thick dark brown #5A3A22 outline, chunky rounded silhouette, restrained soft bevel and top-left highlight, crisp readable at 48 pixels. No button, no colored disc, no background tile. "+desc)
add("coin",[128,128],"A single golden coin, front view with only very slight thickness, rich golden rim and warm golden center, one simple original embossed LEAF motif in the center, leaf stem and central vein clearly legible. No star, text, lettering or currency symbol.")
add("pill_cream",[260,90],"One horizontal cream rounded capsule pill UI background, aspect ratio 26:9, semicircular ends, soft warm brown outline and very gentle inner bevel, warm top highlight. Blank central face, no icons or text.")
add("panel_win",[860,900],"One nearly square slightly taller-than-wide cream UI panel with a thick warm honey-wood frame, gently rounded rectangular corners, smoothly bevelled edges and restrained wood grain. Large clean plain cream interior for later UI contents. Front view, symmetric, entire panel visible. No text, stars, ribbon, symbols, buttons, leaves, or decorations.")
add("ribbon",[900,200],"One wide leaf-green horizontal ribbon banner, aspect ratio 9:2, large blank central rectangular fabric band with subtly curved edges and folded fishtail tails at both ends, gentle glossy highlight and dark green soft edge. No text, ornament, star, or icon.")
add("star_full",[192,192],"A single symmetric five-point glossy golden star, chunky rounded tips, softly bevelled golden edge and warm orange shading, clean bright gold face with a soft upper-left gleam. No face, eyes, sparkle attachment or other object.")
add("star_empty",[192,192],"A single symmetric five-point empty-rating star silhouette, chunky rounded tips, uniformly soft muted warm brown with a darker soft outline and only a subtle bevel, no bright gold or shine. No face, eyes, interior icon or other object.")
add("sparkle",[128,128],"One soft warm ivory-gold four-point sparkle for a particle sprite, vertically and horizontally symmetric, four long tapering rays, bright small center and soft luminous transparent falloff. No other stars, trail, colored square or disk.")
add("hand_pointer",[200,200],"One cartoon white glove tapping pointer hand with a thick soft dark brown #5A3A22 outline and gentle cream shading. Index finger extended diagonally toward the TOP LEFT, fingertip located near (48,35) on a 200 square; other three fingers naturally curled, thumb visible, small rounded cuff at bottom right. Exactly one hand, no arrow, no click rings, no background, no text. Keep whole glove inside canvas.")

(ROOT/"manifest.json").write_text(json.dumps(items, indent=2)+"\n")
(ROOT/"prompts.md").write_text("# T30 v5 image generation prompts\n\nAll images use the built-in image generation tool. Each section is one separate call; references are listed explicitly. Original game assets, no competitor artwork supplied.\n\n"+"\n\n".join(f"## {i['name']}\n\nTarget: {i['size'][0]}×{i['size'][1]} PNG; transparent: {i['transparent']}; reference: {i['reference'] or 'none'}.\n\n```text\n{i['prompt']}\n```" for i in items)+"\n")
print(f"Prepared {len(items)} prompts")
