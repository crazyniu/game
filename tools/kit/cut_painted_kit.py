"""Cut the Codex-painted UI kit sheets into named pieces with nine-slice margins.

python3 tools/kit/cut_painted_kit.py
tools/kit/painted/kit_buttons.png, kit_frames.png -> PerchSort/art/kit/<name>.png and art/kit/kit.json
({name: {"size": [w, h], "margins": [left, top, right, bottom]}}). Pieces are found as connected alpha regions and
named by their position on the sheet (see NAMES); margins keep the rounded ends and the outline intact.
"""
import json
import os
from collections import deque

import numpy as np
from PIL import Image

ROOT = os.path.dirname(os.path.dirname(os.path.dirname(os.path.abspath(__file__))))
SRC = os.path.join(ROOT, "tools", "kit", "painted")
OUT = os.path.join(ROOT, "PerchSort", "art", "kit")

# Reading order (rows top to bottom, then left to right) of each sheet.
NAMES = {
    "kit_buttons.png": ["pill_green", "pill_orange", "pill_blue", "pill_red", "pill_grey", "toggle_on",
                        "disc_green", "disc_orange", "disc_blue", "disc_red", "disc_ring"],
    "kit_frames.png": ["ribbon", "capsule", "panel", "card", "hud_bar"],
    "kit_extra.png": ["capsule_plain", "toggle_off", "badge"],
}
# Nine-slice margins as a share of the piece: horizontal pills keep their round ends, frames their corners.
MARGIN_SHARE = {"pill": (0.5, 0.45), "toggle": (0.5, 0.45), "disc": None, "ribbon": (0.3, 0.45),
                "capsule": (0.5, 0.45), "badge": None, "panel": (0.14, 0.2), "card": (0.2, 0.17), "hud_bar": (0.5, 0.45)}


def components(alpha):
    mask = alpha > 24
    lab = np.zeros(mask.shape, np.int32)
    boxes = []
    h, w = mask.shape
    for y0, x0 in zip(*np.nonzero(mask[::3, ::3])):
        y0, x0 = y0 * 3, x0 * 3
        if lab[y0, x0] or not mask[y0, x0]:
            continue
        n = len(boxes) + 1
        q = deque([(y0, x0)])
        lab[y0, x0] = n
        ys, xs = [y0], [x0]
        while q:
            y, x = q.popleft()
            for yy, xx in ((y + 1, x), (y - 1, x), (y, x + 1), (y, x - 1)):
                if 0 <= yy < h and 0 <= xx < w and mask[yy, xx] and not lab[yy, xx]:
                    lab[yy, xx] = n
                    q.append((yy, xx))
                    ys.append(yy)
                    xs.append(xx)
        if len(ys) > 4000:
            boxes.append((min(xs), min(ys), max(xs) + 1, max(ys) + 1, n))
    return lab, boxes


def reading_order(boxes):
    boxes = sorted(boxes, key=lambda b: (b[1] + b[3]) / 2)
    rows, row = [], []
    for b in boxes:
        if row and (b[1] + b[3]) / 2 - (row[-1][1] + row[-1][3]) / 2 > 90:
            rows.append(row)
            row = []
        row.append(b)
    rows.append(row)
    return [b for r in rows for b in sorted(r, key=lambda b: b[0])]


def main():
    os.makedirs(OUT, exist_ok=True)
    meta = {}
    for sheet, names in NAMES.items():
        im = np.array(Image.open(os.path.join(SRC, sheet)).convert("RGBA"))
        lab, boxes = components(im[..., 3])
        boxes = reading_order(boxes)
        if len(boxes) != len(names):
            raise SystemExit("%s: found %d pieces, expected %d" % (sheet, len(boxes), len(names)))
        for (x0, y0, x1, y1, n), name in zip(boxes, names):
            piece = im[y0:y1, x0:x1].copy()
            piece[..., 3] = np.where(lab[y0:y1, x0:x1] == n, piece[..., 3], 0)
            Image.fromarray(piece, "RGBA").save(os.path.join(OUT, name + ".png"))
            w, h = int(x1 - x0), int(y1 - y0)
            kind = "hud_bar" if name.startswith("hud") else name.split("_")[0]
            share = MARGIN_SHARE.get(kind)
            if share is None:
                margins = [0, 0, 0, 0]
            else:
                # Round-ended pieces keep half their height at each end; frames keep a share of their width.
                mx = int(h * share[0]) if kind in ("pill", "toggle", "capsule", "hud_bar") else int(w * share[0])
                my = int(h * share[1])
                margins = [mx, my, mx, my]
            meta[name] = {"size": [w, h], "margins": margins}
            print(name, (w, h), margins)
    with open(os.path.join(OUT, "kit.json"), "w") as f:
        json.dump(meta, f, indent=1)


main()
