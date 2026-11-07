"""Lettering comparison for the user: the same UI samples in several lettering treatments.

python3 tools/kit/lettering_options.py [letters_dir_b]   -> build/style/lettering.png
Rows: current (ink, flat), A (current letters, white with a warm outline and soft shadow), B (round letters from
letters_dir_b, same treatment) when given. Not used by the game.
"""
import json
import os
import sys

from PIL import Image, ImageDraw, ImageFilter

ROOT = os.path.dirname(os.path.dirname(os.path.dirname(os.path.abspath(__file__))))
P = os.path.join(ROOT, "PerchSort")
INK = (90, 58, 30)
EDGE = (150, 82, 28)
CREAM = (255, 247, 232)
RIM = (233, 214, 176)


def load_letters(d):
    return d, json.load(open(os.path.join(d, "letters.json")))


def glyph_mask(letters, text, cap, gap=0.07):
    d, meta = letters
    k = cap / meta["cap"]
    parts, x = [], 0.0
    for ch in text:
        if ch == " ":
            x += meta["space"] * k
            continue
        g = meta["glyphs"][str(ord(ch))]
        im = Image.open(os.path.join(d, "letter_%04x.png" % ord(ch))).convert("RGBA").split()[3]
        im = im.resize((max(1, round(g["w"] * k)), max(1, round(g["h"] * k))), Image.LANCZOS)
        parts.append((im, x, g["dy"] * cap))
        x += im.width + gap * cap
    pad = int(cap * 0.3)
    mask = Image.new("L", (int(x) + pad * 2, int(cap * 1.5) + pad * 2), 0)
    for im, px, dy in parts:
        mask.paste(im, (int(px) + pad, int(cap * 1.15 + dy) + pad), im)
    return mask, pad


def text(base, letters, t, cx, cy, cap, style):
    mask, pad = glyph_mask(letters, t, cap, 0.07 if style.startswith("flat") else 0.17)
    layer = Image.new("RGBA", mask.size, (0, 0, 0, 0))
    if style.startswith("flat"):
        colour = INK if style == "flat" else (138, 86, 46)
        layer.paste(Image.new("RGBA", mask.size, colour + (255,)), (0, 0), mask)
    else:
        r = max(2, int(cap * 0.11))
        edge = mask.filter(ImageFilter.MaxFilter(r * 2 + 1))
        shadow = edge.filter(ImageFilter.GaussianBlur(cap * 0.04))
        sh = Image.new("RGBA", mask.size, (60, 30, 10, 0))
        sh.putalpha(shadow.point(lambda v: int(v * 0.35)))
        layer.alpha_composite(sh, (0, int(cap * 0.06)))
        layer.paste(Image.new("RGBA", mask.size, EDGE + (255,)), (0, 0), edge)
        layer.paste(Image.new("RGBA", mask.size, (255, 255, 255, 255)), (0, 0), mask)
    x = int(cx - mask.width / 2)
    y = int(cy - (cap * 1.15 + pad) + cap * 0.5)
    base.alpha_composite(layer, (x, y))


def shape(base, box, radius, fill, rim, edge):
    sh = Image.new("RGBA", base.size, (0, 0, 0, 0))
    ImageDraw.Draw(sh).rounded_rectangle((box[0], box[1] + 8, box[2], box[3] + 8), radius, fill=(0, 0, 0, 46))
    base.alpha_composite(sh.filter(ImageFilter.GaussianBlur(10)))
    d = ImageDraw.Draw(base)
    d.rounded_rectangle(box, radius, fill=rim)
    d.rounded_rectangle((box[0], box[1], box[2], box[3] - edge), radius, fill=fill)


def icon(base, name, cx, cy, size, color):
    im = Image.open(os.path.join(P, "art", "icons", "icon_%s.png" % name)).convert("RGBA")
    im = im.resize((int(size), int(size)), Image.LANCZOS)
    tint = Image.new("RGBA", im.size, color + (0,))
    tint.putalpha(im.split()[3])
    base.alpha_composite(tint, (int(cx - size / 2), int(cy - size / 2)))


def row(letters, style, close_kind):
    bg = Image.open(os.path.join(P, "world", "scene", "board_w1_bg.png")).convert("RGBA").crop((0, 0, 1080, 420))
    base = Image.new("RGBA", (1080, 420))
    base.alpha_composite(bg)
    # top bar
    fill, rim = ((239, 85, 72), (194, 58, 47)) if close_kind == "red" else (CREAM, RIM)
    shape(base, (48, 40, 160, 152), 56, fill, rim, 10)
    icon(base, "close", 104, 92, 62, (255, 255, 255) if close_kind == "red" else INK)
    text(base, letters, "Shop", 540, 96, 64, style)
    shape(base, (760, 48, 1032, 144), 48, CREAM, RIM, 8)
    coin = Image.open(os.path.join(P, "art", "coin.png")).convert("RGBA").resize((72, 72))
    base.alpha_composite(coin, (776, 56))
    text(base, letters, "1240", 930, 92, 44, style)
    # card and popup title
    shape(base, (48, 190, 400, 400), 40, CREAM, RIM, 10)
    text(base, letters, "Firefly", 224, 236, 40, style)
    bulb = Image.open(os.path.join(P, "art", "booster_hint_illus.png")).convert("RGBA")
    bulb = bulb.crop(bulb.getbbox()).resize((110, 110))
    base.alpha_composite(bulb, (169, 268))
    shape(base, (440, 190, 1032, 400), 56, CREAM, RIM, 12)
    text(base, letters, "Well done!", 736, 250, 64, style)
    text(base, letters, "Morning Bell", 736, 340, 44, style)
    return base


def main():
    current = load_letters(os.path.join(P, "art", "letters"))
    rows = [("current", row(current, "flat", "cream")), ("A", row(current, "edge", "red"))]
    if len(sys.argv) > 1:
        rows.append(("B", row(load_letters(sys.argv[1]), "edge", "red")))
        rows.append(("C", row(load_letters(sys.argv[1]), "flat_soft", "red")))
    out = Image.new("RGBA", (1180, 440 * len(rows)), (255, 255, 255, 255))
    draw = ImageDraw.Draw(out)
    for i, (name, im) in enumerate(rows):
        out.alpha_composite(im, (100, i * 440))
        draw.rectangle((10, i * 440 + 170, 90, i * 440 + 250), fill=(60, 60, 60))
        mask, _ = glyph_mask(load_letters(os.path.join(P, "art", "letters")), name if name != "current" else "Now", 40)
        out.paste((255, 255, 255), (14, i * 440 + 180), mask.crop((0, 0, min(mask.width, 76), mask.height)))
    os.makedirs(os.path.join(ROOT, "build", "style"), exist_ok=True)
    out.convert("RGB").save(os.path.join(ROOT, "build", "style", "lettering.png"))
    print("rows:", [n for n, _ in rows])


main()
