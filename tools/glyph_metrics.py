#!/usr/bin/env python3
"""Measure source glyph alpha bounds. Run with Pillow after changing glyph PNGs."""
import json
from pathlib import Path

from PIL import Image


def main():
    art = Path(__file__).resolve().parents[1] / "PerchSort/Assets/Resources/Art"
    glyphs = []
    for path in sorted([*art.glob("icon_*.png"), *art.glob("tool_*.png")]):
        with Image.open(path) as image:
            alpha = image.convert("RGBA").getchannel("A")
            bounds = alpha.point(lambda value: 255 if value >= 40 else 0).getbbox()
            if bounds is None:
                raise ValueError(f"Glyph has no opaque pixels: {path}")
            left, top, right, bottom = bounds
            glyphs.append(dict(name=path.stem, canvasWidth=image.width, canvasHeight=image.height,
                               x=left, y=top, width=right-left, height=bottom-top))
    destination = art / "glyph_metrics.json"
    destination.write_text(json.dumps(dict(alphaThreshold=40, glyphs=glyphs), indent=2) + "\n")
    print(f"Measured {len(glyphs)} glyphs: {destination}")


if __name__ == "__main__":
    main()
