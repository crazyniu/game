#!/usr/bin/env python3
"""Bakes the shop's worded buttons and tabs into one complete picture per language: the glossy
base, its picture, and the font-baked word (tools/island/bake_labels.py), centred on the measured face of the base (not on the picture's
box, whose bottom lip and shadow would push the word up). The game shows each button as that single picture.

python3 tools/island/bake_buttons.py
Output: PerchSort/art/buttons/btn_<open|equip>_<lang>.png and btn_tab_<name>_<on|off>_<lang>.png.
"""
import os

import numpy as np
from PIL import Image

from soften_palette import soft_rim

ROOT = os.path.join(os.path.dirname(os.path.abspath(__file__)), "..", "..", "PerchSort", "art")
OUT = os.path.join(ROOT, "buttons")
LANGS = ["en", "zh"]
SCALE = 2   # baked at twice the on-screen size


def load(rel):
    return Image.open(os.path.join(ROOT, rel)).convert("RGBA")


def label(lang, wid, style):
    """A font-baked word (tools/island/bake_labels.py), cut to its ink."""
    return trim(load("labels/label_%s_%s_%s.png" % (wid, style, lang)))


def trim(img):
    return img.crop(img.getchannel("A").point(lambda a: 255 if a > 24 else 0).getbbox())


def stretch(base, size, cap):
    """Horizontal three-slice: both round ends keep their shape (cap = end width in base pixels at base height)."""
    w, h = size
    k = h / base.height
    scaled = base.resize((max(1, round(base.width * k)), h), Image.LANCZOS)
    c = round(cap * k)
    left = scaled.crop((0, 0, c, h))
    right = scaled.crop((scaled.width - c, 0, scaled.width, h))
    mid_src = scaled.crop((scaled.width // 2 - 2, 0, scaled.width // 2 + 2, h))
    out = Image.new("RGBA", (w, h))
    out.alpha_composite(left, (0, 0))
    out.alpha_composite(mid_src.resize((max(1, w - 2 * c), h), Image.LANCZOS), (c, 0))
    out.alpha_composite(right, (w - c, 0))
    return out


def face_center_y(img, green):
    """Vertical centre of the coloured face (green or cream), ignoring the rim, lip and shadow."""
    a = np.asarray(img).astype(int)
    if green:
        mask = (a[..., 1] > a[..., 0] + 40) & (a[..., 3] > 200)
    else:
        mask = (a[..., 0] > 200) & (a[..., 1] > 180) & (a[..., 2] > 130) & (a[..., 3] > 200)
    rows = np.nonzero(mask.sum(1) > img.width * 0.3)[0]
    return (rows.min() + rows.max()) / 2.0 if len(rows) else img.height / 2.0


def fit(img, max_w, max_h):
    k = min(max_w / img.width, max_h / img.height)
    return img.resize((max(1, round(img.width * k)), max(1, round(img.height * k))), Image.LANCZOS)


def place_group(canvas, parts, center, gap):
    """Pastes pictures side by side, the whole row centred on `center`, each centred vertically on it."""
    total = sum(p.width for p in parts) + gap * (len(parts) - 1)
    x = round(center[0] - total / 2.0)
    for p in parts:
        canvas.alpha_composite(p, (x, round(center[1] - p.height / 2.0)))
        x += p.width + gap


GREEN = load("island/island_pill_glossy.png")   # the home Play base with its triangle painted out
CREAM = soft_rim(load("island/island_pill_cream.png"))
CREAM_CAP = 82


def green_face(w, h):
    """The glossy green of the home page's Play, scaled whole (a three-slice would cut through its gloss)."""
    return GREEN.resize((w, h), Image.LANCZOS)


def green_button(size, word_img, icon=None, word_h=0.5):
    w, h = size[0] * SCALE, size[1] * SCALE
    img = green_face(w, h)
    cy = face_center_y(img, True)
    parts = []
    room = w - h * 0.9
    if icon is not None:
        ic = fit(icon, h * 0.7, h * 0.7)
        parts.append(ic)
        room -= ic.width + h * 0.08
    parts.append(fit(word_img, room, h * word_h))
    place_group(img, parts, (w / 2.0, cy), round(h * 0.08))
    return img


def tab(selected, picture, word_img, size):
    w, h = size[0] * SCALE, size[1] * SCALE
    if selected:
        img = green_face(w, h)
    else:
        base = soft_rim(load("island/island_tab_plain.png"))
        img = stretch(base, (w, h), base.height * 0.5)
    cy = face_center_y(img, selected)
    pic = fit(trim(picture), h * 0.96, h * 0.96)
    img.alpha_composite(pic, (round(h * 0.14), round(h / 2.0 - pic.height / 2.0)))
    left = h * 0.14 + pic.width
    room = w - left - h * 0.34
    wd = fit(word_img, room, h * 0.52)
    img.alpha_composite(wd, (round(left + (w - h * 0.22 - left - wd.width) / 2.0), round(cy - wd.height / 2.0)))
    return img


def main():
    os.makedirs(OUT, exist_ok=True)
    shop = {"open": (420, 128), "equip": (320, 104)}
    for lang in LANGS:
        for name, size in shop.items():
            green_button(size, label(lang, name, "white"), None, 0.46).save(os.path.join(OUT, "btn_%s_%s.png" % (name, lang)))
        for name in ["boosters", "outfits", "nests"]:
            pic = load("island/island_tab_%s.png" % name)
            for on in [True, False]:
                wd = label(lang, "tab_" + name, "white" if on else "ink")
                tab(on, pic, wd, (322, 124)).save(
                    os.path.join(OUT, "btn_tab_%s_%s_%s.png" % (name, "on" if on else "off", lang)))


if __name__ == "__main__":
    main()
