#!/usr/bin/env python3
"""Check the delivered PNGs with Python's standard library; no packages needed."""
import struct
import sys
import zlib
from pathlib import Path

ROOT = Path(__file__).resolve().parents[1]


def png_info(path):
    data = path.read_bytes()
    assert data[:8] == b"\x89PNG\r\n\x1a\n", "not a PNG"
    offset, compressed = 8, bytearray()
    while offset < len(data):
        length = struct.unpack_from(">I", data, offset)[0]
        kind = data[offset + 4:offset + 8]
        chunk = data[offset + 8:offset + 8 + length]
        if kind == b"IHDR":
            width, height, depth, mode, compression, filtering, interlace = struct.unpack(">IIBBBBB", chunk)
        elif kind == b"IDAT":
            compressed.extend(chunk)
        offset += length + 12
    assert depth == 8 and interlace == 0, "expected 8-bit non-interlaced PNG"
    channels = {0: 1, 2: 3, 6: 4}[mode]
    raw = zlib.decompress(compressed)
    assert len(raw) == (width * channels + 1) * height, "incomplete pixels"
    return width, height, mode, raw


def alpha_rows(width, height, raw):
    """PNG filters operate per channel, so only the alpha channel needs decoding."""
    stride, previous = width * 4 + 1, bytearray(width)
    for y in range(height):
        line = raw[y * stride:(y + 1) * stride]
        values = bytearray(line[4::4])
        kind = line[0]
        for x, delta in enumerate(values):
            left = values[x - 1] if x else 0
            up = previous[x]
            upper_left = previous[x - 1] if x else 0
            if kind == 0:
                prediction = 0
            elif kind == 1:
                prediction = left
            elif kind == 2:
                prediction = up
            elif kind == 3:
                prediction = (left + up) // 2
            elif kind == 4:
                p = left + up - upper_left
                distances = (abs(p - left), abs(p - up), abs(p - upper_left))
                prediction = (left, up, upper_left)[distances.index(min(distances))]
            else:
                raise ValueError(f"unknown PNG filter {kind}")
            values[x] = (delta + prediction) & 255
        yield values
        previous = values


def main():
    expected = [(f"raw/birds/bird_{i:02d}.png", (1024, 1024), True) for i in range(1, 11)]
    expected += [
        ("raw/env/perch.png", (1024, 256), True),
        ("raw/env/tree_crown.png", (1536, 768), True),
        ("raw/env/background.png", (1080, 1920), False),
        ("raw/ui/icon_1024.png", (1024, 1024), False),
        ("contact_sheet.png", (1440, 780), False),
        ("contact_sheet_gray_64.png", (500, 252), False),
    ]
    failures = []
    for file, size, transparent in expected:
        try:
            width, height, mode, raw = png_info(ROOT / file)
            assert (width, height) == size, f"size {(width, height)}, expected {size}"
            if transparent:
                assert mode == 6, "must be RGBA"
                rows = list(alpha_rows(width, height, raw))
                assert all(not any(row) for row in rows[:16] + rows[-16:]), "top/bottom margin is not transparent"
                assert all(not any(row[:16] + row[-16:]) for row in rows), "left/right margin is not transparent"
                assert any(255 in row for row in rows), "no solid subject pixels"
                if "/birds/" in file:
                    assert any(rows[870]), "no feet on y=870 baseline"
            else:
                assert mode in (0, 2, 6), "unexpected color mode"
                if mode == 6:
                    assert all(min(row) == 255 for row in alpha_rows(width, height, raw)), "must be fully opaque"
            if file == "contact_sheet_gray_64.png":
                assert mode == 0, "64px review must be grayscale"
            print(f"PASS {file}  {width}x{height}")
        except (AssertionError, OSError, ValueError, KeyError, zlib.error) as error:
            failures.append(f"{file}: {error}")
    if failures:
        print("\nFAIL\n" + "\n".join(failures))
        return 1
    print("\nPASS: 14 production assets + 2 contact sheets; 10 RGBA birds; transparent corners/margins; aligned feet.")
    print("Visual review: contact_sheet_gray_64.png and qa/alpha_edges.png (readability and fringe are human checks).")
    return 0


if __name__ == "__main__":
    sys.exit(main())
