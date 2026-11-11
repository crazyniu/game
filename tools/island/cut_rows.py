#!/usr/bin/env python3
"""Cuts a transparent sheet of painted words (one word or phrase per line) into one PNG per line.

python3 tools/island/cut_rows.py <sheet.png> <out_dir> <name1,name2,...> [--min-area=150] [--pad=6] [--floor=60]

Connected regions are grouped into lines by vertical overlap, so a descender touching the next line still stays with
its own word. The number of lines must equal the number of names. Every line is cropped to its visible bounds with a
small padding; faint haze left by the image generator is cleared.
"""
import os
import sys

import numpy as np
from PIL import Image


def label(mask):
    """Connected regions of a boolean mask (4-neighbour). Returns a label image and a list of [x0, y0, x1, y1, area]."""
    h, w = mask.shape
    lab = np.zeros(mask.shape, np.int32)
    boxes = [None]
    for y0, x0 in zip(*np.nonzero(mask)):
        if lab[y0, x0]:
            continue
        n = len(boxes)
        lab[y0, x0] = n
        stack = [(y0, x0)]
        x_lo = x_hi = x0
        y_lo = y_hi = y0
        area = 0
        while stack:
            y, x = stack.pop()
            area += 1
            x_lo, x_hi, y_lo, y_hi = min(x_lo, x), max(x_hi, x), min(y_lo, y), max(y_hi, y)
            for yy, xx in ((y + 1, x), (y - 1, x), (y, x + 1), (y, x - 1)):
                if 0 <= yy < h and 0 <= xx < w and mask[yy, xx] and not lab[yy, xx]:
                    lab[yy, xx] = n
                    stack.append((yy, xx))
        boxes.append([x_lo, y_lo, x_hi + 1, y_hi + 1, area])
    return lab, boxes


def lines_of(lab, boxes, min_area):
    """Groups regions into text lines: regions whose vertical extents overlap by more than half of the shorter one."""
    ids = [i for i in range(1, len(boxes)) if boxes[i][4] >= min_area]
    parent = {i: i for i in ids}

    def find(i):
        while parent[i] != i:
            parent[i] = parent[parent[i]]
            i = parent[i]
        return i

    for a in ids:
        for b in ids:
            if a < b:
                top, bottom = max(boxes[a][1], boxes[b][1]), min(boxes[a][3], boxes[b][3])
                shorter = min(boxes[a][3] - boxes[a][1], boxes[b][3] - boxes[b][1])
                if bottom - top > 0.5 * shorter:
                    parent[find(a)] = find(b)
    groups = {}
    for i in ids:
        groups.setdefault(find(i), []).append(i)
    return sorted(groups.values(), key=lambda g: sum(boxes[i][1] + boxes[i][3] for i in g) / len(g))


def cut(sheet, count, pad=6, floor=60, min_area=150):
    """Cuts a sheet image into `count` RGBA arrays, one per text line, top to bottom."""
    im = np.array(Image.open(sheet).convert("RGBA"))
    lab, boxes = label(im[..., 3] >= floor)
    lines = lines_of(lab, boxes, min_area)
    if len(lines) != count:
        raise SystemExit("%s: found %d lines, expected %d" % (sheet, len(lines), count))
    pieces = []
    for g in lines:
        x0, y0 = min(boxes[i][0] for i in g), min(boxes[i][1] for i in g)
        x1, y1 = max(boxes[i][2] for i in g), max(boxes[i][3] for i in g)
        piece = np.zeros((y1 - y0 + 2 * pad, x1 - x0 + 2 * pad, 4), np.uint8)
        crop = im[y0:y1, x0:x1].copy()
        crop[~np.isin(lab[y0:y1, x0:x1], g)] = 0
        piece[pad:pad + y1 - y0, pad:pad + x1 - x0] = crop
        piece[piece[..., 3] == 0, :3] = 0
        pieces.append(piece)
    return pieces


def main():
    args = [a for a in sys.argv[1:] if not a.startswith("--")]
    opts = dict(a[2:].split("=") for a in sys.argv[1:] if a.startswith("--") and "=" in a)
    sheet, out, names = args[0], args[1], args[2].split(",")
    os.makedirs(out, exist_ok=True)
    pieces = cut(sheet, len(names), int(opts.get("pad", 6)), int(opts.get("floor", 60)), int(opts.get("min-area", 150)))
    for piece, name in zip(pieces, names):
        Image.fromarray(piece, "RGBA").save(os.path.join(out, name + ".png"))
        print(name, piece.shape[1], piece.shape[0])


if __name__ == "__main__":
    main()
