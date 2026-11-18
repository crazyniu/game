#!/usr/bin/env python3
"""whole.py <species> <name>: use the Codex paintings of the whole costumed bird as the animation frames.

Each out/<species>_<frame>_worn.png is registered to the plain frame on the whole silhouette (the cap area, which the
costume changes, is left out of the comparison) with one scale and one shift per frame, so the bird keeps the plain
animation's size and position and neighbouring frames cannot drift apart. The button centre is measured on the result.
Writes out/whole/<species>_<frame>_<name>.png (320x320, the whole bird) and out/whole/<species>_<name>.json.
"""
import glob, json, os, sys
import numpy as np
from PIL import Image, ImageFilter

HERE = os.path.dirname(os.path.abspath(__file__))
BIRDS = os.path.abspath(os.path.join(HERE, "../../../PerchSort/assets/resources/birds"))
C = 320


def dilate(mask, n):
    return np.asarray(Image.fromarray((mask * 255).astype(np.uint8)).filter(ImageFilter.MaxFilter(n))) > 0


def job(args):
    species, name, frame = args
    plain = Image.open("%s/%s_%s.png" % (BIRDS, species, frame)).convert("RGBA")
    raw = Image.open("%s/out/%s_%s_worn.png" % (HERE, species, frame)).convert("RGBA")
    pa = np.asarray(plain, np.float32)[..., 3] / 255.0
    big = raw.resize((C, C), Image.LANCZOS)
    # the costume area of the painting: the blue cap (fur is brown and never counts), grown for the ear tufts and brows under it
    a = np.asarray(big, np.float32)
    r, g, b = a[..., 0], a[..., 1], a[..., 2]
    blue = (b > 120) & (b > r + 40) & (a[..., 3] > 200)
    ys, xs = np.nonzero(blue)
    skip = np.zeros((C, C), bool)
    skip[:ys.max() + 40, max(0, xs.min() - 30):min(C, xs.max() + 30)] = True
    keep = ~skip
    best = None
    for s in np.arange(0.94, 1.0601, 0.01):
        size = round(C * s)
        wa = np.asarray(big.resize((size, size), Image.LANCZOS), np.float32)[..., 3] / 255.0
        for dy in range(-12, 13):
            for dx in range(-12, 13):
                ox, oy = round((C - size) / 2) + dx, round((C - size) / 2) + dy
                w = np.zeros((C, C), np.float32)
                x0, y0 = max(ox, 0), max(oy, 0)
                x1, y1 = min(ox + size, C), min(oy + size, C)
                if x1 <= x0 or y1 <= y0:
                    continue
                w[y0:y1, x0:x1] = wa[y0 - oy:y1 - oy, x0 - ox:x1 - ox]
                cost = float(np.abs(w - pa)[keep].mean())
                if best is None or cost < best[0]:
                    best = (cost, s, dx, dy, (ox, oy, size))
    cost, s, dx, dy, (ox, oy, size) = best
    canvas = Image.new("RGBA", (C, C), (0, 0, 0, 0))
    canvas.paste(big.resize((size, size), Image.LANCZOS), (ox, oy))
    out = np.asarray(canvas, np.float32)
    red = (out[..., 0] > 170) & (out[..., 1] < 90) & (out[..., 2] < 90) & (out[..., 3] > 200)
    red[:, :int(xs.min())] = False; red[:, int(xs.max()) + 1:] = False; red[int((ys.min() + ys.max()) / 2):, :] = False
    ry, rx = np.nonzero(red)
    hub = [round(float(rx.mean()), 1), round(float(ry.mean()), 1)] if len(rx) > 6 else None
    canvas.save("%s/out/whole/%s_%s_%s.png" % (HERE, species, frame, name))
    return frame, {"hub": hub, "scale": round(float(s), 2), "shift": [dx, dy], "cost": round(cost, 4)}


def main(species, name):
    from concurrent.futures import ProcessPoolExecutor
    os.makedirs(HERE + "/out/whole", exist_ok=True)
    frames = sorted(os.path.basename(f)[len(species) + 1:-len("_worn.png")] for f in glob.glob("%s/out/%s_*_worn.png" % (HERE, species)))
    table = {}
    with ProcessPoolExecutor(4) as pool:
        path = "%s/out/whole/%s_%s.json" % (HERE, species, name)
        table = json.load(open(path)) if os.path.exists(path) else {}
        todo = [f for f in frames if f not in table or not os.path.exists("%s/out/whole/%s_%s_%s.png" % (HERE, species, f, name))]
        for f, info in pool.map(job, [(species, name, f) for f in todo]):
            table[f] = info
            print(f, info, flush=True)
    json.dump(table, open("%s/out/whole/%s_%s.json" % (HERE, species, name), "w"), indent=0)


if __name__ == "__main__":
    main(sys.argv[1], sys.argv[2])
