#!/usr/bin/env python3
"""align.py <species> <slot>: register the painted frames of a skin to the plain frames and check them.

Each out/<species>_<slot>_<frame>_worn.png is scaled and shifted (one scale, one shift per frame) so that the lower body
(for a skin the wings, belly and feet, which a costume changes least; for a world bird the torso and wings, because its crest, tail and feet differ) lies on the plain frame's lower body; the bird keeps the plain
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
    species, slot, frame, kind = args
    sys.path.insert(0, HERE)
    from paint_frames import plain_image
    plain = plain_image(species, frame)
    raw = Image.open("%s/out/%s_%s_%s_worn.png" % (HERE, species, slot, frame)).convert("RGBA")
    pa = np.asarray(plain, np.float32)[..., 3] / 255.0
    ys, xs = np.nonzero(pa > 0.5)
    top, bot = ys.min(), ys.max()
    keep = np.zeros((C, C), bool)
    if kind == "skin":
        keep[int(top + 0.52 * (bot - top)):, :] = True      # the lower body: a costume changes it least
    else:
        keep[int(top + 0.30 * (bot - top)):int(top + 0.75 * (bot - top)), :] = True   # torso and wings: a world bird's crest, tail and feet differ
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
    if kind != "skin":
        # a world bird's tail may hang below its feet: put the feet (the lowest rows as wide as a foot pair) on the plain bird's feet line
        def feet_row(mask):
            width = (mask > 0.5).sum(axis=1)
            rows = np.nonzero(width >= 0.18 * width.max())[0]
            return int(rows.max())
        placed = place(alpha(raw, size), size, round((C - size) / 2) + dx, round((C - size) / 2) + dy)
        dy += feet_row(pa) - feet_row(placed)
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
    flag = "ok" if (low > (0.86 if kind == "skin" else 0.72) and inter / union > (0.74 if kind == "skin" else 0.62) and not stray) else "check"
    px = np.asarray(canvas).copy()
    px[px[..., 3] <= 24] = 0   # faint alpha specks would inflate the ink box recorded in the manifest
    canvas = Image.fromarray(px, "RGBA")
    canvas.save("%s/out/skin/%s_%s_%s.png" % (HERE, species, slot, frame))
    return frame, {"scale": round(float(s), 3), "shift": [dx, dy], "lower_iou": round(float(low), 3), "iou": round(float(inter / union), 3), "stray": bool(stray), "status": flag}


def main(species, slot, kind='skin'):
    os.makedirs(HERE + "/out/skin", exist_ok=True)
    pre = "%s/out/%s_%s_" % (HERE, species, slot)
    sys.path.insert(0, HERE)
    from paint_frames import FRAME_SETS, MIRRORED
    frames = [f for f in FRAME_SETS[kind] if os.path.exists("%s%s_worn.png" % (pre, f))]
    with ProcessPoolExecutor(4) as pool:
        table = dict(pool.map(job, [(species, slot, f, kind) for f in frames]))
    # the left-facing flight frames are the right-facing ones mirrored
    for right in MIRRORED:
        if right in table:
            left = right.replace("flight_flap_", "flight_flap_l_").replace("flight_takeoff_", "flight_takeoff_l_").replace("flight_land_", "flight_land_l_")
            Image.open("%s/out/skin/%s_%s_%s.png" % (HERE, species, slot, right)).transpose(Image.FLIP_LEFT_RIGHT).save(
                "%s/out/skin/%s_%s_%s.png" % (HERE, species, slot, left))
    json.dump(table, open("%s/out/skin/%s_%s.json" % (HERE, species, slot), "w"), indent=0)
    bad = [f for f, v in table.items() if v["status"] != "ok"]
    print("%s_%s: %d frames, %d to check: %s" % (species, slot, len(table), len(bad), " ".join(bad)))


if __name__ == "__main__":
    main(sys.argv[1], sys.argv[2], sys.argv[3] if len(sys.argv) > 3 else 'skin')
