#!/usr/bin/env python3
"""Brighten the leaf greens of the painted kit toward the bright foliage green of the map.

python3 tools/kit/brighten_greens.py
Reads each piece as committed in Git (so a rerun never stacks the change) and writes the derived piece into
PerchSort/art/kit. Only green hues move: they get lighter and slightly more yellow; shading and outlines are kept.
"""
import colorsys
import io
import os
import subprocess

import numpy as np
from PIL import Image

ROOT = os.path.dirname(os.path.dirname(os.path.dirname(os.path.abspath(__file__))))
PIECES = ["pill_green", "disc_green", "ribbon", "toggle_on", "card"]
GAIN_V, GAIN_S, HUE_SHIFT = 1.28, 1.10, -0.012


def brighten(im):
    a = np.asarray(im.convert("RGBA"), dtype=np.float32) / 255.0
    rgb = a[..., :3]
    mx, mn = rgb.max(-1), rgb.min(-1)
    d = mx - mn
    v = mx
    s = np.where(mx > 0, d / np.maximum(mx, 1e-6), 0)
    r, g, b = rgb[..., 0], rgb[..., 1], rgb[..., 2]
    h = np.zeros_like(mx)
    m = d > 1e-6
    rc = np.where(m, (mx - r) / np.maximum(d, 1e-6), 0)
    gc = np.where(m, (mx - g) / np.maximum(d, 1e-6), 0)
    bc = np.where(m, (mx - b) / np.maximum(d, 1e-6), 0)
    h = np.where(mx == r, bc - gc, np.where(mx == g, 2.0 + rc - bc, 4.0 + gc - rc))
    h = (h / 6.0) % 1.0
    green = m & (h > 0.18) & (h < 0.46) & (s > 0.18)
    h2 = np.where(green, h + HUE_SHIFT, h)
    s2 = np.where(green, np.minimum(1.0, s * GAIN_S), s)
    v2 = np.where(green, np.minimum(1.0, v * GAIN_V), v)
    # hsv -> rgb
    i = np.floor(h2 * 6.0).astype(int) % 6
    f = h2 * 6.0 - np.floor(h2 * 6.0)
    p = v2 * (1 - s2)
    q = v2 * (1 - f * s2)
    t = v2 * (1 - (1 - f) * s2)
    choices = [(v2, t, p), (q, v2, p), (p, v2, t), (p, q, v2), (t, p, v2), (v2, p, q)]
    out = np.zeros_like(rgb)
    for k, (cr, cg, cb) in enumerate(choices):
        sel = i == k
        out[..., 0] = np.where(sel, cr, out[..., 0])
        out[..., 1] = np.where(sel, cg, out[..., 1])
        out[..., 2] = np.where(sel, cb, out[..., 2])
    out = np.where(m[..., None], out, rgb)   # greys stay as they were
    res = np.concatenate([out, a[..., 3:]], axis=-1)
    return Image.fromarray((np.clip(res, 0, 1) * 255 + 0.5).astype(np.uint8), "RGBA")


def main():
    for name in PIECES:
        rel = "PerchSort/art/kit/%s.png" % name
        raw = subprocess.check_output(["git", "-C", ROOT, "show", "HEAD:" + rel])
        brighten(Image.open(io.BytesIO(raw))).save(os.path.join(ROOT, rel))
        print(rel)


if __name__ == "__main__":
    main()
