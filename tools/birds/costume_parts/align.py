#!/usr/bin/env python3
"""align.py <species> <slot>: register the painted frames of a skin to the plain frames and check them.

Each out/<species>_<slot>_<frame>_worn.png is scaled and shifted (one scale, one shift per frame) so that the lower body
(wings, belly, feet: the part a costume changes least) lies on the plain frame's lower body; the bird keeps the plain
animation's size and position and neighbouring frames cannot drift apart. Writes out/skin/<species>_<slot>_<frame>.png
(320x320, plus the mirrored flight_flap_l frames) and out/skin/<species>_<slot>.json with a per-frame report; frames flagged "check" should be repainted.
"""
import glob, json, os, sys
import numpy as np
from PIL import Image
from concurrent.futures import ProcessPoolExecutor

HERE = os.path.dirname(os.path.abspath(__file__))
BIRDS = os.path.abspath(os.path.join(HERE, "../../../PerchSort/assets/resources/birds"))
C = 320


def alpha(im, size):
    return np.asarray(im.resize((size, size), Image.LANCZOS), np.float32)[..., 3] / 255.0


def place(wa, size, ox, oy, out=C):
    w = np.zeros((out, out), np.float32)
    x0, y0, x1, y1 = max(ox, 0), max(oy, 0), min(ox + size, out), min(oy + size, out)
    if x1 > x0 and y1 > y0:
        w[y0:y1, x0:x1] = wa[y0 - oy:y1 - oy, x0 - ox:x1 - ox]
    return w


def job(args):
    species, slot, frame = args
    plain = Image.open("%s/%s_%s.png" % (BIRDS, species, frame)).convert("RGBA")
    raw = Image.open("%s/out/%s_%s_%s_worn.png" % (HERE, species, slot, frame)).convert("RGBA")
    pa = np.asarray(plain, np.float32)[..., 3] / 255.0
    ys, xs = np.nonzero(pa > 0.5)
    top, bot = ys.min(), ys.max()
    keep = np.zeros((C, C), bool); keep[int(top + 0.52 * (bot - top)):, :] = True
    # coarse search at half resolution (a quarter of the work), then refine at full resolution
    H = C // 2
    pa2 = pa.reshape(H, 2, H, 2).mean((1, 3)); keep2 = keep.reshape(H, 2, H, 2).all((1, 3))
    best = None
    for s in np.arange(0.90, 1.101, 0.01):
        size = round(H * s); wa = alpha(raw, size)
        for dy in range(-7, 8):
            for dx in range(-7, 8):
                w = place(wa, size, round((H - size) / 2) + dx, round((H - size) / 2) + dy, H)
                cost = float(np.abs(w - pa2)[keep2].mean())
                if best is None or cost < best[0]:
                    best = (cost, s, dx * 2, dy * 2)
    _, s0, dx0, dy0 = best
    best = None
    for s in np.arange(s0 - 0.01, s0 + 0.0101, 0.005):
        size = round(C * s); wa = alpha(raw, size)
        for dy in range(dy0 - 2, dy0 + 3):
            for dx in range(dx0 - 2, dx0 + 3):
                w = place(wa, size, round((C - size) / 2) + dx, round((C - size) / 2) + dy)
                cost = float(np.abs(w - pa)[keep].mean())
                if best is None or cost < best[0]:
                    best = (cost, s, dx, dy)
    cost, s, dx, dy = best
    size = round(C * s)
    canvas = Image.new("RGBA", (C, C), (0, 0, 0, 0))
    canvas.paste(raw.resize((size, size), Image.LANCZOS), (round((C - size) / 2) + dx, round((C - size) / 2) + dy))
    wa = np.asarray(canvas, np.float32)[..., 3] / 255.0
    inter = np.minimum(wa, pa).sum(); union = np.maximum(wa, pa).sum()
    low = (np.minimum(wa, pa)[keep].sum() / max(np.maximum(wa, pa)[keep].sum(), 1))
    # stray second bird: another big blob of alpha away from the main one
    from scipy import ndimage
    lab, n = ndimage.label(wa > 0.5)
    sizes = sorted((lab == i).sum() for i in range(1, n + 1))[::-1] if n else []
    stray = len(sizes) > 1 and sizes[1] > 0.04 * sizes[0]
    flag = "ok" if (low > 0.86 and inter / union > 0.74 and not stray) else "check"
    px = np.asarray(canvas).copy()
    px[px[..., 3] <= 24] = 0   # faint alpha specks would inflate the ink box recorded in the manifest
    canvas = Image.fromarray(px, "RGBA")
    canvas.save("%s/out/skin/%s_%s_%s.png" % (HERE, species, slot, frame))
    return frame, {"scale": round(float(s), 3), "shift": [dx, dy], "lower_iou": round(float(low), 3), "iou": round(float(inter / union), 3), "stray": bool(stray), "status": flag}


def main(species, slot):
    os.makedirs(HERE + "/out/skin", exist_ok=True)
    pre = "%s/out/%s_%s_" % (HERE, species, slot)
    sys.path.insert(0, HERE)
    from paint_frames import SKIN_FRAMES
    frames = [f for f in SKIN_FRAMES if os.path.exists("%s%s_worn.png" % (pre, f))]
    with ProcessPoolExecutor(4) as pool:
        table = dict(pool.map(job, [(species, slot, f) for f in frames]))
    # the left-facing flight frames are the right-facing ones mirrored
    for i in range(1, 7):
        right = "flight_flap_%02d" % i
        if right in table:
            Image.open("%s/out/skin/%s_%s_%s.png" % (HERE, species, slot, right)).transpose(Image.FLIP_LEFT_RIGHT).save(
                "%s/out/skin/%s_%s_flight_flap_l_%02d.png" % (HERE, species, slot, i))
    json.dump(table, open("%s/out/skin/%s_%s.json" % (HERE, species, slot), "w"), indent=0)
    bad = [f for f, v in table.items() if v["status"] != "ok"]
    print("%s_%s: %d frames, %d to check: %s" % (species, slot, len(table), len(bad), " ".join(bad)))


if __name__ == "__main__":
    main(sys.argv[1], sys.argv[2])
