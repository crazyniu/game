#!/usr/bin/env python3
"""publish.py <species> <name> <slot> <propeller.png>: put a whole-bird skin into the game resources.

Copies out/whole/<species>_<frame>_<name>.png to PerchSort/assets/resources/birds/<species>_outfit_<slot>_<frame>.png, writes the
moving part (the propeller picture and the button position of every frame) into birds/outfit_parts.json, and drops the old
overlay art and anchors of that slot. Then run tools/cocos-assets.mjs and tools/cocos-meta.py.
"""
import json, os, sys
from PIL import Image

HERE = os.path.dirname(os.path.abspath(__file__))
BIRDS = os.path.abspath(os.path.join(HERE, "../../../PerchSort/assets/resources/birds"))
species, name, slot, prop = sys.argv[1:5]
skin = "%s_outfit_%s" % (species, slot)
table = json.load(open("%s/out/whole/%s_%s.json" % (HERE, species, name)))
for old in ("front_overlay",):
    p = "%s/%s_%s.png" % (BIRDS, skin, old)
    if os.path.exists(p):
        os.remove(p); os.remove(p + ".meta") if os.path.exists(p + ".meta") else None
hubs = {}
for frame, info in table.items():
    Image.open("%s/out/whole/%s_%s_%s.png" % (HERE, species, frame, name)).save("%s/%s_%s.png" % (BIRDS, skin, frame))
    hubs[frame] = info["hub"]
for blink in ("front_blink_01", "front_blink_02"):
    hubs[blink] = hubs["front"]
im = Image.open(prop).convert("RGBA")
im = im.crop(im.getbbox())
side = max(im.size)
sq = Image.new("RGBA", (side, side)); sq.alpha_composite(im, ((side - im.width) // 2, (side - im.height) // 2))
sq.resize((256, 256), Image.LANCZOS).save("%s/%s_prop.png" % (BIRDS, skin))
parts_path = BIRDS + "/outfit_parts.json"
parts = json.load(open(parts_path)) if os.path.exists(parts_path) else {}
parts[skin] = {"image": skin + "_prop", "width": 115, "squash": 0.3, "turns": 3.2, "hubs": {k: [round(v[0], 1), round(v[1], 1)] for k, v in sorted(hubs.items())}}
json.dump(parts, open(parts_path, "w"), separators=(",", ":")); open(parts_path, "a").write("\n")
anchors_path = BIRDS + "/outfit_anchors.json"
anchors = json.load(open(anchors_path))
if anchors.pop(skin, None) is not None:
    json.dump(anchors, open(anchors_path, "w"), separators=(",", ":")); open(anchors_path, "a").write("\n")
print("published", skin, len(table), "frames")
