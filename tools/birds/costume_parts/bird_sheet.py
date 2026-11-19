#!/usr/bin/env python3
"""bird_sheet.py <bird> <out.jpg>: contact sheet of every installed frame of a bird (WebP or PNG), the slot species' frame under each for comparison."""
import glob, os, sys
from PIL import Image, ImageDraw
HERE = os.path.dirname(os.path.abspath(__file__))
BIRDS = os.path.abspath(os.path.join(HERE, "../../../PerchSort/assets/resources/birds"))
bird, out = sys.argv[1:3]
frames = sorted(os.path.splitext(os.path.basename(f))[0][len(bird) + 1:] for f in glob.glob(BIRDS + "/%s_*.webp" % bird) if "_outfit_" not in f)
T, cols = 150, 10
rows = (len(frames) + cols - 1) // cols
sh = Image.new("RGB", (cols * T, rows * (T + 14)), (225, 225, 225)); dr = ImageDraw.Draw(sh)
for i, n in enumerate(frames):
    r, c = divmod(i, cols)
    im = Image.open("%s/%s_%s.webp" % (BIRDS, bird, n)).convert("RGBA").resize((T, T), Image.LANCZOS)
    sh.paste(im, (c * T, r * (T + 14) + 14), im); dr.text((c * T + 3, r * (T + 14) + 1), n, fill=(0, 0, 0))
sh.save(out, quality=82)
