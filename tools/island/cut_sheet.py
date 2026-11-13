#!/usr/bin/env python3
"""Cuts a transparent sheet of independent pieces into named PNG files.

python3 tools/island/cut_sheet.py <sheet.png> <out_dir> <name1,name2,...> [--min-area=4000] [--pad=4] [--floor=60] [--merge=0]

Pieces are connected alpha regions, ordered left to right inside rows and rows top to bottom. The number of pieces
found must equal the number of names. Each piece is cropped to its visible bounds with a small padding, loose pixels
of neighbouring pieces are cleared, and the colour of fully transparent pixels is reset so no halo shows.
"""
import os
import sys
from collections import deque

import numpy as np
from PIL import Image, ImageFilter


def components(alpha, min_area, merge=0):
    mask = alpha > 24
    if merge:   # join pieces that sit closer than `merge` pixels (the glyphs of one painted word)
        grown = Image.fromarray((mask * 255).astype(np.uint8)).filter(ImageFilter.MaxFilter(2 * merge + 1))
        mask = np.array(grown) > 0
    h, w = mask.shape
    lab = np.zeros(mask.shape, np.int32)
    boxes = []
    for y0 in range(0, h, 3):
        for x0 in range(0, w, 3):
            if lab[y0, x0] or not mask[y0, x0]:
                continue
            n = len(boxes) + 1
            q = deque([(y0, x0)])
            lab[y0, x0] = n
            xs, ys = [x0], [y0]
            while q:
                y, x = q.popleft()
                for yy, xx in ((y + 1, x), (y - 1, x), (y, x + 1), (y, x - 1)):
                    if 0 <= yy < h and 0 <= xx < w and mask[yy, xx] and not lab[yy, xx]:
                        lab[yy, xx] = n
                        q.append((yy, xx))
                        xs.append(xx)
                        ys.append(yy)
            if len(xs) >= min_area:
                boxes.append((min(xs), min(ys), max(xs) + 1, max(ys) + 1, n))
            else:
                lab[lab == n] = 0
                boxes.append(None)
    return lab, [b for b in boxes if b]


def reading_order(boxes):
    boxes = sorted(boxes, key=lambda b: (b[1] + b[3]) / 2)
    rows, row = [], []
    for b in boxes:
        if row and (b[1] + b[3]) / 2 - sum((r[1] + r[3]) / 2 for r in row) / len(row) > 90:
            rows.append(row)
            row = []
        row.append(b)
    if row:
        rows.append(row)
    return [b for r in rows for b in sorted(r, key=lambda b: b[0])]


def main():
    args = [a for a in sys.argv[1:] if not a.startswith("--")]
    opts = dict(a[2:].split("=") for a in sys.argv[1:] if a.startswith("--") and "=" in a)
    sheet, out, names = args[0], args[1], args[2].split(",")
    min_area = int(opts.get("min-area", 4000))
    pad = int(opts.get("pad", 4))
    floor = int(opts.get("floor", 60))
    merge = int(opts.get("merge", 0))
    os.makedirs(out, exist_ok=True)
    im = np.array(Image.open(sheet).convert("RGBA"))
    lab, boxes = components(im[..., 3], min_area, merge)
    boxes = reading_order(boxes)
    print("sheet", im.shape[1], "x", im.shape[0], "pieces", len(boxes))
    if len(boxes) != len(names):
        for b in boxes:
            print("  piece at", b[:4])
        raise SystemExit("found %d pieces, expected %d" % (len(boxes), len(names)))
    for (x0, y0, x1, y1, n), name in zip(boxes, names):
        x0, y0 = max(0, x0 - pad), max(0, y0 - pad)
        x1, y1 = min(im.shape[1], x1 + pad), min(im.shape[0], y1 + pad)
        piece = im[y0:y1, x0:x1].copy()
        inside = np.isin(lab[y0:y1, x0:x1], (n, 0)) if not merge else (lab[y0:y1, x0:x1] == n)   # drop pixels owned by other pieces
        a = np.where(inside, piece[..., 3], 0)
        a = np.where(a < floor, 0, a)   # image generators leave faint coloured haze around cut-outs
        piece[..., 3] = a
        piece[a == 0, :3] = 0
        Image.fromarray(piece, "RGBA").save(os.path.join(out, name + ".png"))
        print(name, x1 - x0, y1 - y0)


main()
