#!/usr/bin/env python3
"""normalize.py <raw dir> <out dir>: bring every painted world bird to one visual size on one canvas.

Each raw picture (transparent background, one bird on a short branch) is cropped to its pixels, scaled so the
square root of its visible area is the same for every bird (so a long-tailed bird and a round bird weigh alike),
then limited to a common maximum width and height, centred on its visible centre of mass and set on one baseline
(the bottom of the branch). Writes <out>/w<world>_<key>.png (512x512) and one contact sheet per world.
"""
import os, sys, glob
import numpy as np
from PIL import Image

CANVAS = 512
BASE = 470          # baseline row (bottom of the branch)
TARGET = 250.0      # sqrt of the visible area, in canvas pixels
MAX_W, MAX_H = 440, 400


def normalize(path):
    im = Image.open(path).convert("RGBA")
    a = np.asarray(im.getchannel("A"))
    ys, xs = np.nonzero(a > 40)
    im = im.crop((xs.min(), ys.min(), xs.max() + 1, ys.max() + 1))
    a = np.asarray(im.getchannel("A")) > 40
    k = TARGET / np.sqrt(a.sum())
    w, h = im.size
    k = min(k, MAX_W / w, MAX_H / h)
    im = im.resize((max(1, round(w * k)), max(1, round(h * k))), Image.LANCZOS)
    a = np.asarray(im.getchannel("A")) > 40
    cx = np.nonzero(a)[1].mean()
    out = Image.new("RGBA", (CANVAS, CANVAS), (0, 0, 0, 0))
    x = int(round(CANVAS / 2 - cx))
    x = max(0, min(CANVAS - im.size[0], x))
    out.alpha_composite(im, (x, BASE - im.size[1]))
    return out


def main(raw, dest):
    os.makedirs(dest, exist_ok=True)
    worlds = {}
    for p in sorted(glob.glob(os.path.join(raw, "w*_*.png"))):
        name = os.path.basename(p)
        out = normalize(p)
        out.save(os.path.join(dest, name))
        worlds.setdefault(name.split("_")[0], []).append(out)
    for w, birds in worlds.items():
        cell = 300
        sheet = Image.new("RGB", (3 * cell, 3 * cell), (232, 240, 228))
        for i, b in enumerate(birds[:9]):
            t = b.resize((cell, cell), Image.LANCZOS)
            sheet.paste(t, ((i % 3) * cell, (i // 3) * cell), t)
        sheet.save(os.path.join(dest, "sheet_%s.png" % w))
    print("normalized", sum(len(v) for v in worlds.values()), "birds")


if __name__ == "__main__":
    main(sys.argv[1], sys.argv[2])
