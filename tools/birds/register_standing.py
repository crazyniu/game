#!/usr/bin/env python3
"""register_standing.py [--apply]: keep every bird standing on its branch through its standing frames.

A bird stands on its rest frame (`front`) and blinks with `front_blink_01..02`; the game puts the same row of every frame on the branch.
Each painting differs a little, so a blink frame drawn smaller, larger or shifted makes the bird jump or float while it blinks, and a skin's
front drawn at another size than its bird's front floats or sinks. This finds one scale and one shift per frame:
- a blink frame against the front of the same bird or skin (the whole silhouette: the frames differ only in the eyelids);
- a skin's front against its bird's front (the lower body only: a costume changes the head and chest).
Frames off by more than 3 % in size or 3 pixels are listed; with --apply they are resampled and written as single files into
PerchSort/assets/resources/birds, then folded into the sheets with pack_sheets.py. Run from the repository root with pillow and numpy.
"""
import os, sys
import numpy as np
from PIL import Image
sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))
import pack_sheets as sheets



def alpha(im, size):
    return np.asarray(im.resize((size, size), Image.LANCZOS), np.float32)[..., 3] / 255.0


def place(a, dx, dy, n):
    """A scaled alpha picture a, centred and shifted, on an n x n canvas."""
    size = a.shape[0]
    out = np.zeros((n, n), np.float32)
    ox, oy = (n - size) // 2 + dx, (n - size) // 2 + dy
    x0, y0, x1, y1 = max(ox, 0), max(oy, 0), min(ox + size, n), min(oy + size, n)
    if x1 > x0 and y1 > y0:
        out[y0:y1, x0:x1] = a[y0 - oy:y1 - oy, x0 - ox:x1 - ox]
    return out


def search(img, target, keep, scales, shifts_x, shifts_y):
    n = target.shape[0]
    best = None
    for s in scales:
        a = alpha(img, round(n * s))
        for dy in shifts_y:
            for dx in shifts_x:
                c = float(np.abs(place(a, dx, dy, n) - target)[keep].mean())
                if best is None or c < best[0]:
                    best = (c, s, dx, dy)
    return best[1:]


def fit(img, ref, lower):
    """Best (scale, dx, dy) at full resolution that lays img over ref; `lower` compares only the lower body of ref.
    Every scale from 0.80 to 1.25 and every shift up to 52 pixels is tried at quarter size, then refined at half and full size."""
    n = ref.width
    levels = []
    for size in (n // 4, n // 2, n):
        target = alpha(ref, size)
        ys = np.nonzero(target.max(axis=1) > 0.5)[0]
        keep = np.zeros((size, size), bool)
        keep[int(ys[0] + (0.52 if lower else 0.0) * (ys[-1] - ys[0])):, :] = True
        levels.append((target, keep))
    s, dx, dy = search(img, *levels[0], np.arange(0.80, 1.251, 0.01), range(-10, 11), range(-13, 14))
    s, dx, dy = search(img, *levels[1], np.arange(s - 0.01, s + 0.011, 0.005), range(dx * 2 - 2, dx * 2 + 3), range(dy * 2 - 2, dy * 2 + 3))
    return search(img, *levels[2], np.arange(s - 0.005, s + 0.0051, 0.0025), range(dx * 2 - 1, dx * 2 + 2), range(dy * 2 - 1, dy * 2 + 2))


def resample(img, scale, dx, dy):
    n = img.width
    size = round(n * scale)
    canvas = Image.new("RGBA", (n, n), (0, 0, 0, 0))
    canvas.paste(img.resize((size, size), Image.LANCZOS), ((n - size) // 2 + dx, (n - size) // 2 + dy))
    return canvas


def check(job):
    name, ref_name, lower, ref = job
    return fit(sheets.frame_image(name), ref or sheets.frame_image(ref_name), lower)


def main():
    apply = "--apply" in sys.argv
    table = sheets.table()
    jobs = []
    for group in sorted(table):
        frames = table[group]["frames"]
        if group + "_front" not in frames:
            continue
        for blink in ("front_blink_01", "front_blink_02"):
            if group + "_" + blink in frames:
                jobs.append((group + "_" + blink, group + "_front", False))
        if "_outfit_" in group:
            bird = group.split("_outfit_")[0]
            if bird + "_front" in table.get(bird, {}).get("frames", {}):
                jobs.append((group + "_front", bird + "_front", True))
    # skins' fronts first (their blinks are then compared with the registered front), the rest in parallel
    from concurrent.futures import ProcessPoolExecutor
    fixed = {}
    with ProcessPoolExecutor() as pool:
        for stage in (True, False):
            todo = [j for j in jobs if j[2] == stage]
            for (name, ref_name, lower), (s, dx, dy) in zip(todo, pool.map(check, [(n, r, l, fixed.get(r)) for n, r, l in todo])):
                if abs(s - 1) > 0.03 or abs(dx) > 3 or abs(dy) > 3:
                    print("%-40s scale %.3f shift %+d %+d" % (name, s, dx, dy), flush=True)
                    fixed[name] = resample(sheets.frame_image(name), s, dx, dy)
                    if apply:
                        single = os.path.join(sheets.BIRDS, name)
                        if os.path.exists(single + ".webp"):
                            os.remove(single + ".webp")   # pack_sheets.py must not fold in the uncorrected picture
                        fixed[name].save(single + ".png")
    print("%d frames checked, %d off%s" % (len(jobs), len(fixed), ", written" if apply else ""))


if __name__ == "__main__":
    main()
