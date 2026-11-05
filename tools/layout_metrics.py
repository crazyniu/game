#!/usr/bin/env python3
"""Measure the real usable areas of the UI frame art so layouts are built from measurements, not guesses.

For every frame/pill/ribbon sprite writes, in sprite pixels (top-left origin):
  opaque   - bounding box of visible pixels (alpha > 40)
  inner    - largest cream/content area a child may use (frames) or the flat band (ribbons, pills)
Output: PerchSort/Assets/Resources/Art/layout_metrics.json
"""
import json, os
import numpy as np
from PIL import Image
from collections import deque

ART = os.path.join(os.path.dirname(__file__), "..", "PerchSort", "Assets", "Resources", "Art")

def load(name):
    return np.array(Image.open(os.path.join(ART, name + ".png")).convert("RGBA")).astype(int)

def bbox(mask):
    ys, xs = np.where(mask)
    return [int(xs.min()), int(ys.min()), int(xs.max() - xs.min() + 1), int(ys.max() - ys.min() + 1)]

def opaque(a):
    return bbox(a[..., 3] > 40)

def cream_inner(a, margin):
    """Flood fill the cream interior from the centre; return its bbox shrunk by margin."""
    h, w = a.shape[:2]
    r, g, b, al = a[..., 0], a[..., 1], a[..., 2], a[..., 3]
    cream = (al > 200) & (r > 215) & (g > 185) & (b > 140) & (r - b < 120)
    seen = np.zeros((h, w), bool)
    q = deque([(h // 2, w // 2)])
    # start from the nearest cream pixel to the centre
    if not cream[h // 2, w // 2]:
        ys, xs = np.where(cream)
        i = np.argmin((ys - h // 2) ** 2 + (xs - w // 2) ** 2)
        q = deque([(ys[i], xs[i])])
    seen[q[0]] = True
    while q:
        y, x = q.popleft()
        for dy, dx in ((1, 0), (-1, 0), (0, 1), (0, -1)):
            ny, nx = y + dy, x + dx
            if 0 <= ny < h and 0 <= nx < w and cream[ny, nx] and not seen[ny, nx]:
                seen[ny, nx] = True; q.append((ny, nx))
    # ignore thin leaks: keep rows/cols where the filled width is substantial
    rows = np.where(seen.sum(1) > w * 0.35)[0]
    cols = np.where(seen.sum(0) > h * 0.25)[0]
    x0, x1, y0, y1 = cols.min() + margin, cols.max() - margin, rows.min() + margin, rows.max() - margin
    return [int(x0), int(y0), int(x1 - x0 + 1), int(y1 - y0 + 1)]

def green_band(a):
    r, g, b, al = a[..., 0], a[..., 1], a[..., 2], a[..., 3]
    green = (al > 200) & (g > r + 25) & (g > b + 60)
    rows = np.where(green.sum(1) > a.shape[1] * 0.35)[0]
    cols = np.where(green[rows.min():rows.max() + 1].sum(0) > (rows.max() - rows.min()) * 0.5)[0]
    return [int(cols.min()), int(rows.min()), int(cols.max() - cols.min() + 1), int(rows.max() - rows.min() + 1)]

def content_band(a, margin_x, margin_y):
    x, y, w, h = opaque(a)
    return [x + margin_x, y + margin_y, w - 2 * margin_x, h - 2 * margin_y]

out = {}
for name, margin in (("card_wood", 14), ("panel_wood", 14), ("aviary_board", 18)):
    a = load(name)
    out[name] = {"size": [a.shape[1], a.shape[0]], "opaque": opaque(a), "inner": cream_inner(a, margin)}
for name in ("name_ribbon", "ribbon_green"):
    a = load(name)
    out[name] = {"size": [a.shape[1], a.shape[0]], "opaque": opaque(a), "inner": green_band(a)}
for name, mx, my in (("stat_pill", 18, 12), ("house_count_chip", 16, 10), ("coin_pill", 90, 24), ("btn_green_pill", 18, 14), ("leaf_badge", 40, 40)):
    a = load(name)
    out[name] = {"size": [a.shape[1], a.shape[0]], "opaque": opaque(a), "inner": content_band(a, mx, my)}
path = os.path.join(ART, "layout_metrics.json")
with open(path, "w") as f:
    json.dump(out, f, indent=2); f.write("\n")
print(json.dumps(out, indent=1))
