#!/usr/bin/env python3
"""pack_sheets.py: pack the frames of every bird and every skin into one picture each, so a bird downloads as one file.

The frames of a bird (<bird>_*) or of a skin (<bird>_outfit_<n>_*) in PerchSort/assets/resources/birds, as single files and as the frames of
its existing sheet, are trimmed to their ink plus a transparent margin, packed into birds/sheets/<group>.webp and listed in
tools/birds/sheets.json: {group: {"tex": [w, h], "frames": {name: [x, y, left, top, width, height, frame width, frame height]}}}, where
(x, y) is the piece in the sheet and (left, top, width, height) the same piece in the frame. The single files and their metas are deleted.
tools/cocos-assets.mjs turns the table into manifest entries and Art.ts cuts the frames out of the loaded sheet. Painting tools read a frame
with frame_image(). Run from the repository root with pillow and numpy, then `node tools/cocos-assets.mjs` and `python3 tools/cocos-meta.py`.
"""
import json, os, re
import numpy as np
from PIL import Image

ROOT = os.path.abspath(os.path.join(os.path.dirname(__file__), "..", ".."))
BIRDS = os.path.join(ROOT, "PerchSort/assets/resources/birds")
SHEETS = os.path.join(BIRDS, "sheets")
TABLE = os.path.join(ROOT, "tools/birds/sheets.json")   # build input only; the game reads the manifest
MARGIN = 4      # transparent border kept round the ink: the aviary silhouette draws a rim 3 pixels outside it
GAP = 2         # empty pixels between pieces, so filtering never reads a neighbour
WIDTH = 2048    # sheets are 2048 wide; the ten world-1 birds with all 71 actions need a little over 2048 high (phones take 4096)
HEIGHT = 4096


def table():
    return json.load(open(TABLE)) if os.path.exists(TABLE) else {}


def birds():
    src = open(os.path.join(ROOT, "PerchSort/assets/scripts/meta/Worlds.ts")).read()
    rows = src[src.index("BIRDS: string[][] = ["):src.index("];", src.index("BIRDS: string[][] = ["))]
    return sorted(set(re.findall(r"'(\w+)'", rows)) | {"crow"}, key=len, reverse=True)


def group_of(name, known):
    m = re.match(r"^(\w+?_outfit_\d+)_", name)
    if m:
        return m.group(1)
    return next((b for b in known if name.startswith(b + "_")), None)


def frame_image(name):
    """One frame as its full-size RGBA picture, from a single file or from its bird's sheet."""
    for ext in (".png", ".webp"):
        path = os.path.join(BIRDS, name + ext)
        if os.path.exists(path):
            return Image.open(path).convert("RGBA")
    for group, sheet in table().items():
        if name in sheet["frames"]:
            x, y, left, top, w, h, fw, fh = sheet["frames"][name]
            canvas = Image.new("RGBA", (fw, fh), (0, 0, 0, 0))
            canvas.paste(Image.open(os.path.join(SHEETS, group + ".webp")).convert("RGBA").crop((x, y, x + w, y + h)), (left, top))
            return canvas
    raise FileNotFoundError(name)


def piece(image):
    a = np.asarray(image)[..., 3]
    ys, xs = np.nonzero(a > 0)
    if not len(xs):
        return 0, 0, 1, 1
    left, top = max(xs.min() - MARGIN, 0), max(ys.min() - MARGIN, 0)
    right, bottom = min(xs.max() + 1 + MARGIN, image.width), min(ys.max() + 1 + MARGIN, image.height)
    return int(left), int(top), int(right - left), int(bottom - top)


def pack(sizes):
    """Shelf packing, tallest first: returns the top-left corner of every piece and the sheet size."""
    order = sorted(range(len(sizes)), key=lambda i: -sizes[i][1])
    spots, x, y, shelf = {}, 0, 0, 0
    for i in order:
        w, h = sizes[i]
        if x + w > WIDTH:
            x, y, shelf = 0, y + shelf + GAP, 0
        spots[i] = (x, y)
        x += w + GAP
        shelf = max(shelf, h)
    width = max(spots[i][0] + sizes[i][0] for i in spots)
    height = y + shelf
    assert height <= HEIGHT, "sheet taller than %d" % HEIGHT
    return spots, (width, height)


def unpack(group, entry):
    """The frames of a group's current sheet as full-size pictures (entry: the group's table entry, read once by the caller)."""
    sheet = Image.open(os.path.join(SHEETS, group + ".webp")).convert("RGBA")
    frames = {}
    for name, (x, y, left, top, w, h, fw, fh) in entry.get("frames", {}).items():
        frames[name] = Image.new("RGBA", (fw, fh), (0, 0, 0, 0))
        frames[name].paste(sheet.crop((x, y, x + w, y + h)), (left, top))
    return frames


def build(group, files, old):
    """Packs one group; returns its table entry. Runs in a worker process and never reads the table, which the caller rewrites."""
    frames = unpack(group, old) if old else {}
    for f in files:
        frames[os.path.splitext(f)[0]] = Image.open(os.path.join(BIRDS, f)).convert("RGBA")
    names = sorted(frames)
    boxes = [piece(frames[n]) for n in names]
    spots, size = pack([(b[2], b[3]) for b in boxes])
    sheet = Image.new("RGBA", size, (0, 0, 0, 0))
    entry = {"tex": list(size), "frames": {}}
    for i, n in enumerate(names):
        left, top, w, h = boxes[i]
        sheet.paste(frames[n].crop((left, top, left + w, top + h)), spots[i])
        entry["frames"][n] = [spots[i][0], spots[i][1], left, top, w, h, frames[n].width, frames[n].height]
    path = os.path.join(SHEETS, group + ".webp")
    sheet.save(path + ".tmp", "WEBP", quality=90, alpha_quality=100, method=4)
    os.replace(path + ".tmp", path)
    if os.path.exists(path + ".meta"):
        os.remove(path + ".meta")   # the size changed; cocos-meta.py writes it again
    print("%s: %d frames, %dx%d, %d KB" % (group, len(names), size[0], size[1], os.path.getsize(path) // 1024), flush=True)
    return entry


def main():
    from concurrent.futures import ProcessPoolExecutor
    known, sheets = birds(), table()
    singles = {}
    for f in sorted(os.listdir(BIRDS)):
        stem, ext = os.path.splitext(f)
        if ext in (".png", ".webp"):
            group = group_of(stem, known)
            if group:
                singles.setdefault(group, []).append(f)
    os.makedirs(SHEETS, exist_ok=True)
    with ProcessPoolExecutor() as pool:
        jobs = {g: pool.submit(build, g, files, sheets.get(g)) for g, files in sorted(singles.items())}
        for group, job in jobs.items():
            sheets[group] = job.result()
            json.dump(sheets, open(TABLE, "w"), separators=(",", ":"), sort_keys=True)
            for f in singles[group]:
                for p in (os.path.join(BIRDS, f), os.path.join(BIRDS, f + ".meta")):
                    if os.path.exists(p):
                        os.remove(p)


if __name__ == "__main__":
    main()
