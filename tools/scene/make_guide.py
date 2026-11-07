"""Composition guide for a world's board painting: python3 tools/scene/make_guide.py <world> <rows> [top] [bottom]

Writes tools/scene/guide_wN.png (1024x1536 canvas, the 9:16 screen centred, branch bars where branches go,
red outlines where UI sits) and guide_wN.json (top y of each branch bar in screen pixels). Left rows sit at
top + r * pitch, right rows half a pitch lower; pitch = (bottom - top) / (rows - 0.5).
"""
import json
import os
import sys

from PIL import Image, ImageDraw

world, rows = int(sys.argv[1]), int(sys.argv[2])
top = float(sys.argv[3]) if len(sys.argv) > 3 else 700.0
bottom = float(sys.argv[4]) if len(sys.argv) > 4 else 1660.0
here = os.path.dirname(os.path.abspath(__file__))
W, H = 864, 1536
s = W / 1080
g = Image.new("RGB", (1024, 1536), (235, 235, 235))
d = ImageDraw.Draw(g)
ox = (1024 - W) // 2
d.rectangle([ox, 0, ox + W - 1, H - 1], fill=(255, 255, 255))
X = lambda x: ox + x * s
pitch = (bottom - top) / (rows - 0.5)
thick = 34 * min(1.0, pitch / 257)
slots = []
for r in range(rows):
    yl = top + r * pitch
    yr = yl + pitch / 2
    d.rectangle([X(0), yl * s, X(540), (yl + thick) * s], fill=(190, 130, 70))
    d.rectangle([X(540), yr * s, X(1080), (yr + thick) * s], fill=(190, 130, 70))
    slots += [{"side": "L", "row": r, "y": yl}, {"side": "R", "row": r, "y": yr}]
for box in ((40, 40, 420, 300), (400, 0, 680, 480), (860, 40, 1040, 170), (250, 1700, 830, 1880)):
    d.rectangle([X(box[0]), box[1] * s, X(box[2]), box[3] * s], outline=(255, 120, 120), width=3)
g.save(os.path.join(here, "guide_w%d.png" % world))
json.dump({"world": world, "rows": rows, "slots": slots}, open(os.path.join(here, "guide_w%d.json" % world), "w"), indent=1)
print("guide w%d rows %d pitch %.1f" % (world, rows, pitch))
