#!/usr/bin/env python3
"""Rebuilds art/kit (the painted surfaces every popup and HUD screen draws through UI.kit_piece) from the island pieces in
art/island, so one set of artwork drives the whole interface. Other colours are hue shifts of the calm green.

python3 tools/island/build_kit.py [popup_frame.png]
"""
import colorsys
import json
import os
import sys

import numpy as np
from PIL import Image

ROOT = os.path.join(os.path.dirname(os.path.abspath(__file__)), "..", "..", "PerchSort", "art")
ISLAND = os.path.join(ROOT, "island")
KIT = os.path.join(ROOT, "kit")


def load(name):
    return Image.open(os.path.join(ISLAND, name + ".png")).convert("RGBA")


def shift(img, hue, sat=1.0, val=1.0):
    """Hue shift for coloured (saturated) pixels only, so cream rims and white shine stay."""
    a = np.asarray(img).astype(np.float32) / 255.0
    rgb = a[..., :3]
    mx, mn = rgb.max(-1), rgb.min(-1)
    s = np.where(mx > 0, (mx - mn) / np.maximum(mx, 1e-6), 0)
    out = rgb.copy()
    mask = s > 0.35
    ys, xs = np.nonzero(mask)
    for y, x in zip(ys, xs):
        h, ss, v = colorsys.rgb_to_hsv(*rgb[y, x])
        out[y, x] = colorsys.hsv_to_rgb(hue, min(1.0, ss * sat), min(1.0, v * val))
    res = np.dstack([out, a[..., 3:]])
    return Image.fromarray((res * 255).astype(np.uint8), "RGBA")


def main():
    meta = {}

    def put(name, img, margins):
        img.save(os.path.join(KIT, name + ".png"))
        meta[name] = {"size": list(img.size), "margins": margins}

    def pill_margins(img):
        h = img.height // 2
        return [h, h, h, h]

    green = load("island_pill_green")
    cream = load("island_pill_cream")
    put("pill_green", green, pill_margins(green))
    put("pill_orange", cream, pill_margins(cream))     # secondary actions are the cream pill (dark word)
    put("pill_blue", shift(green, 0.56), pill_margins(green))
    put("pill_red", shift(green, 0.0, 1.1, 1.05), pill_margins(green))
    put("pill_grey", shift(green, 0.1, 0.0, 1.15), pill_margins(green))
    gold = load("island_pill_gold")
    put("pill_gold", gold, pill_margins(gold))
    put("capsule", cream, pill_margins(cream))
    put("capsule_plain", cream, pill_margins(cream))
    badge = load("island_badge_green")
    put("disc_green", badge, [0, 0, 0, 0])
    put("disc_cream", load("island_round_plate"), [0, 0, 0, 0])
    put("disc_orange", shift(badge, 0.08), [0, 0, 0, 0])
    put("disc_blue", shift(badge, 0.56), [0, 0, 0, 0])
    put("disc_red", shift(badge, 0.0, 1.1, 1.05), [0, 0, 0, 0])
    put("badge", load("island_badge_red"), [0, 0, 0, 0])
    card = load("island_grid_card")
    put("card", card, [60, 60, 60, 60])
    put("card_gold", shift(card, 0.12, 1.0, 1.0), [60, 60, 60, 60])
    ribbon = load("island_ribbon_plaque")
    put("ribbon", ribbon, [110, 56, 110, 56])
    for state in ("on", "off"):
        t = load("island_toggle_" + state)
        put("toggle_" + state, t, [t.height // 2] * 4)
    if len(sys.argv) > 1:
        frame = Image.open(sys.argv[1]).convert("RGBA")
        m = int(sys.argv[2]) if len(sys.argv) > 2 else frame.width // 5
        put("panel", frame, [m, m, m, m])
    old = json.load(open(os.path.join(KIT, "kit.json")))
    old.update(meta)
    json.dump(old, open(os.path.join(KIT, "kit.json"), "w"), indent=1)
    print("kit rebuilt:", ", ".join(sorted(meta)))


if __name__ == "__main__":
    main()
