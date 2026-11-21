#!/usr/bin/env python3
"""publish_skin.py <species_slot>...: put finished skins into the game resources as WebP.

Takes out/skin/<species>_<slot>_<frame>.png (aligned, 320x320), writes PerchSort/assets/resources/birds/<species>_outfit_<slot>_<frame>.webp
at quality 85, removes every older file of that skin (overlay art, icons, other formats and their .meta) and its entries in
outfit_parts.json. Afterwards run `node tools/cocos-assets.mjs` and `node tools/cocos/meta.mjs`.
"""
import glob, json, os, sys
from PIL import Image

HERE = os.path.dirname(os.path.abspath(__file__))
BIRDS = os.path.abspath(os.path.join(HERE, "../../../PerchSort/assets/resources/birds"))


def drop_key(name, skin):
    path = BIRDS + "/" + name
    if not os.path.exists(path):
        return
    table = json.load(open(path))
    if table.pop(skin, None) is not None:
        json.dump(table, open(path, "w"), separators=(",", ":")); open(path, "a").write("\n")


for key in sys.argv[1:]:
    species, slot = key.split("_")
    skin = "%s_outfit_%s" % (species, slot)
    frames = sorted(glob.glob("%s/out/skin/%s_%s_*.png" % (HERE, species, slot)))
    assert len(frames) >= 15, "%s: only %d frames" % (key, len(frames))
    for old in glob.glob("%s/%s.*" % (BIRDS, skin)) + glob.glob("%s/%s_*" % (BIRDS, skin)):
        os.remove(old)
    for f in frames:
        frame = os.path.basename(f)[len(species) + len(slot) + 2:-4]
        Image.open(f).convert("RGBA").save("%s/%s_%s.webp" % (BIRDS, skin, frame), "WEBP", quality=85, method=4)
    drop_key("outfit_parts.json", skin)
    print("published", skin, len(frames), "frames")
