#!/usr/bin/env python3
"""Bake fixed text INTO the artwork so text can never drift outside its ribbon or badge.

Outputs (PerchSort/Assets/Resources/Art/labels/):
  label_name_<species>.png  - green name ribbon with the species name centred on the visible band
  label_name_unknown.png    - the same ribbon with a question mark
  label_level_<N>.png       - the level leaf badge with the level number centred (N = 1..40)
Run from the repo root: python3 tools/bake_labels.py
"""
import json, os
from PIL import Image, ImageDraw, ImageFont

ROOT = os.path.join(os.path.dirname(__file__), "..", "PerchSort", "Assets")
ART = os.path.join(ROOT, "Resources", "Art")
OUT = os.path.join(ART, "labels")
FONT = os.path.join(ROOT, "Fonts", "Fredoka-Medium.ttf")
METRICS = json.load(open(os.path.join(ART, "layout_metrics.json")))

SPECIES = [("sunny", "Sunny"), ("sky", "Skye"), ("berry", "Berry"), ("mint", "Minty"), ("cocoa", "Cocoa"),
           ("snow", "Flurry"), ("ember", "Ember"), ("crow", "Rook"), ("ink", "Inky"), ("hoot", "Hoot"), ("unknown", "?")]

def fit_text(draw, text, box_w, box_h, max_size, stroke):
    size = max_size
    while size > 12:
        font = ImageFont.truetype(FONT, size)
        l, t, r, b = draw.textbbox((0, 0), text, font=font, stroke_width=stroke)
        if r - l <= box_w and b - t <= box_h:
            return font, (l, t, r, b)
        size -= 2
    font = ImageFont.truetype(FONT, 12)
    return font, draw.textbbox((0, 0), text, font=font, stroke_width=stroke)

def bake(base_name, inner_key, text, out_name, fill, outline, stroke, fraction_w, fraction_h, max_size, dy=0, dx=0):
    base = Image.open(os.path.join(ART, base_name + ".png")).convert("RGBA")
    x, y, w, h = METRICS[inner_key]["inner"]
    draw = ImageDraw.Draw(base)
    font, (l, t, r, b) = fit_text(draw, text, w * fraction_w, h * fraction_h, max_size, stroke)
    cx, cy = x + w / 2 + dx, y + h / 2 + dy
    # textbbox is relative to the anchor point; centre the INK box on the band centre
    px, py = cx - (l + r) / 2, cy - (t + b) / 2
    draw.text((px, py), text, font=font, fill=fill, stroke_width=stroke, stroke_fill=outline)
    base.save(os.path.join(OUT, out_name + ".png"))

os.makedirs(OUT, exist_ok=True)
for key, name in SPECIES:
    bake("ribbon_green", "ribbon_green", name, f"label_name_{key}", (255, 255, 255, 255), (38, 84, 16, 255), 4, 0.78, 0.74, 108, dy=8)
for n in range(1, 41):
    bake("leaf_badge", "leaf_badge", str(n), f"label_level_{n}", (255, 255, 255, 255), (30, 90, 20, 255), 3, 0.60, 0.78, 100, dy=4, dx=10)
print("baked", len(os.listdir(OUT)), "images into", OUT)
