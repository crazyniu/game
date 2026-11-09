#!/usr/bin/env python3
"""Bake the settings documents (privacy policy, terms, credits, contact) into painted text pages.

uv run --no-project --isolated --with pillow tools/kit/build_documents.py [locale]
Source: tools/kit/locales/<locale>_docs.json ({id: {title, body}}). Output: PerchSort/art/locale/<locale>/doc_<id>.png,
white text on transparency (the game tints it ink), body text in the game lettering faces (Lilita One, ZCOOL KuaiLe) wrapped to WIDTH px, plus
doc_<id>_title.png in the display face. Words are baked offline; the game never renders a font.
"""
import json
import sys
from pathlib import Path

from PIL import Image, ImageDraw, ImageFont

ROOT = Path(__file__).resolve().parents[2]
WIDTH = 1520          # baked at 2x of the 760 px reading column
SIZE = 56             # body size at 2x (28 px on screen)
LEADING = 1.45


NO_LINE_START = "，。、；：！？）”」』》"


def wrap(draw, text, font, by_character=False):
    lines = []
    for para in text.split("\n"):
        if not para:
            lines.append("")
            continue
        line = ""
        # Chinese has no spaces: break between characters (never before closing punctuation) but keep Latin words whole.
        words = para.split(" ")
        if by_character:
            words = []
            for ch in para:
                if words and (ch in NO_LINE_START or (ch.isascii() and words[-1][-1].isascii() and ch != " ")):
                    words[-1] += ch
                else:
                    words.append(ch)
        glue = "" if by_character else " "
        for word in words:
            trial = (line + glue + word).strip() if not by_character else line + word
            if draw.textlength(trial, font=font) <= WIDTH:
                line = trial
            else:
                lines.append(line)
                line = word
        lines.append(line)
    return lines


def main():
    locale = sys.argv[1] if len(sys.argv) > 1 else "en"
    docs = json.loads((ROOT / "tools/kit/locales" / f"{locale}_docs.json").read_text(encoding="utf-8"))
    out = ROOT / "PerchSort/art/locale" / locale
    chinese = locale == "zh"
    body = ImageFont.truetype(str(ROOT / "tools/kit/fonts" / ("zcool_kuaile.ttf" if chinese else "lilita_one.ttf")), SIZE,
                              layout_engine=ImageFont.Layout.RAQM)
    probe = ImageDraw.Draw(Image.new("L", (10, 10)))
    for doc_id, doc in docs.items():
        lines = wrap(probe, doc["body"], body, chinese)
        step = int(SIZE * LEADING)
        img = Image.new("RGBA", (WIDTH + 8, step * len(lines) + 16), (255, 255, 255, 0))
        draw = ImageDraw.Draw(img)
        for i, line in enumerate(lines):
            draw.text((4, 8 + i * step), line, font=body, fill="white", language=locale,
                      stroke_width=2 if chinese else 0, stroke_fill="white")
        img.save(out / f"doc_{doc_id}.png")
        print(doc_id, img.size, len(lines), "lines")


if __name__ == "__main__":
    main()
