#!/usr/bin/env python3
"""Derive the gold pieces of the island kit from existing ones (no new painting):

- island_pill_gold: the green pill with its green face recoloured to glossy gold (rewards, worded gold chips).
- island_price_gold: the feather price bar with its dark plum face recoloured to gold (prices on shop cards).

Run from the repository root: python3 tools/island/build_gold_pills.py
"""
import colorsys
from pathlib import Path

from PIL import Image

ART = Path(__file__).resolve().parents[2] / "PerchSort" / "art" / "island"
TOP = (255, 232, 104)
BOTTOM = (246, 172, 24)


def gold_at(t: float, shade: float) -> tuple:
    r, g, b = (TOP[i] + (BOTTOM[i] - TOP[i]) * t for i in range(3))
    return tuple(max(0, min(255, int(c * shade))) for c in (r, g, b))


def recolour(src: str, dst: str, picks, shade_of) -> None:
    im = Image.open(ART / f"{src}.png").convert("RGBA")
    px = im.load()
    box = im.getbbox()
    top, bottom = box[1], box[3]
    for y in range(im.height):
        t = (y - top) / max(1, bottom - top)
        for x in range(im.width):
            r, g, b, a = px[x, y]
            if a == 0:
                continue
            h, l, s = colorsys.rgb_to_hls(r / 255, g / 255, b / 255)
            if picks(h * 360, l, s):
                cr, cg, cb = gold_at(t, shade_of(l))
                px[x, y] = (cr, cg, cb, a)
    im.save(ART / f"{dst}.png")
    print("wrote", dst)


# Green face: hue 70-160. Its own lightness keeps the gloss and the darker rim.
recolour("island_pill_green", "island_pill_gold", lambda h, l, s: 60 <= h <= 170 and s > 0.25,
         lambda l: 0.62 + l * 0.75)
# Plum face: hue 260-360 or very dark. Lifted so the face reads as bright gold with a soft top highlight.
recolour("island_price_bar", "island_price_gold",
         lambda h, l, s: (h >= 250 or h <= 10) and l < 0.45 and s > 0.08,
         lambda l: 0.9 + l * 0.4)
