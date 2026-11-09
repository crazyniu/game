#!/usr/bin/env python3
"""collect.py <skin>: copy painted skin frames out/<skin>_<frame>.png into PerchSort/birds at 320x320.

Each painted frame is aligned to its plain source frame so frames never jump: the lowest opaque row (feet) and the
alpha centroid of the lower half (body, unaffected by horns or hats) are moved onto the source's. Prints the shift.
"""
import glob, os, sys
import numpy as np
from PIL import Image

ROOT = os.path.dirname(os.path.dirname(os.path.dirname(os.path.dirname(os.path.abspath(__file__)))))
D = os.path.join(ROOT, "tools/birds/painted_skins")
BIRDS = os.path.join(ROOT, "PerchSort/birds")
SIZE = 320


def anchor(img):
    a = np.asarray(img.getchannel("A")) > 128
    rows = np.where(a.any(axis=1))[0]
    top, bottom = rows[0], rows[-1]
    lower = a[(top + bottom) // 2:bottom + 1]
    ys, xs = np.nonzero(lower)
    return float(xs.mean()), float(bottom)


def main(skin):
    species = skin.split("_")[0]
    for path in sorted(glob.glob(os.path.join(D, "out", skin + "_*.png"))):
        frame = os.path.basename(path)[len(skin) + 1:-4]
        src = Image.open(os.path.join(BIRDS, "%s_%s.png" % (species, frame))).convert("RGBA")
        out = Image.open(path).convert("RGBA").resize((SIZE, SIZE), Image.LANCZOS)
        sx, sb = anchor(src)
        ox, ob = anchor(out)
        dx, dy = round(sx - ox), round(sb - ob)
        fixed = Image.new("RGBA", (SIZE, SIZE), (0, 0, 0, 0))
        fixed.alpha_composite(out, (max(dx, 0), max(dy, 0)), (max(-dx, 0), max(-dy, 0)))
        fixed.save(os.path.join(BIRDS, "%s_%s.png" % (skin, frame)))
        print("%-24s shift %+d,%+d" % (frame, dx, dy))


if __name__ == "__main__":
    main(sys.argv[1])
