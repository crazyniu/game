#!/usr/bin/env python3
"""Derives the calm versions of the island kit pieces that sit side by side on the shop and settings screens, so the
screens use one green, one warm brown and soft card rims (the originals stay untouched):

- island_pill_glossy: the home page's Play base with its triangle painted out, its green moved to the design green.
- island_toggle_on_soft: the switch, same green.
- island_row_card_soft, island_grid_card_soft, island_pill_cream_soft: cream cards and chips whose yellow rim and dark
  orange outline become a light tan.

python3 tools/island/soften_palette.py
"""
import colorsys
import os

import numpy as np
from PIL import Image, ImageDraw, ImageFilter

ART = os.path.join(os.path.dirname(os.path.abspath(__file__)), "..", "..", "PerchSort", "art")
ISLAND = os.path.join(ART, "island")
DESIGN_GREEN = (67, 182, 22)   # art/approved_ui/settings.png, Help button face


def hsv_map(img, select, fn):
    a = np.asarray(img).astype(np.float32) / 255.0
    rgb = a[..., :3]
    out = rgb.copy()
    ys, xs = np.nonzero(select(a))
    for y, x in zip(ys, xs):
        out[y, x] = fn(*colorsys.rgb_to_hsv(*rgb[y, x]))
    return Image.fromarray((np.dstack([out, a[..., 3:]]) * 255).astype(np.uint8), "RGBA")


def to_design_green(img, value=0.9):
    th, ts, tv = colorsys.rgb_to_hsv(*[c / 255.0 for c in DESIGN_GREEN])

    def greenish(a):
        r, g, b = a[..., 0], a[..., 1], a[..., 2]
        return (g > r + 0.12) & (g > b + 0.12) & (a[..., 3] > 0.02)

    def fn(h, s, v):
        # Keep each pixel's light and shade (gloss, rim shadow) but move the hue and calm the saturation.
        return colorsys.hsv_to_rgb(th, min(1.0, s * 0.9), min(1.0, v * value))

    return hsv_map(img, greenish, fn)


def glossy_blank():
    im = Image.open(os.path.join(ART, "btn_home_island_play_blank.png")).convert("RGBA")
    w = im.width
    x0, x1, y0, y1 = 200, 790, 95, 700   # the painted triangle and its glow
    patch = im.crop((w - x1, y0, w - x0, y1)).transpose(Image.FLIP_LEFT_RIGHT)
    mask = Image.new("L", patch.size, 0)
    ImageDraw.Draw(mask).rectangle((25, 25, patch.width - 25, patch.height - 25), fill=255)
    out = im.copy()
    out.paste(patch, (x0, y0), mask.filter(ImageFilter.GaussianBlur(14)))
    return to_design_green(out)


def soft_rim(img):
    """Yellow rim and orange outline to a light tan, the cream face untouched."""
    tan = np.array([0.90, 0.80, 0.64])
    line = np.array([0.80, 0.64, 0.46])

    def rim(a):
        r, g, b = a[..., 0], a[..., 1], a[..., 2]
        mx = np.max(a[..., :3], -1)
        mn = np.min(a[..., :3], -1)
        sat = (mx - mn) / np.maximum(mx, 1e-6)
        return (sat > 0.28) & (a[..., 3] > 0.02)

    def fn(h, s, v):
        rgb = np.array(colorsys.hsv_to_rgb(h, s, v))
        target = line if v < 0.78 else tan
        k = 0.75
        return tuple(rgb * (1 - k) + target * k)

    return hsv_map(img, rim, fn)


def main():
    glossy_blank().save(os.path.join(ISLAND, "island_pill_glossy.png"))
    # The switch's own green is darker than the buttons'; lift it to the same face brightness.
    to_design_green(Image.open(os.path.join(ISLAND, "island_toggle_on.png")).convert("RGBA"), 1.17).save(
        os.path.join(ISLAND, "island_toggle_on_soft.png"))
    for name in ["island_row_card", "island_grid_card", "island_pill_cream"]:
        soft_rim(Image.open(os.path.join(ISLAND, name + ".png")).convert("RGBA")).save(
            os.path.join(ISLAND, name + "_soft.png"))


if __name__ == "__main__":
    main()
