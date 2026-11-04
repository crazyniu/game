"""Size generated cosmetics for delivery and build the requested inspection sheets."""
import json
import re
import uuid
from collections import deque
from pathlib import Path

import numpy as np
from PIL import Image, ImageDraw, ImageOps

ROOT = Path(__file__).resolve().parents[2]
ART = ROOT / "art/cosmetics"
RES = ROOT / "PerchSort/Assets/Resources"
EVIDENCE = ROOT / "docs/work-log/cosmetic-art/evidence"


def feet(image):
    pixels = np.asarray(image)
    solid = pixels[:, :, 3] > 180
    bottom = np.where(solid.sum(axis=1) > image.width * 0.025)[0][-1]
    y, x = np.indices(solid.shape)
    red, green, blue = [pixels[:, :, i].astype(float) for i in range(3)]
    mask = solid & (y > bottom - image.height * 0.045)
    mask &= (red > 150) & (green > 55) & (green < 225) & (blue < 100) & (red > green * 1.1)
    xs = x[mask]
    middle = (xs.min() + xs.max()) / 2
    left = np.median(xs[xs < middle])
    right = np.median(xs[xs > middle])
    return (float(left + right) / 2, int(bottom), float(right - left))


def normalize_bird(raw, base):
    rx, ry, rw = feet(raw)
    bx, by, bw = feet(base)
    scale = bw / rw
    resized = raw.resize((round(raw.width * scale), round(raw.height * scale)), Image.Resampling.LANCZOS)
    rx, ry, _ = feet(resized)
    offset = (round(bx - rx), by - ry)
    result = Image.new("RGBA", (320, 320))
    result.alpha_composite(resized, offset)
    return result, {"scale": scale, "offset": offset, "base_feet": [bx, by, bw], "delivered_feet": list(feet(result))}


def meta(path, template):
    target = Path(str(path) + ".meta")
    if target.exists():
        return
    text = Path(str(template) + ".meta").read_text()
    text = re.sub(r"^guid: .*", "guid: " + uuid.uuid4().hex, text, flags=re.M)
    text = text.replace(template.stem, path.stem)
    target.write_text(text)


def aperture(image):
    p = np.asarray(image)
    y, x = np.indices(p.shape[:2])
    mask = (p[:, :, 3] > 200) & (p[:, :, 0] < 100) & (p[:, :, 1] < 55) & (p[:, :, 2] < 45)
    mask &= (x > image.width * .25) & (x < image.width * .75)
    mask &= (y > image.height * .3) & (y < image.height * .85)
    seed = (round(image.height * .6), round(image.width * .5))
    connected = np.zeros_like(mask)
    connected[seed] = True
    pending = deque([seed])
    while pending:
        cy, cx = pending.popleft()
        for ny, nx in [(cy - 1, cx), (cy + 1, cx), (cy, cx - 1), (cy, cx + 1)]:
            if mask[ny, nx] and not connected[ny, nx]:
                connected[ny, nx] = True
                pending.append((ny, nx))
    mask = connected
    xx, yy = x[mask], y[mask]
    return [int(xx.min()), int(yy.min()), int(xx.max()) + 1, int(yy.max()) + 1]


def normalize_house(raw, base):
    r, b = aperture(raw), aperture(base)
    sx, sy = (b[2] - b[0]) / (r[2] - r[0]), (b[3] - b[1]) / (r[3] - r[1])
    width, height = round(raw.width * sx), round(raw.height * sy)
    for _ in range(3):
        resized = raw.resize((width, height), Image.Resampling.LANCZOS)
        box = aperture(resized)
        dw, dh = (b[2] - b[0]) - (box[2] - box[0]), (b[3] - b[1]) - (box[3] - box[1])
        if dw == 0 and dh == 0:
            break
        width += dw
        height += dh
    offset = (b[0] - box[0], b[1] - box[1])
    result = Image.new("RGBA", (512, 512))
    result.alpha_composite(resized, offset)
    return result, {"scale": [sx, sy], "offset": offset, "base_aperture": b, "delivered_aperture": aperture(result)}


def qa_sheet(images, labels, path):
    width = 220 * len(images)
    sheet = Image.new("RGB", (width, 580), "#e5e0d6")
    draw = ImageDraw.Draw(sheet)
    for i, (im, label) in enumerate(zip(images, labels)):
        x = i * 220
        draw.text((x + 8, 8), label, fill="black")
        for y, color in [(28, "#fffaf1"), (244, "#17242c")]:
            sheet.paste(color, (x, y, x + 220, y + 210))
            small = im.resize((200, 200), Image.Resampling.LANCZOS)
            sheet.paste(small, (x + 10, y + 5), small)
        tiny = im.resize((64, 64), Image.Resampling.LANCZOS)
        sheet.paste(tiny, (x + 22, 484), tiny)
        gray = ImageOps.grayscale(tiny).convert("RGBA")
        gray.putalpha(tiny.getchannel("A"))
        sheet.paste(gray, (x + 124, 484), gray)
        draw.text((x + 20, 560), "64 px / grayscale", fill="black")
    sheet.save(path, quality=90)


def main():
    records = json.loads((ART / "generation-log.json").read_text())
    assets = []
    for r in records:
        if r["status"] not in ("accepted", "candidate"):
            continue
        raw = Image.open(ART / "raw" / (r["name"] + ".png")).convert("RGBA")
        if "species" in r:
            base = RES / "Birds" / (r["species"] + "_idle.png")
            result, geometry = normalize_bird(raw, Image.open(base))
            dest = RES / "Birds" / f"{r['species']}_outfit_{r['tier']}.png"
            template = RES / "Birds/sunny_idle.png"
        else:
            base = RES / "Art/house_body.png"
            result, geometry = normalize_house(raw, Image.open(base))
            dest = RES / "Art" / f"house_style_{r['style']}_body.png"
            template = base
        result.save(dest)
        meta(dest, template)
        assets.append({"file": str(dest.relative_to(ROOT)), "size": list(result.size), "pivot": [0.5, 0.5], "raw": "art/cosmetics/raw/" + r["name"] + ".png", "geometry": geometry})
    manifest = [{k: v for k, v in a.items() if k != "geometry"} for a in assets]
    (ART / "art-manifest.json").write_text('{\n  "assets": [\n    ' + ',\n    '.join(json.dumps(a) for a in manifest) + '\n  ]\n}\n')
    inspection = {
        "method": "Visual base/reference comparison, light/dark backgrounds, actual 64 px color and grayscale. Delivery normalization uses resampling and translation only.",
        "rejections": {r["name"]: r["reason"] for r in records if r["status"] == "rejected"},
        "unselected": {r["name"]: r["reason"] for r in records if r["status"] == "unselected"},
        "geometry": [{"file": a["file"], "measurement": a["geometry"]} for a in assets],
    }
    (ART / "qa/inspection.json").write_text(json.dumps(inspection, indent=2) + "\n")
    species = list(dict.fromkeys(r["species"] for r in records if "species" in r))
    for name in species:
        paths = [RES / "Birds" / (name + "_idle.png")] + sorted((RES / "Birds").glob(name + "_outfit_*.png"))
        qa_sheet([Image.open(p) for p in paths], [p.stem for p in paths], EVIDENCE / (name + ".jpg"))
    original = Image.open(RES / "Art/house_body.png")
    original.alpha_composite(Image.open(RES / "Art/house_roof_yellow.png"))
    for path in sorted((RES / "Art").glob("house_style_*_body.png")):
        qa_sheet([original, Image.open(path)], ["original composite", path.stem], EVIDENCE / (path.stem + ".jpg"))


if __name__ == "__main__":
    main()
