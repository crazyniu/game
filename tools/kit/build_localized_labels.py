#!/usr/bin/env python3
"""Bake whole locale phrases and numeral cells for runtime texture rendering.

Run: uv run --no-project --isolated --with pillow tools/kit/build_localized_labels.py [locale]
On macOS, expose FriBiDi's library directory through DYLD_LIBRARY_PATH.
"""

import argparse
import json
import math
from pathlib import Path

from PIL import Image, ImageDraw, ImageFont


ROOT = Path(__file__).resolve().parents[2]
FONT_SIZE = 768


def load_font(catalog_path, settings, weight):
    font = ImageFont.truetype(
        str(catalog_path.parent / settings["file"]),
        FONT_SIZE,
        layout_engine=ImageFont.Layout.RAQM,
    )
    if settings["variable_weight"]:
        font.set_variation_by_axes([weight])
    return font


def render(text, font, locale, metrics, destination, heading=False):
    scale = metrics["cap"] / -font.getbbox("H", anchor="ls")[1]
    stroke = round(metrics["cap"] * 0.10) if heading else 0
    padding = metrics["padding"] + stroke
    lines = text.split("\n")
    bounds = [font.getbbox(line, anchor="ls", language=locale) for line in lines]
    left = min(0, *(box[0] for box in bounds))
    right = max(
        max(box[2], font.getlength(line, language=locale))
        for line, box in zip(lines, bounds)
    )
    width = math.ceil((right - left) * scale + padding * 2)
    height = metrics["line_height"] * len(lines) + stroke * 2
    image = Image.new("RGBA", (math.ceil(width / scale), math.ceil(height / scale)))
    draw = ImageDraw.Draw(image)
    outline = {"stroke_width": round(stroke / scale), "stroke_fill": "#96521c"} if heading else {}
    for row, line in enumerate(lines):
        x = padding / scale - left
        baseline = (metrics["baseline"] + row * metrics["line_height"] + stroke) / scale
        draw.text((x, baseline), line, font=font, fill="white", anchor="ls", language=locale, **outline)
    image.resize((width, height), Image.Resampling.LANCZOS).save(destination)


def render_numbers(catalog_path, catalog, output, symbol_font):
    settings = catalog["number_font"]
    font = load_font(catalog_path, settings, settings["weight"])
    locale = catalog["locale"]
    cap = catalog["metrics"]["cap"]
    padding = 6
    digit_boxes = [font.getbbox(char, anchor="ls", language=locale) for char in "0123456789"]
    top = min(box[1] for box in digit_boxes)
    bottom = max(box[3] for box in digit_boxes)
    scale = cap / (bottom - top)
    digit_advance = math.ceil(max(box[2] - box[0] for box in digit_boxes) * scale) + padding * 2
    glyphs = {}
    advances = {}
    for char in "0123456789/?+-x%∞":
        digit = char in "0123456789"
        face = font if digit else symbol_font
        left, ink_top, right, ink_bottom = face.getbbox(char, anchor="ls", language=locale)
        glyph_scale = scale if digit else min(
            cap / -face.getbbox("H", anchor="ls")[1], cap / (ink_bottom - ink_top)
        )
        cell = digit_advance if digit else math.ceil((right - left) * glyph_scale) + padding * 2
        image = Image.new("RGBA", (math.ceil(cell / glyph_scale), math.ceil(cap / glyph_scale)))
        x = (image.width - (right - left)) / 2 - left
        y = -top if digit else (image.height - (ink_bottom - ink_top)) / 2 - ink_top
        ImageDraw.Draw(image).text((x, y), char, font=face, fill="white", anchor="ls", language=locale)
        code = {"/": "slash", "?": "question", "+": "plus", "-": "minus", "%": "percent", "∞": "infinity"}.get(char, char)
        filename = f"number_{code}.png"
        image.resize((cell, cap), Image.Resampling.LANCZOS).save(output / filename)
        glyphs[char] = filename
        advances[char] = cell
    return {"cap": cap, "line_height": cap, "baseline": round(-top * scale),
            "advance": digit_advance, "advances": advances, "glyphs": glyphs}


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("locale", nargs="?", default="en")
    selection = parser.add_mutually_exclusive_group()
    selection.add_argument("--numbers-only", action="store_true", help="Replace numeral cells and their manifest metrics only")
    selection.add_argument("--labels-only", nargs="+", metavar="LABEL_ID", help="Replace only the named labels and their manifest entries")
    args = parser.parse_args()
    catalog_path = Path(__file__).with_name("locales") / f"{args.locale}.json"
    catalog = json.loads(catalog_path.read_text(encoding="utf-8"))
    locale = catalog["locale"]
    settings = catalog["font"]
    metrics = catalog["metrics"]
    output = ROOT / "PerchSort" / "art" / "locale" / locale
    output.mkdir(parents=True, exist_ok=True)
    fonts = {
        role: load_font(catalog_path, settings, weight)
        for role, weight in settings["weights"].items()
    }
    shared = {key: metrics[key] for key in ("cap", "line_height", "baseline")}
    manifest = json.loads((output / "manifest.json").read_text(encoding="utf-8")) if args.numbers_only or args.labels_only else {
        "locale": locale, "labels": {}
    }
    labels = {label_id: catalog["labels"][label_id] for label_id in args.labels_only} if args.labels_only else catalog["labels"]
    if args.numbers_only:
        labels = {}
    headings = 0
    for label_id, text in labels.items():
        filename = "label_" + label_id.replace(".", "_") + ".png"
        role = "title" if label_id.endswith(".title") else "name"
        face = fonts[role]   # one typeface family for every word and numeral, a lighter weight than the old display face
        render(text, face, locale, metrics, output / filename)
        manifest["labels"][label_id] = {"text": text, "file": filename, **shared}
        if label_id in ("aviary.title", "shop.title") or (
            label_id.startswith(("bird.", "world.")) and label_id.endswith(".name")
        ):
            heading_file = "heading_" + label_id.replace(".", "_") + ".png"
            render(text, fonts["title"], locale, metrics, output / heading_file, heading=True)
            manifest["labels"][label_id]["heading_file"] = heading_file
            headings += 1

    if not args.labels_only:
        manifest["numbers"] = render_numbers(catalog_path, catalog, output, fonts["numbers"])
    (output / "manifest.json").write_text(
        json.dumps(manifest, ensure_ascii=False, indent=2) + "\n", encoding="utf-8"
    )
    number_count = 0 if args.labels_only else len(manifest["numbers"]["glyphs"])
    print(f"Rendered {len(labels)} labels, {headings} outlined headings, "
          f"and {number_count} numeral cells to {output}")


if __name__ == "__main__":
    main()
