#!/usr/bin/env python3
"""Bakes every worded button of the shop and settings screens into one complete picture per language: the glossy
base, its picture or icon, and the painted word, centred on the measured face of the base (not on the picture's
box, whose bottom lip and shadow would push the word up). The game shows each button as that single picture.

python3 tools/island/bake_buttons.py
Output: PerchSort/art/buttons/btn_<name>_<lang>.png (and btn_language_pick_<english|chinese>.png, one per state).
"""
import os

import numpy as np
from PIL import Image

ROOT = os.path.join(os.path.dirname(os.path.abspath(__file__)), "..", "..", "PerchSort", "art")
OUT = os.path.join(ROOT, "buttons")
LANGS = ["en", "zh"]
SCALE = 2   # baked at twice the on-screen size


def load(rel):
    return Image.open(os.path.join(ROOT, rel)).convert("RGBA")


def word(lang, wid):
    img = load("locale/%s/island_words/%s.png" % (lang, wid))
    return img.crop(img.getchannel("A").point(lambda a: 255 if a > 24 else 0).getbbox())


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
CREAM = load("island/island_pill_cream.png")
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


def cream_button(size, word_img, word_h=0.5):
    w, h = size[0] * SCALE, size[1] * SCALE
    img = stretch(CREAM, (w, h), CREAM_CAP)
    cy = face_center_y(img, False)
    place_group(img, [fit(word_img, w - h * 0.9, h * word_h)], (w / 2.0, cy), 0)
    return img


def tab(selected, picture, word_img, size):
    w, h = size[0] * SCALE, size[1] * SCALE
    if selected:
        img = green_face(w, h)
    else:
        base = load("island/island_tab_plain.png")
        img = stretch(base, (w, h), base.height * 0.5)
    cy = face_center_y(img, selected)
    pic = fit(trim(picture), h * 0.96, h * 0.96)
    img.alpha_composite(pic, (round(h * 0.14), round(h / 2.0 - pic.height / 2.0)))
    left = h * 0.14 + pic.width
    room = w - left - h * 0.34
    wd = fit(word_img, room, h * 0.52)
    img.alpha_composite(wd, (round(left + (w - h * 0.22 - left - wd.width) / 2.0), round(cy - wd.height / 2.0)))
    return img


def language_switch(pick, size):
    """Cream track with the green half under the chosen language; each language named in its own words."""
    w, h = size[0] * SCALE, size[1] * SCALE
    img = stretch(CREAM, (w, h), CREAM_CAP)
    inset = round(h * 0.05)
    half = (w - inset * 2) // 2
    knob = green_face(half, h - inset * 2)
    img.alpha_composite(knob, (inset if pick == "english" else inset + half, inset))
    cy = face_center_y(img.crop((inset, 0, inset + half, h)) if pick == "english" else img.crop((inset + half, 0, w, h)), True)
    names = {"english": ("en",), "chinese": ("zh",)}
    for i, key in enumerate(["english", "chinese"]):
        lang = names[key][0]
        chosen = key == pick
        wd = word(lang, ("white_" if chosen else "brown_") + "settings_language_name")
        wd = fit(wd, half * 0.72, h * 0.56)
        x0 = inset + i * half
        img.alpha_composite(wd, (round(x0 + half / 2.0 - wd.width / 2.0), round(cy - wd.height / 2.0)))
    return img


def main():
    os.makedirs(OUT, exist_ok=True)
    question = trim(Image.open(os.path.join(ROOT, "island", "island_question.png")).convert("RGBA"))
    pills = {
        "help": ((620, 172), "white_help", question),
        "open": ((420, 128), "white_open", None),
        "equip": ((320, 104), "white_equip", None),
        "equipped": ((320, 104), "white_equipped", None),
    }
    for lang in LANGS:
        for name, (size, wid, icon) in pills.items():
            green_button(size, word(lang, wid), icon, 0.56 if name == "help" else 0.5).save(os.path.join(OUT, "btn_%s_%s.png" % (name, lang)))
        # Settings while playing: Restart (secondary, cream) beside Home (green), one size.
        cream_button((396, 120), word(lang, "brown_settings_restart")).save(os.path.join(OUT, "btn_restart_%s.png" % lang))
        green_button((396, 120), word(lang, "white_settings_home")).save(os.path.join(OUT, "btn_home_%s.png" % lang))
        for name in ["boosters", "outfits", "nests"]:
            pic = load("island/island_tab_%s.png" % name)
            for on in [True, False]:
                wd = word(lang, ("white_tab_" if on else "tab_") + name)
                tab(on, pic, wd, (322, 124)).save(
                    os.path.join(OUT, "btn_tab_%s_%s_%s.png" % (name, "on" if on else "off", lang)))
    for pick in ["english", "chinese"]:
        language_switch(pick, (816, 140)).save(os.path.join(OUT, "btn_language_pick_%s.png" % pick))


if __name__ == "__main__":
    main()
