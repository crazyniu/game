"""Cut the painted icon and lettering sheets into game sprites.

python3 tools/kit/cut_kit.py

tools/kit/icons_white.png, icons_white_more.png (white icons on black) -> PerchSort/art/icons/icon_<name>.png
tools/kit/letters_white.png (9 x 9 white letters on black)  -> PerchSort/art/letters/letter_<code>.png
                                                              + PerchSort/art/letters/letters.json
Sprites are white with alpha taken from brightness, so the kit can tint them (ink on cream, white on colour).
letters.json: per character code point {"w", "h", "dy"}: dy is the glyph top relative to the baseline in units of
the cap height (H), plus "cap" (cap height in pixels) and "space".
"""
import json
import os

import numpy as np
from PIL import Image

ROOT = os.path.dirname(os.path.dirname(os.path.dirname(os.path.abspath(__file__))))
KIT = os.path.join(ROOT, "tools", "kit")
ART = os.path.join(ROOT, "PerchSort", "art")

ICONS = ["undo", "bulb", "sapling", "gear", "home", "play", "retry", "check", "close", "sound_on",
         "sound_off", "shop", "aviary", "lock", "left", "right", "footprint", "sun", "moon", "star",
         "eye", "bell", "scarecrow", "rain", "hanger"]
ICONS_MORE = ["hop", "plus", "info", "gift", "egg", "feather", "trophy", "pin", "coins"]
LETTERS = list("ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789.,!?'-:/+&()%#") + ["×", "∞"]
DESCENDERS = set("gjpqy,()")


def white_alpha(cell):
    a = np.asarray(cell.convert("L"), dtype=np.float32)
    alpha = np.clip((a - 20.0) / 200.0, 0.0, 1.0)
    out = np.zeros(a.shape + (4,), np.uint8)
    out[..., :3] = 255
    out[..., 3] = (alpha * 255).astype(np.uint8)
    return out


def bbox(alpha, thr=40):
    ys, xs = np.nonzero(alpha > thr)
    if len(xs) == 0:
        return None
    return xs.min(), ys.min(), xs.max() + 1, ys.max() + 1


def cut_grid(path, n):
    im = Image.open(path).convert("RGB")
    cw, ch = im.width / n, im.height / n
    cells = []
    for r in range(n):
        for c in range(n):
            box = (int(c * cw), int(r * ch), int((c + 1) * cw), int((r + 1) * ch))
            cells.append((r, c, box, white_alpha(im.crop(box))))
    return cells


def icons():
    out = os.path.join(ART, "icons")
    os.makedirs(out, exist_ok=True)
    for sheet, n, names in (("icons_white.png", 5, ICONS), ("icons_white_more.png", 3, ICONS_MORE)):
        for (r, c, box, rgba), name in zip(cut_grid(os.path.join(KIT, sheet), n), names):
            b = bbox(rgba[..., 3])
            crop = rgba[b[1]:b[3], b[0]:b[2]]
            side = max(crop.shape[0], crop.shape[1]) + 8
            canvas = np.zeros((side, side, 4), np.uint8)
            y0, x0 = (side - crop.shape[0]) // 2, (side - crop.shape[1]) // 2
            canvas[y0:y0 + crop.shape[0], x0:x0 + crop.shape[1]] = crop
            Image.fromarray(canvas, "RGBA").save(os.path.join(out, "icon_%s.png" % name))
    print("icons:", len(ICONS) + len(ICONS_MORE))


def runs(profile, min_gap):
    """Spans where profile > 0, merging gaps shorter than min_gap."""
    on = profile > 0
    spans = []
    start = None
    gap = 0
    for i, v in enumerate(on):
        if v:
            if start is None:
                start = i
            gap = 0
            end = i
        elif start is not None:
            gap += 1
            if gap >= min_gap:
                spans.append((start, end + 1))
                start = None
    if start is not None:
        spans.append((start, end + 1))
    return spans


def letters():
    """Characters are found by projection gaps (rows, then columns), not by a fixed grid."""
    out = os.path.join(ART, "letters")
    os.makedirs(out, exist_ok=True)
    rgba = white_alpha(Image.open(os.path.join(KIT, "letters_white.png")))
    alpha = rgba[..., 3]
    ink = (alpha > 60).astype(np.int32)
    row_spans = runs(ink.sum(1), 12)
    found = []
    for r, (y0, y1) in enumerate(row_spans):
        cols = runs(ink[y0:y1].sum(0), 8)
        for (x0, x1) in cols:
            found.append((r, x0, y0, x1, y1))
    print("rows", len(row_spans), "characters", len(found), "expected", len(LETTERS))
    if len(found) != len(LETTERS):
        raise SystemExit("character count mismatch; check the sheet")
    glyphs = {}
    rows = {}
    for (r, x0, y0, x1, y1), ch in zip(found, LETTERS):
        b = bbox(alpha[y0:y1, x0:x1])
        b = (x0 + b[0], y0 + b[1], x0 + b[2], y0 + b[3])
        glyphs[ch] = (r, b)
        if ch not in DESCENDERS and ch not in "-'+:%#\u00d7\u221e.":
            rows.setdefault(r, []).append(b[3])
    baseline = {r: float(np.median(v)) for r, v in rows.items()}
    cap = float(glyphs["H"][1][3] - glyphs["H"][1][1])
    meta = {"cap": cap, "space": round(cap * 0.32, 1), "glyphs": {}}
    for ch, (r, b) in glyphs.items():
        crop = rgba[b[1]:b[3], b[0]:b[2]]
        Image.fromarray(crop, "RGBA").save(os.path.join(out, "letter_%04x.png" % ord(ch)))
        base = baseline.get(r, baseline[min(baseline, key=lambda k: abs(k - r))])
        meta["glyphs"][str(ord(ch))] = {"w": int(b[2] - b[0]), "h": int(b[3] - b[1]),
                                        "dy": round((b[1] - base) / cap, 4)}
    with open(os.path.join(out, "letters.json"), "w") as f:
        json.dump(meta, f, indent=1)
    print("letters:", len(glyphs), "cap", cap)


icons()
letters()
