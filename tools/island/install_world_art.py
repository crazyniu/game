#!/usr/bin/env python3
"""Installs the per-world island paintings: the world map painting (1080x1620) and the board background (1080x1920)
of each world, plus the board's night version derived from its day painting (moonlit grade, soft sky glow). Branch
sprites and slot files are left as they are.

python3 tools/island/install_world_art.py <jobs dir> [world ...]
  <jobs dir>/w<N>/map.png and <jobs dir>/w<N>/board.png
"""
import os
import sys

import numpy as np
from PIL import Image

ROOT = os.path.join(os.path.dirname(os.path.abspath(__file__)), "..", "..", "PerchSort")


def fit(path, size):
    img = Image.open(path).convert("RGB")
    k = max(size[0] / img.width, size[1] / img.height)
    img = img.resize((round(img.width * k), round(img.height * k)), Image.LANCZOS)
    left = (img.width - size[0]) // 2
    top = (img.height - size[1]) // 2
    return img.crop((left, top, left + size[0], top + size[1]))


def night(day):
    a = np.asarray(day).astype(np.float32) / 255.0
    lum = a @ np.array([0.3, 0.55, 0.15], np.float32)
    moon = np.stack([lum * 0.42 + a[..., 0] * 0.12, lum * 0.5 + a[..., 1] * 0.14, lum * 0.78 + a[..., 2] * 0.22], -1)
    h = a.shape[0]
    y = np.linspace(0.0, 1.0, h, dtype=np.float32)[:, None, None]
    sky = np.clip(1.0 - y * 3.2, 0.0, 1.0)            # a soft violet glow over the top band
    moon += sky * np.array([0.10, 0.06, 0.16], np.float32)
    return Image.fromarray(np.clip(moon * 255.0, 0, 255).astype(np.uint8), "RGB")


def main():
    jobs = sys.argv[1]
    worlds = [int(w) for w in sys.argv[2:]] or list(range(1, 11))
    for w in worlds:
        d = os.path.join(jobs, "w%d" % w)
        if os.path.exists(os.path.join(d, "map.png")):
            fit(os.path.join(d, "map.png"), (1080, 1620)).save(os.path.join(ROOT, "art", "map_world_w%d.png" % w))
        if os.path.exists(os.path.join(d, "board.png")):
            day = fit(os.path.join(d, "board.png"), (1080, 1920))
            day.save(os.path.join(ROOT, "world", "scene", "board_w%d_bg.png" % w))
            night(day).save(os.path.join(ROOT, "world", "scene", "board_w%d_night_bg.png" % w))
        print("world", w, "installed")


if __name__ == "__main__":
    main()
