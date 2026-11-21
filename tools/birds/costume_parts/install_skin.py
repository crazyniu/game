#!/usr/bin/env python3
"""install_skin.py <species_slot> <zip>: put a packed skin into the game resources.

Removes every older file of the skin (overlay art, icons, other formats, .meta) and its entry in outfit_parts.json, then unpacks the WebP frames. Afterwards run `node tools/cocos-assets.mjs` and `node tools/cocos/meta.mjs`.
"""
import glob, json, os, sys, zipfile

BIRDS = os.path.abspath(os.path.join(os.path.dirname(os.path.abspath(__file__)), "../../../PerchSort/assets/resources/birds"))
key, archive = sys.argv[1:3]
species, slot = key.split("_")
skin = "%s_outfit_%s" % (species, slot)
for old in glob.glob("%s/%s.*" % (BIRDS, skin)) + glob.glob("%s/%s_*" % (BIRDS, skin)):
    os.remove(old)
with zipfile.ZipFile(archive) as z:
    z.extractall(BIRDS)
    count = len(z.namelist())
for name in ("outfit_parts.json",):
    path = BIRDS + "/" + name
    if os.path.exists(path):
        table = json.load(open(path))
        if table.pop(skin, None) is not None:
            json.dump(table, open(path, "w"), separators=(",", ":")); open(path, "a").write("\n")
print("installed", skin, count, "frames")
