#!/usr/bin/env python3
"""Bakes the simple-UI word pictures offline from one font per language (Lilita One for English, Noto Sans SC Bold for
Chinese), so every word on a screen has the same letter shapes, weight and colour. The game shows these pictures; no
font is rendered at runtime.

python3 tools/island/bake_labels.py
Output: PerchSort/art/labels/label_<id>_<style>_<lang>.png, one line box per font (same height and baseline for every
word), built at CAP px.
Styles: ink (warm brown on cream), soft (lighter brown, captions), title (white, brown outline) and white (white with a soft green-dark shadow, for green buttons).
"""
import json
import os

from PIL import Image, ImageChops, ImageDraw, ImageFilter, ImageFont

HERE = os.path.dirname(os.path.abspath(__file__))
ROOT = os.path.join(HERE, "..", "..")
OUT = os.path.join(ROOT, "PerchSort", "art", "labels")
FONTS = {"en": os.path.join(ROOT, "tools", "kit", "fonts", "lilita_one.ttf"),
         "zh": os.path.join(ROOT, "tools", "kit", "fonts", "noto_sans_sc_bold.otf")}
CAP = 120                  # build size: the font's em in px
MARGIN = 24
INK = (107, 58, 23)        # warm brown
SOFT = (150, 104, 62)      # lighter brown for one-line captions
WHITE_SHADOW = (30, 100, 20)

# id -> (English, Chinese, styles). The language names are written in their own language in both packs.
WORDS = {
    "settings": ("Settings", "设置", ["ink"]),
    "music": ("Music", "音乐", ["ink"]),
    "sound": ("Sound", "音效", ["ink"]),
    "vibration": ("Vibration", "震动", ["ink"]),
    "language": ("Language", "语言", ["ink"]),
    "help": ("Help", "帮助", ["white"]),
    "restart": ("Restart", "重来", ["ink"]),
    "home": ("Home", "主页", ["white"]),
    "lang_en": ("English", "English", ["ink", "white"]),
    "lang_zh": ("中文", "中文", ["ink", "white"]),
    # Shop
    "shop": ("Shop", "商店", ["title"]),
    "tab_boosters": ("Boosters", "道具", ["ink", "white"]),
    "tab_outfits": ("Outfits", "装扮", ["ink", "white"]),
    "tab_nests": ("Nests", "鸟窝", ["ink", "white"]),
    "equip": ("Equip", "装备", ["white"]),
    "equipped": ("Equipped", "已装备", ["soft"]),
    "open": ("Open", "打开", ["white"]),
    "buy": ("Buy", "购买", ["white"]),
    "item_firefly": ("Firefly", "萤火灯", ["ink"]),
    "item_sprout": ("Sprout", "新枝芽", ["ink"]),
    "item_owl_eye": ("Owl Eye", "猫头鹰眼", ["ink"]),
    "item_wake_bell": ("Wake Bell", "唤醒铃", ["ink"]),
    "item_scarecrow": ("Scarecrow", "稻草人", ["ink"]),
    "item_spring_rain": ("Spring Rain", "春雨", ["ink"]),
    "cap_firefly": ("Lights a good move", "照亮好走法", ["soft"]),
    "cap_sprout": ("Grows a new perch", "长出一根新树枝", ["soft"]),
    "cap_owl_eye": ("See inside an egg", "看清蛋里的鸟", ["soft"]),
    "cap_wake_bell": ("Wakes sleeping birds", "叫醒睡着的鸟", ["soft"]),
    "cap_scarecrow": ("Shoos crows away", "赶走乌鸦", ["soft"]),
    "cap_spring_rain": ("Grows your branches", "让树枝长大", ["soft"]),
    "house_mushroom": ("Mushroom", "蘑菇", ["ink"]),
    "house_teapot": ("Teapot", "茶壶", ["ink"]),
    "house_lantern": ("Lantern", "灯笼", ["ink"]),
    "house_acorn": ("Acorn", "橡果", ["ink"]),
    "house_pumpkin": ("Pumpkin", "南瓜", ["ink"]),
    "house_beehive": ("Beehive", "蜂巢", ["ink"]),
    "house_teacup": ("Teacup", "茶杯", ["ink"]),
    "house_pinecone": ("Pinecone", "松果", ["ink"]),
    "house_cage": ("Cage", "鸟笼", ["ink"]),
    "bird_sunny": ("Sunny", "阳阳", ["ink"]),
    "bird_sky": ("Skye", "天天", ["ink"]),
    "bird_berry": ("Berry", "莓莓", ["ink"]),
    "bird_mint": ("Minty", "薄荷", ["ink"]),
    "bird_cocoa": ("Cocoa", "可可", ["ink"]),
    "bird_snow": ("Flurry", "雪球", ["ink"]),
    "bird_ember": ("Ember", "火火", ["ink"]),
    "bird_crow": ("Rook", "乌乌", ["ink"]),
    "bird_ink": ("Inky", "墨墨", ["ink"]),
    "bird_hoot": ("Hoot", "咕咕", ["ink"]),
    "outfits_owned": ("outfits", "套装扮", ["soft"]),
}


# The bundled Chinese face is a subset for the old labels; new words need the full Simplified Chinese face (Noto Sans
# CJK Bold, OFL, index 2 of the collection). Set NOTO_CJK to its path where it lives elsewhere.
NOTO_CJK = os.environ.get("NOTO_CJK", "/usr/share/fonts/opentype/noto/NotoSansCJK-Bold.ttc")


def font_for(text, lang):
    # A language name is drawn in its own script's font.
    if any(ord(ch) > 0x2E80 for ch in text):
        font = ImageFont.truetype(NOTO_CJK, CAP, index=2)
        missing = [ch for ch in text if ch.strip() and font.getmask(ch).getbbox() is None]
        if missing:
            raise SystemExit("no glyph for %s in %s" % ("".join(missing), NOTO_CJK))
        return font
    return ImageFont.truetype(FONTS["en"], CAP)


def render(text, lang, style):
    """One fixed line box per font (ascent + descent), so every word of a style has the same height and baseline and
    can be shown at one size; only the width follows the word."""
    font = font_for(text, lang)
    ascent, descent = font.getmetrics()
    left, _, right, _ = font.getbbox(text)
    w, h = right - left + MARGIN * 2, ascent + descent + MARGIN * 2
    mask = Image.new("L", (w, h), 0)
    ImageDraw.Draw(mask).text((MARGIN - left, MARGIN), text, font=font, fill=255)
    out = Image.new("RGBA", (w, h), (0, 0, 0, 0))
    if style == "title":   # screen titles over the sky: white, warm-brown outline, soft drop
        ring = Image.new("L", (w, h), 0)
        ImageDraw.Draw(ring).text((MARGIN - left, MARGIN), text, font=font, fill=255, stroke_width=9, stroke_fill=255)
        drop = ImageChops.offset(ring, 0, 6).filter(ImageFilter.GaussianBlur(3))
        out.paste(Image.new("RGBA", (w, h), (60, 30, 8, 255)), (0, 0), drop.point(lambda a: a * 0.4))
        out.paste(Image.new("RGBA", (w, h), INK + (255,)), (0, 0), ring)
        out.paste(Image.new("RGBA", (w, h), (255, 255, 255, 255)), (0, 0), mask)
    elif style == "white":
        shadow = ImageChops.offset(mask, 0, 5).filter(ImageFilter.GaussianBlur(2))
        out.paste(Image.new("RGBA", (w, h), WHITE_SHADOW + (255,)), (0, 0), shadow.point(lambda a: a * 0.55))
        out.paste(Image.new("RGBA", (w, h), (255, 255, 255, 255)), (0, 0), mask)
    else:
        out.paste(Image.new("RGBA", (w, h), (SOFT if style == "soft" else INK) + (255,)), (0, 0), mask)
    return out


def visual_mid(text):
    """Height (px from the picture top) of the line's optical middle: half the capital height for Latin, the middle of
    a full ideograph for Chinese. Words are centred on this, not on the box, so descenders never push them up."""
    font = font_for(text, "")
    if any(ord(ch) > 0x2E80 for ch in text):
        _, top, _, bottom = font.getbbox("中")
        return MARGIN + (top + bottom) / 2.0
    _, top, _, _ = font.getbbox("H")
    ascent, _ = font.getmetrics()
    return MARGIN + (top + ascent) / 2.0


def main():
    os.makedirs(OUT, exist_ok=True)
    meta = {"cap": CAP}
    for wid, (en, zh, styles) in WORDS.items():
        for lang, text in [("en", en), ("zh", zh)]:
            for style in styles:
                name = "label_%s_%s_%s" % (wid, style, lang)
                img = render(text, lang, style)
                img.save(os.path.join(OUT, name + ".png"))
                meta[name] = round(visual_mid(text), 1)
    with open(os.path.join(OUT, "labels.json"), "w") as f:
        json.dump(meta, f, indent=1, sort_keys=True)
    print("baked", len(os.listdir(OUT)), "files")


if __name__ == "__main__":
    main()
