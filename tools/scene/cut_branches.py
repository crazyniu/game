"""Cut a world's board painting into a branch-free background and one layer per branch slot.

python3 tools/scene/cut_branches.py <world> [--preview]

Inputs (tools/scene): board_wN_raw.png (the approved painting, 1024x1536, screen = centred 9:16 area) and
board_wN_empty.png (the same painting with every branch painted out). Branch heights are detected in the painting; guide_wN.json gives the expected count per side.
Outputs: PerchSort/world/scene/board_wN_bg.png (1080x1920), board_wN_branch_{L|R}{row}.png and
board_wN_slots.json: for each slot the layer offset, the perch line (top surface over the seats, screen y),
the root x (screen edge) and the tip x. The board places birds on the measured perch line.
"""
import json
import os
import sys
from collections import deque

import numpy as np
from PIL import Image, ImageFilter

ROOT = os.path.dirname(os.path.dirname(os.path.dirname(os.path.abspath(__file__))))
W, H = 1080, 1920


def screen(path):
    im = Image.open(path).convert("RGB")
    w = int(im.height * 9 / 16)
    ox = (im.width - w) // 2
    return im.crop((ox, 0, ox + w, im.height)).resize((W, H), Image.LANCZOS)


def hsv(a):
    rgb = a / 255.0
    mx, mn = rgb.max(-1), rgb.min(-1)
    v = mx
    s = np.where(mx > 0, (mx - mn) / np.maximum(mx, 1e-6), 0)
    r, g, b = rgb[..., 0], rgb[..., 1], rgb[..., 2]
    d = np.maximum(mx - mn, 1e-6)
    h = np.where(mx == r, ((g - b) / d) % 6, np.where(mx == g, (b - r) / d + 2, (r - g) / d + 4)) * 60
    return h, s, v


def components(mask):
    """Label 4-connected components; returns label array and sizes."""
    lab = np.zeros(mask.shape, np.int32)
    sizes = [0]
    hh, ww = mask.shape
    n = 0
    for y0, x0 in zip(*np.nonzero(mask)):
        if lab[y0, x0]:
            continue
        n += 1
        q = deque([(y0, x0)])
        lab[y0, x0] = n
        c = 0
        while q:
            y, x = q.popleft()
            c += 1
            for yy, xx in ((y + 1, x), (y - 1, x), (y, x + 1), (y, x - 1)):
                if 0 <= yy < hh and 0 <= xx < ww and mask[yy, xx] and not lab[yy, xx]:
                    lab[yy, xx] = n
                    q.append((yy, xx))
        sizes.append(c)
    return lab, sizes


def morph(mask, size, grow=True):
    im = Image.fromarray((mask * 255).astype(np.uint8))
    im = im.filter(ImageFilter.MaxFilter(size) if grow else ImageFilter.MinFilter(size))
    return np.array(im) > 127


def cut_slot(raw, empty, side, y_guide):
    y0, y1 = int(max(0, y_guide - 110)), int(min(H, y_guide + 80))
    x0, x1 = (0, 700) if side == "L" else (380, W)
    a = raw[y0:y1, x0:x1].astype(np.float32)
    b = empty[y0:y1, x0:x1].astype(np.float32)
    diff = np.abs(a - b).max(-1)
    h, s, v = hsv(a)
    wood = (h >= 12) & (h <= 42) & (s >= 0.38) & (v >= 0.35)
    leaf = (h >= 55) & (h <= 110) & (s >= 0.35) & (v >= 0.45)
    cand = (diff > 40) | ((diff > 22) & (wood | leaf))   # strong change anywhere, or a softer change in wood/leaf colour
    cand = morph(morph(cand, 5, True), 5, False)   # close pinholes
    lab, sizes = components(cand)
    edge_col = 0 if side == "L" else cand.shape[1] - 1
    keep = set()
    edge_labels = set(lab[:, edge_col][lab[:, edge_col] > 0].tolist())
    if edge_labels:
        main = max(edge_labels, key=lambda k: sizes[k])
        keep.add(main)
        ys, xs = np.nonzero(lab == main)
        tip = xs.max() if side == "L" else xs.min()
        # leaf tuft: any sizeable component near the tip
        for k in range(1, len(sizes)):
            if k in keep or sizes[k] < 60:
                continue
            kys, kxs = np.nonzero(lab == k)
            near = np.abs((kxs.mean()) - tip) < 120 and abs(kys.mean() - ys.mean()) < 90
            if near:
                keep.add(k)
    mask = np.isin(lab, list(keep))
    mask = morph(mask, 3, True)
    alpha = np.array(Image.fromarray((mask * 255).astype(np.uint8)).filter(ImageFilter.GaussianBlur(1.2)))
    layer = np.dstack([raw[y0:y1, x0:x1], alpha]).astype(np.uint8)
    # perch line: top surface of the wood over the seat span
    woodm = mask & wood
    tops = []
    cols = range(150, 520) if side == "L" else range(W - 520 - x0, W - 150 - x0)
    for c in cols:
        rows = np.nonzero(woodm[:, c])[0]
        if len(rows):
            tops.append(rows.min())
    perch = y0 + float(np.median(tops)) if tops else float(y_guide)
    ys, xs = np.nonzero(mask)
    if len(xs) == 0:
        return None
    bx0, bx1, by0, by1 = xs.min(), xs.max() + 1, ys.min(), ys.max() + 1
    img = Image.fromarray(layer[by0:by1, bx0:bx1], "RGBA")
    main_cols = np.nonzero((lab == main).any(0))[0] if edge_labels else np.nonzero(mask.any(0))[0]
    tip_x = x0 + (main_cols.max() if side == "L" else main_cols.min())
    return img, (x0 + int(bx0), y0 + int(by0)), round(perch, 1), int(tip_x)


def detect_rows(raw, empty, side):
    """Screen y spans of the painted branches on one side, found where the painting differs from the branch-free
    version in wood or leaf colour across a stretch of the branch shaft."""
    x0, x1 = (30, 380) if side == "L" else (W - 380, W - 30)
    a = raw[:, x0:x1].astype(np.float32)
    b = empty[:, x0:x1].astype(np.float32)
    diff = np.abs(a - b).max(-1)
    h, s_, v = hsv(a)
    colour = ((h >= 12) & (h <= 110) & (s_ >= 0.3) & (v >= 0.3))
    strength = ((diff > 30) & colour).mean(1)
    on = strength > 0.35
    spans, start = [], None
    for y, flag in enumerate(on):
        if flag and start is None:
            start = y
        elif not flag and start is not None:
            if y - start >= 10:
                spans.append((start, y))
            start = None
    spans = [sp for sp in spans if sp[0] < 1780]   # the ground strip at the bottom edge is not a branch
    merged = []
    for sp in spans:
        if merged and sp[0] - merged[-1][1] < 12:
            merged[-1] = (merged[-1][0], sp[1])
        else:
            merged.append(sp)
    return merged


def main():
    world = int(sys.argv[1])
    src = os.path.join(ROOT, "tools", "scene")
    out = os.path.join(ROOT, "PerchSort", "world", "scene")
    os.makedirs(out, exist_ok=True)
    raw = np.array(screen(os.path.join(src, "board_w%d_raw.png" % world)))
    empty_im = screen(os.path.join(src, "board_w%d_empty.png" % world))
    empty = np.array(empty_im)
    with open(os.path.join(src, "guide_w%d.json" % world)) as f:
        guide = json.load(f)
    rows = max(sl["row"] for sl in guide["slots"]) + 1
    empty_im.save(os.path.join(out, "board_w%d_bg.png" % world))
    slots = []
    preview = empty_im.copy().convert("RGBA")
    for side in ("L", "R"):
        found = detect_rows(raw, empty, side)
        print(side, "branch bands", found)
        if len(found) != rows:
            raise SystemExit("world %d side %s: %d painted branches, expected %d" % (world, side, len(found), rows))
        for row, (top, bottom) in enumerate(found):
            res = cut_slot(raw, empty, side, float(top))
            if res is None:
                raise SystemExit("no branch at %s row %d" % (side, row))
            img, at, perch, tip = res
            name = "board_w%d_branch_%s%d" % (world, side, row)
            img.save(os.path.join(out, name + ".png"))
            slots.append({"side": side, "row": row, "name": name, "at": list(at), "perch": perch, "tip": tip})
            preview.alpha_composite(img, at)
            print(name, "at", at, "perch", perch, "band", (top, bottom), "tip", tip)
    with open(os.path.join(out, "board_w%d_slots.json" % world), "w") as f:
        json.dump({"world": world, "slots": slots}, f, indent=1)
    if "--preview" in sys.argv:
        os.makedirs(os.path.join(ROOT, "build"), exist_ok=True)
        preview.save(os.path.join(ROOT, "build", "scene_w%d_rebuilt.png" % world))


main()
