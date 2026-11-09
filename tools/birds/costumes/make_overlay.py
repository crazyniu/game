#!/usr/bin/env python3
"""Turn a painted costume image (out/<species>_outfit_<n>_raw.png) into the game's outfit art.

Registers the painting to the plain species front frame (scale and shift fitted on the feet and lower body, which
no costume changes), keeps only the pixels the costume changed (never the eyes, so blinks stay plain), and writes
PerchSort/birds/<species>_outfit_<n>_front_overlay.png, <species>_outfit_<n>_front.png and the two front blinks as
exactly plain frame + overlay.  Usage: make_overlay.py <species> <n> [--sheet DIR]
"""
import os, sys
import numpy as np
from PIL import Image, ImageFilter

ROOT = os.path.abspath(os.path.join(os.path.dirname(__file__), "..", "..", ".."))
BIRDS = os.path.join(ROOT, "PerchSort", "birds")
HERE = os.path.dirname(os.path.abspath(__file__))
C = 320
HEAD_BOTTOM = 215
FEET_CUT = 262


def load(name):
    return Image.open(os.path.join(BIRDS, name + ".png")).convert("RGBA")


def register(raw, plain):
    """Best (scale, dx, dy) mapping the raw painting onto the plain frame, judged on the lower body."""
    pa = np.asarray(plain, np.float32)
    ref = pa[..., 3][235:310] / 255.0
    best = None
    big = raw.resize((C, C), Image.LANCZOS)
    for s in np.arange(0.96, 1.0401, 0.01):
        size = round(C * s)
        im = big.resize((size, size), Image.LANCZOS)
        for dy in range(-8, 9, 2):
            for dx in range(-8, 9, 2):
                canvas = Image.new("RGBA", (C, C), (0, 0, 0, 0))
                canvas.paste(im, (round((C - size) / 2) + dx, round((C - size) / 2) + dy))
                a = np.asarray(canvas, np.float32)[..., 3][235:310] / 255.0
                cost = float(np.abs(a - ref).mean())
                if best is None or cost < best[0]:
                    best = (cost, s, dx, dy, canvas)
    # refine in single pixels around the best
    return best


def main(sp, n, sheet=None):
    plain = load(sp + "_front")
    raw = Image.open(os.path.join(HERE, "out", "%s_outfit_%s_raw.png" % (sp, n))).convert("RGBA")
    cost, s, dx, dy, aligned = register(raw, plain)
    print(sp, n, "register cost %.4f scale %.2f shift %d,%d" % (cost, s, dx, dy))
    a = np.asarray(aligned, np.float32)
    p = np.asarray(plain, np.float32)
    # the change mask: colour or coverage differs from the plain frame
    diff = np.abs(a[..., :3] * a[..., 3:] / 255 - p[..., :3] * p[..., 3:] / 255).max(axis=2)
    diff = np.maximum(diff, np.abs(a[..., 3] - p[..., 3]) * 0.8)
    mask = diff > 30
    # the eyes are never part of a costume: whatever differs between the plain frame and its closed-eye blink
    blink = np.asarray(load(sp + "_front_blink_02"), np.float32)
    raw_eyes = (np.abs(blink[..., :3] - p[..., :3]).max(axis=2) > 40).astype(np.uint8) * 255
    dense = np.asarray(Image.fromarray(raw_eyes).filter(ImageFilter.GaussianBlur(4)), np.float32) / 255.0
    eye_img = Image.fromarray(((dense > 0.30) * 255).astype(np.uint8)).filter(ImageFilter.MaxFilter(9))
    eye_keep = np.asarray(eye_img) > 0
    m = Image.fromarray((mask * 255).astype(np.uint8)).filter(ImageFilter.MedianFilter(3)).filter(ImageFilter.MaxFilter(5))
    mask = np.asarray(m) > 0
    # the whole head box comes from the painting (a hood or hat is never the same colour as the head under it);
    # below it only what really changed (scarf, collar, frills): wings and feet stay the animated plain frame
    ys, xs = np.where(p[..., 3] > 128)
    head = np.zeros(mask.shape, bool)
    head[:HEAD_BOTTOM, xs.min() + 22:xs.max() - 22] = True
    mask |= head
    mask &= ~eye_keep
    m = Image.fromarray((mask * 255).astype(np.uint8)).filter(ImageFilter.GaussianBlur(1.2))
    m = np.asarray(m, np.float32) / 255.0
    m[eye_keep] = 0
    m[FEET_CUT:] = 0   # legs and feet are never costume; registration noise there would show as specks
    over = a.copy()
    over[..., 3] = a[..., 3] * m
    over_img = Image.fromarray(over.astype(np.uint8), "RGBA")
    out = lambda name, im: im.save(os.path.join(BIRDS, "%s_outfit_%s_%s.png" % (sp, n, name)))
    out("front_overlay", over_img)
    front = plain.copy(); front.alpha_composite(over_img); out("front", front)
    for i in (1, 2):
        b = load("%s_front_blink_%02d" % (sp, i)); b.alpha_composite(over_img); out("front_blink_%02d" % i, b)
    if sheet:
        os.makedirs(sheet, exist_ok=True)
        s2 = Image.new("RGBA", (C * 4, C), (200, 210, 190, 255))
        for k, im in enumerate([plain, aligned, over_img, front]):
            s2.alpha_composite(im, (k * C, 0))
        s2.save(os.path.join(sheet, "%s_%s.png" % (sp, n)))


if __name__ == "__main__":
    main(sys.argv[1], sys.argv[2], sys.argv[4] if len(sys.argv) > 4 and sys.argv[3] == "--sheet" else None)
