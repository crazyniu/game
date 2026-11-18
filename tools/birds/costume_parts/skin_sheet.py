#!/usr/bin/env python3
"""skin_sheet.py <species> <slot> <out.jpg>: contact sheet of an aligned skin (top of each pair) over the plain frames (bottom)."""
import glob, json, os, sys
from PIL import Image, ImageDraw
HERE = os.path.dirname(os.path.abspath(__file__))
BIRDS = os.path.abspath(os.path.join(HERE, "../../../PerchSort/assets/resources/birds"))
species, slot, out = sys.argv[1:4]
table = json.load(open("%s/out/skin/%s_%s.json" % (HERE, species, slot)))
names = sorted(table)
T, cols = 150, 10
rows = (len(names) + cols - 1) // cols
sh = Image.new("RGB", (cols * T, rows * (2 * T + 14)), (225, 225, 225)); dr = ImageDraw.Draw(sh)
for i, n in enumerate(names):
    r, c = divmod(i, cols)
    for k, path in enumerate(("%s/out/skin/%s_%s_%s.png" % (HERE, species, slot, n), "%s/%s_%s.png" % (BIRDS, species, n))):
        im = Image.open(path).convert("RGBA").resize((T, T), Image.LANCZOS)
        sh.paste(im, (c * T, r * (2 * T + 14) + 14 + k * T), im)
    dr.text((c * T + 3, r * (2 * T + 14) + 1), n + ("" if table[n]["status"] == "ok" else " CHECK"), fill=(200, 0, 0) if table[n]["status"] != "ok" else (0, 0, 0))
sh.save(out, quality=82)
