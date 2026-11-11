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


GOLD_TOP = (255, 232, 110)
GOLD_BOTTOM = (245, 160, 20)
BROWN = (122, 58, 14, 255)


def render(text, font, locale, metrics, destination, heading=False):
    """Plain labels are white; a heading is island lettering: gold gradient fill, soft top shine, brown outline and shadow."""
    scale = metrics["cap"] / -font.getbbox("H", anchor="ls")[1]
    stroke = round(metrics["cap"] * 0.12) if heading else 0
    padding = metrics["padding"] + stroke
    lines = text.split("\n")
    bounds = [font.getbbox(line, anchor="ls", language=locale) for line in lines]
    left = min(0, *(box[0] for box in bounds))
    right = max(
        max(box[2], font.getlength(line, language=locale))
        for line, box in zip(lines, bounds)
    )
    width = math.ceil((right - left) * scale + padding * 2)
    height = metrics["line_height"] * len(lines) + stroke * 2 + (round(metrics["cap"] * 0.06) if heading else 0)
    size = (math.ceil(width / scale), math.ceil(height / scale))
    image = Image.new("RGBA", size)
    draw = ImageDraw.Draw(image)
    outline = {"stroke_width": round(stroke / scale), "stroke_fill": "#96521c"} if heading else {}
    if not heading and metrics.get("embolden"):   # thin faces (Chinese) are thickened with a same-colour stroke
        outline = {"stroke_width": round(metrics["embolden"] / scale), "stroke_fill": "white"}
    if heading:
        outline_layer = Image.new("RGBA", size)
        fill_mask = Image.new("L", size)
        odraw = ImageDraw.Draw(outline_layer)
        mdraw = ImageDraw.Draw(fill_mask)
        for row, line in enumerate(lines):
            x = padding / scale - left
            baseline = (metrics["baseline"] + row * metrics["line_height"] + stroke) / scale
            odraw.text((x, baseline), line, font=font, fill=BROWN, anchor="ls", language=locale,
                       stroke_width=round(stroke / scale), stroke_fill=BROWN)
            mdraw.text((x, baseline), line, font=font, fill=255, anchor="ls", language=locale)
        gradient = Image.new("RGBA", size)
        gpix = gradient.load()
        for y in range(size[1]):
            t = min(1.0, max(0.0, y / size[1]))
            col = tuple(round(GOLD_TOP[i] * (1 - t) + GOLD_BOTTOM[i] * t) for i in range(3)) + (255,)
            for x in range(size[0]):
                gpix[x, y] = col
        shine = Image.new("RGBA", size, (255, 255, 255, 0))
        sdraw = ImageDraw.Draw(shine)
        sdraw.rectangle((0, 0, size[0], int(size[1] * 0.42)), fill=(255, 255, 255, 70))
        gradient = Image.alpha_composite(gradient, shine)
        shadow = Image.new("RGBA", size)
        shadow.paste((60, 25, 5, 110), (0, round(metrics["cap"] * 0.05 / scale)), outline_layer.split()[3])
        image = Image.alpha_composite(shadow, outline_layer)
        image.paste(gradient, (0, 0), fill_mask)
    else:
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
    existing = output / "manifest.json"
    previous = json.loads(existing.read_text(encoding="utf-8")) if existing.exists() else {}
    manifest = previous if args.numbers_only or args.labels_only else {"locale": locale, "labels": {}}
    # Painted (image-generated) words are not rendered here; a full rebuild must keep them.
    if "painted" in previous:
        manifest["painted"] = previous["painted"]
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
        if label_id.endswith((".title", ".name")) or label_id in ("map.chapters", "popup.best"):
            heading_file = "heading_" + label_id.replace(".", "_") + ".png"
            render(text, fonts["title"], locale, metrics, output / heading_file, heading=True)
            manifest["labels"][label_id]["heading_file"] = heading_file
            headings += 1

    if not args.labels_only:
        number_face = load_font(catalog_path, catalog["number_font"], catalog["number_font"]["weight"])
        manifest["numbers"] = render_numbers(catalog_path, catalog, output, number_face)
    (output / "manifest.json").write_text(
        json.dumps(manifest, ensure_ascii=False, indent=2) + "\n", encoding="utf-8"
    )
    number_count = 0 if args.labels_only else len(manifest["numbers"]["glyphs"])
    print(f"Rendered {len(labels)} labels, {headings} outlined headings, "
          f"and {number_count} numeral cells to {output}")


if __name__ == "__main__":
    main()
