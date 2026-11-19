#!/usr/bin/env python3
"""clean_alpha.py: zero the faint alpha noise around the painted skin frames.

Codex paintings carry nearly transparent specks (alpha 1-24) around the figure. The resource manifest records the ink box of a frame from
every non-zero alpha, so a speck at the canvas edge inflates it and the aviary scales the bird down to fit. This sets every alpha of 24 or less to
fully transparent in PerchSort/assets/resources/birds/*_outfit_*.webp and the owl propeller beanie PNGs. Run from the repository
root, then `node tools/cocos-assets.mjs`.
"""
import glob, os
from concurrent.futures import ProcessPoolExecutor
import numpy as np
from PIL import Image

BIRDS = os.path.join(os.path.dirname(os.path.abspath(__file__)), "../../PerchSort/assets/resources/birds")


def clean(path):
    a = np.asarray(Image.open(path).convert("RGBA")).copy()
    noise = (a[..., 3] > 0) & (a[..., 3] <= 24)
    if not noise.any():
        return 0
    a[noise] = 0
    if path.endswith(".png"):
        Image.fromarray(a, "RGBA").save(path, "PNG", optimize=True)
    else:
        Image.fromarray(a, "RGBA").save(path, "WEBP", quality=90, method=4)
    return 1


if __name__ == "__main__":
    files = sorted(glob.glob(BIRDS + "/*_outfit_*.webp") + glob.glob(BIRDS + "/*_outfit_*.png"))
    with ProcessPoolExecutor(4) as pool:
        changed = sum(pool.map(clean, files, chunksize=16))
    print("%d of %d frames cleaned" % (changed, len(files)))
