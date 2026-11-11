#!/usr/bin/env python3
"""Installs the painted Settings popup (local Codex, art/approved_ui/settings.png as the target): one whole panel picture
per language with its words painted in, and the pieces that change or are pressed, painted in the same session.

python3 tools/island/install_settings_art.py <job dir>
  <job dir>/out/panel_en_clean.png, panel_zh.png  -> art/island/panel_settings_<lang>.png (1080 x 1920, the panel at
                                                     PANEL_AT, its ropes repeated up to the screen top)
  toggle_on/off.png                               -> art/buttons/btn_switch_on/off.png (one shared size)
  language_english/chinese.png                    -> art/buttons/btn_language_pick_<english|chinese>.png
  help_<lang>.png, restart_<lang>.png, home_<lang>.png -> art/buttons/btn_<name>_<lang>.png
"""
import os
import sys

from PIL import Image

ART = os.path.join(os.path.dirname(os.path.abspath(__file__)), "..", "..", "PerchSort", "art")
PANEL_AT = (28, 200)      # top-left of the 1024 x 1536 panel picture on the 1080 x 1920 screen (game/popups.gd)
ROPE_BAND = 24            # the top rows of the picture (rope only), repeated upwards


def alpha_box(img):
    return img.getchannel("A").point(lambda a: 255 if a > 16 else 0).getbbox()


def panel(src):
    img = Image.open(src).convert("RGBA")
    screen = Image.new("RGBA", (1080, 1920))
    band = img.crop((0, 0, img.width, ROPE_BAND))
    y = PANEL_AT[1] - ROPE_BAND
    while y > -ROPE_BAND:
        screen.paste(band, (PANEL_AT[0], y), band)
        y -= ROPE_BAND
    screen.alpha_composite(img, PANEL_AT)
    return screen


def main():
    out = os.path.join(sys.argv[1], "out")
    buttons = os.path.join(ART, "buttons")
    for lang, name in [("en", "panel_en_clean.png"), ("zh", "panel_zh.png")]:
        panel(os.path.join(out, name)).save(os.path.join(ART, "island", "panel_settings_%s.png" % lang))
    on = Image.open(os.path.join(out, "toggle_on.png")).convert("RGBA")
    off = Image.open(os.path.join(out, "toggle_off.png")).convert("RGBA")
    a, b = alpha_box(on), alpha_box(off)
    # Each state cut to its own visible track, then both stored at one size so the switch never jumps.
    size = (400, 180)
    on.crop(a).resize(size, Image.LANCZOS).save(os.path.join(buttons, "btn_switch_on.png"))
    off.crop(b).resize(size, Image.LANCZOS).save(os.path.join(buttons, "btn_switch_off.png"))
    for pick in ["english", "chinese"]:
        img = Image.open(os.path.join(out, "language_%s.png" % pick)).convert("RGBA")
        img.crop(alpha_box(img)).save(os.path.join(buttons, "btn_language_pick_%s.png" % pick))
    for name in ["help", "restart", "home"]:
        for lang in ["en", "zh"]:
            src = os.path.join(out, "%s_%s.png" % (name, lang))
            if os.path.exists(src):
                img = Image.open(src).convert("RGBA")
                img.crop(alpha_box(img)).save(os.path.join(buttons, "btn_%s_%s.png" % (name, lang)))


if __name__ == "__main__":
    main()
