#!/usr/bin/env python3
"""Brighten the leaf greens of the painted kit toward the bright foliage green of the map.

python3 tools/kit/brighten_greens.py
Reads each piece from the last commit before this change (so a rerun never stacks the change; run it once, on the
original pieces, and do not rerun it after the result is committed) and writes the derived piece into
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
    hue_out = np.where(green, h + HUE_SHIFT, h)
    sat_out = np.where(green, np.minimum(1.0, s * GAIN_S), s)
    val_out = np.where(green, np.minimum(1.0, v * GAIN_V), v)
    # hsv -> rgb
    i = np.floor(hue_out * 6.0).astype(int) % 6
    f = hue_out * 6.0 - np.floor(hue_out * 6.0)
    p = val_out * (1 - sat_out)
    q = val_out * (1 - f * sat_out)
    t = val_out * (1 - (1 - f) * sat_out)
    choices = [(val_out, t, p), (q, val_out, p), (p, val_out, t), (p, q, val_out), (t, p, val_out), (val_out, p, q)]
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
